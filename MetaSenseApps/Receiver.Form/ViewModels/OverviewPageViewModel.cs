 using System;
using System.Collections.Generic;
using NodeLibrary;
using NodeLibrary.Native;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;

namespace Receiver.ViewModels
{
    public class OverviewPageViewModel : NodeAwareViewModel {

        private DateTime _lastTimestampReceived;

        private readonly ISettingsData _settings;
        private readonly IBLEBackgroundReader _backgroundReader;
        // DM: add variable
        private bool connectivityButtonEnabled;
        // DM: end

        private static Dictionary<string, Color> AQIColorMap;
        static OverviewPageViewModel(){
            AQIColorMap = new Dictionary<string, Color>(6);
            AQIColorMap.Add("Good", Color.GreenYellow);
            AQIColorMap.Add("Moderate", Color.Yellow);
            AQIColorMap.Add("Unhealthy for Sensitive Groups", Color.DarkOrange);
            AQIColorMap.Add("Unhealthy", Color.Red);
            AQIColorMap.Add("Very Unhealthy", Color.DarkViolet);
            AQIColorMap.Add("Hazardous", Color.MediumVioletRed);
        }
        public bool CanRefreshStatus => App.Node != null;

        protected override void OnNodeSelectedChanged(object sender, MetaSenseNodeViewModel metaSenseNodeViewModel)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(NodeViewModel));
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(ConnectButtonName));
            ForgetSensorButton.RaiseCanExecuteChanged();
            ConnectButton.RaiseCanExecuteChanged();

            var serviceRunning = _settings.Get("service.running");
            if ("running".Equals(serviceRunning))
            {
                _backgroundReader.StopBackgroundBLEService();
            }
        }
        public MetaSenseNodeViewModel NodeViewModel => App.NodeViewModel;
        public string ConnectButtonName => App.Connected ? "Disconnect" : "Connect";
        
        public OverviewPageViewModel(
            IBLEBackgroundReader backgroundReader,
            ISettingsData settings, 
            IAppProperties appProperties, 
            INavigationService navigationService) : base(appProperties, navigationService)
        {
            //_saveToFile = saveToFile;
            //_dialogService = dialogService;
            _backgroundReader = backgroundReader;
            _settings = settings;

            // DM: add initialization and subscribe
            connectivityButtonEnabled = true;

            // control whether the connectivity button is enabled or disabled
            MessagingCenter.Subscribe<MetaSenseNode, bool>(this, "MetaSenseConnectivityButton", (node, enabled) =>
            {
                connectivityButtonEnabled = enabled;
                ConnectButton.RaiseCanExecuteChanged();
            });
            // DM: end

            // DM: rewrite to include locking ability
            /*
            ConnectButton = new DelegateCommand(() =>
            {
                if (App.Connected)
                    App.DiconnectSelectedNode();
                else
                    App.ConnectSelectedNode();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(ConnectButtonName));
            }, ()=> NodeViewModel!=null);
            */

            ConnectButton = new DelegateCommand(() =>
            {
                // DM: disable connect button upon press
                connectivityButtonEnabled = false;
                ConnectButton.RaiseCanExecuteChanged();
                // DM: end
                if (App.Connected)
                    App.DiconnectSelectedNode();
                else
                    App.ConnectSelectedNode();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(ConnectButtonName));
            }, () => (NodeViewModel != null && connectivityButtonEnabled));
            // DM: end

            SensorSelectorButton = new DelegateCommand(() => NavigationService.NavigateAsync("BluetoothPage"));
            // ReSharper disable once ExplicitCallerInfoArgument
            ForgetSensorButton = new DelegateCommand(() => { App.NodeViewModel = null; RaisePropertyChanged(nameof(NodeViewModel)); }, ()=> NodeViewModel!=null);
        }

        ~OverviewPageViewModel()
        {
            try
            {
                // DM: add unsubscribe
                MessagingCenter.Unsubscribe<MetaSenseNode>(this, "MetaSenseConnectivityButton");
                // DM: end
                if (App.Node == null) return;
                _backgroundReader.StopBackgroundBLEService();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private double _aqi;
        public double AQI
        {
            get => _aqi;
            set => SetProperty(ref _aqi, value);
        }
        private string _aqiContributor;
        public string AQIContributor
        {
            get => _aqiContributor;
            set => SetProperty(ref _aqiContributor, value);
        }
        private string _aqiCategory;
        public string AQICategory
        {
            get => _aqiCategory;
            set => SetProperty(ref _aqiCategory, value);
        }
        public string AirQualityDescription => AQIContributor!=null ? String.Format("The current air quality index is {0} due to the conentration of {1}.\n", AQI, AQIContributor ) : "";
        public Color AQIColor => AQICategory!=null && AQIColorMap.ContainsKey(AQICategory) ? AQIColorMap[AQICategory] : Color.Default;
        private double _coPpm;
        public double COppm
        {
            get => _coPpm;
            set => SetProperty(ref _coPpm, value);
        }
        private double _o3Ppb;
        public double O3ppb
        {
            get => _o3Ppb;
            set => SetProperty(ref _o3Ppb, value);
        }
        private double _no2Ppb;
        public double NO2ppb
        {
            get => _no2Ppb;
            set => SetProperty(ref _no2Ppb, value);
        }
        protected override void ProcessLastRead()
        {
            LastTimestampReceived = App.LastTimestampReceived ?? default(DateTime);
            if (App.Conversion != null)
            {
                var gasConcentration = App.Conversion.Convert(LastRead);
                //gas.CoA*1000, gas.CoW * 1000, gas.OxA * 1000, gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hupr.HumPercent, hupr.PresMilliBar*1000, hupr.HumCelsius);
                COppm = gasConcentration.COppm;
                O3ppb = gasConcentration.O3ppb;
                NO2ppb = gasConcentration.NO2ppb;
                var aqiTuple = AQICalculator.CalculateAQI(gasConcentration);
                AQI = aqiTuple.Item1;
                AQIContributor = aqiTuple.Item2;
                AQICategory = AQICalculator.AQICategory(aqiTuple.Item1);
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(AQIColor));
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(AirQualityDescription));
            }
        }
        
        public DateTime LastTimestampReceived
        {
            get => _lastTimestampReceived;
            private set => SetProperty(ref _lastTimestampReceived, value, ProcessLastRead);
        }

        #region Commands
        public DelegateCommand ForgetSensorButton { get; protected set; }
        public DelegateCommand SensorSelectorButton { get; protected set; }
        public DelegateCommand ConnectButton { get; protected set; }
        #endregion
    }
}
