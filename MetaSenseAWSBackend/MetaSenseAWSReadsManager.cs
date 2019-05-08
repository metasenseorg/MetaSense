using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Amazon.S3;
using Amazon.S3.Model;
using BackendAPI.Compression;
using BackendAPI.Data;

namespace BackendAPI
{
    public class MetaSenseAWSReadsManager
    {
        public static readonly string KeyNamePartial = $"{KeyNameStart}/temp";
        public static readonly string KeyNameData = $"{KeyNameStart}/data";
        public static readonly string KeyNameIncomplete = $"{KeyNameStart}/incomplete";
        public static readonly string KeyNameMessageLog = $"{KeyNameStart}/msgLogs";
        public static readonly RegionEndpoint S3Region = RegionEndpoint.USWest2;
        public const string BucketName = "data.metasense.ucsd.edu";
        public const string KeyNameStart = "particle";
        public const string IdQueryStringName = "sensorid";
        private readonly IAmazonS3 _client = new AmazonS3Client(S3Region);
        private ILambdaLogger logger;

        public MetaSenseAWSReadsManager(ILambdaLogger logger)
        {
            this.logger = logger;
        }

        public List<MetaSenseMessage> ConvertCompressedMessagesArray(byte[] base64EncodedBytes)
        {
            var decoder = new HeatshrinkDecoder(base64EncodedBytes);
            var decodedBytes = decoder.Decode();
            var ret = new List<MetaSenseMessage>();
            var pos = 0;
            try
            {
                for (var i = 0; i < 20; i++)
                {
                    var msg = MetaSenseMessage.FromBinaryMessage(decodedBytes, pos, out var msgLen);
                    if (msgLen == 0)
                    {
                        throw new Exception($"Failed to decode element {i} in ConvertCompressedMessagesArray.");
                    }
                    ret.Add(msg);
                    pos += msgLen;
                }
            }
            catch (Exception ex)
            {
                //Ignore
                //Failed processing some messege
            }

            return ret;
        }

