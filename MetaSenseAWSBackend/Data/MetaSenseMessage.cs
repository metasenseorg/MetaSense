using System;
using Newtonsoft.Json;

namespace BackendAPI.Data
{
    public enum Flags
    {
        //Boolean Flags
        [JsonProperty("s_sd")]
        StreamSD,
        [JsonProperty("s_wifi")]
        StreamWifi,
        [JsonProperty("s_ble")]
        StreamBLE,
        [JsonProperty("wifi_en")]
        WifiEn,
        [JsonProperty("sleep_en")]
        SleepEn,
        [JsonProperty("usb_en")]
        UsbEn,
        [JsonProperty("usb_pass")]
        UsbPass,
        [JsonProperty("co2_en")]
        Co2En,
        [JsonProperty("voc_en")]
        VocEn,
        //Long Flags
        [JsonProperty("power")]
        Power,
        [JsonProperty("s_inter")]
        SInter,
        //Int (and enum) flags
        [JsonProperty("f_sd")]
        FlagSD,
        [JsonProperty("f_wifi")]
        FlagWifi,
        [JsonProperty("f_ble")]
        FlagBLE,
        //String Flags
        [JsonProperty("ssid")]
        Ssid,
        [JsonProperty("pass")]
        Pass,
        [JsonProperty("node_id")]
        NodeId,
        [JsonProperty("afe_ser")]
        AfeSer,
        [JsonProperty("mac_addr")]
        MacAddr,
        //Execute Command
        [JsonProperty("reset")]
        Reset,
        [JsonProperty("cl_wifi")]
        ClWifi,
        [JsonProperty("st_rom")]
        StRom
    }

    public class MetaSenseMessage
    {
        [JsonProperty("raw")]
        public MetaSenseRawGasReadings Raw;
        [JsonProperty("hu_pr")]
        public MetaSenseRawHuPrReadings HuPr;
        //[JsonProperty("conf")]
        //public MetaSenseAfeConfiguration Conf;
        [JsonProperty("co2")]
        public MetaSenseCo2Readings Co2;
        [JsonProperty("voc")]
        public MetaSenseVocReadings Voc;
        [JsonProperty("loc")]
        public LocationInfo Loc;
        [JsonProperty("req")]
        public string Req;
        [JsonProperty("ts")]
        public long? Ts;
        [JsonIgnore]
        public DateTime TimeStamp => Ts.HasValue ? TimeManagementUtils.UnixToDateTime(Ts.Value) : default(DateTime);


        //Boolean Flags
        [JsonProperty("s_sd")] public bool? SSd;
        [JsonProperty("s_wifi")] public bool? SWifi;
        [JsonProperty("s_ble")] public bool? StreamBLE;
        [JsonProperty("wifi_en")] public bool? WifiEn;
        [JsonProperty("sleep_en")] public bool? SleepEn;
        [JsonProperty("usb_en")] public bool? UsbEn;
        [JsonProperty("usb_pass")] public bool? UsbPass;
        [JsonProperty("co2_en")] public bool? Co2En;
        [JsonProperty("voc_en")] public bool? VocEn;
        //Long Flags
        [JsonProperty("power")] public long? Power;
        [JsonProperty("s_inter")] public long? SInter;
        //Int (and enum) flags
        [JsonProperty("f_sd")] public int? FlagSD;
        [JsonProperty("f_wifi")] public int? FlagWifi;
        [JsonProperty("f_ble")] public int? FlagBLE;
        //String Flags
        [JsonProperty("ssid")] public string Ssid;
        [JsonProperty("pass")] public string Pass;
        [JsonProperty("node_id")] public string NodeId;
        [JsonProperty("afe_ser")] public string AfeSer;
        [JsonProperty("mac_addr")] public string MacAddr;

        public MetaSenseMessage() { }
        public MetaSenseMessage(Flags reqFlag) {
            Req = reqFlag.ToString();
        }

