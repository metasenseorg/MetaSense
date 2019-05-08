using System;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using NodeLibrary;

namespace Core.Droid.BLE
{
    internal class BLEDeviceDroid
    {
        private class BLEDeviceBroadcastReceiver : BroadcastReceiver
        {
            private readonly BLEDeviceDroid _device;
            public BLEDeviceBroadcastReceiver(BLEDeviceDroid dev) { _device = dev; }
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    var action = intent.Action;
                    // Get the BluetoothDevice object from the Intent
                    var device = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) as BluetoothDevice;
                    if (device == null || _device._device?.Address == null || !_device._device.Address.Equals(device.Address)) return;
                    // When discovery finds a device
                    if (BluetoothDevice.ActionBondStateChanged.Equals(action))
                    {
                        _device.IsBonded = device.BondState == Bond.Bonded;
                        _device.BondStateChanged.OnEvent(_device.IsBonded);

                    }
                    else if (BluetoothDevice.ActionAclConnected.Equals(action))
                    {
                        _device.IsAclConnected = true;
                        _device.AclConnectionChanged.OnEvent(true);
                    }
                    else if (BluetoothDevice.ActionAclDisconnected.Equals(action))
                    {
                        _device.IsAclConnected = false;
                        _device.AclConnectionChanged.OnEvent(false);
                    }
                    else if (BluetoothDevice.ActionAclDisconnectRequested.Equals(action))
                    {

                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private readonly BroadcastReceiver _receiver;
        private readonly BLEAdapterDroid _bleAdapter;
        private readonly BluetoothDevice _device;

        public bool IsAclConnected { get; protected set; }
        public bool IsBonded { get; protected set; }

        public readonly WaitOnEvent<bool> AclConnectionChanged;
        public readonly WaitOnEvent<bool> BondStateChanged;

        public BLEDeviceDroid(BluetoothDevice device, BLEAdapterDroid bleAdapter)
        {
            _bleAdapter = bleAdapter;
            _device = device;
            _receiver = new BLEDeviceBroadcastReceiver(this);

            AclConnectionChanged = new WaitOnEvent<bool>();
            BondStateChanged = new WaitOnEvent<bool>();
            EnableEvents();
        }
         ~BLEDeviceDroid()
        {
            DisableEvents();
        }

        public void EnableEvents()
        {
            var filter = new IntentFilter(BluetoothDevice.ActionBondStateChanged);
            filter.AddAction(BluetoothDevice.ActionAclConnected);
            filter.AddAction(BluetoothDevice.ActionAclDisconnected);
            filter.AddAction(BluetoothDevice.ActionAclDisconnectRequested);
            _bleAdapter.Context.RegisterReceiver(_receiver, filter);
        }
        public void DisableEvents()
        {
            _bleAdapter.Context.UnregisterReceiver(_receiver);
        }
        public async Task<bool> BindDevice()
        {
            if (await _bleAdapter.CheckPowerState())
            {
                var waiting = BondStateChanged.ReceiveEventAsync();
                if (_device.CreateBond())
                    return await waiting;
            }
            return false;
        }

    }
}