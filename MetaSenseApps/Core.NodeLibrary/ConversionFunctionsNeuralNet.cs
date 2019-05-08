using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary
{
    /// <summary>
    /// Functions that convert the sensor measurements to the amount of each pollutant gas using
    /// one or more neural networks.
    /// </summary>
    public abstract class ConversionFunctionsNeuralNet : IConversionFunctions
    {
        private const int NO2_IND = 0;
        private const int O3_IND = 1;
        private const int CO_IND = 2;

        public ConversionInputOutput _prevConversion;
        public long _prevConversionTime;

        public ConversionFunctionsNeuralNet()
        {
            _prevConversion = null;
            _prevConversionTime = default(long);
        }

        /// <summary>
        /// Convert the sensor measurements to CO (ppm).
        /// </summary>
        /// <param name="coA_mV">CO auxiliary electrode reading (mV)</param>
        /// <param name="coW_mV">CO working electrode reading (mV)</param>
        /// <param name="o3A_mV">O3 auxiliary electrode reading (mV)</param>
        /// <param name="o3W_mV">O3 working electrode reading (mV)</param>
        /// <param name="no2A_mV">NO2 auxiliary electrode reading (mV)</param>
        /// <param name="no2W_mV">NO2 working electrode reading (mV)</param>
        /// <param name="temperature_mV">PT temperature sensor reading (mV)</param>
        /// <param name="humidity_pc">Relative humidity percent</param>
        /// <param name="pressure_millibar">Pressure (mbar)</param>
        /// <param name="temp_C">Temperature (degrees Celsius)</param>
        /// <returns>A double representing the CO (ppm).</returns>
        public double COppm(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV,
           double no2A_mV, double no2W_mV, double temperature_mV, double humidity_pc,
           double pressure_millibar, double temp_C)
        {
            var calculation = Convert(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV,
                temperature_mV, humidity_pc, pressure_millibar, temp_C);
            return calculation.COppm;
        }

        /// <summary>
        /// Convert sensor measurements to NO2 (ppb).
        /// </summary>
        /// <param name="coA_mV">CO auxiliary electrode reading (mV)</param>
        /// <param name="coW_mV">CO working electrode reading (mV)</param>
        /// <param name="o3A_mV">O3 auxiliary electrode reading (mV)</param>
        /// <param name="o3W_mV">O3 working electrode reading (mV)</param>
        /// <param name="no2A_mV">NO2 auxiliary electrode reading (mV)</param>
        /// <param name="no2W_mV">NO2 working electrode reading (mV)</param>
        /// <param name="temperature_mV">PT temperature sensor reading (mV)</param>
        /// <param name="humidity_pc">Relative humidity percent</param>
        /// <param name="pressure_millibar">Pressure (mbar)</param>
        /// <param name="temp_C">Temperature (degrees Celsius)</param>
        /// <returns>A double representing the NO2 (ppb).</returns>
        public double NO2ppb(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV,
            double no2A_mV, double no2W_mV, double temperature_mV, double humidity_pc,
            double pressure_millibar, double temp_C)
        {
            var calculation = Convert(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV,
                temperature_mV, humidity_pc, pressure_millibar, temp_C);
            return calculation.NO2ppb;
        }

        /// <summary>
        /// Convert sensor measurements to O3 (ppb).
        /// </summary>
        /// <param name="coA_mV">CO auxiliary electrode reading (mV)</param>
        /// <param name="coW_mV">CO working electrode reading (mV)</param>
        /// <param name="o3A_mV">O3 auxiliary electrode reading (mV)</param>
        /// <param name="o3W_mV">O3 working electrode reading (mV)</param>
        /// <param name="no2A_mV">NO2 auxiliary electrode reading (mV)</param>
        /// <param name="no2W_mV">NO2 working electrode reading (mV)</param>
        /// <param name="temperature_mV">PT temperature sensor reading (mV)</param>
        /// <param name="humidity_pc">Relative humidity percent</param>
        /// <param name="pressure_millibar">Pressure (mbar)</param>
        /// <param name="temp_C">Temperature (degrees Celsius)</param>
        /// <returns>A double representing the O3 (ppb).</returns>
        public double O3ppb(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV,
            double no2A_mV, double no2W_mV, double temperature_mV, double humidity_pc,
            double pressure_millibar, double temp_C)
        {
            var calculation = Convert(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV,
                temperature_mV, humidity_pc, pressure_millibar, temp_C);
            return calculation.O3ppb;
        }

        /// <summary>
        /// Convert sensor measurements to the amount of each pollutant gas.
        /// 
        /// Note: conversions from ConversionFunctionsSimpleNeuralNet and
        /// ConversionFunctionsMultiSensor are stored in the same table in the database.
        /// </summary>
        /// <param name="coA_mV">CO auxiliary electrode reading (mV)</param>
        /// <param name="coW_mV">CO working electrode reading (mV)</param>
        /// <param name="o3A_mV">O3 auxiliary electrode reading (mV)</param>
        /// <param name="o3W_mV">O3 working electrode reading (mV)</param>
        /// <param name="no2A_mV">NO2 auxiliary electrode reading (mV)</param>
        /// <param name="no2W_mV">NO2 working electrode reading (mV)</param>
        /// <param name="temperature_mV">PT temperature sensor reading (mV)</param>
        /// <param name="humidity_pc">Relative humidity percent</param>
        /// <param name="pressure_millibar">Pressure (mbar)</param>
        /// <param name="temp_C">Temperature (degrees Celsius)</param>
        /// <returns>A GasReading with the pollutant gases in ppm or ppb.</returns>
        public GasReading Convert(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV,
            double no2A_mV, double no2W_mV, double temperature_mV, double humidity_pc,
            double pressure_millibar, double temp_C)
        {
            var results = new GasReading();
            FeaturesInput inputFeatures = FeaturesInput.ExtractFeaturesInput(coA_mV, coW_mV, o3A_mV, o3W_mV,
                    no2A_mV, no2W_mV, temperature_mV, humidity_pc, pressure_millibar, temp_C);

            if (_prevConversion != null && inputFeatures.Equals(_prevConversion.Input))
            {
                Log.Trace("SAME AS PREVIOUS INPUT");
                Log.Trace($"Features: [[{inputFeatures.NO2}, {inputFeatures.O3}, " +
                    $"{inputFeatures.CO}]], [[{inputFeatures.Temperature}, " +
                    $"{inputFeatures.AbsoluteHumidity}, {inputFeatures.Pressure}]]");
                results = _prevConversion.Output;
            }
            else
            {
                results = NeuralNetConvert(inputFeatures);
                _prevConversion = new ConversionInputOutput(inputFeatures, results);
            }

            StoreConversionOutput(results);
            return results;
        }

        /// <summary>
        /// Convert read from database to the amount of each pollutant gas.
        /// </summary>
        /// <param name="read">Reading that contains information from the sensor</param>
        /// <returns>A GasReading with the pollutant gases in ppm or ppb.</returns>
        public GasReading Convert(Read read)
        {
            var hupr = MetaSenseConverters.ConvertHuPr(read.GetHuPr());
            var gas = MetaSenseConverters.ConvertGas(read.GetGas());

            return Convert(gas.CoA * 1000, gas.CoW * 1000, gas.OxA * 1000,
                gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hupr.HumPercent,
                hupr.PresMilliBar, hupr.HumCelsius);
        }

        /// <summary>
        /// Convert message from sensor to the amount of each pollutant gas.
        /// </summary>
        /// <param name="message">Message that contains information from the sensor</param>
        /// <returns>A GasReading with the pollutant gases in ppm or ppb.</returns>
        public GasReading Convert(MetaSenseMessage message)
        {
            var hupr = MetaSenseConverters.ConvertHuPr(message.HuPr);
            var gas = MetaSenseConverters.ConvertGas(message.Raw);

            return Convert(gas.CoA * 1000, gas.CoW * 1000, gas.OxA * 1000,
                gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hupr.HumPercent,
                hupr.PresMilliBar, hupr.HumCelsius);
        }

        /// <summary>
        /// Store the output of the neural network conversion in the database.
        /// </summary>
        /// <param name="convResults">The pollutant gases in ppm or ppb after using the neural
        ///     network to convert from the inputs</param>
        /// <returns>The amount of rows inserted tp the table in the database.</returns>
        public int StoreConversionOutput(GasReading convResults)
        {
            return SettingsData.Default.AddNNConversionOutput(convResults.NO2ppb,
                convResults.O3ppb, convResults.COppm,
                (long)(MetaSenseNode.DateTimeToUnix(DateTime.Now)));
        }

        /// <summary>
        /// Find the conversion results for the given features if it already exists in the cache.
        /// 
        /// Note: currently not in use.
        /// </summary>
        /// <param name="features">The inputs to the neural network</param>
        /// <returns>A GasReading with the pollutant gases in ppm or ppb or null if the features
        ///     were not found in the cache.</returns>
        public GasReading SearchCacheForConversion(double[,] features)
        {
            var cacheSearch = SettingsData.Default.GetNNConversionResults(features[0, 0],
                features[0, 1], features[0, 2], features[0, 3], features[0, 4], features[0, 5]);

            if (cacheSearch != null)
            {
                GasReading storedConversions = new GasReading();
                storedConversions.NO2ppb = cacheSearch[NO2_IND];
                storedConversions.O3ppb = cacheSearch[O3_IND];
                storedConversions.COppm = cacheSearch[CO_IND];
                return storedConversions;
            }

            return null;
        }

        /// <summary>
        /// Store the features used and the results of the conversion in the cache.
        /// 
        /// Note: currently not in use.
        /// </summary>
        /// <param name="features">The inputs to the neural network</param>
        /// <param name="results">The pollutant gases in ppm or ppb after using the neural
        ///     network to convert from features</param>
        public void CacheConversion(double[,] features, GasReading results)
        {
            SettingsData.Default.AddNNConversion(features[0, 0], features[0, 1], features[0, 2],
                features[0, 3], features[0, 4], features[0, 5], results.NO2ppb, results.O3ppb,
                results.COppm);
        }

        /// <summary>
        /// Update the last time the conversion in the cache was accessed and the amount of times
        /// it was accessed.
        /// 
        /// Note: currently not in use.
        /// </summary>
        /// <param name="features">The input to the neural network</param>
        public void UpdateConversion(double[,] features)
        {
            SettingsData.Default.UpdateNNConversionTime(features[0, 0], features[0, 1],
                features[0, 2], features[0, 3], features[0, 4], features[0, 5]);
        }

        /// <summary>
        /// Convert the sensor measurements to the amount of each pollutant gas by simulating
        /// neural networks.
        /// </summary>
        /// <param name="featuresInput">The input to the neural network</param>
        /// <returns>A GasReading with the pollutant gases in ppm or ppb.</returns>
        public abstract GasReading NeuralNetConvert(FeaturesInput featuresInput);
    }
}
