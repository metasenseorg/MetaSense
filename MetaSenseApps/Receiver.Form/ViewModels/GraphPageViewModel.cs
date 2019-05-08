using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NodeLibrary;
using Prism.Commands;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;
using Prism.Navigation;

namespace Receiver.ViewModels
{
    public sealed class GraphPageViewModel : NodeAwareViewModel {
        public ObservableCollection<ChartDataPoint> GraphValues { get; private set; }
        private bool _timer;
        private async void StartTimer()
        {
            _timer = true;
            while(_timer)
            {
                await Task.Delay(30000);
                RefreshGraph();
            }
        }
        private void StopTimer()
        {
            _timer = false;
        }
        public GraphPageViewModel(/*IConversionFunctions conversion,*/ IAppProperties appProperties, INavigationService navigationService) : base(appProperties, navigationService)
        {
            //_conversion = conversion;
            GraphValues = new ObservableCollection<ChartDataPoint>();

            HoursToShowScaled = 2;

            SetNo2W = new DelegateCommand<VisualElement>(ve => { ComputeGraph("Volt", "NO2 Work", v => MetaSenseConverters.ConvertGas(v.GetGas()).No2W); });
            SetNo2A = new DelegateCommand<VisualElement>(ve => { ComputeGraph("Volt", "NO2 Auxiliary", v => MetaSenseConverters.ConvertGas(v.GetGas()).No2A); });

            SetCoW = new DelegateCommand<VisualElement>(ve => { ComputeGraph("Volt", "CO Work", v => MetaSenseConverters.ConvertGas(v.GetGas()).CoW); });
            SetCoA = new DelegateCommand<VisualElement>(ve => { ComputeGraph("Volt", "CO Auxiliary", v => MetaSenseConverters.ConvertGas(v.GetGas()).CoA); });

            SetOxW = new DelegateCommand<VisualElement>(ve => { ComputeGraph("Volt", "Ozone Work", v => MetaSenseConverters.ConvertGas(v.GetGas()).OxW); });
            SetOxA = new DelegateCommand<VisualElement>(ve => { ComputeGraph("Volt", "Ozone Auxiliary", v => MetaSenseConverters.ConvertGas(v.GetGas()).OxA); });

            SetHuh = new DelegateCommand<VisualElement>(ve => { ComputeGraph("Percent", "Humidity", v => MetaSenseConverters.ConvertHuPr(v.GetHuPr()).HumPercent*100); });
            SetBar = new DelegateCommand<VisualElement>(ve => { ComputeGraph("Bar", "Pressure", v => MetaSenseConverters.ConvertHuPr(v.GetHuPr()).PresMilliBar); });
            SetTemp = new DelegateCommand<VisualElement>(ve => { ComputeGraph("Celsius", "Temperature", v => MetaSenseConverters.ConvertHuPr(v.GetHuPr()).HumCelsius); });

            Func<Read, GasReading> ppms = (v) =>
            {
                var gasConcentration = App.Conversion?.Convert(v);//gas.CoA * 1000, gas.CoW * 1000, gas.OxA * 1000, gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hp.HumPercent, hp.PresMilliBar * 1000, hp.HumCelsius);
                return gasConcentration;
            };

            SetConcentrationCO = new DelegateCommand<VisualElement>(ve => { ComputeGraph("ppm", "CO", v =>
            {
                var hp = MetaSenseConverters.ConvertHuPr(v.GetHuPr());
                var gas = MetaSenseConverters.ConvertGas(v.GetGas());
                return App.Conversion?.COppm(gas.CoA * 1000, gas.CoW * 1000, gas.OxA * 1000, gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hp.HumPercent, hp.PresMilliBar, hp.HumCelsius) ?? 0;
            }); });
            SetConcentrationO3 = new DelegateCommand<VisualElement>(ve => { ComputeGraph("ppb", "O3", v => {
                var hp = MetaSenseConverters.ConvertHuPr(v.GetHuPr());
                var gas = MetaSenseConverters.ConvertGas(v.GetGas());
                return App.Conversion?.O3ppb(gas.CoA * 1000, gas.CoW * 1000, gas.OxA * 1000, gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hp.HumPercent, hp.PresMilliBar, hp.HumCelsius) ?? 0;
            }); });
            SetConcentrationNO2 = new DelegateCommand<VisualElement>(ve => { ComputeGraph("ppb", "NO2", v => {
                var hp = MetaSenseConverters.ConvertHuPr(v.GetHuPr());
                var gas = MetaSenseConverters.ConvertGas(v.GetGas());
                return App.Conversion?.NO2ppb(gas.CoA * 1000, gas.CoW * 1000, gas.OxA * 1000, gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hp.HumPercent, hp.PresMilliBar, hp.HumCelsius) ?? 0;
            }); });


            //SetNo2W.Execute(null);
            SetConcentrationNO2.Execute(null);
        }
        ~GraphPageViewModel()
        {
            StopTimer();
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            StartTimer();
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            StopTimer();
        }
        //protected async override Task RefreshStatus() { }

