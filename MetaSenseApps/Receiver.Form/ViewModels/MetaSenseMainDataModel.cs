using System;
using System.Threading.Tasks;
using Core.ViewModels.Properties;
using NodeLibrary;
using NodeLibrary.Native;
using Xamarin.Forms;

namespace Receiver.ViewModels
{
    public class MetaSenseMainDataModel
    {
        private readonly ISensorLocation _sensorLocation;
        private readonly IBLEUtils _bleUtils;

        private MetaSenseNode _node;
        private string _mac;
        
        //private bool _inProgress;
        //private const int OngoingNotificationId = 111;

        public MetaSenseMainDataModel(
            [NotNull] ISensorLocation sensorLocation, 
            [NotNull] IBLEUtils bleUtils)
        {
            if (sensorLocation == null) throw new ArgumentNullException(nameof(sensorLocation));
            if (bleUtils == null) throw new ArgumentNullException(nameof(bleUtils));
            _sensorLocation = sensorLocation;
            _bleUtils = bleUtils;
        }
        public async Task<bool> StartNode(string mac)
        {
            _mac = mac;
            if (_mac == null) return false;
            try
            {
                return 
                    await _sensorLocation.StartWhenConnected() && 
                    await InitNode(_mac);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return false;
        }
        public async void StopCurrentNode()
        {
            try
            {
                await _sensorLocation.Stop();
                if (_node == null)
                    return;
                await _node.DisconnectAsync();
                // DM: remove event handlers after disconnecting
                _node.ReachableChanged -= Node_ReachableChanged;
                _node.MessageReceived -= Node_MessageReceived;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        private async Task<bool> InitNode(string nodeMac)
        {
            try
            {
                if (nodeMac == null || _bleUtils==null) return false;
                var deviceInfo = await _bleUtils.DeviceInfoFromMac(nodeMac);
                if (deviceInfo != null)
                {
                    if (_node != null)
                    {
                        if (_node.Info.Id.Equals(deviceInfo.Id))
                        {
                            //Node already connected
                            await _node.ConnectAsync();
                            return true;
                        }
                        //Need to connect different node
                        await _node.DisconnectAsync();
                        _node.ReachableChanged -= Node_ReachableChanged;
                        _node.MessageReceived -= Node_MessageReceived;
                    }
                    _node = await _bleUtils.NodeFactory(deviceInfo);
                    _node.ReachableChanged += Node_ReachableChanged;
                    _node.MessageReceived += Node_MessageReceived;
                    await _node.ConnectAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return false;
        }
        private async void SyncNodeTime()
        {
            if (_node == null)
                return;
            await _node.SendEnableWakeup();
            await Task.Delay(250);
            await _node.SendAck();
            await Task.Delay(250);
            await _node.SendDisableWakeup();
        }
        private void Node_MessageReceived(object sender, MetaSenseMessage message)
        {
            if (message == null) return;
            var location = _sensorLocation.CurrentLocation;
            message.Loc = location;


            if (message.Ts.HasValue)
            {
                var lastTimestampReceived = MetaSenseNode.UnixToDateTime(message.Ts.Value);
                if (DateTime.Now.Subtract(lastTimestampReceived).Duration().TotalSeconds > 5)
                    SyncNodeTime();
            }
            if (message.Raw == null || message.HuPr == null || message.Ts == null) return;
            var read = new Read(
                message.Ts.Value,
                message.Raw,
                message.HuPr,
                message.Co2,
                message.Voc,
                message.Loc);
            try
            {
                SettingsData.Default.AddRead(read);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            try
            {
                MessagingCenter.Send(message, "MetaSenseRead");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        private void Node_ReachableChanged(object sender, bool isRecheable)
        {
            MessagingCenter.Send(new MetaSenseNodeReachable(isRecheable), "MetaSenseNodeReachableChanged");
            if (isRecheable)
            {
                //BLE device paired again ready to reconnect
                if (_node != null)
                {
#pragma warning disable 4014
                    //InitNode(_node.Info.MacAddress);
#pragma warning restore 4014
                }
            }
        }
    }

    internal class MetaSenseNodeReachable
    {
        public bool Connected { get; }

        public MetaSenseNodeReachable(bool reach)
        {
            Connected = reach;
        }
    }
}
