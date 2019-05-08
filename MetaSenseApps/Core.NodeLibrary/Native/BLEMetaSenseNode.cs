using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary.Native
{
    public abstract class BLEMetaSenseNode
    {
        protected readonly string MAC;
        private readonly StringBuilder _tmpData;

        protected BLEMetaSenseNode(string mac)
        {
            MAC = mac;
            _tmpData = new StringBuilder();
        }

        public abstract bool Connected { get; }
        public abstract Task<bool> ConnectAsync();
        public abstract Task DisconnectAsync();
        public abstract Task<int?> ReadRssi();

        public event EventHandler<MetaSenseMessage> MessageReceived;
        protected void OnRawDataReceived(byte[] data)
        {
            var encoding = new UTF8Encoding();
            var str = encoding.GetString(data, 0, data.Length);
            //Ignore echo of commandt to enable/disable sleep
            try
            {
                lock (_tmpData)
                {
                    if (!str.StartsWith("OK+PIO2:"))
                        _tmpData.Append(str);

                    var msgs = _tmpData.ToString().Split('\n');
                    for (var i = 0; i < msgs.Length - 1; i++)
                    {
                        try
                        {
                            OnMessageReceived(msgs[i]);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                    _tmpData.Clear();
                    if (msgs.Length > 0)
                        _tmpData.Append(msgs[msgs.Length - 1]);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        private void OnMessageReceived(string msg)
        {
            Log.Trace(msg);
            try
            {
                var m = MetaSenseMessage.FromJsonString(msg);
                if (m != null)
                    MessageReceived?.Invoke(this, m);
            }
            catch (Exception e)
            {
                Log.Trace(e.Message);
            }
        }

        public bool Reachable { get; private set; }
        public event EventHandler<bool> ReachableChanged;
        protected void OnReachableChanged(bool val)
        {
            if (Reachable == val) return;
            Reachable = val;
            ReachableChanged?.Invoke(this, val);
        }

        protected abstract Task<bool> SendRawData(byte[] data);
        public async Task SendString(string str)
        {
            var arr = str.ToCharArray();
            var fragments = new List<List<char>>();
            List<char> tmp = null;
            for (var i = 0; i < arr.Length; i++)
            {
                if ((i % 20) == 0)
                {
                    tmp = new List<char>();
                    fragments.Add(tmp);
                }
                Debug.Assert(tmp != null, "tmp != null");
                tmp.Add(arr[i]);
            }
            var encoding = new UTF8Encoding();
            foreach (var f in fragments)
            {
                var raw = encoding.GetBytes(f.ToArray());
                await SendRawData(raw);
            }
        }
    }
}
