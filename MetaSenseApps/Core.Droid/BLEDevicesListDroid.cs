using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Runtime;
using Core.Droid;
using Core.Droid.BLE;
using NodeLibrary;
using NodeLibrary.Native;
using Xamarin.Forms;

[assembly: Dependency(typeof(BLEDevicesListDroid))]
namespace Core.Droid
{
    internal sealed class BLEDevicesListDroid : BLEDevicesList
    {
        public BLEDevicesListDroid() : this(Forms.Context) { }
        public BLEDevicesListDroid(Context context)
        {
            _bleAdapter = new BLEAdapterDroid(context);
            _bleAdapter.ScanCallback.HandlerBatchScanResults += OnBatchScanResults;
            _bleAdapter.ScanCallback.HandlerScanFailed += OnScanFailed;
            _bleAdapter.ScanCallback.HandlerScanResult += OnScanResult;
        }
        ~BLEDevicesListDroid()
        {
            // ReSharper disable DelegateSubtraction
            _bleAdapter.ScanCallback.HandlerBatchScanResults -= OnBatchScanResults;
            _bleAdapter.ScanCallback.HandlerScanFailed -= OnScanFailed;
            _bleAdapter.ScanCallback.HandlerScanResult -= OnScanResult;
            // ReSharper restore DelegateSubtraction
            if (IsScanning)
                StopScanDevices();
        }

        private readonly BLEAdapterDroid _bleAdapter;
        private bool IsScanning => _bleAdapter.IsScanning;
        private BLENodeInfoDroid DeviceInfo(ScanResult result)
        {
            var info = new BLENodeInfoDroid(result.Device.Address, 
                result.Device.BondState==Bond.Bonded, result.Device.Name, result.Rssi, _bleAdapter);
            return info;
        }
        private BLENodeInfoDroid DeviceInfo(BluetoothDevice device)
        {
            var info = new BLENodeInfoDroid(device.Address, 
                device.BondState == Bond.Bonded, device.Name, 0, _bleAdapter);
            return info;
        }

        private void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            Log.Info("OnScanResult");
            try
            {
                var mac = result.Device.Address;
                lock (DevicesDictionary)
                {
                    if ((!DevicesDictionary.ContainsKey(mac)) && (callbackType == ScanCallbackType.FirstMatch || callbackType == ScanCallbackType.AllMatches))
                    {
                        var info = DeviceInfo(result);
                        DevicesDictionary.Add(mac, info);
                        OnDeviceAdd(info);
                    }
                    else if ((DevicesDictionary.ContainsKey(mac)) && (callbackType == ScanCallbackType.FirstMatch || callbackType == ScanCallbackType.AllMatches))
                    {
                        var info = DevicesDictionary[mac] as BLENodeInfoDroid;
                        if (info != null)
                        {
                            info.Update(result);
                            OnDeviceUpdate(info);
                        }
                    }
                    else if (DevicesDictionary.ContainsKey(result.Device.Address) && (callbackType == ScanCallbackType.MatchLost))
                    {
                        var dev = DevicesDictionary[mac];
                        DevicesDictionary.Remove(mac);
                        OnDeviceRemove(dev);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            Log.Info("OnScanResult done");
        }
        private void OnBatchScanResults(IList<ScanResult> results)
        {
            Log.Info("OnBatchScanResults");
        }
        private void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            Log.Info("OnScanFailed");
            if (errorCode == ScanFailure.ApplicationRegistrationFailed)
                _bleAdapter.ResetAdapter();
        }

        public override async void StartScanDevices(int seconds)
        {
            if (IsScanning)
                StopScanDevices();
            ClearList();
            
            foreach (var d in await _bleAdapter.GetBondedDevices())
            {
                var info = DeviceInfo(d);
                DevicesDictionary.Add(info.MacAddress, info);
                OnDeviceAdd(info);
            }
            if(await _bleAdapter.StartScanDevices())
            {
                await Task.Delay(seconds * 1000);
                StopScanDevices();
            }
        }
        public override async void StopScanDevices()
        {
            if (_bleAdapter.IsScanning)
                await _bleAdapter.StopScanDevices();
        }

    }
}
