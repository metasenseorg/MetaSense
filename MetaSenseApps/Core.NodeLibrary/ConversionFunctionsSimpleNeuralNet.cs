using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary
{
    /// <summary>
    /// Parameters of trained neural networks to calibrate the conversion functions to a sensor for
    /// the simple neural network.
    /// </summary>
    public class JsonCalibrationParamsSimpleNeuralNet
    {
        [JsonProperty("act_functions_no2_o3")]
        public List<string> activationFunctionsNO2O3;
        [JsonProperty("weights_no2_o3")]
        public List<List<List<double>>> weightsNO2O3;
        [JsonProperty("biases_no2_o3")]
        public List<List<double>> biasesNO2O3;
        [JsonProperty("act_functions_co")]
        public List<string> activationFunctionsCO;
        [JsonProperty("weights_co")]
        public List<List<List<double>>> weightsCO;
        [JsonProperty("biases_co")]
        public List<List<double>> biasesCO;
    }

    /// <summary>
    /// Functions that convert the sensor measurements to the amount of each pollutant gas by
    /// inputting the voltage differences and measurements of the environment to one neural
    /// network.
    /// 
    /// Note: since the amount of data available to train the NO2 and O3 model was significantly
    /// higher than the amount of data available to train the CO model, separate models were
    /// created.
    /// </summary>
    public class ConversionFunctionsSimpleNeuralNet : ConversionFunctionsNeuralNet
    {
        private JsonCalibrationParamsSimpleNeuralNet _paramsNeuralNet;
        private NeuralNetwork _neuralNetNO2O3;
        private NeuralNetwork _neuralNetCO;

        /// <summary>
        /// Deserialize the json into parameters for the neural network for NO2 and O3 and the
        /// neural network for CO.
        /// </summary>
        /// <param name="json">The JSON containing the parameters for the neural networks used for
        ///     converting</param>
        public ConversionFunctionsSimpleNeuralNet(string json)
        {
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include
            };
            JsonConvert.DefaultSettings = () => settings;
            _paramsNeuralNet =
                JsonConvert.DeserializeObject<JsonCalibrationParamsSimpleNeuralNet>(json);

            _neuralNetNO2O3 =
                new NeuralNetwork(_paramsNeuralNet.activationFunctionsNO2O3,
                _paramsNeuralNet.weightsNO2O3, _paramsNeuralNet.biasesNO2O3);

            _neuralNetCO = new NeuralNetwork(_paramsNeuralNet.activationFunctionsCO,
                _paramsNeuralNet.weightsCO, _paramsNeuralNet.biasesCO);
        }

        /// <summary>
        /// Convert the sensor measurements to the amount of each pollutant gas by using one neural
        /// network for NO2 and O3 and one neural network for CO.
        /// </summary>
        /// <param name="featuresInput">The input to the neural networks</param>
        /// <returns>A GasReading with the pollutant gases in ppm or ppb.</returns>
        public override GasReading NeuralNetConvert(FeaturesInput featuresInput)
        {
            var conversions = new GasReading();
            var inputFeatureMat = Matrix<double>.Build.DenseOfArray(featuresInput.To2dArray());

            var conversionNO2O3 = _neuralNetNO2O3.Convert(inputFeatureMat);
            conversions.NO2ppb = conversionNO2O3.ElementAt(0);
            conversions.O3ppb = conversionNO2O3.ElementAt(1);

            var conversionCO = _neuralNetCO.Convert(inputFeatureMat);
            conversions.COppm = conversionCO.ElementAt(0) / 1000;

            // debug printing
            Log.Trace($"Features: [[{featuresInput.NO2}, {featuresInput.O3}, " +
                $"{featuresInput.CO}, {featuresInput.Temperature}, " +
                $"{featuresInput.AbsoluteHumidity}, {featuresInput.Pressure}]]");
            Log.Trace($"Neural Network Output: {conversions.NO2ppb}  {conversions.O3ppb}  " +
                $"{conversions.COppm}");

            return conversions;
        }

        public bool ValidCalibrationFile
        {
            get
            {
                if (_paramsNeuralNet == null) return false;

                // check activation functions
                if (_paramsNeuralNet.activationFunctionsNO2O3 == null
                    || _paramsNeuralNet.activationFunctionsNO2O3.Count <= 0)
                    return false;
                if (_paramsNeuralNet.activationFunctionsCO == null
                    || _paramsNeuralNet.activationFunctionsCO.Count <= 0)
                    return false;

                // check weights
                if (_paramsNeuralNet.weightsNO2O3 == null
                    || _paramsNeuralNet.weightsNO2O3.Count <= 0)
                    return false;
                if (_paramsNeuralNet.weightsCO == null
                    || _paramsNeuralNet.weightsCO.Count <= 0)
                    return false;

                // check biases
                if (_paramsNeuralNet.biasesNO2O3 == null
                    || _paramsNeuralNet.biasesNO2O3.Count <= 0)
                    return false;
                if (_paramsNeuralNet.biasesCO == null
                    || _paramsNeuralNet.biasesCO.Count <= 0)
                    return false;

                // check that there are weights and biases for each layer
                if (_paramsNeuralNet.weightsNO2O3.Count() !=
                    _paramsNeuralNet.biasesNO2O3.Count())
                    return false;
                if (_paramsNeuralNet.weightsCO.Count() !=
                    _paramsNeuralNet.biasesCO.Count())
                    return false;

                return true;
            }
        }
    }
}
