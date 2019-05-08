using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Core.Droid.BLE;
using NodeLibrary;
using NodeLibrary.Native;

namespace Core.Droid
{
    internal sealed class BLENodeInfoDroid : NodeInfo
    {
        private BluetoothDevice _device;
        private BLEDeviceDroid _bleDevice;
        private readonly BLEAdapterDroid _bleAdapter;

        internal BLENodeInfoDroid(string macAddress, BLEAdapterDroid bleAdapter) : base(macAddress, macAddress, macAddress, NodeTransport.BLE)
        {
            _bleAdapter = bleAdapter;
            StartDeviceEvents();
        }
        public BLENodeInfoDroid(string macAddress, bool pair, string name, int rssi, BLEAdapterDroid bleAdapter) : this(macAddress, bleAdapter)
        {
            Paired = pair;
            Name = name;
            SignalStrength = BLEAdapterDroid.RssiToSignalStrength(rssi);
        }
        ~BLENodeInfoDroid()
        {
            EndDeviceEvents();
        }
        
        private async void StartDeviceEvents()
        {
            _device = await _bleAdapter.GetDevice(MacAddress);
            _bleDevice = new BLEDeviceDroid(_device, _bleAdapter);
            _bleDevice.BondStateChanged.Event += BondStateChanged_Event;  
            Update(_device);
            _bleDevice.EnableEvents();
        }
        private void EndDeviceEvents()
        {
            _bleDevice.DisableEvents();
            _bleDevice.BondStateChanged.Event -= BondStateChanged_Event;
        }

        private void BondStateChanged_Event(object sender, bool e)
        {
            Paired = e;
            OnUpdated();
        }

        public void Update(BluetoothDevice device)
        {
            var changed = false;
            if (device.Name != null)
            {
                if (!device.Name.Equals(Name))
                {
                    Name = device.Name;
                    changed = true;
                }
            }
            var pair = device.BondState == Bond.Bonded;
            if (pair != Paired)
            {
                Paired = pair;
                changed = true;
            }
            if (changed)
                OnUpdated();
        }
        public void Update(ScanResult result)
        {
            var changed = false;
            if (result.Device.Name != null)
            {
                if (!result.Device.Name.Equals(Name))
                {
                    Name = result.Device.Name;
                    changed = true;
                }
            }
            var pair = result.Device.BondState == Bond.Bonded;
            if (pair != Paired)
            {
                Paired = pair;
                changed = true;
            }
            var val = BLEAdapterDroid.RssiToSignalStrength(result.Rssi);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (SignalStrength != val)
            {
                SignalStrength = val;
                changed = true;
            }
            if (changed)
                OnUpdated();
        }
        public Task<bool> DoPair()
        {
            return _bleDevice.BindDevice(); 
        }
        public override Task<MetaSenseNode> NodeFactory()
        {
            return Task.FromResult(new MetaSenseNode(this, new BLEMetaSenseNodeDroid(MacAddress, _bleAdapter)));
        }
 
     }
}
