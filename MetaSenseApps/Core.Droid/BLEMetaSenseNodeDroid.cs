using System.Linq;
using System.Threading.Tasks;
using Android.Bluetooth;
using Core.Droid.BLE;
using Java.Util;
using NodeLibrary.Native;

namespace Core.Droid
{
    internal class BLEMetaSenseNodeDroid : BLEMetaSenseNode
    {
        private readonly UUID _clientCharacteristicConfig = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
        private readonly UUID _serviceId = UUID.FromString("0000ffe0-0000-1000-8000-00805f9b34fb");
        private readonly UUID _characteristicsId = UUID.FromString("0000ffe1-0000-1000-8000-00805f9b34fb");

        private readonly BLEAdapterDroid _bleAdapter;
        private GattPair _gattPair;
        private BluetoothGattCharacteristic _characteristic;
        private bool _reacheable;
        public BLEMetaSenseNodeDroid(string  macAddress, BLEAdapterDroid bleAdapter) : base(macAddress)
        {
            _bleAdapter = bleAdapter;
        }
        private async Task<bool> EnableNotificationForReads(GattPair pair, BluetoothGattCharacteristic c)
        {
            if (_gattPair?.Gatt == null || _characteristic == null) return false;
            pair.Gatt.SetCharacteristicNotification(c, true);
            var descriptor = c.GetDescriptor(_clientCharacteristicConfig);
            descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
            if (!pair.Gatt.WriteDescriptor(descriptor)) return false;
            return (await _gattPair.Callback.WaitDescriptorWrite(descriptor)) == GattStatus.Success;
        }
        private async Task<bool> DisableNotificationForReads(GattPair pair, BluetoothGattCharacteristic c)
        {
            if (_gattPair?.Gatt == null || _characteristic == null) return false;
            pair.Gatt.SetCharacteristicNotification(c, false);
            var descriptor = c.GetDescriptor(_clientCharacteristicConfig);
            descriptor.SetValue(BluetoothGattDescriptor.DisableNotificationValue.ToArray());
            if (!pair.Gatt.WriteDescriptor(descriptor)) return false;
            return (await _gattPair.Callback.WaitDescriptorWrite(descriptor)) == GattStatus.Success;
        }
        public void OnHandlerCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            var val = characteristic.GetValue();
            OnRawDataReceived(val);
        }
        private void OnConnectionStateChange(object sender, ProfileState e)
        {
            _reacheable = e == ProfileState.Connected;
            OnReachableChanged(_reacheable);
        }
        #region Platform Specific Implementation of public/protected members
        public override bool Connected => _characteristic != null;
        protected override async Task<bool> SendRawData(byte[] data)
        {
            if (_gattPair?.Gatt == null || _characteristic == null) return false;
            if (!_characteristic.Properties.HasFlag(GattProperty.WriteNoResponse)) return false;// This characteristic does not support notifications
            _characteristic.SetValue(data);
            _characteristic.WriteType = GattWriteType.NoResponse;
            var waitable = _gattPair.Callback.GetWaitableResponse(_characteristic);
            if (!_gattPair.Gatt.WriteCharacteristic(_characteristic))
                return false;
            return await waitable.WaitOn() == GattStatus.Success;
        }
        public override async Task<int?> ReadRssi()
        {
            if (_gattPair?.Gatt == null)
                return null;
            _gattPair.Gatt.ReadRemoteRssi();
            var rssi = await _gattPair.Callback.WaitReadRemoteRssi(_gattPair.Gatt);
            return rssi;
        }
        public override async Task<bool> ConnectAsync()
        {
            if (!(_gattPair?.Gatt == null || _characteristic == null))
            {
                var macConnected = _gattPair.Gatt.Device.Address;
                if (!Equals(MAC, macConnected))
                    await DisconnectAsync();
            }

            var btd = await _bleAdapter.GetDevice(MAC);
            if (btd?.BondState != Bond.Bonded) return false;
            _gattPair = _bleAdapter.ConnectGatt(btd);
            var gatt = _gattPair.Gatt;
            var callback = _gattPair.Callback;
            if (gatt == null) return false;
            var res = await callback.WaitConnectionStateChange(gatt);
            if (res != ProfileState.Connected) return false;
            OnReachableChanged(true);

            callback.ConnectionStateChange += OnConnectionStateChange;
            callback.HandlerCharacteristicChanged += OnHandlerCharacteristicChanged;

            gatt.DiscoverServices();

            await callback.WaitServiceDiscovered(gatt);
            var service = gatt.GetService(_serviceId);
            var count = 0;
            while (service == null)
            {
                await Task.Delay(1000);
                service = gatt.GetService(_serviceId);
                if (service != null)
                    break;
                count++;
                if (count > 10)
                    break;
            }
            if (service == null) return false;
            _characteristic = service.GetCharacteristic(_characteristicsId);
            if (_characteristic == null) return false;
            gatt.SetCharacteristicNotification(_characteristic, true);
            _gattPair = new GattPair(gatt, callback);

            return await EnableNotificationForReads(_gattPair, _characteristic);
        }
        public override async Task DisconnectAsync()
        {
            if (_gattPair?.Gatt == null || _characteristic == null) return;
            _gattPair.Gatt.SetCharacteristicNotification(_characteristic, false);
            await DisableNotificationForReads(_gattPair, _characteristic);
            _gattPair.Callback.HandlerCharacteristicChanged -= OnHandlerCharacteristicChanged;
            _gattPair.Callback.ConnectionStateChange -= OnConnectionStateChange;
            _gattPair.Gatt.Close();
            _gattPair = null;
            _characteristic = null;
            OnReachableChanged(false);
        }
        #endregion
    }
}
