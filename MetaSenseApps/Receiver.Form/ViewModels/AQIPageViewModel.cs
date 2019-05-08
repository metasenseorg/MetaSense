using NodeLibrary;
using NodeLibrary.Native;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Receiver.ViewModels
{
    /// <summary>
    /// View model for the AQI Page to display the instantaneous AQI to the user.
    /// </summary>
    public sealed class AQIPageViewModel : NodeAwareViewModel
    {
        private const double INITIAL_ARROW_X_SPACING = 0.1;
        private const double ARROW_Y_SPACING = 0.74;
        private const double ARROW_WIDTH = 0.04;
        private const double ARROW_HEIGHT = 0.04;
        private const double CENTERING_OFFSET = 0.01041666666;

        private const double INITIAL_MARKER_X_SPACING = 0.1;
        private const double MARKER_Y_SPACING = 0.8;
        private const double MARKER_WIDTH = 0.01;
        private const double MARKER_HEIGHT = 0.065;

        private const string CLOUD_IMAGE_BASE_NAME = "solid_icon_";
        private static readonly IBLEDeviceImages Images =
            DependencyService.Get<IBLEDeviceImages>();

        private const string UNKNOWN = "-";

        private ISharer _sharer;
        private IGeocoder _geocoder;
        private MetaSenseTimer _timer;

        /// <summary>
        /// Initialize platform dependent objects, initialize button commands, and initialize
        /// properties to inactive or unknown values.
        /// </summary>
        /// <param name="appProperties">The properties of the app</param>
        /// <param name="navigationService">The navigation service used to navigate between pages</param>
        public AQIPageViewModel(IAppProperties appProperties, INavigationService navigationService) : base(appProperties, navigationService)
        {
            _sharer = DependencyService.Get<ISharer>();
            _geocoder = DependencyService.Get<IGeocoder>();
            ShareButton = new DelegateCommand(Share);
            HelpButton = new DelegateCommand(async () => {
                await NavigationService.NavigateAsync("PollutantHelpPage");
            });

            // initialize properties to inactive or unknown values
            AQI = -1;
            AQIText = UNKNOWN;
            ArrowLayoutBounds = new Rectangle(INITIAL_ARROW_X_SPACING - CENTERING_OFFSET,
                ARROW_Y_SPACING, ARROW_WIDTH, ARROW_HEIGHT);
            AQIMarkerLayoutBounds = new Rectangle(INITIAL_MARKER_X_SPACING, MARKER_Y_SPACING,
                MARKER_WIDTH, MARKER_HEIGHT);
            LastMessageReceivedAt = default(DateTime);
            CloudImage = ImageSource.FromFile(CLOUD_IMAGE_BASE_NAME + "inactive");
            Active = false;

            InitializeTimer();

            if (LastRead != null)
            {
                ProcessLastRead();
            }
        }

        /// <summary>
        /// Update the properties binded to the view with the values from the last read.
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

                LastMessageReceivedAt = DateTime.Now;
                var aqiTuple = AQICalculator.CalculateAQI(pollutantAqiTuple);
                AQI = aqiTuple.Item1;
                AQICategory = AQICalculator.AQICategory(aqiTuple.Item1);
                AQIContributor = aqiTuple.Item2;
                ContributorAndLastUpdate = string.Format("{0} at {1:t}", AQIContributor,
                    LastMessageReceivedAt);
                Location = LastRead.Loc;
                TimeFromLastUpdate = $"updated {MetaSenseTimer.ACTIVE_RECENT}";
                Active = true;
                UpdateCloudImage();
                AdjustAQIMarker();
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
        /// Set the AQI marker's bounds to point at the current AQI's position within the AQI color
        /// spectrum.
        /// </summary>
        private void AdjustAQIMarker()
        {
            double psuedoAQI;

            // displayed color spectrum only goes up to 300
            if (AQI > 300)
                psuedoAQI = 300;
            else
                psuedoAQI = AQI;

            double percent = psuedoAQI / 300 * 0.8;
            double markerOffset = 0.1 + percent;
            double arrowOffset = 0.1 + (0.99 * percent) / (1 - ARROW_WIDTH) - CENTERING_OFFSET;
            AQIMarkerLayoutBounds = new Rectangle(markerOffset, MARKER_Y_SPACING, MARKER_WIDTH,
                MARKER_HEIGHT);
            ArrowLayoutBounds = new Rectangle(arrowOffset, ARROW_Y_SPACING, ARROW_WIDTH,
                ARROW_HEIGHT);
        }

        /// <summary>
        /// Share the AQI, AQI category, and location.
        /// </summary>
        private void Share()
        {
            if (LastRead != null)
            {
                MetaSenseAQIMessage aqiMessage = new MetaSenseAQIMessage(AQI, AQICategory,
                    _geocoder.ReverseGeocode(Location.Latitude, Location.Longitude));
                _sharer.Share(aqiMessage);
            }
        }

        /// <summary>
        /// Start the timer to check the relative time of how long ago the last message was
        /// received.
        /// </summary>
        private void InitializeTimer()
        {
            MetaSenseTimer.InitializeDict();

            // checks every five minutes
            _timer = new MetaSenseTimer(new System.TimeSpan(0, 5, 0), () =>
            {
                if (LastMessageReceivedAt != default(DateTime))
                {
                    string time = MetaSenseTimer.CalculateRelativeTimeDiff(DateTime.Now,
                        LastMessageReceivedAt);

                    // show the date if the last message was received too far in the past
                    if (time == MetaSenseTimer.ABSOLUTE_TIME_NEEDED)
                    {
                        time = LastMessageReceivedAt.ToString("dddd MMMM d");
                    }

                    TimeFromLastUpdate = $"updated {time}";

                    Active = (time == MetaSenseTimer.ACTIVE_RECENT) ? true : false;
                    UpdateCloudImage();
                }
            });
        }

        /// <summary>
        /// Update the cloud image to represent the color of the current AQI category.
        /// </summary>
        private void UpdateCloudImage()
        {
            string imageFile = CLOUD_IMAGE_BASE_NAME;

            if (!Active)
            {
                imageFile += "inactive";
            }
            else if (AQICategory.Equals("Hazardous"))
            {
                imageFile += "very_unhealthy";
            }
            else
            {
                string category = AQICategory.Replace(' ', '_').ToLower();
                imageFile += category;
            }

            var cloudImageSource = ImageSource.FromFile(imageFile);
            var newSourceFile = ((FileImageSource)cloudImageSource).File;
            var currentSourceFile = ((FileImageSource)CloudImage).File;

            // cannot compare imagesources normally with equals() so must perform check before
            // calling setproperty()
            if (CloudImage == null || !currentSourceFile.Equals(newSourceFile))
            {
                CloudImage = ImageSource.FromFile(imageFile);
            }
        }

        /// <summary>
        /// Start the timer when the page has been navigated to.
        /// </summary>
        /// <param name="parameters">The navigation parameters</param>
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            _timer.StartTimer();
        }

        /// <summary>
        /// Stop the timer when the page has been navigated from.
        /// </summary>
        /// <param name="parameters">The navigation parameters</param>
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            _timer.StopTimer();
        }

        /// <summary>
        /// Stop the timer if it has not been stopped.
        /// </summary>
        ~AQIPageViewModel() {
            if (_timer != null)
            {
                _timer.StopTimer();
            }
        }

        public MetaSenseNodeViewModel NodeViewModel => App.NodeViewModel;

        private double _aqi;
        public double AQI
        {
            get => _aqi;
            set => SetProperty(ref _aqi, value, () => AQIText = AQI.ToString());
        }

        // created aqi text to display '-' when no data has been received yet but
        // kept aqi as well for easy comparisons
        private string _aqiText;
        public string AQIText
        {
            get => _aqiText;
            set => SetProperty(ref _aqiText, value);
        }

        private string _aqiCategory;
        public string AQICategory
        {
            get => _aqiCategory;
            set => SetProperty(ref _aqiCategory, value);
        }

        private string _aqiContributor;
        public string AQIContributor
        {
            get => _aqiContributor;
            set => SetProperty(ref _aqiContributor, value);
        }

        private string _timeFromLastUpdate;
        public string TimeFromLastUpdate
        {
            get => _timeFromLastUpdate;
            set => SetProperty(ref _timeFromLastUpdate, value);
        }

        private string _contributorAndLastUpdate;
        public string ContributorAndLastUpdate
        {
            get => _contributorAndLastUpdate;
            set => SetProperty(ref _contributorAndLastUpdate, value);
        }

        private Rectangle _arrowLayoutBounds;
        public Rectangle ArrowLayoutBounds
        {
            get => _arrowLayoutBounds;
            set => SetProperty(ref _arrowLayoutBounds, value);
        }

        private Rectangle _aqiMarkerLayoutBounds;
        public Rectangle AQIMarkerLayoutBounds
        {
            get => _aqiMarkerLayoutBounds;
            set => SetProperty(ref _aqiMarkerLayoutBounds, value);
        }

        private DateTime _lastMessageReceivedAt;
        public DateTime LastMessageReceivedAt
        {
            get => _lastMessageReceivedAt;
            set => SetProperty(ref _lastMessageReceivedAt, value);
        }

        public LocationInfo Location
        {
            get;
            private set;
        }

        private bool _active;
        public bool Active
        {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        private ImageSource _cloudImage;
        public ImageSource CloudImage
        {
            get => _cloudImage;
            set => SetProperty(ref _cloudImage, value);
        }

        public DelegateCommand ShareButton { get; private set; }
        public DelegateCommand HelpButton { get; private set; }
    }
}