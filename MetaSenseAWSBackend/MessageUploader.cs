using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using BackendAPI.Data;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace BackendAPI
{
    public class MessageUploader
    {
        public static async Task ProcessIncompletePackesBatchAsync(string CoreId, DateTime startTime, DateTime endTime, ILambdaContext context)
        {
            var metaSenseAwsReadsManager = new MetaSenseAWSReadsManager(context.Logger);
            context.Logger.LogLine($"Processing data for {CoreId} from {startTime} to {endTime}.");

            await metaSenseAwsReadsManager.ComposePartialReadsIncomplete(CoreId, startTime, endTime);
        }
        public static Task ParticleCloudComposePartialAsync(S3Event evnt, ILambdaContext context)
        {
            var metaSenseAwsReadsManager = new MetaSenseAWSReadsManager(context.Logger);
            return Task.CompletedTask;
            //TODO React to S3
        }
        public static async Task ParticleCloudHandlerAsync(MetaSenseParticleEvent evnt, ILambdaContext context)
        {
            var metaSenseAwsReadsManager = new MetaSenseAWSReadsManager(context.Logger);
            context.Logger.LogLine($"Message Type: {evnt.Event}:CoreId: {evnt.CoreId} - Format: {evnt.Format} - Firmware Version: {evnt.FwVersion}");
            var b64 = evnt.Format == "b64";
            var json = evnt.Format == "json";
            switch (evnt.Event)
            {
                case "MSG":
                    if (json)
                    {
                        var msg = MetaSenseMessage.FromJsonString(evnt.Data);
                        if (msg == null)
                        {
                            context.Logger.LogLine($"Failed to decode message {evnt.Data} in as json Message.");
                        }
                        else
                        {
                            if (msg.Raw != null)
                            {
                                context.Logger.LogLine($"Storing read at {msg.TimeStamp} for sensor {evnt.CoreId}");
                                await metaSenseAwsReadsManager.StoreRead(evnt.CoreId, msg, MetaSenseAWSReadsManager.KeyNameData);
                            }
                            else
                            {
                                await metaSenseAwsReadsManager.LogMessage(evnt.CoreId, evnt.PublishedAt, evnt.Data);
                            }
                        }
                    }
                    break;
                case "BMSG":
                    if (b64)
                    {
                        var base64EncodedBytes = Convert.FromBase64String(evnt.Data);
                        var msg = MetaSenseMessage.FromBinaryMessage(base64EncodedBytes, 0, out var msgLen);
                        if (msgLen == 0)
                        {
                            context.Logger.LogLine($"Failed to decode message {evnt.Data} in as b64 Message.");
                        }
                        else
                        {
                            context.Logger.LogLine($"Storing read at {msg.TimeStamp} for sensor {evnt.CoreId}");
                            await metaSenseAwsReadsManager.StoreRead(evnt.CoreId, msg, MetaSenseAWSReadsManager.KeyNameData);
                        }
                    }
                    break;
                case "BATCH":
                    if (b64)
                    {
                        context.Logger.LogLine(evnt.Data);
                        var timestamp = evnt.Data.Substring(0, 8);
                        await metaSenseAwsReadsManager.ProcessPartialRead(evnt.CoreId, timestamp, int.Parse(evnt.Data.Substring(8, 2)), int.Parse(evnt.Data.Substring(10, 2)), evnt.PublishedAt, evnt.Data.Substring(12));
                    }
                    break;
            }
        }
        public static async Task<APIGatewayProxyResponse> SensorUploadBinaryAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var metaSenseAwsReadsManager = new MetaSenseAWSReadsManager(context.Logger);
            string sensorid = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(MetaSenseAWSReadsManager.IdQueryStringName))
                sensorid = request.PathParameters[MetaSenseAWSReadsManager.IdQueryStringName];
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(MetaSenseAWSReadsManager.IdQueryStringName))
                sensorid = request.QueryStringParameters[MetaSenseAWSReadsManager.IdQueryStringName];
            if (string.IsNullOrEmpty(sensorid))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {MetaSenseAWSReadsManager.IdQueryStringName}"
                };
            }

            context.Logger.LogLine($"Uploading single read for sensorid: {sensorid}");
            if (request.IsBase64Encoded)
            {
                var decodedBytes = Convert.FromBase64String(request.Body);
                int msgLen;
                var msg = MetaSenseMessage.FromBinaryMessage(decodedBytes, 0, out msgLen);
                if (msgLen == 0)
                {
                    throw new Exception($"Failed to decode element the binary message.");
                }
                context.Logger.LogLine($"Storing read at {msg.TimeStamp} for sensor {sensorid}");
                await metaSenseAwsReadsManager.StoreRead(sensorid, msg, MetaSenseAWSReadsManager.KeyNameData);
            }
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
        public static async Task<APIGatewayProxyResponse> SensorBatchUploadBinaryAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var metaSenseAwsReadsManager = new MetaSenseAWSReadsManager(context.Logger);
            string sensorid = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(MetaSenseAWSReadsManager.IdQueryStringName))
                sensorid = request.PathParameters[MetaSenseAWSReadsManager.IdQueryStringName];
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(MetaSenseAWSReadsManager.IdQueryStringName))
                sensorid = request.QueryStringParameters[MetaSenseAWSReadsManager.IdQueryStringName];
            if (string.IsNullOrEmpty(sensorid))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {MetaSenseAWSReadsManager.IdQueryStringName}"
                };
            }

            context.Logger.LogLine($"Uploading batch data for sensorid: {sensorid}");

            //if (request.IsBase64Encoded)
            //{
            context.Logger.LogLine($"Body: {request.Body}");
                var msgs = metaSenseAwsReadsManager.ConvertCompressedMessagesArray(Convert.FromBase64String(request.Body));
            context.Logger.LogLine($"{msgs.Count} Messages were Decoded.");
                foreach (var msg in msgs)
                {
                    context.Logger.LogLine($"Storing read at {msg.TimeStamp} for sensor {sensorid}");
                    await metaSenseAwsReadsManager.StoreRead(sensorid, msg, MetaSenseAWSReadsManager.KeyNameData);
                }
            //}
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}