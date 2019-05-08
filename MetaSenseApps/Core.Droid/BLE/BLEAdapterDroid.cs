using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Core.Droid.Platform;
using NodeLibrary;
using Xamarin.Forms;
using ScanMode = Android.Bluetooth.LE.ScanMode;

namespace Core.Droid.BLE
{
    internal class BLEAdapterDroid
    {
        //private readonly UUID _serviceId = UUID.FromString("0000ffe0-0000-1000-8000-00805f9b34fb");
        //private readonly UUID _characteristicsId = UUID.FromString("0000ffe1-0000-1000-8000-00805f9b34fb");
        //private readonly UUID _clientCharacteristicConfig = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        private readonly BluetoothAdapter _adapter;
        public WaitOnScanCallback ScanCallback { get; }
        //public BluetoothAdapter BluetoothAdapter { get { return adapter; } }

        public bool IsScanning { get; private set; }

        private static int _requestCode;
        private readonly Dictionary<int, Task> _waitList = new Dictionary<int, Task>();
        private readonly Dictionary<int, CancellationTokenSource> _cancList = new Dictionary<int, CancellationTokenSource>();
        private readonly Dictionary<int, ActivityResultMessage> _resultList = new Dictionary<int, ActivityResultMessage>();

        private readonly object _thisLock = new object();

        public Context Context;

        public BLEAdapterDroid(Context context)
        {
            Context = context;
            try
            {
                var bluetoothService = (BluetoothManager)context.GetSystemService(Context.BluetoothService);
                _adapter = bluetoothService.Adapter;
                ScanCallback = new WaitOnScanCallback();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            MessagingCenter.Subscribe<ActivityResultMessage>(this, ActivityResultMessage.Key, message =>
            {
                lock (_thisLock)
                {
                    var rc = message.RequestCode;
                    if (_cancList.ContainsKey(rc))
                    {
                        CancellationTokenSource tokenSource = _cancList[rc];
                        _cancList.Remove(rc);
                        _waitList.Remove(rc);
                        _resultList[rc] = message;
                        tokenSource.Cancel();
                    }
                }
            });
        }

        public void ResetAdapter()
        {
            //Neet to restart the bluetooth adapter
            //TODO Test this works
            try
            {
                if (!_adapter.IsEnabled) return;
                _adapter.Disable();
                _adapter.Enable();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        private int RequestCode {
            get
            {
                lock (_thisLock)
                {
                    var tokenSource = new CancellationTokenSource();
                    var rc = _requestCode++;
                    _waitList.Add(rc, Task.Delay(-1, tokenSource.Token));
                    _cancList.Add(rc, tokenSource);
                    return rc;
                }
            }
        }
        private ActivityResultMessage GetResponse(int rc)
        {
            lock (_thisLock)
            {
                var result = _resultList[rc];
                _resultList.Remove(rc);
                return result;
            }
        }
        private async Task<ActivityResultMessage> StartActivityForResult(Intent intent)
        {
            var rc = RequestCode;
            try
            {
                ((Activity)Forms.Context).StartActivityForResult(intent, rc);
                await _waitList[rc];
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return GetResponse(rc);
        }

        public static double? RssiToSignalStrength(int rssi)
        {
            return Math.Log(100.0 + rssi) / 4.6;
        }
        public async Task<bool> CheckPowerState()
        {
            if (_adapter == null)
            {
                return false;
            }
            if (_adapter.State == State.On)
            {
                return true;
            }
            var enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
            var res = await StartActivityForResult(enableBtIntent);
            return res.ResultCode == Result.Ok;
        }
        public async Task<bool> PairDevice(BluetoothDevice dev)
        {
            if (!await CheckPowerState()) return false;
            var intent = new Intent(BluetoothDevice.ActionPairingRequest);
            intent.PutExtra(BluetoothDevice.ExtraDevice, dev);
            intent.PutExtra(BluetoothDevice.ExtraPairingVariant, BluetoothDevice.PairingVariantPin);
            intent.SetFlags(ActivityFlags.NewTask);

            var br = new WaitOnBroadcastReceiver<bool>();
            var filter = new IntentFilter(BluetoothDevice.ActionBondStateChanged);
            br.ReceiveEvent += (sender, e) =>
            {
                var action = intent.Action;
                // When discovery finds a device
                if (!BluetoothDevice.ActionBondStateChanged.Equals(action)) return;
                // Get the BluetoothDevice object from the Intent
                var device = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) as BluetoothDevice;
                if (device != null && dev.Address.Equals(device.Address))
                {
                    br.ReturnValue = device.BondState == Bond.Bonded;
                }
            };
            Context.RegisterReceiver(br, filter);

            var res = await StartActivityForResult(intent);

            if (res.ResultCode != Result.Ok)
                return false;
            return await br.Complete();
        }
        //public async Task<BluetoothDevice> UpdatedInfo(BluetoothDevice dev)
        //{
        //    if (!await CheckPowerState()) return dev;
        //    var intent = new Intent();
        //    intent.PutExtra(BluetoothDevice.ExtraDevice, dev);
        //    var newDev = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) as BluetoothDevice;
        //    var rssi = intent.GetShortExtra(BluetoothDevice.ExtraRssi, (short)0);
        //    Log.Info("rssi: " + rssi);
        //    return dev;
        //}
        public async Task<ICollection<BluetoothDevice>> GetBondedDevices()
        {
            if (await CheckPowerState())
            {
                return _adapter.BondedDevices;
            }
            return new List<BluetoothDevice>();
        }
        public async Task<bool> StartScanDevice(string macAddress)
        {
            if (await CheckPowerState())
            {
                IsScanning = true;
                var filters = new List<ScanFilter> {new ScanFilter.Builder().SetDeviceAddress(macAddress).Build()};
                var settings = new ScanSettings.Builder()
                    .SetCallbackType(ScanCallbackType.AllMatches)
                    .SetScanMode(ScanMode.LowLatency)
                    .SetMatchMode(BluetoothScanMatchMode.Aggressive)
                    .Build();
                _adapter.BluetoothLeScanner.StartScan(filters, settings, ScanCallback);
                return true;
            }
            IsScanning = false;
            return false;
        }
        public async Task<bool> StartScanDevices()
        {
            if (await CheckPowerState())
            {
                IsScanning = true;
                _adapter.BluetoothLeScanner.StartScan(ScanCallback);
                return true;
            }
            IsScanning = false;
            return false;
        }
        public async Task<bool> StopScanDevices()
        {
            IsScanning = false;
            if (!await CheckPowerState()) return false;
            _adapter.BluetoothLeScanner.StopScan(ScanCallback);
            return true;
        }
        public async Task<BluetoothDevice> GetDevice(string mac)
        {
            if (await CheckPowerState())
                return _adapter.GetRemoteDevice(mac);
            return null;
        }
        public GattPair ConnectGatt(BluetoothDevice device)
        {
            var cb = new WaitOnBluetoothGattCallback();
            var gatt = device.ConnectGatt(Context, true, cb);
            return new GattPair(gatt, cb);
        }
    }
}