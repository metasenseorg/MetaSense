using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Core.Droid;
using Core.Droid.Platform;
using NodeLibrary;
using NodeLibrary.Native;
using Xamarin.Forms;
using ILocationListener = Android.Gms.Location.ILocationListener;
using Object = Java.Lang.Object;

[assembly: Dependency(typeof(SensorLocationDroid))]
namespace Core.Droid
{
    public sealed class SensorLocationDroid : Object, ISensorLocation, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, ILocationListener
    {
        private readonly Context _context;
        private GoogleApiClient _mGoogleApiClient;

        private readonly object _locationLock = new object();
        private Location _mLastLocation;
        
        private Location _mLastPathLocation;
        private bool _mLastPathHovering=true;
        private LocationPath _mLocationPath;
        private readonly WaitOnEvent<bool> _connectedWaitOnEvent;

        public bool Running { get; private set; }
        public bool Connected { get; private set; }
        public async Task<bool> WaitForConnected()
        {
            if (Connected) return true;
            await _connectedWaitOnEvent.ReceiveEventAsync();
            return Connected;
        }
        public async Task<bool> StartWhenConnected()
        {
            await WaitForConnected();
            return await Start();
        }
        public LocationInfo RequestLocation()
        {
            if (!Connected) return null;
            var res = LocationServices.FusedLocationApi.GetLastLocation(_mGoogleApiClient);
            return ToLocationInfo(res);
        }

        public async Task<bool> Start()
        {
            var events = new WaitOnEvent<bool>(); 
            Device.BeginInvokeOnMainThread(async () => {
                if (!Connected)
                {
                    events.OnEvent(false);
                    return;
                }
                var mLocationRequest = PowerEfficient ? LocationRequestPowerEfficent : LocationRequestHighAccuracy;
                if (!await CheckLocationSettings(mLocationRequest))
                {
                    events.OnEvent(false);
                    return;
                }
                Running = await RequestUpdates(mLocationRequest);
                events.OnEvent(Running);
            });
            return await events.ReceiveEventAsync();
        }
        public async Task<bool> Stop()
        {
            if (!Connected) return true;
            if (!Running) return true;
            Running = await StopUpdates();
            return Running;
        }

        private async Task<bool> CheckLocationSettings(LocationRequest mLocationRequest)
        {
            var builder = new LocationSettingsRequest.Builder().AddLocationRequest(mLocationRequest);
            var result = await LocationServices.SettingsApi.CheckLocationSettingsAsync(_mGoogleApiClient, builder.Build());

            switch (result.Status.StatusCode)
            {
                case CommonStatusCodes.Success:
                    return true;
                case CommonStatusCodes.ResolutionRequired:
                    //Can do this is the main activity is avialable
                    var act = Activity;
                    if (act != null)
                    {
                        var wait = new WaitOnActivity();
                        result.Status.StartResolutionForResult(act, wait.RequestId);
                        var resResult = await wait.CompleteActivity();
                        return resResult.ResultCode == Result.Ok;
                    }
                    return false;
            }
            return false;
        }
        private async Task<bool> RequestUpdates(LocationRequest mLocationRequest)
        {
            var res = await LocationServices.FusedLocationApi.RequestLocationUpdatesAsync(_mGoogleApiClient, mLocationRequest, this);
            return res.IsSuccess;
        }
        private async Task<bool> StopUpdates()
        {
            var res = await LocationServices.FusedLocationApi.RemoveLocationUpdatesAsync(_mGoogleApiClient, this);
            return res.IsSuccess;
        }

        private static LocationRequest LocationRequestHighAccuracy
        {
            get
            {
                var mLocationRequest = new LocationRequest()
                    .SetInterval(500)
                    .SetFastestInterval(100)
                    .SetPriority(LocationRequest.PriorityHighAccuracy);
                return mLocationRequest;
            }
        }
        private static LocationRequest LocationRequestPowerEfficent
        {
            get
            {
                var mLocationRequest = new LocationRequest()
                    .SetInterval(1000)
                    .SetFastestInterval(1000)
                    .SetPriority(LocationRequest.PriorityBalancedPowerAccuracy);
                return mLocationRequest;
            }
        }

        private LocationPath.PathSegment Segment(Location location)
        {
            var pathSegment = new LocationPath.PathSegment(
                ToLocationInfo(_mLastLocation),
                ToLocationInfo(location),
                MillisecondsToDateTime(_mLastLocation.Time),
                MillisecondsToDateTime(location.Time));
            return pathSegment;
        }


