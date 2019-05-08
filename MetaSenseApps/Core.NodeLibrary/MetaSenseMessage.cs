using System;
using Newtonsoft.Json;

namespace NodeLibrary
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

        // DM: add methods to make median filtering easier
        /// <summary>
        /// Returns an array containing all the raw gas readings.
        /// </summary>
        /// <returns>A double array containing the raw gas readings in the order of:
        ///     Rng, S1A, S1W, S2A, S2W, S3A, S3W, Temperature, Voc.</returns>
        public double[] ToDoubleArray()
        {
            return new double[9] { Rng, S1A, S1W, S2A, S2W, S3A, S3W, Temperature, Voc };
        }

        /// <summary>
        /// Returns an array containing the labels of all the raw gas readings.
        /// </summary>
        /// <returns>A string array containing the labels of the raw gas readings in the order of:
        ///     Rng, S1A, S1W, S2A, S2W, S3A, S3W, Temperature, Voc.</returns>
        public static string[] GetArrayLabels()
        {
            return new string[9] { "Rng", "S1A", "S1W", "S2A", "S2W", "S3A", "S3W", "Temperature",
                "Voc" };
        }
        // DM: end
    }

    public class MetaSenseLocation
    {
        public double? Latitude;
        public double? Longitude;
        public double? Altitude;
        public float? Accuracy;
        public float? Bearing;
        public float? Speed;
    }
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

        // DM: add methods to make median filtering easier
        /// <summary>
        /// Returns an array containing all the raw hupr readings.
        /// </summary>
        /// <returns>A double array containing the raw hupr readings in the order of:
        ///     humidity sensor temperature (degrees Celsius), humidity sensor humidity percent,
        ///     barometric sensor pressure (mbar), barometric sensor temperature (degrees
        ///         Celsius)</returns>
        public double[] ToArray()
        {
            return new double[4] { HumiditySensorTemperatureCelsius, HumiditySensorHumidityPercent,
                BarometricSensorPressureMilliBar, BarometricSensorTemperatureCelsius };
        }

        /// <summary>
        /// Returns an array containing the labels of all the raw hupr readings.
        /// </summary>
        /// <returns>A string array containing the labels of the raw hupr readings in the order of:
        ///     humidity sensor temperature (degrees Celsius), humidity sensor humidity percent,
        ///     barometric sensor pressure (mbar), barometric sensor temperature (degrees
        ///         Celsius)</returns>
        public static string[] GetArrayLabels()
        {
            return new string[4] { "HumiditySensorTemperatureCelsius",
                "HumiditySensorHumidityPercent", "BarometricSensorPressureMilliBar",
                "BarometricSensorTemperatureCelsius" };
        }
        // DM: end
    }
    //public class MetaSenseAfeConfiguration
    //{
    //    public string Ser;
    //    public string Name;
    //    //public int PTV;
    //    //public SensorCalibrationParameters[] cal;
    //}
    //public class SensorCalibrationParameters
    //{
    //    public string Typ;
    //    public int We0;
    //    public int Ws0;
    //    public int Ae0;
    //    public int As0;
    //    public int Sns;
    //    public int No2Sns;
    //}

    public class MetaSenseCo2Readings
    {
        [JsonProperty("CO2")]
        public double Co2;
    }

    public class MetaSenseVocReadings
    {
        [JsonProperty("vPP")]
        public double VPp;
        [JsonProperty("vIP")]
        public double VIp;
    }

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
    }

}
