using Newtonsoft.Json;

namespace BackendAPI.Data
{
    public class MetaSenseVocReadings
    {
        [JsonProperty("vPP")]
        public double VPp;
        [JsonProperty("vIP")]
        public double VIp;
    }
}