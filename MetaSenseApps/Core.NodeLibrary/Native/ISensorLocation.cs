using System;
using System.Threading.Tasks;

namespace NodeLibrary.Native
{
    public interface ISensorLocation
    {
        //int Interval { get; set; }
        bool PowerEfficient { get; set; }
        bool RecordPath { get; set; }
        bool Connected { get; }

        LocationInfo CurrentLocation { get; }
        LocationPath Last24Hours { get; }
        event EventHandler<LocationInfo> LocationUpdate;
        event EventHandler<LocationPath.PathElement> PathUpdate;
        LocationInfo RequestLocation();
        Task<bool> WaitForConnected();
        Task<bool> StartWhenConnected();
        Task<bool> Start();
        Task<bool> Stop();
    }
}