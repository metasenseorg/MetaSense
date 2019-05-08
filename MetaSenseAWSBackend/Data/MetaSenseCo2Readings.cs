using Newtonsoft.Json;

namespace BackendAPI.Data
{
    public class MetaSenseCo2Readings
    {
        [JsonProperty("CO2")]
        public double Co2;
    }
}