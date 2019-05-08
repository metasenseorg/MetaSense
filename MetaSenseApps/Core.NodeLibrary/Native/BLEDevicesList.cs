using System.Collections.Generic;
using System.Linq;

namespace NodeLibrary.Native
{
    public abstract class BLEDevicesList : IBLEDevicesList
    {
        public event BLEDeviceUpdatedEventHandler AddDevice;
        public event BLEDeviceUpdatedEventHandler RemoveDevice;
        public event BLEDeviceUpdatedEventHandler UpdateDevice;

        protected void OnDeviceAdd(NodeInfo device)
        {
            AddDevice?.Invoke(device);
        }
        protected void OnDeviceRemove(NodeInfo device)
        {
            RemoveDevice?.Invoke(device);
        }
        protected void OnDeviceUpdate(NodeInfo device)
        {
            UpdateDevice?.Invoke(device);
        }

        public List<NodeInfo> Devices
        {
            get
            {
                lock (DevicesDictionary)
                {
                    return DevicesDictionary.Values.ToList();
                }
            }
        }
        public List<NodeInfo> PairedDevices
        {
            get
            {
                lock (DevicesDictionary)
                {
                    return DevicesDictionary.Values.Where(i => i.Paired.HasValue && i.Paired.Value).ToList();
                }
            }
        }

        protected Dictionary<string, NodeInfo> DevicesDictionary { get; } = new Dictionary<string, NodeInfo>();
        protected void ClearList()
        {
            lock (DevicesDictionary)
            {
                foreach (var mac in DevicesDictionary.Keys.ToArray())
                {
                    var dev = DevicesDictionary[mac];
                    DevicesDictionary.Remove(mac);
                    OnDeviceRemove(dev);
                }
            }
        }

        public abstract void StartScanDevices(int seconds);
        public abstract void StopScanDevices();    
    }
}
