namespace BackendAPI.Data
{
    public class MetaSenseVoltGasReadings
    {
        public double No2A;
        public double No2W;
        public double OxA;
        public double OxW;
        public double CoA;
        public double CoW;
        public double Temp;
        public double Nc;
    }
    public class MetaSenseHuPrReadings
    {
        public double HumCelsius;
        public double HumPercent;
        public double PresCelsius;
        public double PresMilliBar;
    }


    public class MetaSenseConverters
    {
        private readonly MetaSenseMessage _msg;
        public MetaSenseConverters(MetaSenseMessage msg) { _msg = msg; }
        public MetaSenseHuPrReadings HuPr()
        {
            return ConvertHuPr(_msg.HuPr);
        }
        public static MetaSenseHuPrReadings ConvertHuPr(MetaSenseRawHuPrReadings huPr)
        {
            var ret = new MetaSenseHuPrReadings
            {
                HumCelsius = huPr.HumiditySensorTemperatureCelsius,
                HumPercent = huPr.HumiditySensorHumidityPercent / 100.0,
                PresMilliBar = huPr.BarometricSensorPressureMilliBar,
                PresCelsius = huPr.BarometricSensorTemperatureCelsius
            };
            return ret;
        }
        private static double ConvertRawGasToVoltage(int rng, int rawValue)
        {
            double gain = rng;
            if (rng == 0)
                gain = 2.0 / 3.0;
            var voltCalc = 4.096 / (gain * 0x7FFF);
            return (rawValue * voltCalc);
        }
        public MetaSenseVoltGasReadings Gas()
        {
            return ConvertGas(_msg.Raw);
        }
        public static MetaSenseVoltGasReadings ConvertGas(MetaSenseRawGasReadings raw)
        {
            var ret = new MetaSenseVoltGasReadings
            {
                CoA = ConvertRawGasToVoltage(raw.Rng, raw.S3A),
                CoW = ConvertRawGasToVoltage(raw.Rng, raw.S3W),
                OxA = ConvertRawGasToVoltage(raw.Rng, raw.S2A),
                OxW = ConvertRawGasToVoltage(raw.Rng, raw.S2W),
                No2A = ConvertRawGasToVoltage(raw.Rng, raw.S1A),
                No2W = ConvertRawGasToVoltage(raw.Rng, raw.S1W),
                Nc = ConvertRawGasToVoltage(raw.Rng, raw.Voc),
                Temp = ConvertRawGasToVoltage(raw.Rng, raw.Temperature)
            };
            return ret;
        }

    }
}