        public string ToJsonString()
        {
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            JsonConvert.DefaultSettings = () => settings;
            return JsonConvert.SerializeObject(this);
        }
        public static MetaSenseMessage FromJsonString(string str)
        {
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            JsonConvert.DefaultSettings = () => settings;
            try
            {
                return JsonConvert.DeserializeObject<MetaSenseMessage>(str);
            }
            catch (Exception)
            {
                //Ignore decoding errors (they are due to empty strings and debug strings
                return null;
            }
        }
        private static byte ConvertRange(UInt16 range)
        {
            if (range == 0x0000)
                return 0;
            if (range == 0x0200)
                return 1;
            if (range == 0x0400)
                return 2;
            if (range == 0x0600)
                return 4;
            if (range == 0x0800)
                return 8;
            if (range == 0x0A00)
                return 16;
            return 0;
        }
        public static MetaSenseMessage FromBinaryMessage(byte[] buf, int pos, out int len)
        {
            var messageTime = BinaryUtils.GetUInt32(buf, pos);
            var logInfo = $"Time read in stream {TimeManagementUtils.UnixToDateTime(messageTime)}";
            var msgType = BinaryUtils.GetSubByte(buf, pos + 4, 0, 2);
            var hasTime = BinaryUtils.GetSubByte(buf, pos + 4, 2, 1);
            var hasRawGas = BinaryUtils.GetSubByte(buf, pos + 4, 3, 1);
            var hasHumBar = BinaryUtils.GetSubByte(buf, pos + 4, 4, 1);
            var hasVoc = BinaryUtils.GetSubByte(buf, pos + 4, 5, 1);
            var hasCO2 = BinaryUtils.GetSubByte(buf, pos + 4, 6, 1);

            var barT = BinaryUtils.GetUInt16(buf, pos + 6);
            var barP = BinaryUtils.GetUInt16(buf, pos + 8);
            var humT = BinaryUtils.GetUInt16(buf, pos + 10);
            var humH = BinaryUtils.GetUInt16(buf, pos + 12);
            var range = ConvertRange(BinaryUtils.GetUInt16(buf, pos + 14));
            var adc00 = BinaryUtils.GetUInt16(buf, pos + 16);
            var adc01 = BinaryUtils.GetUInt16(buf, pos + 18);
            var adc02 = BinaryUtils.GetUInt16(buf, pos + 20);
            var adc03 = BinaryUtils.GetUInt16(buf, pos + 22);
            var adc10 = BinaryUtils.GetUInt16(buf, pos + 24);
            var adc11 = BinaryUtils.GetUInt16(buf, pos + 26);
            var adc12 = BinaryUtils.GetUInt16(buf, pos + 28);
            var adc13 = BinaryUtils.GetUInt16(buf, pos + 30);

            MetaSenseMessage msg = new MetaSenseMessage();
            if (hasTime > 0) msg.Ts = messageTime;
            if (hasRawGas > 0)
                msg.Raw = new MetaSenseRawGasReadings
                {
                    Rng = range,
                    S1A = adc00,
                    S1W = adc01,
                    S2A = adc12,
                    S2W = adc13,
                    S3A = adc10,
                    S3W = adc11,
                    Temperature = adc02,
                    Voc = adc03
                };
            if (hasHumBar > 0)
                msg.HuPr = new MetaSenseRawHuPrReadings
                {
                    BarometricSensorPressureMilliBar = (double)barP / 10.0,
                    BarometricSensorTemperatureCelsius = (double)barT / 10.0,
                    HumiditySensorHumidityPercent = (double)humH / 100.0,
                    HumiditySensorTemperatureCelsius = (double)humT / 100.0
                };

            if (msgType == 0x02)
            {
                var co2Ppm = BinaryUtils.GetUInt16(buf, pos + 32);
                var vocIaqPpb = BinaryUtils.GetUInt32(buf, pos + 34);
                var vocPidPpb = BinaryUtils.GetUInt32(buf, pos + 38);

                if (hasCO2 > 0)
                    msg.Co2 = new MetaSenseCo2Readings { Co2 = co2Ppm };
                if (hasVoc > 0)
                {
                    msg.Voc = new MetaSenseVocReadings { VIp = vocIaqPpb, VPp = vocPidPpb };
                }

            }
            if (msgType == 0x01) len = 32;
            else if (msgType == 0x02) len = 44;
            else len = 0;
            return msg;
        }

    }

}
