using Newtonsoft.Json;

namespace BackendAPI.Data
{
    public class MetaSenseRawHuPrReadings
    {
        [JsonProperty("hT")]
        public double HumiditySensorTemperatureCelsius;
        [JsonProperty("hH")]
        public double HumiditySensorHumidityPercent;
        [JsonProperty("bP")]
        public double BarometricSensorPressureMilliBar;
        [JsonProperty("bT")]
        public double BarometricSensorTemperatureCelsius;
    }
}