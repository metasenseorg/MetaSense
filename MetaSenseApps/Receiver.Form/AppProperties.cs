using System;
using Core.ViewModels;
using NodeLibrary;
using NodeLibrary.Native;
using Prism.Navigation;
using Xamarin.Forms;
using Receiver.ViewModels;

namespace Receiver
{
    public sealed class AppProperties : ViewModelBase, IAppProperties
    {
        private IBLEBackgroundReader _bleBackgroundReader;
        private ISettingsData _settingsData;
        
        public AppProperties(IBLEBackgroundReader bleBackgroundReader, ISettingsData settingsData, INavigationService navigationService) : base(navigationService)
        {
            _settingsData = settingsData;
            _bleBackgroundReader = bleBackgroundReader;
            bleBackgroundReader.BackgroundBLEServiceMessageReceived += OnMessageReceived;
            InitializeConversionFromDatabase();
            InitCurrentNode();
            MessagingCenter.Subscribe<MetaSenseMessage>(this, "MetaSenseRead", message =>
            {
                try
                {
                    OnMessageReceived(this, message);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            });
        }

        private void InitializeConversionFromDatabase()
        {
            var function = _settingsData.Get("ConversionFunction");
            if ("alphasense".Equals(function)) ConversionType = ConversionType.alphasense;
            else if ("sharad".Equals(function)) ConversionType = ConversionType.sharad;
            // DM: set conversion type to neural network or multi-sensor if the previous
            //     conversion function was neural or multisensor
            else if ("neural".Equals(function)) ConversionType = ConversionType.neural;
            else if ("multisensor".Equals(function)) ConversionType = ConversionType.multisensor;
            // DM: end
            else ConversionType = ConversionType.none;
        }

        public bool IsAlphasenseJsonValid(string json)
        {
            var conv = new ConversionFunctionsAlphasense(json);
            return conv.ValidCalibrationFile;
        }
        public bool IsSharadJsonValid(string json)
        {
            var conv = new ConversionFunctionsSharad(json); 
            return conv.ValidCalibrationFile;
        }
        // DM: add validation check for neural network json and multi-sensor json
        public bool IsNeuralNetworkJsonValid(string json)
        {
            var conv = new ConversionFunctionsSimpleNeuralNet(json);
            return conv.ValidCalibrationFile;
        }
        public bool IsMultiSensorJsonValid(string json)
        {
            var conv = new ConversionFunctionsMultiSensor(json);
            return conv.ValidCalibrationFile;
        }
        // DM: end
        public ConversionType ConversionType
        {
            get => _conversionType;
            set {
                //if (_conversionType ==value) return;

                if (value==ConversionType.alphasense)
                {
                    var json = AlphasenseJson;
                    var conv = new ConversionFunctionsAlphasense(json);
                    if (conv.ValidCalibrationFile)
                    {
                        Conversion = conv;
                        _settingsData.Set("ConversionFunction", "alphasense");
                        _conversionType = ConversionType.alphasense;
                        return;
                    }
                }
                else if (value==ConversionType.sharad)
                {
                    var json = SharadJson;
                    var conv = new ConversionFunctionsSharad(json);
                    if (conv.ValidCalibrationFile)
                    {
                        Conversion = conv;
                        _settingsData.Set("ConversionFunction","sharad");
                        _conversionType = ConversionType.sharad;
                        return;
                    }
                }
                // DM: set conversion, conversion type, and conversion function to neural network
                //     or multi-sensor if valid
                else if (value == ConversionType.neural)
                {
                    var json = NeuralNetworkJson;
                    var conv = new ConversionFunctionsSimpleNeuralNet(json);
                    if (conv.ValidCalibrationFile)
                    {
                        Conversion = conv;
                        _settingsData.Set("ConversionFunction", "neural");
                        _conversionType = ConversionType.neural;
                        return;
                    }
                }
                else if (value == ConversionType.multisensor)
                {
                    var json = MultiSensorJson;
                    var conv = new ConversionFunctionsMultiSensor(json);
                    if (conv.ValidCalibrationFile)
                    {
                        Conversion = conv;
                        _settingsData.Set("ConversionFunction", "multisensor");
                        _conversionType = ConversionType.multisensor;
                        return;
                    }
                }
                // DM: end
                _conversionType = ConversionType.none;
                Conversion = null;
            }
        }
        public string AlphasenseJson {
            get => _settingsData.Get("ConversionAlphasenseJson");
            set
            {
                var conv = new ConversionFunctionsAlphasense(value);
                if (conv.ValidCalibrationFile)
                    _settingsData.Set("ConversionAlphasenseJson", value);
                else
                    throw new Exception("JSON File Validation Failed");
            }
        }
        public string SharadJson {
            get => _settingsData.Get("ConversionSharadJson");
            set
            {
                var conv = new ConversionFunctionsSharad(value);
                if (conv.ValidCalibrationFile)
                    _settingsData.Set("ConversionSharadJson", value);
                else
                    throw new Exception("JSON File Validation Failed");
            }
        }
        // DM: add neural network json and multi-sensor json properties that store
        //     and retrieve the (valid) json from the database
        public string NeuralNetworkJson {
            get => _settingsData.Get("ConversionNeuralNetworkJson");
            set
            {
                var conv = new ConversionFunctionsSimpleNeuralNet(value);
                if (conv.ValidCalibrationFile)
                {
                    _settingsData.Set("ConversionNeuralNetworkJson", value);
                }
                else
                {
                    throw new Exception("JSON File Validation Failed");
                }
            }
        }

        public string MultiSensorJson
        {
            get => _settingsData.Get("ConversionMultiSensorJson");
            set
            {
                var conv = new ConversionFunctionsMultiSensor(value);
                if (conv.ValidCalibrationFile)
                {
                    _settingsData.Set("ConversionMultiSensorJson", value);
                }
                else
                {
                    throw new Exception("JSON File Validation Failed");
                }
            }
        }
        // DM: end
        public IConversionFunctions Conversion { get; set; }

        public event EventHandler<MetaSenseMessage> MessageReceived;
        public event EventHandler<MetaSenseNodeViewModel> NodeSelectedChanged; 
        public MetaSenseMessage LastMessageReceived { get; private set; }
        
        private DateTime? _lastMessageReceivedAt;
        private DateTime? _lastTimestampReceived;
        public bool Connected { get; private set; }
        
        public void ConnectSelectedNode()
        {
            try
            {
                var info = Node?.Info;
                if (info == null) return;
                DependencyService.Get<IBLEBackgroundReader>().StartBackgroundBLEService(info.MacAddress);
                _settingsData.Set("service.running", "running");
                Connected = true;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public void DiconnectSelectedNode()
        {
            try
            {
                if (Node == null) return;
                DependencyService.Get<IBLEBackgroundReader>().StopBackgroundBLEService();
                _settingsData.Delete("service.running");
                Connected = false;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        private MetaSenseNodeViewModel _nodeViewModel;
        private ConversionType _conversionType;

        public MetaSenseNodeViewModel NodeViewModel {
            get => _nodeViewModel;
            set
            {
                if (_nodeViewModel!=null && value!=null && Equals(_nodeViewModel.MacAddress, value.MacAddress)) return;
                if (Equals(_nodeViewModel, value)) return;
                _nodeViewModel = value;

                if (value?.NodeInfo == null)
                {
                    _settingsData.Delete("node.mac");
                    _settingsData.Delete("node.afe");
                    _settingsData.Delete("node.id");
                    _settingsData.Delete("node.name");
                    DiconnectSelectedNode();
                    Connected = false;
                }
                else
                {
                    var info = value.NodeInfo;
                    _settingsData.Set("node.mac", info.MacAddress);
                    _settingsData.Set("node.afe", info.AfeSerial);
                    _settingsData.Set("node.id", info.Id.ToString());
                    _settingsData.Set("node.name", info.Name);
                    if (Connected)
                    {
                        ConnectSelectedNode();
                        Connected = true;
                    }
                }
                NodeSelectedChanged?.Invoke(null,_nodeViewModel);
                RaisePropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(Node));
            }
        }

        public MetaSenseNode Node => _nodeViewModel?.Node;

        public DateTime? LastMessageReceivedAt
        {
            get { return _lastMessageReceivedAt; }
            private set {
                if (Equals(_lastMessageReceivedAt, value)) return;
                _lastMessageReceivedAt = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? LastTimestampReceived
        {
            get { return _lastTimestampReceived; }
            private set
            {
                if (Equals(_lastTimestampReceived, value)) return;
                _lastTimestampReceived = value;
                RaisePropertyChanged();
            }
        }

        private async void InitCurrentNode()
        {
            try
            {
                Connected = "running".Equals(_settingsData.Get("service.running"));
                var nodeMac = _settingsData.Get("node.mac");
                if (nodeMac == null) return;
                //Reload last node
                var bleDevices = DependencyService.Get<IBLEUtils>();
                var deviceInfo = await bleDevices.DeviceInfoFromMac(nodeMac);
                if (deviceInfo != null)
                    NodeViewModel = new MetaSenseNodeViewModel(deviceInfo);
                //if (NodeViewModel == null) return;
                //if ((LastMessageReceivedAt.HasValue) && ((DateTime.Now - LastMessageReceivedAt.Value).TotalSeconds < 30))
                //    NodeViewModel.Connected = true;
                //else
                //    NodeViewModel.Connected = false;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        private void OnMessageReceived(object sender, MetaSenseMessage e)
        {
            LastMessageReceivedAt = DateTime.Now;
            LastTimestampReceived = e?.Ts != null ? (DateTime?)MetaSenseNode.UnixToDateTime(e.Ts.Value) : null;
            LastMessageReceived = e;
            MessageReceived?.Invoke(null, e);
        }
    }
}
