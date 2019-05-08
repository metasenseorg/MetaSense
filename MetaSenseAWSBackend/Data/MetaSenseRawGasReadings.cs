using Newtonsoft.Json;

namespace BackendAPI.Data
{
    public class MetaSenseRawGasReadings
    {
        [JsonProperty("Rng")]
        public int Rng;
        [JsonProperty("S1A")]
        public int S1A;
        [JsonProperty("S1W")]
        public int S1W;
        [JsonProperty("S2A")]
        public int S2A;
        [JsonProperty("S2W")]
        public int S2W;
        [JsonProperty("S3A")]
        public int S3A;
        [JsonProperty("S3W")]
        public int S3W;
        [JsonProperty("PT")]
        public int Temperature;
        [JsonProperty("NC")]
        public int Voc;
    }
}