namespace BackendAPI.Data
{
    public class Read
    {
        public string SensorMAC { get; set; }
        public long Ts { get; set; }
        public int Rng { get; set; }
        public int S1A { get; set; }
        public int S1W { get; set; }
        public int S2A { get; set; }
        public int S2W { get; set; }
        public int S3A { get; set; }
        public int S3W { get; set; }
        public int Pt { get; set; }
        public int Nc { get; set; }

        // ReSharper disable InconsistentNaming
        public double HT { get; set; }
        public double HH { get; set; }
        public double BP { get; set; }
        public double BT { get; set; }
        // ReSharper restore InconsistentNaming

        public double? Co2 { get; set; }
        public double? VPp { get; set; }
        public double? VIp { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public float? Accuracy { get; set; }
        public float? Bearing { get; set; }
        public float? Speed { get; set; }

        public Read() { }
        public Read(string sensorMAC, long ts, MetaSenseRawGasReadings gas, MetaSenseRawHuPrReadings humPr, MetaSenseCo2Readings co2, MetaSenseVocReadings voc, LocationInfo location)
        {
            Ts = ts;
            SensorMAC = sensorMAC;
            Nc = gas.Voc;
            Pt = gas.Temperature;
            Rng = gas.Rng;
            S1A = gas.S1A;
            S1W = gas.S1W;
            S2A = gas.S2A;
            S2W = gas.S2W;
            S3A = gas.S3A;
            S3W = gas.S3W;

            BP = humPr.BarometricSensorPressureMilliBar;
            BT = humPr.BarometricSensorTemperatureCelsius;
            HH = humPr.HumiditySensorHumidityPercent;
            HT = humPr.HumiditySensorTemperatureCelsius;

            if (co2 != null)
                Co2 = co2.Co2;

            if (voc != null)
            {
                VPp = voc.VPp;
                VIp = voc.VIp;
            }

            if (location != null)
            {
                Latitude = location.Latitude;
                Longitude = location.Longitude;
                Altitude = location.Altitude;
                if (location.Radius != null) Accuracy = (float)location.Radius;
                if (location.Direction != null) Bearing = (float)location.Direction;
                if (location.Speed != null) Speed = (float)location.Speed;
            }
        }
        public MetaSenseRawGasReadings GetGas()
        {
            var val = new MetaSenseRawGasReadings
            {
                Voc = Nc,
                Temperature = Pt,
                Rng = Rng,
                S1A = S1A,
                S1W = S1W,
                S2A = S2A,
                S2W = S2W,
                S3A = S3A,
                S3W = S3W
            };
            return val;
        }
        public MetaSenseRawHuPrReadings GetHuPr()
        {
            var val = new MetaSenseRawHuPrReadings
            {
                BarometricSensorPressureMilliBar = BP,
                BarometricSensorTemperatureCelsius = BT,
                HumiditySensorHumidityPercent = HH,
                HumiditySensorTemperatureCelsius = HT
            };
            return val;
        }
        public MetaSenseCo2Readings GetCo2()
        {
            if (!Co2.HasValue) return null;
            var val = new MetaSenseCo2Readings { Co2 = Co2.Value };
            return val;
        }
        public MetaSenseVocReadings GetVoc()
        {
            if (!VIp.HasValue || !VPp.HasValue) return null;
            var val = new MetaSenseVocReadings
            {
                VIp = VIp.Value,
                VPp = VPp.Value
            };
            return val;
        }
        public LocationInfo GetLocation()
        {
            var loc = new LocationInfo(
            Latitude ?? 0,
            Longitude ?? 0,
            Accuracy ?? 0,
            Altitude ?? 0,
            Speed ?? 0,
            Bearing ?? 0,
            TimeManagementUtils.UnixToDateTime(Ts));
            return loc;
        }
    }
}
