using System;
using Newtonsoft.Json;

namespace NodeLibrary
{
    public class JsonCalibrationParamsSharad
    {
        public class Element
        {
            [JsonProperty("weights")]
            public double[] weights;
            [JsonProperty("intercept")]
            public double? intercept;
        }
        [JsonProperty("O3")]
        public Element O3;
        [JsonProperty("CO")]
        public Element CO;
        [JsonProperty("NO2")]
        public Element NO2;
    }

    public class ConversionFunctionsSharad : IConversionFunctions
    {
        public bool ValidCalibrationFile
        {
            get
            {
                if (_calibrationParamsSharad == null) return false;
                if (_calibrationParamsSharad.CO == null || _calibrationParamsSharad.NO2 == null || _calibrationParamsSharad.O3 == null) return false;
                if (!VerifySensorCalibration(_calibrationParamsSharad.CO)) return false;
                if (!VerifySensorCalibration(_calibrationParamsSharad.NO2)) return false;
                if (!VerifySensorCalibration(_calibrationParamsSharad.O3)) return false;
                return true;
            }
        }
        private bool VerifySensorCalibration(JsonCalibrationParamsSharad.Element calibration)
        {
            if (!calibration.intercept.HasValue) return false;
            if (calibration.weights==null) return false;
            return true;
        }

        public ConversionFunctionsSharad(string json)
        {
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include
            };
            JsonConvert.DefaultSettings = () => settings;
            try
            {
                _calibrationParamsSharad = JsonConvert.DeserializeObject<JsonCalibrationParamsSharad>(json);
            }
            catch (Exception e)
            {
                //Ignore decoding errors (they are due to empty strings and debug strings
                throw e;
            }
        }
        private readonly JsonCalibrationParamsSharad _calibrationParamsSharad;

        private static double CO(double coA, double coW, double o3A, double o3W, double no2A, double no2W, double temperature, double humidity,
            double[] coWeights, double coIntercept)
        {
            double COppm = 0;
            double[] coVector/*[15]*/ = { 1, coA, coW, temperature, humidity, coA * coA, coA * coW, coA * temperature, coA * humidity,
                coW * coW, coW * temperature, coW * humidity, temperature * temperature, temperature * humidity, humidity * humidity };
            for (var i = 0; i < 15; i++)
            {
                COppm += coVector[i] * coWeights[i];
            }
            COppm += coIntercept;
            return COppm > 0 ? COppm : 0;
        }
        private static double O3(double coA, double coW, double o3A, double o3W, double no2A, double no2W, double temperature, double humidity,
            double[] o3Weights, double o3Intercept)
        {
            double O3ppb = 0;
            double[] o3Vector/*[7]*/ = { 1, o3A, o3W, coA, coW, temperature, humidity };
            for (var i = 0; i < 7; i++)
            {
                O3ppb += o3Vector[i] * o3Weights[i];
            }
            O3ppb += o3Intercept;

            return O3ppb > 0 ? O3ppb : 0;
        }
        private static double NO2(double coA, double coW, double o3A, double o3W, double no2A, double no2W, double temperature, double humidity,
            double[] no2Weights, double no2Intercept)
        {
            double[] no2Vector/*[45]*/ = { 1, o3A, o3W, no2A, no2W, coA, coW, temperature, humidity, o3A * o3A, o3A * o3W, o3A * no2A, o3A * no2W,
                o3A * coA, o3A * coW, o3A * temperature, o3A * humidity, o3W * o3W, o3W * no2A, o3W * no2W, o3W * coA, o3W * coW, o3W * temperature,
                o3W * humidity, no2A * no2A, no2A * no2W, no2A * coA, no2A * coW, no2A * temperature, no2A * humidity, no2W * no2W, no2W * coA, no2W * coW,
                no2W * temperature, no2W * humidity, coA * coA, coA * coW, coA * temperature, coA * humidity, coW * coW, coW * temperature, coW * humidity,
                temperature * temperature, temperature * humidity, humidity * humidity };

            double NO2ppb = 0;
            for (var i = 0; i < 45; i++)
            {
                NO2ppb += no2Vector[i] * no2Weights[i];
            }
            NO2ppb += no2Intercept;

            return NO2ppb > 0 ? NO2ppb : 0;
        }

        public double COppm(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C)
        {
            return CO(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV, temp_C, humidity_pc * 100,
                _calibrationParamsSharad.CO.weights, _calibrationParamsSharad.CO.intercept.Value);
        }
        public double NO2ppb(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C)
        {
            return NO2(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV, temp_C, humidity_pc * 100,
                _calibrationParamsSharad.NO2.weights, _calibrationParamsSharad.NO2.intercept.Value);
        }
        public double O3ppb(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C)
        {
            return O3(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV, temp_C, humidity_pc * 100,
                _calibrationParamsSharad.O3.weights, _calibrationParamsSharad.O3.intercept.Value);
        }

        public GasReading Convert(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C)
        {
            var reading = new GasReading();
            reading.COppm = COppm(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV, temperature_mV, humidity_pc, pressure_millibar, temp_C);
            reading.NO2ppb = NO2ppb(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV, temperature_mV, humidity_pc, pressure_millibar, temp_C);
            reading.O3ppb = O3ppb(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV, temperature_mV, humidity_pc, pressure_millibar, temp_C);
            return reading;
        }

        public GasReading Convert(Read read)
        {
            var hupr = MetaSenseConverters.ConvertHuPr(read.GetHuPr());
            var gas = MetaSenseConverters.ConvertGas(read.GetGas());

            return Convert(gas.CoA * 1000, gas.CoW * 1000, gas.OxA * 1000,
                gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hupr.HumPercent, hupr.PresMilliBar, hupr.HumCelsius);

        }

        public GasReading Convert(MetaSenseMessage message)
        {
            var hupr = MetaSenseConverters.ConvertHuPr(message.HuPr);
            var gas = MetaSenseConverters.ConvertGas(message.Raw);

            return Convert(gas.CoA * 1000, gas.CoW * 1000, gas.OxA * 1000,
                gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hupr.HumPercent, hupr.PresMilliBar, hupr.HumCelsius);
        }
    }
}