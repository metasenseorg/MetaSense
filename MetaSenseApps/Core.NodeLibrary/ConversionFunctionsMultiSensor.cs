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
    /// Parameters of trained neural networks to calibrate the conversion functions to a sensor
    /// for the multi-sensor model.
    /// </summary>
    public class JsonCalibrationParamsMultiSensor
    {
        [JsonProperty("act_functions_sensor_no2_o3")]
        public List<string> sensorActivationFunctionsNO2O3;
        [JsonProperty("weights_sensor_no2_o3")]
        public List<List<List<double>>> sensorWeightsNO2O3;
        [JsonProperty("biases_sensor_no2_o3")]
        public List<List<double>> sensorBiasesNO2O3;
        [JsonProperty("act_functions_calibration_no2_o3")]
        public List<string> calibActivationFunctionsNO2O3;
        [JsonProperty("weights_calibration_no2_o3")]
        public List<List<List<double>>> calibWeightsNO2O3;
        [JsonProperty("biases_calibration_no2_o3")]
        public List<List<double>> calibBiasesNO2O3;
        [JsonProperty("act_functions_co")]
        public List<string> activationFunctionsCO;
        [JsonProperty("weights_co")]
        public List<List<List<double>>> weightsCO;
        [JsonProperty("biases_co")]
        public List<List<double>> biasesCO;
    }

    /// <summary>
    /// Functions that convert the sensor measurements to the amount of each pollutant gas using
    /// two levels of neural networks: one to convert the voltage differences into their
    /// representations and the other to convert the representations and measurements of the
    /// environment to the amount of each pollutant gas.
    /// 
    /// Note: the CO is calculated using one level of neural network with the voltage differences
    /// and measurements of the environment input at the same time instead of the multi-sensor
    /// approach.
    /// </summary>
    public class ConversionFunctionsMultiSensor : ConversionFunctionsNeuralNet
    { 
        private JsonCalibrationParamsMultiSensor _paramsMultiSensor;
        private NeuralNetwork _sensorNeuralNetNO2O3;
        private NeuralNetwork _calibNeuralNetNO2O3;
        private NeuralNetwork _neuralNetCO;

        /// <summary>
        /// Deserialize the json into the parameters for the neural networks of the multi-sensor
        /// model used for NO2 and O3 and the parameters of the single neural network used for CO.
        /// </summary>
        /// <param name="json">The JSON containing the parameters for the neural networks used for
        ///     converting</param>
        public ConversionFunctionsMultiSensor(string json)
        {
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include
            };
            JsonConvert.DefaultSettings = () => settings;
            _paramsMultiSensor =
                JsonConvert.DeserializeObject<JsonCalibrationParamsMultiSensor>(json);

            _sensorNeuralNetNO2O3 =
                new NeuralNetwork(_paramsMultiSensor.sensorActivationFunctionsNO2O3,
                _paramsMultiSensor.sensorWeightsNO2O3, _paramsMultiSensor.sensorBiasesNO2O3);

            _calibNeuralNetNO2O3 =
                new NeuralNetwork(_paramsMultiSensor.calibActivationFunctionsNO2O3,
                _paramsMultiSensor.calibWeightsNO2O3, _paramsMultiSensor.calibBiasesNO2O3);

            _neuralNetCO = new NeuralNetwork(_paramsMultiSensor.activationFunctionsCO,
                _paramsMultiSensor.weightsCO, _paramsMultiSensor.biasesCO);
        }

        /// <summary>
        /// Convert the sensor measurements to the amount of each pollutant gas by using two levels
        /// of neural networks for NO2 and O3 (multi-sensor approach) and one level for CO (simple
        /// approach).
        /// </summary>
        /// <param name="featuresInput">The input to the neural networks</param>
        /// <returns>A GasReading with the pollutant gases in ppm or ppb.</returns>
        public override GasReading NeuralNetConvert(FeaturesInput featuresInput)
        {
            var conversions = new GasReading();
            var sensorInputMat = Matrix<double>.Build.DenseOfArray(featuresInput.VoltDiffsTo2dArray());
            var sensorOutput = _sensorNeuralNetNO2O3.Convert(sensorInputMat);

            // feed output of sensor neural network as input to the calibration neural network
            double[,] calibInputArr = new double[1, 6] { { sensorOutput.ElementAt(0),
                sensorOutput.ElementAt(1), sensorOutput.ElementAt(2), featuresInput.Temperature,
                featuresInput.AbsoluteHumidity, featuresInput.Pressure } };
            var calibInputMat = Matrix<double>.Build.DenseOfArray(calibInputArr);

            // convert to NO2 and O3
            var conversionNO2O3 = _calibNeuralNetNO2O3.Convert(calibInputMat);
            conversions.NO2ppb = conversionNO2O3.ElementAt(0);
            conversions.O3ppb = conversionNO2O3.ElementAt(1);

            // convert to CO
            var coInputMat = Matrix<double>.Build.DenseOfArray(featuresInput.To2dArray());
            var conversionCO = _neuralNetCO.Convert(coInputMat);
            conversions.COppm = conversionCO.ElementAt(0) / 1000;

            // debug printing
            Log.Trace($"Features: [[{featuresInput.NO2}, {featuresInput.O3}, " +
                $"{featuresInput.CO}]], [[{featuresInput.Temperature}, " +
                $"{featuresInput.AbsoluteHumidity}, {featuresInput.Pressure}]]");
            Log.Trace($"Multi-sensor Output: {conversions.NO2ppb}  {conversions.O3ppb}  " +
                $"{conversions.COppm}");

            return conversions;
        }

        public bool ValidCalibrationFile
        {
            get
            {
                if (_paramsMultiSensor == null) return false;

                // check activation functions
                if (_paramsMultiSensor.sensorActivationFunctionsNO2O3 == null
                    || _paramsMultiSensor.sensorActivationFunctionsNO2O3.Count <= 0)
                    return false;
                if (_paramsMultiSensor.calibActivationFunctionsNO2O3 == null
                    || _paramsMultiSensor.calibActivationFunctionsNO2O3.Count <= 0)
                    return false;
                if (_paramsMultiSensor.activationFunctionsCO == null
                    || _paramsMultiSensor.activationFunctionsCO.Count <= 0)
                    return false;

                // check weights
                if (_paramsMultiSensor.sensorWeightsNO2O3 == null
                    || _paramsMultiSensor.sensorWeightsNO2O3.Count <= 0)
                    return false;
                if (_paramsMultiSensor.calibWeightsNO2O3 == null
                    || _paramsMultiSensor.calibWeightsNO2O3.Count <= 0)
                    return false;
                if (_paramsMultiSensor.weightsCO == null
                    || _paramsMultiSensor.weightsCO.Count <= 0)
                    return false;

                // check biases
                if (_paramsMultiSensor.sensorBiasesNO2O3 == null
                    || _paramsMultiSensor.sensorBiasesNO2O3.Count <= 0)
                    return false;
                if (_paramsMultiSensor.calibBiasesNO2O3 == null
                    || _paramsMultiSensor.calibBiasesNO2O3.Count <= 0)
                    return false;
                if (_paramsMultiSensor.biasesCO == null
                    || _paramsMultiSensor.biasesCO.Count <= 0)
                    return false;

                // check that there are weights and biases for each layer
                if (_paramsMultiSensor.sensorWeightsNO2O3.Count() !=
                    _paramsMultiSensor.sensorBiasesNO2O3.Count())
                    return false;
                if (_paramsMultiSensor.calibWeightsNO2O3.Count() !=
                    _paramsMultiSensor.calibBiasesNO2O3.Count())
                    return false;
                if (_paramsMultiSensor.weightsCO.Count() !=
                    _paramsMultiSensor.biasesCO.Count())
                    return false;

                return true;
            }
        }
    }
}
