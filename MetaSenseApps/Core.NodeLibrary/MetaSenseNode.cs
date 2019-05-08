using System;
using System.Threading.Tasks;
using NodeLibrary.Native;
using Xamarin.Forms;

namespace NodeLibrary
{
    public enum NodeTransport { BLE, Cloud, Usb }
    public class MetaSenseNode
    {
        private readonly BLEMetaSenseNode _bleNode;
        public MetaSenseNode(NodeInfo info, BLEMetaSenseNode bleNode)
        {
            Info = info;
            _bleNode = bleNode;
            _bleNode.MessageReceived += BLENodeMessageReceived;
            _bleNode.ReachableChanged += OnReachableChanged;
        }
        ~MetaSenseNode()
        {
            if (_bleNode == null) return;
            _bleNode.MessageReceived -= BLENodeMessageReceived;
            _bleNode.ReachableChanged -= OnReachableChanged;
        }
        private void BLENodeMessageReceived(object sender, MetaSenseMessage e)
        {
            OnMessageReceived(e);
        }

        public bool IsConnected => _bleNode.Connected;
        // DM: make ConnectAsync() an async method that returns Task<bool>
        public async Task<bool> ConnectAsync()
        {
            // DM: instead of returning immediately, send messages to lock disconnect button while connecting
            // return _bleNode.ConnectAsync();
            await Task.Run(() =>
            {
                MessagingCenter.Send(this, "MetaSenseConnectivityButton", false);
            });
            var result = await _bleNode.ConnectAsync();
            await Task.Run(() =>
            {
                MessagingCenter.Send(this, "MetaSenseConnectivityButton", true);
            });
            return result;
        }
        // DM: make DisconnectAsync() an async method
        public async Task DisconnectAsync()
        {
            // DM: instead of returning immediately, send messages to lock connect button while disconnecting
            // return _bleNode.DisconnectAsync();
            await Task.Run(() =>
            {
                MessagingCenter.Send(this, "MetaSenseConnectivityButton", false);
            });
            await _bleNode.DisconnectAsync();
            await Task.Run(() =>
            {
                MessagingCenter.Send(this, "MetaSenseConnectivityButton", true);
            });
        }
        public Task<int?> ReadRssi() { return _bleNode.ReadRssi(); }
        public bool IsReachable => _bleNode.Reachable;

        public NodeInfo Info { get; }
        public string TransportName => Info.Transport.ToString();

        public event EventHandler<bool> ReachableChanged;
        protected void OnReachableChanged(object sender, bool val)
        {
            ReachableChanged?.Invoke(this, val);
        }
        public event EventHandler<MetaSenseMessage> MessageReceived;
        protected void OnMessageReceived(MetaSenseMessage msg)
        {
            MessageReceived?.Invoke(this, msg);
            Log.Trace(msg.ToJsonString());
        }