        public DelegateCommand<VisualElement> SetNo2W { get; }
        public DelegateCommand<VisualElement> SetNo2A { get; }
        public DelegateCommand<VisualElement> SetCoW { get; }
        public DelegateCommand<VisualElement> SetCoA { get; }
        public DelegateCommand<VisualElement> SetOxW { get; }
        public DelegateCommand<VisualElement> SetOxA { get; }

        public DelegateCommand<VisualElement> SetHuh { get; }
        public DelegateCommand<VisualElement> SetTemp { get; }
        public DelegateCommand<VisualElement> SetBar { get; }

        public DelegateCommand<VisualElement> SetConcentrationNO2 { get; }
        public DelegateCommand<VisualElement> SetConcentrationO3 { get; }
        public DelegateCommand<VisualElement> SetConcentrationCO { get; }


        private void ComputeGraph(string axis, string measure, Func<Read, double> selector)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _currentSelector = selector;
                AxisName = axis;
                MeasureName = measure;
                RefreshGraph();
            });            
        }
        
        private int _hoursToShow;
        public int HoursToShow
        {
            get => _hoursToShow;
            set => SetProperty(ref _hoursToShow, value, RefreshGraph);
        }

        private int _hoursToShowScaled;
        public int HoursToShowScaled
        {
            get => _hoursToShowScaled;
            set => SetProperty(ref _hoursToShowScaled, value, () => HoursToShow = value*6);
        }

        private string _axisName;
        public string AxisName
        {
            get => _axisName;
            set => SetProperty(ref _axisName, value);
        }
        private string _measureName;
        public string MeasureName
        {
            get => _measureName;
            set => SetProperty(ref _measureName, value);
        }
        private Func<Read, double> _currentSelector;
        //private IConversionFunctions _conversion;

        private object _running = false;
        private void RefreshGraph()
        {
            lock (_running)
            {
                if ((bool) _running)
                    return;
                _running = true;
            }
            Task.Run(() =>
            {
                try
                {
                    if (AxisName != null && MeasureName != null && _currentSelector != null)
                        UpdateGraphPoints();
                }
                finally
                {
                    _running = false;
                }
            });
        }
        private void UpdateGraphPoints()
        {
            var values = SettingsData.Default.LastHours(HoursToShow, _currentSelector).ToList();
            var minutesInterval = (60 * HoursToShow) / 150.0;
            var start = DateTime.Now - TimeSpan.FromHours(HoursToShow);

            //var minGroups = from read in vals group selector(read) by read.Ts / 60 * 60;
            //return from gr in minGroups let min = MetaSenseNode.UnixToDateTime(gr.Key) let val = gr.Average() select new Tuple<DateTime, double>(min, val);
            
            var groups = values.GroupBy(v => Math.Truncate((v.Item1 - start).TotalMinutes / minutesInterval),
                v => v.Item2);

            var results = from g in groups
                select new ChartDataPoint(
                    start + TimeSpan.FromMinutes(minutesInterval * g.Key + minutesInterval / 2),
                    g.Average());

            var ordered = from chartDataPoint in results orderby chartDataPoint.XValue select chartDataPoint;
            //var elems = ordered.Count();
            Device.BeginInvokeOnMainThread(() =>
            {
                GraphValues.Clear();
                foreach (var val in ordered)
                {
                    GraphValues.Add(val);
                }
            });
        }
    }
}
