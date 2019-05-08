using Android.Bluetooth;

namespace Core.Droid.BLE
{
    internal sealed class GattPair
    {
        public GattPair(BluetoothGatt gatt, WaitOnBluetoothGattCallback callback)
        {
            Gatt = gatt;
            Callback = callback;
        } 
        public WaitOnBluetoothGattCallback Callback { private set; get; }
        public BluetoothGatt Gatt { private set; get; }
    }
}