using System;
using Newtonsoft.Json;

namespace BackendAPI.Data
{
    public class MetaSenseParticleEvent
    {
        [JsonProperty("event")]
        public string Event;
        [JsonProperty("data")]
        public string Data;
        [JsonProperty("published_at")]
        public DateTime PublishedAt;
        [JsonProperty("coreid")]
        public string CoreId;
        [JsonProperty("userid")]
        public string UserId;
        [JsonProperty("fw_version")]
        public int FwVersion;
        [JsonProperty("public")]
        public bool Public;
        [JsonProperty("format")]
        public string Format;
    }
}