        public async Task<List<string>> ListPartials(string sensor)
        {
            var ret = new List<string>();
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = BucketName,
                    Prefix = $"{KeyNamePartial}/{sensor}/",
                    
                    MaxKeys = 100
                };
                ListObjectsV2Response response;
                do
                {
                    response = await _client.ListObjectsV2Async(request);
                    // Process response.
                    foreach (var entry in response.S3Objects)
                    {
                        ret.Add(entry.Key);
                        Console.WriteLine("key = {0} size = {1}",
                            entry.Key, entry.Size);
                    }
                    Console.WriteLine("Next Continuation Token: {0}", response.NextContinuationToken);
                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated == true);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                     ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine(
                        "To sign up for service, go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine(
                        "Error occurred. Message:'{0}' when listing objects",
                        amazonS3Exception.Message);
                }
            }
            return ret;
        }

        public async Task LogMessage(string sensor, DateTime evntPublishedAt, string message)
        {
            
                try
                {
                    var datePartition = evntPublishedAt.ToString("yyyy-MM-dd");
                    var filename = $"Ts={TimeManagementUtils.DateTimeToUnix(evntPublishedAt)}";

                    var putRequest = new PutObjectRequest
                    {
                        BucketName = BucketName,
                        Key = $"{KeyNameMessageLog}/sensorid={sensor}/date={datePartition}/{filename}",
                        ContentBody = message,
                        ContentType = "application/json"
                    };
                    //putRequest.Metadata.Add("timestamp", evntPublishedAt.ToString("R"));
                    var response = await _client.PutObjectAsync(putRequest);
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    if (amazonS3Exception.ErrorCode != null &&
                        (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                         ||
                         amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                    {
                        logger.LogLine("Invalid AWS Credentials provided.");
                    }
                    else
                    {
                        logger.LogLine($"Error occurred. Message:'{amazonS3Exception.Message}' when writing an object.");
                    }
                }
            
        }

        public async Task StoreRead(string sensor, MetaSenseMessage message, string prefix)
        {
            if (message.Ts == null) return;
            if (message.Raw == null) return;
            var read = new Read(
                sensor,
                message.Ts.Value,
                message.Raw,
                message.HuPr,
                message.Co2,
                message.Voc,
                message.Loc);
            var serializer = new JsonSerializer();
            string strRead;
            using (var memStream = new MemoryStream(1000))
            {
                serializer.Serialize(read, memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memStream))
                {
                    strRead = reader.ReadToEnd();
                }
            }
            if (strRead == null) return;

            try
            {
                var datePartition = message.TimeStamp.ToString("yyyy-MM-dd");
                var putRequest = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = $"{prefix}/sensorid={sensor}/date={datePartition}/{read.Ts}",
                    ContentBody = strRead,
                    ContentType = "application/json"
                };
                putRequest.Metadata.Add("timestamp", message.TimeStamp.ToString("R"));
                var response = await _client.PutObjectAsync(putRequest);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                        ||
                        amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    logger.LogLine("Invalid AWS Credentials provided.");
                }
                else
                {
                    logger.LogLine($"Error occurred. Message:'{amazonS3Exception.Message}' when writing an object.");
                }
            }
        }

        public async Task ProcessPartialRead(string sensor, string timestamp, int pos, int num, DateTime publishedTime, string fragment)
        {
            try
            {
                    Console.WriteLine("Uploading an object fragment");
                    try
                    {
                        var putRequest = new PutObjectRequest
                        {
                            BucketName = BucketName,
                            Key = $"{KeyNamePartial}/{sensor}/{timestamp}:{num}/{pos}",
                            ContentBody = fragment
                        };
                        putRequest.Metadata.Add("PublishTime", publishedTime.ToString("R"));
                        Console.WriteLine($"Putting fragment to {putRequest.BucketName}:{putRequest.Key}.");
                        var response = await _client.PutObjectAsync(putRequest);
                        Console.WriteLine("Fragment put done");
                        if (pos == num - 1)
                        {
                            await ComposePartialReads(sensor);
                        }
                    }
                    catch (AmazonS3Exception amazonS3Exception)
                    {
                        if (amazonS3Exception.ErrorCode != null &&
                            (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                             ||
                             amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                        {
                            Console.WriteLine("Invalid AWS Credentials provided.");
                            logger.LogLine("Invalid AWS Credentials provided.");
                        }
                        else if (amazonS3Exception.ErrorCode != null &&
                                 (amazonS3Exception.ErrorCode.Equals("NoSuchKey")))
                        {
                            //Ignore this error we expect this to happen
                            Console.WriteLine("NoSuchKey.");
                        }
                        else
                        {
                            Console.WriteLine(
                                $"Error occurred. Message:'{amazonS3Exception.Message}' in ProcessPartialRead for node {sensor} and time {publishedTime}.");
                            logger.LogLine(
                                $"Error occurred. Message:'{amazonS3Exception.Message}' in ProcessPartialRead for node {sensor} and time {publishedTime}.");
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(
                            $"Error occurred in ProcessPartialRead. Message:'{exception.Message}' when writing an object.");
                        logger.LogLine(
                            $"Error occurred in ProcessPartialRead. Message:'{exception.Message}' when writing an object.");
                    }
                

            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while processing Partial Read: {e.Message}.");
                logger.LogLine($"Exception while processing Partial Read: {e.Message}.");
                throw;
            }
            finally
            {
                Console.WriteLine("Finished ProcessPartialRead");
            }
        }

        public async Task ComposePartialReadsIncomplete(string sensor, DateTime start, DateTime end)
        {
            var startUnix = TimeManagementUtils.DateTimeToUnix(start);
            var endUnix = TimeManagementUtils.DateTimeToUnix(end);
            var lst = await ListPartials(sensor);
            logger.LogLine("List returned");
            try
            {
                var lstTuples = from str in lst
                                select new Tuple<string, string>(str.Substring(0, str.LastIndexOf("/", StringComparison.Ordinal)), str);
                var lstTimestamps = from t in lstTuples
                                    group t by t.Item1 into g
                                    select new Tuple<string, int, List<string>>(
                                        g.Key.Substring(g.Key.LastIndexOf("/", StringComparison.Ordinal) + 1, 8),
                                        int.Parse(g.Key.Substring(g.Key.LastIndexOf(":", StringComparison.Ordinal) + 1)),
                                        g.Select(e => e.Item2).ToList());
                foreach (var fileGroup in lstTimestamps)
                {
                    //List<MetaSenseMessage> messages = new List<MetaSenseMessage>();
                    var groupTime = Convert.ToUInt32(fileGroup.Item1, 16);
                    if (groupTime >= startUnix && groupTime <= endUnix)
                    {
                        logger.LogLine($"Processing file group {fileGroup.Item1}");
                        //if (fileGroup.Item3.Count == fileGroup.Item2)
                        {
                            logger.LogLine($"All fragments found. Ready to compose the message.");
                            //All files are present
                            var sb = new StringBuilder();
                            int f_val = 0;
                            foreach (var filename in fileGroup.Item3.OrderBy(s =>
                                int.Parse(s.Substring(s.LastIndexOf("/", StringComparison.Ordinal) + 1))))
                            {
                                int cur_val = int.Parse(filename.Substring(filename.LastIndexOf("/", StringComparison.Ordinal) + 1));
                                if (cur_val != f_val++)
                                    break;
                                var s = await GetS3ObjString(_client, filename);
                                logger.LogLine($"Appending file {filename} -> {s}");
                                sb.Append((string) s);
                            }

                            var msg = sb.ToString();
                            try
                            {
                                logger.LogLine($"Message to convert: {msg}");
                                var tmpStr = msg;
                                if (msg.Length % 4 != 0)
                                    tmpStr = msg.Substring(0, (msg.Length / 4) * 4);
                                var arr = ConvertCompressedMessagesArray(System.Convert.FromBase64String(tmpStr));
                                //messages.AddRange(arr);
                                foreach (var m in arr)
                                {
                                    logger.LogLine($"Storing read at {m.TimeStamp} for sensor {sensor}");
                                    await StoreRead(sensor, m, KeyNameData);
                                }
                                for (var i = 0; i < f_val; i++)
                                {
                                    logger.LogLine($"Delete partial {fileGroup.Item1}.");
                                    await DeletePartialReads(_client, sensor, fileGroup.Item1, i, fileGroup.Item2);
                                }
                                await MoveFileGroupToError(_client, fileGroup.Item3);
                            }
                            catch (Exception e)
                            {
                                logger.LogLine($"Exception: {e.Message}.");
                                logger.LogLine($"Stacktrace: {e.StackTrace}");
                                logger.LogLine(
                                    $"Error decoding the messages list for {fileGroup.Item1}. Ignoring this group for now.");
                                await MoveFileGroupToError(_client, fileGroup.Item3);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogLine($"Error composing partials: {e.Message}/n{e.StackTrace}");
            }
        }
        public async Task ComposePartialReads(string sensor)
        {
            var lst = await ListPartials(sensor);
            logger.LogLine("List returned");
            try
            {
                var lstTuples = from str in lst
                    select new Tuple<string, string>(str.Substring(0, str.LastIndexOf("/", StringComparison.Ordinal)), str);
                var lstTimestamps = from t in lstTuples
                    group t by t.Item1 into g
                    select new Tuple<string, int, List<string>>(
                        g.Key.Substring(g.Key.LastIndexOf("/", StringComparison.Ordinal) + 1, 8), 
                        int.Parse(g.Key.Substring(g.Key.LastIndexOf(":", StringComparison.Ordinal) + 1)), 
                        g.Select(e => e.Item2).ToList());
                foreach (var fileGroup in lstTimestamps)
                {
                    logger.LogLine($"Processing file group {fileGroup.Item1}");
                    if (fileGroup.Item3.Count == fileGroup.Item2)
                    {
                        logger.LogLine($"All fragments found. Ready to compose the message.");
                        //All files are present
                        var sb = new StringBuilder();
                        foreach (var filename in fileGroup.Item3.OrderBy(s =>
                            int.Parse(s.Substring(s.LastIndexOf("/", StringComparison.Ordinal) + 1))))
                        {
                            var s = await GetS3ObjString(_client, filename);
                            logger.LogLine($"Appending file {filename} -> {s}");
                            sb.Append((string) s);
                        }

                        var msg = sb.ToString();
                        try
                        {
                            logger.LogLine($"Message to convert: {msg}");
                            var arr = ConvertCompressedMessagesArray(System.Convert.FromBase64String(msg)); 
                            //messages.AddRange(arr);
                            foreach (var m in arr)
                            {
                                logger.LogLine($"Storing read at {m.TimeStamp} for sensor {sensor}");
                                await StoreRead(sensor, m, KeyNameData);
                            }
                            for (var i = 0; i < fileGroup.Item2; i++)
                            {
                                logger.LogLine($"Delete partial {fileGroup.Item1}.");
                                await DeletePartialReads(_client, sensor, fileGroup.Item1, i, fileGroup.Item2);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogLine($"Exception: {e.Message}.");
                            logger.LogLine($"Stacktrace: {e.StackTrace}");
                            logger.LogLine($"Error decoding the messages list for {fileGroup.Item1}. Ignoring this group for now.");

                            await MoveFileGroupToError(_client, fileGroup.Item3);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogLine($"Error composing partials: {e.Message}/n{e.StackTrace}");
            }
        }

        public async Task MoveFileGroupToError(IAmazonS3 client, List<string> fileGroupNames)
        {
            foreach (var file in fileGroupNames)
            {
                var newfile = file.Replace("particle/temp/", "particle/err/");
                try
                {
                    var copyRequest = new CopyObjectRequest
                    {
                        SourceBucket = BucketName,
                        DestinationBucket = BucketName,
                        SourceKey = file,
                        DestinationKey = newfile
                    };
                    logger.LogLine($"Movign fragment {file} to {newfile}.");
                    var response = await client.CopyObjectAsync(copyRequest);
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        logger.LogLine($"Fragment {file} copied done");
                        var delRequest = new DeleteObjectRequest
                        {
                            BucketName = BucketName,
                            Key = file
                        };
                        var resp = await client.DeleteObjectAsync(delRequest);
                        if(resp.HttpStatusCode==HttpStatusCode.OK)
                            logger.LogLine($"Fragment {file} deleted");
                    }
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    if (amazonS3Exception.ErrorCode != null &&
                        (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                         ||
                         amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                    {
                        Console.WriteLine("Invalid AWS Credentials provided.");
                        logger.LogLine("Invalid AWS Credentials provided.");
                    }
                    else if (amazonS3Exception.ErrorCode != null &&
                             (amazonS3Exception.ErrorCode.Equals("NoSuchKey")))
                    {
                        //Ignore this error we expect this to happen
                        Console.WriteLine("NoSuchKey.");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Error occurred. Message:'{amazonS3Exception.Message}' in MoveFileGroupToError.");
                        logger.LogLine(
                            $"Error occurred. Message:'{amazonS3Exception.Message}' in MoveFileGroupToError.");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(
                        $"Error occurred in ProcessPartialRead. Message:'{exception.Message}' when writing an object.");
                    logger.LogLine(
                        $"Error occurred in ProcessPartialRead. Message:'{exception.Message}' when writing an object.");
                }
            }
        }

        public async Task<string> GetS3ObjString(IAmazonS3 client, string filename) //string sensor, string timestamp, int fragNum
        {
            string fragment0;
            var getRequest0 = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = filename
            };
            using (var response0 = await client.GetObjectAsync(getRequest0))
            using (var responseStream = response0.ResponseStream)
            using (var reader = new StreamReader(responseStream))
            {
                var publishTime = response0.Metadata["PublishTime"];
                fragment0 = await reader.ReadToEndAsync();
            }
            return fragment0;
        }

        public async Task DeletePartialReads(IAmazonS3 client, string sensor, string timestamp, int fragNum, int fragTotal)
        {
            var delRequest = new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = $"{KeyNamePartial}/{sensor}/{timestamp}:{fragTotal}/{fragNum}"
            };
            var resp = await client.DeleteObjectAsync(delRequest);
            logger.LogLine($"Deleting {delRequest.Key} result {resp.HttpStatusCode}.");
        }
    }
}