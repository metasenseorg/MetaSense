using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace BackendAPI.Data
{
    public class JsonCalibrationParamsSharad
    {
        public class Element
        {
            [JsonProperty("weights")]
            public double[] Weights;
            [JsonProperty("intercept")]
            public double? Intercept;
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
            if (!calibration.Intercept.HasValue) return false;
            if (calibration.Weights==null) return false;
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
            catch (Exception)
            {
                //Ignore decoding errors (they are due to empty strings and debug strings
            }
        }
        private readonly JsonCalibrationParamsSharad _calibrationParamsSharad;

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static double CO(double coA, double coW, double o3A, double o3W, double no2A, double no2W, double temperature, double humidity,
            double[] coWeights, double coIntercept)
        {
            double cOppm = 0;
            double[] coVector/*[15]*/ = { 1, coA, coW, temperature, humidity, coA * coA, coA * coW, coA * temperature, coA * humidity,
                coW * coW, coW * temperature, coW * humidity, temperature * temperature, temperature * humidity, humidity * humidity };
            for (var i = 0; i < 15; i++)
            {
                cOppm += coVector[i] * coWeights[i];
            }
            cOppm += coIntercept;
            return cOppm > 0 ? cOppm : 0;
        }
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static double O3(double coA, double coW, double o3A, double o3W, double no2A, double no2W, double temperature, double humidity,
            double[] o3Weights, double o3Intercept)
        {
            double o3Ppb = 0;
            double[] o3Vector/*[7]*/ = { 1, o3A, o3W, coA, coW, temperature, humidity };
            for (var i = 0; i < 7; i++)
            {
                o3Ppb += o3Vector[i] * o3Weights[i];
            }
            o3Ppb += o3Intercept;

            return o3Ppb > 0 ? o3Ppb : 0;
        }
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static double NO2(double coA, double coW, double o3A, double o3W, double no2A, double no2W, double temperature, double humidity,
            double[] no2Weights, double no2Intercept)
        {
            double[] no2Vector/*[45]*/ = { 1, o3A, o3W, no2A, no2W, coA, coW, temperature, humidity, o3A * o3A, o3A * o3W, o3A * no2A, o3A * no2W,
                o3A * coA, o3A * coW, o3A * temperature, o3A * humidity, o3W * o3W, o3W * no2A, o3W * no2W, o3W * coA, o3W * coW, o3W * temperature,
                o3W * humidity, no2A * no2A, no2A * no2W, no2A * coA, no2A * coW, no2A * temperature, no2A * humidity, no2W * no2W, no2W * coA, no2W * coW,
                no2W * temperature, no2W * humidity, coA * coA, coA * coW, coA * temperature, coA * humidity, coW * coW, coW * temperature, coW * humidity,
                temperature * temperature, temperature * humidity, humidity * humidity };

            double no2Ppb = 0;
            for (var i = 0; i < 45; i++)
            {
                no2Ppb += no2Vector[i] * no2Weights[i];
            }
            no2Ppb += no2Intercept;

            return no2Ppb > 0 ? no2Ppb : 0;
        }

        public double COppm(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC)
        {
            return CO(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, tempC, humidityPc * 100,
                _calibrationParamsSharad.CO.Weights, _calibrationParamsSharad.CO.Intercept.Value);
        }
        public double NO2Ppb(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC)
        {
            return NO2(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, tempC, humidityPc * 100,
                _calibrationParamsSharad.NO2.Weights, _calibrationParamsSharad.NO2.Intercept.Value);
        }
        public double O3Ppb(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC)
        {
            return O3(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, tempC, humidityPc * 100,
                _calibrationParamsSharad.O3.Weights, _calibrationParamsSharad.O3.Intercept.Value);
        }

        public GasReading Convert(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC)
        {
            var reading = new GasReading()
            {
                COppm = COppm(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, temperatureMv, humidityPc, pressureMillibar, tempC),
                NO2Ppb = NO2Ppb(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, temperatureMv, humidityPc, pressureMillibar, tempC),
                O3Ppb = O3Ppb(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, temperatureMv, humidityPc, pressureMillibar, tempC)
            };
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