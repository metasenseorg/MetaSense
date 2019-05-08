using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Runtime;
using NodeLibrary;

namespace Core.Droid.BLE
{
    internal class WaitOnBluetoothGattCallback : BluetoothGattCallback
    {
        private Dictionary<BluetoothGatt, WaitableResponse<object>> waitableGattList = new Dictionary<BluetoothGatt, WaitableResponse<object>>();
        private Dictionary<BluetoothGattCharacteristic, WaitableResponse<GattStatus>> waitableCharList = new Dictionary<BluetoothGattCharacteristic, WaitableResponse<GattStatus>>();
        private Dictionary<BluetoothGattDescriptor, WaitableResponse<GattStatus>> waitableDescList = new Dictionary<BluetoothGattDescriptor, WaitableResponse<GattStatus>>();

        public event EventHandler<ProfileState> ConnectionStateChange;

        private WaitableResponse<object> GetWaitableResponse(BluetoothGatt key)
        {
            lock (waitableGattList)
            {
                if (waitableGattList.ContainsKey(key))
                    return waitableGattList[key];
                var resp = new WaitableResponse<object>(-1);
                waitableGattList[key] = resp;
                return resp;
            }
        }
        public WaitableResponse<GattStatus> GetWaitableResponse(BluetoothGattCharacteristic key)
        {
            lock (waitableCharList)
            {
                if (waitableCharList.ContainsKey(key))
                    return waitableCharList[key];
                var resp = new WaitableResponse<GattStatus>(1000);
                waitableCharList[key] = resp;
                return resp;
            }
        }
        public WaitableResponse<GattStatus> GetWaitableResponse(BluetoothGattDescriptor key)
        {
            lock (waitableDescList)
            {
                if (waitableDescList.ContainsKey(key))
                    return waitableDescList[key];
                var resp = new WaitableResponse<GattStatus>(1000);
                waitableDescList[key] = resp;
                return resp;
            }
        }

