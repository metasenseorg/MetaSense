using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NodeLibrary
{
    public sealed class AlphasenseCalibration
    {
        public sealed class SensorCalibration
        {
            [JsonProperty("WE_E0_mV")]
            public double? WE_E0_mV { get; private set; }
            [JsonProperty("WE_S0_mV")]
            public double? WE_S0_mV { get; private set; }
            [JsonProperty("WE_0_mV")]
            public double? WE_0_mV { get; private set; }
            [JsonProperty("AE_E0_mV")]
            public double? AE_E0_mV { get; private set; }
            [JsonProperty("AE_S0_mV")]
            public double? AE_S0_mV { get; private set; }
            [JsonProperty("AE_0_mV")]
            public double? AE_0_mV { get; private set; }
            [JsonProperty("Sensitivity_nA_ppb")]
            public double? Sensitivity_nA_ppb { get; private set; }
            [JsonProperty("Sensitivity_NO2_nA_ppb")]
            public double? Sensitivity_NO2_nA_ppb { get; private set; }
            [JsonProperty("PCB_gain_mV_nA")]
            public double? PCB_gain_mV_nA { get; private set; }
            [JsonProperty("Sensitivity_mV_ppb")]
            public double? Sensitivity_mV_ppb { get; private set; }
            [JsonProperty("Sensitivity_NO2_mV_ppb")]
            public double? Sensitivity_NO2_mV_ppb { get; private set; }
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

        private Dictionary<int,double> CO_n = new Dictionary<int, double>{{-30, 1.0}, {-20, 1.0}, { -10, 1.0 }, { 0, 1.0 }, { 10, 1.0 }, { 20, -1.0 }, { 30, -0.76 }, { 40, -0.76 }, { 50, -0.76 } };
        private Dictionary<int, double> NO2_n = new Dictionary<int, double> { { -30, 1.09 }, { -20, 1.09 }, { -10, 1.09 }, { 0, 1.09 }, { 10, 1.09 }, { 20, 1.35 }, { 30, 3.0 }, { 40, 3.0 }, { 50, 3.0 } };
        private Dictionary<int, double> O3_n = new Dictionary<int, double> { { -30, 0.75 }, { -20, 0.75 }, { -10, 0.75 }, { 0, 0.75 }, { 10, 1.28 }, { 20, 1.28 }, { 30, 1.28 }, { 40, 1.28 }, { 50, 0 } };

        private static double n(double T_C, Dictionary<int, double> dict)
        {
            var T_Round = (int)(Math.Round(T_C / 10.0) * 10);
            if (T_Round > 50)
                T_Round = 50;
            if (T_Round < -30)
                T_Round = -30;
            return dict[T_Round];
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
            if (!calibration.AE_0_mV.HasValue) return false;
            if (!calibration.AE_E0_mV.HasValue) return false;
            if (!calibration.AE_S0_mV.HasValue) return false;
            if (!calibration.WE_0_mV.HasValue) return false;
            if (!calibration.WE_E0_mV.HasValue) return false;
            if (!calibration.WE_S0_mV.HasValue) return false;
            if (!calibration.PCB_gain_mV_nA.HasValue) return false;
            if (!calibration.Sensitivity_nA_ppb.HasValue) return false;
            if (!calibration.Sensitivity_NO2_nA_ppb.HasValue) return false;
            if (!calibration.Sensitivity_mV_ppb.HasValue) return false;
            if (!calibration.Sensitivity_NO2_mV_ppb.HasValue) return false;
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
            catch (Exception e)
            {
                //Ignore decoding errors (they are due to empty strings and debug strings
                throw e;
            }
        }
        public ConversionFunctionsAlphasense(AlphasenseCalibration calibration)
        {
            _calibration = calibration;
        }

        private static double BasicConcentrationFormula(double A_mV, double W_mV, double temp_C, AlphasenseCalibration.SensorCalibration calibration, Dictionary<int, double> dictionary)
        {
            var A_mV_zeroed = A_mV - calibration.AE_0_mV;
            var W_mV_zeroed = W_mV - calibration.WE_0_mV;
            var n = ConversionFunctionsAlphasense.n(temp_C, dictionary);

            var scaled_A_mV = A_mV_zeroed * n;
            var corrected_WE = W_mV_zeroed - scaled_A_mV;

            var concentration = corrected_WE / calibration.Sensitivity_mV_ppb;
            return concentration.Value;
        }

        public double COppm(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C)
        {
            return BasicConcentrationFormula(coA_mV, coW_mV, temp_C, _calibration.CO, CO_n)/1000;

            //var coA_mV_zeroed = coA_mV - _calibration.CO.AE_0_mV;
            //var coW_mV_zeroed = coW_mV - _calibration.CO.WE_0_mV;
            //var n = this.n(temp_C, CO_n);

            //var scaled_coA_mV = coA_mV_zeroed * n;

            //var corrected_WE = coW_mV_zeroed - scaled_coA_mV;

            //var concentration = corrected_WE / _calibration.CO.Sensitivity_mV_ppb;
            //return concentration;
        }

        public double NO2ppb(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C)
        {
            return BasicConcentrationFormula(no2A_mV, no2W_mV, temp_C, _calibration.NO2, NO2_n);
        }

        public double O3ppb(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C)
        {
            var no2 = NO2ppb(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV, temperature_mV, humidity_pc, pressure_millibar, temp_C);

            var A_mV_zeroed = o3A_mV - _calibration.O3.AE_0_mV;
            var W_mV_zeroed = o3W_mV - _calibration.O3.WE_0_mV;
            var n = ConversionFunctionsAlphasense.n(temp_C, O3_n);
            var scaled_A_mV = A_mV_zeroed * n;
            var corrected_WE = W_mV_zeroed - scaled_A_mV;

            var corrected_WE_NO2 = no2 * _calibration.O3.Sensitivity_NO2_mV_ppb;
            var corrected_WE_O3 = corrected_WE - corrected_WE_NO2;

            var concentration_O3 = corrected_WE_O3 / _calibration.O3.Sensitivity_mV_ppb;

            return concentration_O3.Value;
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
