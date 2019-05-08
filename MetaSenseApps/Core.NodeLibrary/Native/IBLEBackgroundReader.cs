using System;

namespace NodeLibrary.Native
{
    public interface IBLEBackgroundReader
    {
        event EventHandler<MetaSenseMessage> BackgroundBLEServiceMessageReceived;
        void StartBackgroundBLEService(string mac);
        void StopBackgroundBLEService();
    }
}