        public async Task<GattStatus> WaitServiceDiscovered(BluetoothGatt gatt)
        {
            return (GattStatus)await GetWaitableResponse(gatt).WaitOn();
        }
        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);
            GetWaitableResponse(gatt).ResolveWaitOn(status);
        }
        
        public async Task<ProfileState> WaitConnectionStateChange(BluetoothGatt gatt)
        {
            return (ProfileState)await GetWaitableResponse(gatt).WaitOn();
        }
        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);
            GetWaitableResponse(gatt).ResolveWaitOn(newState);
            ConnectionStateChange?.Invoke(this, newState);
        }

        //public async Task<GattStatus> WaitCharacteristicWrite(BluetoothGattCharacteristic characteristic)
        //{
        //    return (GattStatus)await GetWaitableResponse(characteristic).WaitOn();
        //}
        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            base.OnCharacteristicWrite(gatt, characteristic, status);
            lock (waitableCharList)
            {
                if (!waitableCharList.ContainsKey(characteristic)) return;
                var resp = waitableCharList[characteristic];
                waitableCharList.Remove(characteristic);
                resp.ResolveWaitOn(status);
            }
        }
        public async Task<GattStatus> WaitDescriptorRead(BluetoothGattDescriptor descriptor)
        {
            return await GetWaitableResponse(descriptor).WaitOn();
        }
        public async Task<GattStatus> WaitDescriptorWrite(BluetoothGattDescriptor descriptor)
        {
            return await GetWaitableResponse(descriptor).WaitOn();
        }
        private void OnDecriptorResolveEvents(BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
        {
            lock (waitableDescList)
            {
                if (!waitableDescList.ContainsKey(descriptor)) return;
                var resp = waitableDescList[descriptor];
                waitableDescList.Remove(descriptor);
                resp.ResolveWaitOn(status);
            }
        }
        public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
        {
            OnDecriptorResolveEvents(descriptor, status);
        }
        public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
        {
            OnDecriptorResolveEvents(descriptor, status);
        }

        public async Task<int?> WaitReadRemoteRssi(BluetoothGatt gatt)
        {
            return (int?)await GetWaitableResponse(gatt).WaitOn();
        }
        public override void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, [GeneratedEnum] GattStatus status)
        {
            base.OnReadRemoteRssi(gatt, rssi, status);
            if (status == GattStatus.Success)
                GetWaitableResponse(gatt).ResolveWaitOn(rssi);
            else
                GetWaitableResponse(gatt).ResolveWaitOn(null);
        }

        public delegate void CharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic);
        public event CharacteristicChanged HandlerCharacteristicChanged;
        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);
            HandlerCharacteristicChanged?.Invoke(gatt, characteristic);
        }
    }


    //internal class WaitOnBluetoothGattCallback : BluetoothGattCallback
    //{
    //    private readonly Dictionary<BluetoothGatt, WaitableResponse<object>> _waitableGattList = new Dictionary<BluetoothGatt, WaitableResponse<object>>();
    //    private readonly Dictionary<BluetoothGattCharacteristic, WaitableResponse<object>> _waitableCharList = new Dictionary<BluetoothGattCharacteristic, WaitableResponse<object>>();
    //    private readonly Dictionary<BluetoothGattDescriptor, WaitableResponse<object>> _waitableDescList = new Dictionary<BluetoothGattDescriptor, WaitableResponse<object>>();

    //    private WaitableResponse<object> GetWaitableResponse(BluetoothGatt key)
    //    {
    //        lock (_waitableGattList)
    //        {
    //            if (_waitableGattList.ContainsKey(key))
    //                return _waitableGattList[key];
    //            var resp = new WaitableResponse<object>();
    //            _waitableGattList[key] = resp;
    //            return resp;
    //        }
    //    }
    //    private WaitableResponse<object> GetWaitableResponse(BluetoothGattCharacteristic key)
    //    {
    //        lock (_waitableCharList)
    //        {
    //            if (_waitableCharList.ContainsKey(key))
    //                return _waitableCharList[key];
    //            var resp = new WaitableResponse<object>();
    //            _waitableCharList[key] = resp;
    //            return resp;
    //        }
    //    }
    //    private WaitableResponse<object> GetWaitableResponse(BluetoothGattDescriptor key)
    //    {
    //        lock (_waitableDescList)
    //        {
    //            if (_waitableDescList.ContainsKey(key))
    //                return _waitableDescList[key];
    //            var resp = new WaitableResponse<object>();
    //            _waitableDescList[key] = resp;
    //            return resp;
    //        }
    //    }

    //    public async Task<GattStatus> WaitServiceDiscovered(BluetoothGatt gatt)
    //    {
    //        return (GattStatus)await GetWaitableResponse(gatt).WaitOn();
    //    }
    //    public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
    //    {
    //        base.OnServicesDiscovered(gatt, status);
    //        GetWaitableResponse(gatt).ResolveWaitOn(status);
    //    }

    //    public async Task<ProfileState> WaitConnectionStateChange(BluetoothGatt gatt)
    //    {
    //        return (ProfileState)await GetWaitableResponse(gatt).WaitOn();
    //    }
    //    public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
    //    {
    //        base.OnConnectionStateChange(gatt, status, newState);
    //        GetWaitableResponse(gatt).ResolveWaitOn(newState);
    //        ConnectionStateChange?.Invoke(this, newState);
    //    }

    //    public async Task<GattStatus> WaitCharacteristicWrite(BluetoothGattCharacteristic characteristic)
    //    {
    //        return (GattStatus)await GetWaitableResponse(characteristic).WaitOn();
    //    }
    //    public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
    //    {
    //        base.OnCharacteristicWrite(gatt, characteristic, status);
    //        GetWaitableResponse(characteristic).ResolveWaitOn(status);
    //    }
    //    public async Task<GattStatus> WaitDescriptorRead(BluetoothGattDescriptor descriptor)
    //    {
    //        return (GattStatus)await GetWaitableResponse(descriptor).WaitOn();
    //    }
    //    public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
    //    {
    //        base.OnDescriptorRead(gatt, descriptor, status);
    //        GetWaitableResponse(descriptor).ResolveWaitOn(status);
    //    }
    //    public async Task<GattStatus> WaitDescriptorWrite(BluetoothGattDescriptor descriptor)
    //    {
    //        return (GattStatus)await GetWaitableResponse(descriptor).WaitOn();
    //    }
    //    public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
    //    {
    //        base.OnDescriptorWrite(gatt, descriptor, status);
    //        GetWaitableResponse(descriptor).ResolveWaitOn(status);
    //    }
    //    public async Task<int?> WaitReadRemoteRssi(BluetoothGatt gatt)
    //    {
    //        return (int?)await GetWaitableResponse(gatt).WaitOn();
    //    }
    //    public override void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, [GeneratedEnum] GattStatus status)
    //    {
    //        base.OnReadRemoteRssi(gatt, rssi, status);
    //        if (status == GattStatus.Success)
    //            GetWaitableResponse(gatt).ResolveWaitOn(rssi);
    //        else
    //            GetWaitableResponse(gatt).ResolveWaitOn(null);
    //    }
    //    public delegate void CharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic);

    //    public event EventHandler<ProfileState> ConnectionStateChange;
    //    public event CharacteristicChanged HandlerCharacteristicChanged;
    //    public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
    //    {
    //        base.OnCharacteristicChanged(gatt, characteristic);
    //        HandlerCharacteristicChanged?.Invoke(gatt, characteristic);
    //    }
    //}

}