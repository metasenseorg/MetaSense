using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Net.Wifi;
using Core.Droid;
using Core.Droid.Platform;
using NodeLibrary;
using NodeLibrary.Native;
using Xamarin.Forms;

[assembly: Dependency(typeof(WiFiUtilsDroid))]
namespace Core.Droid
{
    internal sealed class WiFiUtilsDroid : IWiFiUtils
    {
        public async Task<IList<SSIDInfo>> NearbySSID()
        {
            var wifiService = Forms.Context.GetSystemService(Context.WifiService) as WifiManager;
            var ret = new List<SSIDInfo>();
            if (wifiService == null)
                return ret;
            var receiver = new WaitOnBroadcastReceiver<object>();
            Forms.Context.RegisterReceiver(receiver, new IntentFilter(WifiManager.ScanResultsAvailableAction));
            try
            {
                var scanning = wifiService.StartScan();
                if (scanning)
                    await receiver.Complete();
                Forms.Context.UnregisterReceiver(receiver);
                foreach (var net in wifiService.ScanResults)
                {
                    var capabilities = net.Capabilities.Split('[', ']');
                    string authType = "unknown";
                    string encryption = "unknown";
                    foreach (var c in capabilities)
                    {
                        if (c.Contains("-CCMP"))
                            encryption = "aes";
                        if (c.Contains("-TKIP"))
                            encryption = "tkip";
                        if (c.Contains("-WEP"))
                            encryption = "wep";
                        if (c.Contains("WEP"))
                            authType = "wep";
                        if (c.Contains("WPA-PSK"))
                            authType = "wpa_psk";
                        if (c.Contains("WPA2-PSK"))
                            authType = "wpa2_psk";
                    }
                    ret.Add(new SSIDInfo(net.Ssid, net.Bssid, WifiManager.CalculateSignalLevel(net.Level, 5), authType, encryption));
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return ret;
        }
    }
}