using System.Collections.Generic;

namespace NodeLibrary.Native
{
    public delegate void BLEDeviceUpdatedEventHandler(NodeInfo device);
    public interface IBLEDevicesList
    {
        event BLEDeviceUpdatedEventHandler AddDevice;
        event BLEDeviceUpdatedEventHandler RemoveDevice;
        event BLEDeviceUpdatedEventHandler UpdateDevice;

        List<NodeInfo> Devices { get; }
        List<NodeInfo> PairedDevices { get; }

        void StartScanDevices(int seconds);
        void StopScanDevices();
    }
}