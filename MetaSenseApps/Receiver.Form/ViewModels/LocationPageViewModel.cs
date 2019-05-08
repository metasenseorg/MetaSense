using System;
using System.Linq;
using NodeLibrary;
using Prism.Commands;
using Prism.Navigation;
using SQLitePCL;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Receiver.ViewModels
{
    public class LocationPageViewModel : NodeAwareViewModel
    {
        public LocationPageViewModel(/*IConversionFunctions func,*/ IAppProperties appProperties, INavigationService navigationService) : base(
            appProperties, navigationService)
        {
            //_func = func;
        }

        private bool _running;
        public Map MapRef { get; set; }
        private double? _latitude;
        public double? Latitude
        {
            get => _latitude;
            set => SetProperty(ref _latitude, value);
        }
        private double? _longitude;
        public double? Longitude
        {
            get => _longitude;
            set => SetProperty(ref _longitude, value);
        }
        private double? _altitude;
        public double? Altitude
        {
            get => _altitude;
            set => SetProperty(ref _altitude, value);
        }
        private double? _speed;
        public double? Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }
        private double? _direction;
        //private IConversionFunctions _func;
        private double _humiditySensorTemperatureCelsius;
        private double _humiditySensorHumidityPercent;
        private double _barometricSensorTemperatureCelsius;
        private double _barometricSensorPressureBar;
        private double _no2Ppb;
        private double _o3Ppb;
        private double _cOppm;
        private DateTime _lastTimestampReceived;

        public double? Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }

        public DelegateCommand Start => new DelegateCommand(() =>
        {
            _running = true;
        });
        public DelegateCommand Stop => new DelegateCommand(() =>
        {
            _running = false;
            Latitude = null;
            Longitude = null;
            Altitude = null;
            Speed = null;
            Direction = null;
        });

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            _running = true;
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            _running = false;
        }

        public DelegateCommand Plot24H => new DelegateCommand(() =>
        {
            var path = new LocationPath();
            _running = false;
            var queryTime = MetaSenseNode.DateTimeToUnix(DateTime.Now)- TimeSpan.FromHours(24).TotalSeconds;
            var reads = SettingsData.Default.Readings().Where(read => read.Ts > queryTime).OrderBy(read => read.Ts);


            foreach (var read in reads)
            {
                if (read.Latitude.HasValue && read.Longitude.HasValue)
                    path.Add(new LocationInfo(
                        read.Latitude.Value, 
                        read.Longitude.Value, 
                        read.Accuracy,
                        read.Altitude,
                        read.Speed, 
                        read.Bearing, 
                        MetaSenseNode.UnixToDateTime(read.Ts)));

            }
            foreach (var pathElement in path.Members)
            {
                var label="";
                var pos = default(Position);
                if (pathElement is LocationPath.PathArea)
                {
                    var area = pathElement as LocationPath.PathArea;
                    pos = new Position(area.Center.Latitude, area.Center.Longitude);
                    label = $"{(area.Enter + (area.Exit - area.Enter)).ToLocalTime():MM/dd/yyyy HH:mm}";

                } else if (pathElement is LocationPath.PathSegment)
                {
                    var segment = pathElement as LocationPath.PathSegment;
                    pos = new Position(segment.Start.Latitude+(segment.End.Latitude-segment.Start.Latitude),
                                segment.Start.Longitude + (segment.End.Longitude - segment.Start.Longitude));
                    label = $"{(segment.Enter+(segment.Exit - segment.Enter)).ToLocalTime():MM/dd/yyyy HH:mm}";
                }
                if (label != "")
                    MapRef.Pins.Add(new Pin
                    {
                        Type = PinType.Generic,
                        Position = pos,
                        Label = label
                    });
            }
        });
        private void CenterMap(LocationInfo info, double meters)
        {
            MapRef?.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(info.Latitude, info.Longitude),
                new Distance(meters)));
        }
        private void AddPin(GasReading concentrations, MetaSenseMessage msg)
        {
            var info = msg.Loc;
            if (MapRef == null) return;
            //if (MapRef.Pins.Count > 15)
            //{
            //    var lst = MapRef.Pins.ToList();
            //    lst.RemoveAt(0);
            //    MapRef.Pins.Clear();
            //    foreach (var pin in lst)
            //    {
            //        MapRef.Pins.Add(pin);
            //    }
            //}
            MapRef.Pins.Clear();
            MapRef.Pins.Add(new Pin
            {
                Type = PinType.Generic,
                Position = new Position(info.Latitude, info.Longitude),
                Label = "MetaSense",
                Address = DescMessage(concentrations, msg)
            });
        }
        private string DescMessage(GasReading concentrations, MetaSenseMessage msg)
        {
            return $"Data Received at {DateTime.Now}";
            //return
            //    $"CO {concentrations.COppm}ppm\r\n" +
            //    $"O3 {concentrations.O3ppb}ppb\r\n" +
            //    $"NO2 {concentrations.NO2ppb}ppb\r\n" +
            //    $"atmosferic pressure {msg.HuPr.BarometricSensorPressureMilliBar}bar\r\n" +
            //    $"humidity {msg.HuPr.HumiditySensorHumidityPercent}%\r\n" +
            //    $"tempreature {msg.HuPr.HumiditySensorTemperatureCelsius}C";
        }
        protected override void ProcessLastRead()
        {
            if (!_running) return;
            var read = LastRead;
            var info = LastRead.Loc;
            LastTimestampReceived = App.LastMessageReceivedAt ?? default(DateTime);
            Latitude = info.Latitude;
            Longitude = info.Longitude;
            Altitude = info.Altitude;
            Speed = info.Speed;
            Direction = info.Direction;
            GasReading concentrations;
            if (App.Conversion != null)
                concentrations = App.Conversion.Convert(read);
            else
                concentrations = null;

            Device.BeginInvokeOnMainThread(() =>
            {
                COppm = concentrations?.COppm ?? 0;
                O3ppb = concentrations?.O3ppb ?? 0;
                NO2ppb = concentrations?.NO2ppb ?? 0;

                BarometricSensorPressureBar = read.HuPr.BarometricSensorPressureMilliBar;
                BarometricSensorTemperatureCelsius = read.HuPr.BarometricSensorTemperatureCelsius;
                HumiditySensorHumidityPercent = read.HuPr.HumiditySensorHumidityPercent;
                HumiditySensorTemperatureCelsius = read.HuPr.HumiditySensorTemperatureCelsius;

                if (concentrations!=null)
                    AddPin(concentrations, read);

                var mapsize = MapRef.VisibleRegion;
                if (mapsize != null)
                {
                    var dist = LocationInfo.DistanceInMeters(
                        mapsize.Center.Latitude,
                        mapsize.Center.Longitude,
                        info.Latitude,
                        info.Longitude);
                    if (dist > mapsize.Radius.Meters)
                        CenterMap(info, mapsize.Radius.Meters);
                }
            });
        }

        public double HumiditySensorTemperatureCelsius
        {
            get => _humiditySensorTemperatureCelsius;
            set => SetProperty(ref _humiditySensorTemperatureCelsius, value);
        }

        public double HumiditySensorHumidityPercent
        {
            get => _humiditySensorHumidityPercent;
            set => SetProperty(ref _humiditySensorHumidityPercent, value);
        }

        public double BarometricSensorTemperatureCelsius
        {
            get => _barometricSensorTemperatureCelsius;
            set => SetProperty(ref _barometricSensorTemperatureCelsius, value);
        }

        public double BarometricSensorPressureBar
        {
            get => _barometricSensorPressureBar;
            set => SetProperty(ref _barometricSensorPressureBar, value);
        }

        public double NO2ppb
        {
            get => _no2Ppb;
            set => SetProperty(ref _no2Ppb, value);
        }

        public double O3ppb
        {
            get => _o3Ppb;
            set => SetProperty(ref _o3Ppb, value);
        }

        public double COppm
        {
            get => _cOppm;
            set => SetProperty(ref _cOppm, value);
        }

        public DateTime LastTimestampReceived
        {
            get => _lastTimestampReceived;
            set => SetProperty(ref _lastTimestampReceived, value);
        }
    }
}
