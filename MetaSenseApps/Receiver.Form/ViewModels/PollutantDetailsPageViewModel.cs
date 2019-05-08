using NodeLibrary;
using Prism.Navigation;
using Syncfusion.SfChart.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Receiver.ViewModels
{
    /// <summary>
    /// View model for the pollutant details page to display measurements from the last read and
    /// a graph of the hourly max AQIs for the current day.
    /// </summary>
	public class PollutantDetailsPageViewModel : NodeAwareViewModel
	{
        private const double UNKNOWN_AQI = -1;
        private const double CELSIUS_TO_FAHRENHEIT_MULTIPLIER = 1.8;
        private const int CELSIUS_TO_FAHRENHEIT_CONSTANT = 32;
        private const int HOURS_IN_DAY = 24;
        private const int GRAPH_X_AXIS_PADDING = 1;
        private const string UNKNOWN = "-";

        /// <summary>
        /// Initialize properties used to help graph, properties of measurements, and the graph.
        /// </summary>
        /// <param name="appProperties">The properties of the app</param>
        /// <param name="navigationService">The navigation service used to navigate between pages</param>
		public PollutantDetailsPageViewModel(IAppProperties appProperties, INavigationService navigationService) : base(appProperties, navigationService)
        {
            MaxAQI = 0;
            HourMaxAQIData = new ObservableCollection<ChartDataPoint>();
            TodayDateTime = DateTime.Today;
            TomorrowDateTime = TodayDateTime.AddHours(HOURS_IN_DAY + GRAPH_X_AXIS_PADDING);

            COppm = UNKNOWN;
            O3ppb = UNKNOWN;
            NO2ppb = UNKNOWN;
            Temperature = UNKNOWN;
            Humidity = UNKNOWN;
            Pressure = UNKNOWN;
            MaxAQIAndContributor = UNKNOWN;

            HighlightNone();
            InitializeGraphAsync();

            if (LastRead != null)
            {
                ProcessLastRead();
            }
		}

        /// <summary>
        /// Update the graph and properties binded to the view with the values from the last read.
        /// </summary>
        protected override void ProcessLastRead()
        {
            if (App.Conversion != null)
            {
                var gasConcentration = App.Conversion.Convert(LastRead);
                var pollutantAqiTuple = AQICalculator.CalculatePollutantAQIs(gasConcentration);

                // ignore the last read if an unknown AQI or negative AQI is calculated
                if (!LastReadIsValid(pollutantAqiTuple))
                {
                    return;
                }

                LastMessageReceivedAt = "last updated at " + DateTime.Now.ToString("t");
                var aqiTuple = AQICalculator.CalculateAQI(pollutantAqiTuple);
                var aqi = aqiTuple.Item1;
                var aqiContributor = aqiTuple.Item2;

                if (aqi > MaxAQI)
                {
                    MaxAQI = aqi;
                    MaxAQIAndContributor = String.Format("{0:F1} ({1})", aqi, aqiContributor);
                    HighlightMainContributor(aqiContributor);
                }

                COppm = gasConcentration.COppm.ToString("F2") + " PPM";
                O3ppb = gasConcentration.O3ppb.ToString("F2") + " PPB";
                NO2ppb = gasConcentration.NO2ppb.ToString("F2") + " PPB";

                var temperature = LastRead.HuPr.HumiditySensorTemperatureCelsius *
                    CELSIUS_TO_FAHRENHEIT_MULTIPLIER + CELSIUS_TO_FAHRENHEIT_CONSTANT;
                Temperature = temperature.ToString("F1") + " °F";
                Humidity = LastRead.HuPr.HumiditySensorHumidityPercent.ToString("F1") + "%";
                Pressure = LastRead.HuPr.BarometricSensorPressureMilliBar.ToString("F1") + " MBAR";

                TodayDateTime = DateTime.Today;
                TomorrowDateTime = TodayDateTime.AddHours(HOURS_IN_DAY + GRAPH_X_AXIS_PADDING);

                // clear the hour max AQI data if there are any reads that are dated before the current day
                ClearOldReads();

                if (aqi >= 0)
                {
                    UpdateGraph(aqi);
                }
            }
        }

        /// <summary>
        /// Check whether the last read has nonnegative values for NO2 (ppb), O3 (ppb), and
        /// CO(ppm).
        /// </summary>
        /// <param name="pollutantAqiTuple">A tuple containing the NO2 (ppb), O3 (ppb), and
        ///     CO (ppm) in that order</param>
        /// <returns>True if the last read has nonnegative values for NO2 (ppb), O3 (ppb), and
        ///     CO(ppm); false otherwise.</returns>
        private Boolean LastReadIsValid(Tuple<int, int, int> pollutantAqiTuple)
        {
            if (pollutantAqiTuple.Item1 < 0) return false;
            else if (pollutantAqiTuple.Item2 < 0) return false;
            else if (pollutantAqiTuple.Item3 < 0) return false;

            return true;
        }

        /// <summary>
        /// Clear any stored reads that relate to reads made before the current day.
        /// </summary>
        private void ClearOldReads()
        {
            // clear list of data points used for graph
            var oldReads = HourMaxAQIData.Where(hourMax => (hourMax.XValue.CompareTo(TodayDateTime) < 0));
            if (oldReads.Any())
            {
                HourMaxAQIData.Clear();
            }

            // clear database
            long todayUnixTime = (long)MetaSenseNode.DtToUnix(TodayDateTime);
            SettingsData.Default.DeleteNNConversionOutputs(todayUnixTime);
        }

        /// <summary>
        /// Set the data points of the graph to the hourly max AQIs.
        /// </summary>
        private async void InitializeGraphAsync()
        {
            HourMaxAQIData = await FindTopAQIsAsync();
        }

        /// <summary>
        /// Find the top AQIs of each hour of the current day.
        /// </summary>
        /// <returns>A Task<ObservableCollection<ChartDataPoint>> where the ObservableCollection
        ///     contains the top AQIs of each hour of the current day.</returns>
        private async Task<ObservableCollection<ChartDataPoint>> FindTopAQIsAsync()
        {
            // find datetime of the start of the current day
            var startOfToday = MetaSenseNode.DtToUnix(TodayDateTime);
            var startOfTomorrow = MetaSenseNode.DtToUnix(TodayDateTime.AddHours(HOURS_IN_DAY));

            // get reads from database
            var convOutputsToday = SettingsData.Default.ReadNNConversionOutputs().Where(c => c.Ts >= startOfToday);

            // partition reads into hours of the current day
            var startHourInterval = MetaSenseNode.DtToUnix(TodayDateTime);
            var currHourDateTime = TodayDateTime.AddHours(1);
            var endHourInterval = MetaSenseNode.DtToUnix(currHourDateTime);
            var maxAQI = UNKNOWN_AQI;
            var maxAQIContributor = "";
            var hourMaxCollection = new ObservableCollection<ChartDataPoint>();

            await Task.Run(() =>
            {
                while (endHourInterval <= startOfTomorrow)
                {
                    var hourConvOutputs = convOutputsToday.Where(c => (c.Ts >= startHourInterval && c.Ts < endHourInterval));
                    var hourMaxAQI = -1;

                    // find the top AQI for each hour
                    foreach (var convOutput in hourConvOutputs)
                    {
                        var aqiTuple = AQICalculator.CalculateAQI(convOutput.ToGasReading());
                        var aqi = aqiTuple.Item1;

                        if (aqi > maxAQI)
                        {
                            maxAQI = aqi;
                            maxAQIContributor = aqiTuple.Item2;
                        }
                        if (aqi > hourMaxAQI)
                            hourMaxAQI = aqi;
                    }

                    if (hourMaxAQI >= 0)
                        hourMaxCollection.Add(new ChartDataPoint(currHourDateTime.AddHours(-1), hourMaxAQI));

                    // move to next hour interval
                    startHourInterval = endHourInterval;
                    currHourDateTime = currHourDateTime.AddHours(1);
                    endHourInterval = MetaSenseNode.DtToUnix(currHourDateTime);
                }

                if (maxAQI >= 0)
                {
                    LastMessageReceivedAt = "last updated at " + DateTime.Now.ToString("t");
                    MaxAQI = maxAQI;
                    MaxAQIAndContributor = String.Format("{0:F1} ({1})", MaxAQI, maxAQIContributor);
                    HighlightMainContributor(maxAQIContributor);
                }
            });
            
            return hourMaxCollection;
        }

        /// <summary>
        /// Update the data point in the graph of the current hour if the last AQI is higher or
        /// add the data point if none exists yet.
        /// </summary>
        /// <param name="lastAQI">The AQI of the last read</param>
        public void UpdateGraph(double lastAQI)
        {
            var hourDateTime = TodayDateTime.AddHours(DateTime.Now.Hour);
            var matchingHour = HourMaxAQIData.FirstOrDefault(hourMax => hourMax.XValue.Equals(hourDateTime));
            var matchIndex = HourMaxAQIData.IndexOf(matchingHour);

            // update the max AQI of the current hour if the most recent AQI is higher
            if (matchingHour != null && matchingHour.YValue < lastAQI)
            {
                HourMaxAQIData[matchIndex] = new ChartDataPoint(hourDateTime, lastAQI);
            }
            // plot the AQI of the current hour since none exists
            else if (matchingHour == null)
            {
                HourMaxAQIData.Add(new ChartDataPoint(hourDateTime, lastAQI));
            }
        }

        /// <summary>
        /// Change the background and text color of the main contributor to the current day's
        /// max AQI.
        /// </summary>
        /// <param name="contributor"></param>
        private void HighlightMainContributor(string contributor)
        {
            switch (contributor)
            {
                case "NO2":
                    HighlightNO2();
                    break;
                case "O3":
                    HighlightO3();
                    break;
                case "CO":
                    HighlightCO();
                    break;
                default:
                    HighlightNone();
                    break;
            }
        }

        /// <summary>
        /// Change the background and text color to reflect NO2 as the main contributor.
        /// </summary>
        private void HighlightNO2()
        {
            NO2BackgroundColor = Color.White;
            NO2TextColor = Color.Black;
            O3BackgroundColor = Color.Black;
            O3TextColor = (Color)Application.Current.Resources["StandardTextColor"];
            COBackgroundColor = Color.Black;
            COTextColor = (Color)Application.Current.Resources["StandardTextColor"];
        }

        /// <summary>
        /// Change the background and text color to reflect O3 as the main contributor.
        /// </summary>
        private void HighlightO3()
        {
            NO2BackgroundColor = Color.Black;
            NO2TextColor = (Color)Application.Current.Resources["StandardTextColor"];
            O3BackgroundColor = Color.White;
            O3TextColor = Color.Black;
            COBackgroundColor = Color.Black;
            COTextColor = (Color)Application.Current.Resources["StandardTextColor"];
        }

        /// <summary>
        /// Change the background and text color to reflect CO as the main contributor.
        /// </summary>
        private void HighlightCO()
        {
            NO2BackgroundColor = Color.Black;
            NO2TextColor = (Color)Application.Current.Resources["StandardTextColor"];
            O3BackgroundColor = Color.Black;
            O3TextColor = (Color)Application.Current.Resources["StandardTextColor"];
            COBackgroundColor = Color.White;
            COTextColor = Color.Black;
        }

        /// <summary>
        /// Change the background and text color to reflect no pollutant as the main contributor.
        /// </summary>
        private void HighlightNone()
        {
            NO2BackgroundColor = Color.Black;
            NO2TextColor = (Color)Application.Current.Resources["StandardTextColor"];
            O3BackgroundColor = Color.Black;
            O3TextColor = (Color)Application.Current.Resources["StandardTextColor"];
            COBackgroundColor = Color.Black;
            COTextColor = (Color)Application.Current.Resources["StandardTextColor"];
        }

        private string _lastMessageReceivedAt;
        public string LastMessageReceivedAt
        {
            get => _lastMessageReceivedAt;
            private set => SetProperty(ref _lastMessageReceivedAt, value);
        }

        private string _coPPM;
        public string COppm
        {
            get => _coPPM;
            set => SetProperty(ref _coPPM, value);
        }

        private string _no2PPB;
        public string NO2ppb
        {
            get => _no2PPB;
            set => SetProperty(ref _no2PPB, value);
        }

        private string _o3PPB;
        public string O3ppb
        {
            get => _o3PPB;
            set => SetProperty(ref _o3PPB, value);
        }

        private string _temperature;
        public string Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        private string _humidity;
        public string Humidity
        {
            get => _humidity;
            set => SetProperty(ref _humidity, value);
        }

        private string _pressure;
        public string Pressure
        {
            get => _pressure;
            set => SetProperty(ref _pressure, value);
        }

        private string _maxAQIAndContributor;
        public string MaxAQIAndContributor
        {
            get => _maxAQIAndContributor;
            set => SetProperty(ref _maxAQIAndContributor, value);
        }

        public double MaxAQI
        {
            get;
            protected set;
        }

        private DateTime _todayDateTime;
        public DateTime TodayDateTime
        {
            get => _todayDateTime;
            set => SetProperty(ref _todayDateTime, value);
        }

        private DateTime _tomorrowDateTime;
        public DateTime TomorrowDateTime
        {
            get => _tomorrowDateTime;
            set => SetProperty(ref _tomorrowDateTime, value);
        }

        private ObservableCollection<ChartDataPoint> _hourMaxAQIData;
        public ObservableCollection<ChartDataPoint> HourMaxAQIData
        {
            get => _hourMaxAQIData;
            set => SetProperty(ref _hourMaxAQIData, value);
        }

        private Color _no2BackgroundColor;
        public Color NO2BackgroundColor
        {
            get => _no2BackgroundColor;
            set => SetProperty(ref _no2BackgroundColor, value);
        }

        private Color _no2TextColor;
        public Color NO2TextColor
        {
            get => _no2TextColor;
            set => SetProperty(ref _no2TextColor, value);
        }

        private Color _o3BackgroundColor;
        public Color O3BackgroundColor
        {
            get => _o3BackgroundColor;
            set => SetProperty(ref _o3BackgroundColor, value);
        }

        private Color _o3TextColor;
        public Color O3TextColor
        {
            get => _o3TextColor;
            set => SetProperty(ref _o3TextColor, value);
        }

        private Color _coBackgroundColor;
        public Color COBackgroundColor
        {
            get => _coBackgroundColor;
            set => SetProperty(ref _coBackgroundColor, value);
        }

        private Color _coTextColor;
        public Color COTextColor
        {
            get => _coTextColor;
            set => SetProperty(ref _coTextColor, value);
        }
    }
}