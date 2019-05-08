using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BackendAPI.Data
{
    public sealed class AlphasenseCalibration
    {
        public sealed class SensorCalibration
        {
            [JsonProperty("WE_E0_mV")]
            public double? WeE0Mv { get; private set; }
            [JsonProperty("WE_S0_mV")]
            public double? WeS0Mv { get; private set; }
            [JsonProperty("WE_0_mV")]
            public double? We0Mv { get; private set; }
            [JsonProperty("AE_E0_mV")]
            public double? AeE0Mv { get; private set; }
            [JsonProperty("AE_S0_mV")]
            public double? AeS0Mv { get; private set; }
            [JsonProperty("AE_0_mV")]
            public double? Ae0Mv { get; private set; }
            [JsonProperty("Sensitivity_nA_ppb")]
            public double? SensitivityNaPpb { get; private set; }
            [JsonProperty("Sensitivity_NO2_nA_ppb")]
            public double? SensitivityNO2NaPpb { get; private set; }
            [JsonProperty("PCB_gain_mV_nA")]
            public double? PcbGainMvNa { get; private set; }
            [JsonProperty("Sensitivity_mV_ppb")]
            public double? SensitivityMvPpb { get; private set; }
            [JsonProperty("Sensitivity_NO2_mV_ppb")]
            public double? SensitivityNO2MvPpb { get; private set; }
        }
        [JsonProperty("NO2")]
        public SensorCalibration NO2 { get; private set; }
        [JsonProperty("CO")]
        public SensorCalibration CO { get; private set; }
        [JsonProperty("O3")]
        public SensorCalibration O3 { get; private set; }
    }
    public class ConversionFunctionsAlphasense : IConversionFunctions
    {
        private AlphasenseCalibration _calibration;

        private Dictionary<int,double> _coN = new Dictionary<int, double>{{-30, 1.0}, {-20, 1.0}, { -10, 1.0 }, { 0, 1.0 }, { 10, 1.0 }, { 20, -1.0 }, { 30, -0.76 }, { 40, -0.76 }, { 50, -0.76 } };
        private Dictionary<int, double> _no2N = new Dictionary<int, double> { { -30, 1.09 }, { -20, 1.09 }, { -10, 1.09 }, { 0, 1.09 }, { 10, 1.09 }, { 20, 1.35 }, { 30, 3.0 }, { 40, 3.0 }, { 50, 3.0 } };
        private Dictionary<int, double> _o3N = new Dictionary<int, double> { { -30, 0.75 }, { -20, 0.75 }, { -10, 0.75 }, { 0, 0.75 }, { 10, 1.28 }, { 20, 1.28 }, { 30, 1.28 }, { 40, 1.28 }, { 50, 0 } };

        private static double N(double c, Dictionary<int, double> dict)
        {
            var round = (int)(Math.Round(c / 10.0) * 10);
            if (round > 50)
                round = 50;
            if (round < -30)
                round = -30;
            return dict[round];
        }

        public bool ValidCalibrationFile
        {
            get
            {
                if (_calibration == null) return false;
                if (_calibration.CO == null || _calibration.NO2 == null || _calibration.O3 == null) return false;
                if (!VerifySensorCalibration(_calibration.CO)) return false;
                if (!VerifySensorCalibration(_calibration.NO2)) return false;
                if (!VerifySensorCalibration(_calibration.O3)) return false;
                return true;
            }
        }
        private bool VerifySensorCalibration(AlphasenseCalibration.SensorCalibration calibration)
        {
            if (!calibration.Ae0Mv.HasValue) return false;
            if (!calibration.AeE0Mv.HasValue) return false;
            if (!calibration.AeS0Mv.HasValue) return false;
            if (!calibration.We0Mv.HasValue) return false;
            if (!calibration.WeE0Mv.HasValue) return false;
            if (!calibration.WeS0Mv.HasValue) return false;
            if (!calibration.PcbGainMvNa.HasValue) return false;
            if (!calibration.SensitivityNaPpb.HasValue) return false;
            if (!calibration.SensitivityNO2NaPpb.HasValue) return false;
            if (!calibration.SensitivityMvPpb.HasValue) return false;
            if (!calibration.SensitivityNO2MvPpb.HasValue) return false;
            return true;
        }

        public ConversionFunctionsAlphasense(string json)
        {
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include
            };
            JsonConvert.DefaultSettings = () => settings;
            try
            {
                _calibration = JsonConvert.DeserializeObject<AlphasenseCalibration>(json);
            }
            catch (Exception)
            {
                //Ignore decoding errors (they are due to empty strings and debug strings
            }
        }
        public ConversionFunctionsAlphasense(AlphasenseCalibration calibration)
        {
            _calibration = calibration;
        }

        private static double BasicConcentrationFormula(double aMv, double wMv, double tempC, AlphasenseCalibration.SensorCalibration calibration, Dictionary<int, double> dictionary)
        {
            var aMvZeroed = aMv - calibration.Ae0Mv;
            var wMvZeroed = wMv - calibration.We0Mv;
            var n = N(tempC, dictionary);

            var scaledAMv = aMvZeroed * n;
            var correctedWe = wMvZeroed - scaledAMv;

            var concentration = correctedWe / calibration.SensitivityMvPpb;
            return concentration ?? 0;
        }

        public double COppm(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC)
        {
            return BasicConcentrationFormula(coAMv, coWMv, tempC, _calibration.CO, _coN)/1000;

            //var coA_mV_zeroed = coA_mV - _calibration.CO.AE_0_mV;
            //var coW_mV_zeroed = coW_mV - _calibration.CO.WE_0_mV;
            //var n = this.n(temp_C, CO_n);

            //var scaled_coA_mV = coA_mV_zeroed * n;

            //var corrected_WE = coW_mV_zeroed - scaled_coA_mV;

            //var concentration = corrected_WE / _calibration.CO.Sensitivity_mV_ppb;
            //return concentration;
        }

        public double NO2Ppb(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC)
        {
            return BasicConcentrationFormula(no2AMv, no2WMv, tempC, _calibration.NO2, _no2N);
        }

        public double O3Ppb(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC)
        {
            var no2 = NO2Ppb(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, temperatureMv, humidityPc, pressureMillibar, tempC);

            var aMvZeroed = o3AMv - _calibration.O3.Ae0Mv;
            var wMvZeroed = o3WMv - _calibration.O3.We0Mv;
            var n = N(tempC, _o3N);
            var scaledAMv = aMvZeroed * n;
            var correctedWe = wMvZeroed - scaledAMv;

            var correctedWeNO2 = no2 * _calibration.O3.SensitivityNO2MvPpb;
            var correctedWeO3 = correctedWe - correctedWeNO2;

            var concentrationO3 = correctedWeO3 / _calibration.O3.SensitivityMvPpb;

            return concentrationO3 ?? 0;
        }

        public GasReading Convert(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC)
        {
            var reading = new GasReading();
            reading.COppm = COppm(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, temperatureMv, humidityPc, pressureMillibar, tempC);
            reading.NO2Ppb = NO2Ppb(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, temperatureMv, humidityPc, pressureMillibar, tempC);
            reading.O3Ppb = O3Ppb(coAMv, coWMv, o3AMv, o3WMv, no2AMv, no2WMv, temperatureMv, humidityPc, pressureMillibar, tempC);

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
