using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NodeLibrary.Native;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Receiver.ViewModels
{
    public sealed class NotNullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value!=null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class SettingsPageViewModel : NodeAwareViewModel
    {
        private readonly IFileAccessChooser _chooser;
        public SettingsPageViewModel(IFileAccessChooser chooser, IAppProperties appProperties, INavigationService navigationService) : base(appProperties, navigationService)
        {
            _chooser = chooser;
           
            SaveSettingsButton = new DelegateCommand(
                () =>
                {
                    var type = StringToConversionType(ConversionType);

                    try
                    {
                        App.AlphasenseJson = AlphasenseJson;
                        AlphasenseJsonInError = null;
                    }
                    catch (Exception e)
                    {
                        AlphasenseJsonInError = e.Message;
                    }

                    try
                    {
                        App.SharadJson = SharadJson;
                        SharadJsonInError = null;
                    }
                    catch (Exception e)
                    {
                        SharadJsonInError = e.Message;
                    }

                    // DM: add neural network and multi-sensor
                    try
                    {
                        App.NeuralNetworkJson = NeuralNetworkJson;
                        NeuralNetworkJsonInError = null;
                    }
                    catch (Exception e)
                    {
                        NeuralNetworkJsonInError = e.Message;
                    }

                    try
                    {
                        App.MultiSensorJson = MultiSensorJson;
                        MultiSensorJsonInError = null;
                    }
                    catch (Exception e)
                    {
                        MultiSensorJsonInError = e.Message;
                    }
                    // DM: end

                    // DM: check if neural network or multi-sensor is selected and invalid
                    if (!((type==Receiver.ConversionType.alphasense && AlphasenseJsonInError!=null)
                        || (type == Receiver.ConversionType.sharad && SharadJsonInError != null)
                        || (type == Receiver.ConversionType.neural && NeuralNetworkJsonInError != null)
                        || (type == Receiver.ConversionType.multisensor && MultiSensorJsonInError != null)))
                        App.ConversionType = type;
                    // ReSharper disable once ExplicitCallerInfoArgument
                    RaisePropertyChanged(nameof(CurrentConversionAlgorithm));
                }
                //, () => NodeViewModel != null
                );


            ResetAlphasenseButton = new DelegateCommand(
                () => { AlphasenseJson = App.AlphasenseJson; }
            );
            VerifyAlphasenseButton = new DelegateCommand(
                () =>
                {
                    try
                    {
                        AlphasenseJsonInError = App.IsAlphasenseJsonValid(AlphasenseJson) ? null : "Failed Validation";
                    }
                    catch (Exception e)
                    {
                        AlphasenseJsonInError = e.Message;
                    }
                }
            );
            LoadAlphasenseButton = new DelegateCommand(
                async () =>
                {
                    var stream = await _chooser.OpenFileForRead("*/*");
                    if (stream == null) return;
                    TextReader reader = new StreamReader(stream);
                    string text = await reader.ReadToEndAsync();
                    AlphasenseJson = text;
                }
            );

            ResetSharadButton = new DelegateCommand(
                () => { SharadJson = App.SharadJson; }
            );
            VerifySharadButton = new DelegateCommand(
                () =>
                {
                    try
                    {
                        SharadJsonInError = App.IsSharadJsonValid(SharadJson) ? null : "Failed Validation";
                    }
                    catch (Exception e)
                    {
                        SharadJsonInError = e.Message;
                    }
                }
            );
            LoadSharadButton = new DelegateCommand(
                async () =>
                {
                    var stream = await _chooser.OpenFileForRead("*/*");
                    if (stream == null) return;
                    TextReader reader = new StreamReader(stream);
                    string text  = await reader.ReadToEndAsync();
                    SharadJson = text;
                }
                //,() => false
            );

            // DM: set neural network and multi-sensor buttons
            ResetNeuralNetworkButton = new DelegateCommand(
                () => { NeuralNetworkJson = App.NeuralNetworkJson; }
            );
            VerifyNeuralNetworkButton = new DelegateCommand(
                () =>
                {
                    try
                    {
                        NeuralNetworkJsonInError = App.IsNeuralNetworkJsonValid(NeuralNetworkJson) ? null : "Failed Validation";
                    }
                    catch (Exception e)
                    {
                        NeuralNetworkJsonInError = e.Message;
                    }
                }
            );
            LoadNeuralNetworkButton = new DelegateCommand(
                async () =>
                {
                    var stream = await _chooser.OpenFileForRead("*/*");
                    if (stream == null) return;
                    TextReader reader = new StreamReader(stream);
                    string text = await reader.ReadToEndAsync();
                    NeuralNetworkJson = text;
                }
            );

            ResetMultiSensorButton = new DelegateCommand(
                () => { MultiSensorJson = App.MultiSensorJson; }
            );
            VerifyMultiSensorButton = new DelegateCommand(
                () =>
                {
                    try
                    {
                        MultiSensorJsonInError = App.IsMultiSensorJsonValid(MultiSensorJson) ? null : "Failed Validation";
                    }
                    catch (Exception e)
                    {
                        MultiSensorJsonInError = e.Message;
                    }
                }
            );
            LoadMultiSensorButton = new DelegateCommand(
                async () =>
                {
                    var stream = await _chooser.OpenFileForRead("*/*");
                    if (stream == null) return;
                    TextReader reader = new StreamReader(stream);
                    string text = await reader.ReadToEndAsync();
                    MultiSensorJson = text;
                }
            );
            // DM: end
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            AlphasenseJson = App.AlphasenseJson;
            SharadJson = App.SharadJson;
            // DM: add neural network and multi-sensor
            NeuralNetworkJson = App.NeuralNetworkJson;
            MultiSensorJson = App.MultiSensorJson;
            // DM: end
            var cType = ConversionTypeToString(App.ConversionType);
            ConversionType = cType;
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(CurrentConversionAlgorithm));
        }

        private static ConversionType StringToConversionType(string name)
        {
            var conversionType = name?.ToLower() ?? "";
            ConversionType type;
            if ("alphasense".Equals(conversionType))
                type = Receiver.ConversionType.alphasense;
            else if ("sharad".Equals(conversionType))
                type = Receiver.ConversionType.sharad;
            // DM: check if neural network or multi-sensor corresponds to the conversion type
            else if ("neural".Equals(conversionType))
                type = Receiver.ConversionType.neural;
            else if ("multisensor".Equals(conversionType))
                type = Receiver.ConversionType.multisensor;
            // DM: end
            //else if ("none".Equals(conversionType))
            //    type = Receiver.ConversionType.none;
            else
                type = Receiver.ConversionType.none;
            return type;
        }
        private static string ConversionTypeToString(ConversionType type)
        {
            string s= type.ToString().ToLower();
            return Regex.Replace(s, @"\b(\w)", m => m.Value.ToUpper());
        }

        #region Values 

        public string CurrentConversionAlgorithm => ConversionTypeToString(App.ConversionType);


        private string _conversionType;
        public string ConversionType
        {
            get => _conversionType;
            set => SetProperty(ref _conversionType, value);
        }

        private string _alphasenseJson;
        public string AlphasenseJson
        {
            get => _alphasenseJson;
            set => SetProperty(ref _alphasenseJson, value);
        }
        private string _alphasenseJsonInError;
        public string AlphasenseJsonInError
        {
            get => _alphasenseJsonInError;
            set => SetProperty(ref _alphasenseJsonInError, value);
        }

        private string _sharadJson;
        public string SharadJson
        {
            get => _sharadJson;
            set => SetProperty(ref _sharadJson, value);
        }
        private string _sharadJsonInError;
        public string SharadJsonInError
        {
            get => _sharadJsonInError;
            set => SetProperty(ref _sharadJsonInError, value);
        }

        // DM: add neural network and multi-sensor properties
        private string _neuralNetworkJson;
        public string NeuralNetworkJson
        {
            get => _neuralNetworkJson;
            set => SetProperty(ref _neuralNetworkJson, value);
        }
        private string _neuralNetworkJsonInError;
        public string NeuralNetworkJsonInError
        {
            get => _neuralNetworkJsonInError;
            set => SetProperty(ref _neuralNetworkJsonInError, value);
        }

        private string _multiSensorJson;
        public string MultiSensorJson
        {
            get => _multiSensorJson;
            set => SetProperty(ref _multiSensorJson, value);
        }
        private string _multiSensorJsonInError;
        public string MultiSensorJsonInError
        {
            get => _multiSensorJsonInError;
            set => SetProperty(ref _multiSensorJsonInError, value);
        }
        // DM: end
        #endregion
        #region Cammands
        public DelegateCommand SaveSettingsButton { get; private set; }

        public DelegateCommand ResetAlphasenseButton { get; private set; }
        public DelegateCommand VerifyAlphasenseButton { get; private set; }
        public DelegateCommand LoadAlphasenseButton { get; private set; }

        public DelegateCommand ResetSharadButton { get; private set; }
        public DelegateCommand VerifySharadButton { get; private set; }
        public DelegateCommand LoadSharadButton { get; private set; }

        // DM: add neural network and multi-sensor commands
        public DelegateCommand ResetNeuralNetworkButton { get; private set; }
        public DelegateCommand VerifyNeuralNetworkButton { get; private set; }
        public DelegateCommand LoadNeuralNetworkButton { get; private set; }

        public DelegateCommand ResetMultiSensorButton { get; private set; }
        public DelegateCommand VerifyMultiSensorButton { get; private set; }
        public DelegateCommand LoadMultiSensorButton { get; private set; }
        // DM: end
        #endregion

    }
}
