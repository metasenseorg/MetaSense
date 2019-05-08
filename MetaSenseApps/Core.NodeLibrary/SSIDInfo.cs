namespace NodeLibrary
{
    public class SSIDInfo
    {
        public SSIDInfo(string ssid, string bssid, int signalBars, string authType, string encryption)
        {
            SSID = ssid;
            MAC = bssid;
            Strength = signalBars;
            AuthenticationType = authType;
            EncryptionType = encryption;
        }
        public override string ToString()
        {
            return SSID;
        }
        public string SSID { get; }
        public string MAC { get; }
        public int Strength { get; }
        public string AuthenticationType { get; }
        public string EncryptionType { get; }
    }
}