        public event EventHandler<LocationInfo> LocationUpdate;
        public event EventHandler<LocationPath.PathElement> PathUpdate;
        public void OnLocationChanged(Location location)
        {
            LocationPath.PathElement element=null;
            lock (_locationLock)
            {
                if (RecordPath && _mLastPathLocation!=null)
                {
                    var dist = location.DistanceTo(_mLastPathLocation);
                    if (_mLastPathHovering)
                    {
                        if (dist > 50) //leaving hovering in a 100 m diameter 
                        {
                            var pathArea = new LocationPath.PathArea(
                                ToLocationInfo(_mLastPathLocation), 
                                50,
                                MillisecondsToDateTime(_mLastPathLocation.Time),
                                MillisecondsToDateTime(location.Time));
                            _mLocationPath.Members.Add(pathArea);
                            element = pathArea;
                            var pathSegment = Segment(location);
                            _mLocationPath.Members.Add(pathSegment);
                            _mLastPathHovering = false;
                            _mLastPathLocation = location;
                        }
                    }
                    else
                    {
                        var pathSegment = Segment(location);
                        _mLocationPath.Members.Add(pathSegment);
                        element = pathSegment;
                        if (
                            (dist < 1) ||
                            (dist < 5 && (location.Time - _mLastPathLocation.Time) > 5000) ||
                            (dist < 15 && (location.Time - _mLastPathLocation.Time) > 15000)
                            )
                            _mLastPathHovering = true;

                        if (!_mLastPathHovering)
                            _mLastPathLocation = location;
                    }
                }
                else
                {
                    _mLastPathLocation = location;
                }
                _mLastLocation = location;
            }
            LocationUpdate?.Invoke(this,ToLocationInfo(_mLastLocation));
            if(element!=null)
                PathUpdate?.Invoke(this, element);
        }

        public void OnConnected(Bundle connectionHint)
        {
            Connected = true;
            _connectedWaitOnEvent.OnEvent(true);
            if (_mLastLocation != null) return;
            lock (_locationLock)
            {
                _mLastLocation = LocationServices.FusedLocationApi.GetLastLocation(_mGoogleApiClient);
                _mLastPathLocation = _mLastLocation;
            }
        }
        public void OnConnectionSuspended(int cause)
        {
            Connected = false;
            _connectedWaitOnEvent.OnEvent(false);
        }
        public void OnConnectionFailed(ConnectionResult result)
        {
            Connected = false;
            _connectedWaitOnEvent.OnEvent(false);
            Log.Debug($"OnConnectionFailed({result.ErrorMessage})");
        }

        private static LocationInfo ToLocationInfo(Location location)
        {
            return new LocationInfo(location.Latitude, location.Longitude, location.Accuracy, location.Altitude, location.Speed, location.Bearing, MillisecondsToDateTime(location.Time));
        }
        // ReSharper disable once InconsistentNaming
        private static DateTime UTC_Jan_1_1970 = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
        private static DateTime MillisecondsToDateTime(long ts)
        {
            return UTC_Jan_1_1970.AddMilliseconds(ts);
        }

        public bool PowerEfficient
        {
            get { return _powerEfficient; }
            set
            {
                if (_powerEfficient == value) return;
                _powerEfficient = value;
                if (!Running) return;
                Restart();
            }
        }
        private async void Restart()
        {
            await Stop();
            await Start();
        }
        public bool RecordPath { get; set; }

        public LocationPath Last24Hours => _mLocationPath;
        public LocationInfo CurrentLocation => ToLocationInfo(_mLastLocation);

        private async void InitLocation()
        {
            PowerEfficient = true;
            if (!HasLocationPermission)
            {
                await RequestPermission();
            }
            // Create an instance of GoogleAPIClient.
            if (_mGoogleApiClient == null)
            {
                _mGoogleApiClient = new GoogleApiClient.Builder(_context)
                  .AddConnectionCallbacks(this)
                  .AddOnConnectionFailedListener(this)
                  .AddApi(LocationServices.API)
                  .Build();
            }
            _mGoogleApiClient.Connect();
        }

        private bool HasLocationPermission => 
            Permission.Granted.Equals(ContextCompat.CheckSelfPermission(_context,
            Manifest.Permission.AccessFineLocation));
        private async Task<bool> RequestPermission()
        {
            // We can do this only if the main activity is available to ask user for permission
            var act = Activity;
            if (act == null) return false;

            var wait = new WaitOnActivity();
            ActivityCompat.RequestPermissions(act,
                new[] { Manifest.Permission.AccessFineLocation },
                wait.RequestId);
            await wait.CompleteActivity();
            return HasLocationPermission;
        }
        private Activity Activity
        {
            get
            {
                var act = _context as Activity ?? Forms.Context as Activity;
                return act;
            }
        }

        #region Class Creation
        public SensorLocationDroid() : this(Forms.Context) { }
        public SensorLocationDroid(Context context)
        {
            _context = context;
            _connectedWaitOnEvent = new WaitOnEvent<bool>();
            InitLocation();
        }


        #endregion

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls
        private bool _powerEfficient;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                _mGoogleApiClient?.Disconnect();
                // TODO: set large fields to null.
                _mLastLocation = null;
                _mLocationPath = null;

                _disposedValue = true;
            }
        }
        ~SensorLocationDroid()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
        // This code added to correctly implement the disposable pattern.
        public new void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

    }
}