        public static DateTime UnixToDateTime(double ts)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(ts).ToLocalTime();
            return dtDateTime;
        }
        public static double DateTimeToUnix(DateTime ts)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return DateTime.Now.ToUniversalTime().Subtract(dtDateTime).TotalSeconds;
        }
        // DM: add method
        /// <summary>
        /// Convert the specified DateTime to universal unix time.
        /// </summary>
        /// <param name="dt">The DateTime to convert to universal unix time</param>
        /// <returns></returns>
        public static double DtToUnix(DateTime dt)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dt.ToUniversalTime().Subtract(dtDateTime).TotalSeconds;
        }
        // DM: end
        protected MetaSenseMessage Timestamp(MetaSenseMessage msg)
        {
            msg.Ts = (Int64)Math.Round(DateTimeToUnix(DateTime.Now));
            return msg;
        }

        private Task SendString(string str) { return _bleNode.SendString(str); }
        private Task Send(MetaSenseMessage msg) { return SendString(msg.ToJsonString() + "\r\n"); }

        public async Task SendCommandSaveConfig()
        {
            await SendRequest(Flags.StRom);
        }
        public async Task SendCommandClearWiFi()
        {
            await SendRequest(Flags.ClWifi);
        }
        public async Task SendCommandReset()
        {
            await SendRequest(Flags.Reset);
        }
        public async Task SendCommandSetupWiFi(string sSid, string pAss)
        {
            var msg = new MetaSenseMessage
            {
                Ssid = sSid,
                Pass = pAss
            };
            await Send(msg);
        }
        public async Task SendVocEnable(bool on)
        {
            var msg = new MetaSenseMessage {VocEn = on};
            await Send(msg);
        }
        public async Task SendCo2Enable(bool on)
        {
            var msg = new MetaSenseMessage {Co2En = on};
            await Send(msg);
        }
        public async Task SendRequestCo2Enable()
        {
            await SendRequest(Flags.Co2En);
        }
        public async Task SendRequestVocEnable()
        {
            await SendRequest(Flags.VocEn);
        }
        public async Task SendCommandVocEnable(bool enableVoc)
        {
            var msg = new MetaSenseMessage {VocEn = enableVoc};
            await Send(msg);
        }
        public async Task SendCommandCo2Enable(bool enableCo2)
        {
            var msg = new MetaSenseMessage {Co2En = enableCo2};
            await Send(msg);
        }
        public async Task SendCommandSetAfeSerial(string aFeSerial)
        {
            var msg = new MetaSenseMessage {AfeSer = aFeSerial};
            await Send(msg);
        }
        public async Task SendAck()
        {
            MetaSenseMessage msg = new MetaSenseMessage();
            Timestamp(msg);
            await Send(msg);
        }
        public async Task SendCommandSetInterval(long secs)
        {
            var msg = new MetaSenseMessage {SInter = secs};
            await Send(msg);
        }
        public async Task SendCommandMirrorToUsb(bool value)
        {
            var msg = new MetaSenseMessage {UsbEn = value};
            await Send(msg);
        }
        public async Task SendCommandLogToSd(bool value)
        {
            var msg = new MetaSenseMessage {SSd = value};
            await Send(msg);
        }
        public async Task SendCommandLogToWiFi(bool value)
        {
            var msg = new MetaSenseMessage {SWifi = value};
            await Send(msg);
        }
        public async Task SendCommandLogToBLE(bool value)
        {
            var msg = new MetaSenseMessage {StreamBLE = value};
            await Send(msg);
        }
        public async Task SendCommandSleepEnable(bool value)
        {
            var msg = new MetaSenseMessage {SleepEn = value};
            await Send(msg);
        }
        public async Task SendCommandWiFiEnable(bool value)
        {
            var msg = new MetaSenseMessage {WifiEn = value};
            await Send(msg);
        }
        public async Task SendRequestConf()
        {
            await SendRequest(Flags.AfeSer);
            await Task.Delay(200);
            await SendRequest(Flags.NodeId);
            await Task.Delay(200);
            await SendRequest(Flags.MacAddr);
        }
        public async Task SendRequestInterval()
        {
            await SendRequest(Flags.SInter);
        }
        public async Task SendRequestLogToSd()
        {
            await SendRequest(Flags.StreamSD);
        }
        public async Task SendRequestPower()
        {
            await SendRequest(Flags.Power);
        }
        public async Task SendRequestLogToBLE()
        {
            await SendRequest(Flags.StreamBLE);
        }
        public async Task SendRequestLogToWiFi()
        {
            await SendRequest(Flags.StreamWifi);
        }
        public async Task SendRequestWiFiStatus()
        {
            await SendRequest(Flags.WifiEn);
        }
        public async Task SendRequestSleepStatus()
        {
            await SendRequest(Flags.SleepEn);
        }
        public async Task SendRequestMirrorToUsbStatus()
        {
            await SendRequest(Flags.UsbEn);
        }
        private async Task SendRequest(Flags reqFlags)
        {
            var msg = new MetaSenseMessage(reqFlags);
            await Send(msg);
        }
        public async Task ExecuteWithNodeWokenup(Task act)
        {
            await SendEnableWakeup();
            await Task.Delay(500);
            act.Start();
            await act;
            await Task.Delay(500);
            await SendDisableWakeup();
        }
        public async Task SendEnableWakeup()
        {
            await SendString("AT+PIO21");
            await Task.Delay(100);
        }
        public async Task SendDisableWakeup()
        {
            await SendString("AT+PIO20");
        }
    }
}
