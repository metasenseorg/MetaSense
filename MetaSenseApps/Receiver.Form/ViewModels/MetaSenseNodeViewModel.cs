using System;
using System.ComponentModel;
using NodeLibrary;
using NodeLibrary.Native;
using Xamarin.Forms;
using Prism.Mvvm;

namespace Receiver.ViewModels
{
    public class MetaSenseNodeViewModel : BindableBase
    {
        private static readonly IBLEDeviceImages Images = DependencyService.Get<IBLEDeviceImages>();

        public MetaSenseNode Node { get; private set; }
        public NodeInfo NodeInfo { get; }
        public MetaSenseNodeViewModel(NodeInfo nodeInfo)
        {
            if (nodeInfo == null) return;
            NodeInfo = nodeInfo;
            NodeInfo.Updated += ProcessUpdates;
            _name = nodeInfo.Name;
            _afeSerial = nodeInfo.AfeSerial;
            _macAddress = nodeInfo.MacAddress;
            _paired = nodeInfo.Paired;
            _sensorId = nodeInfo.SensorId;
            _signalStrength = nodeInfo.SignalStrength;
            SetNode(nodeInfo);
            MessagingCenter.Subscribe<MetaSenseNodeReachable>(this, "MetaSenseNodeReachableChanged", message =>
            {
                OnReachableChanged(message.Connected);
            });
        }
        private async void SetNode(NodeInfo nodeInfo)
        {
            Node = await nodeInfo.NodeFactory();
        }

        private void OnReachableChanged(bool e)
        {
            Connected = e;
        }

        ~MetaSenseNodeViewModel()
        {
            NodeInfo.Updated -= ProcessUpdates;
        }
        private void ProcessUpdates(object sender, EventArgs e)
        {
            if (!Equals(_name, Name))
            {
                _name = Name;
                OnPropertyChanged("Name");
            }
            if (!Equals(_afeSerial, AfeSerial))
            {
                _afeSerial = AfeSerial;
                OnPropertyChanged("AfeSerial");
            }
            if (!Equals(_macAddress, MacAddress))
            {
                _macAddress = MacAddress;
                OnPropertyChanged("MacAddress");
            }
            if (!Equals(_sensorId, SensorId))
            {
                _sensorId = SensorId;
                OnPropertyChanged("SensorId");
            }
            if (!Equals(_paired, Paired))
            {
                _paired = Paired;
                OnPropertyChanged("Paired");
                OnPropertyChanged("PairedStatusImage");
            }
            if (!Equals(_signalStrength, SignalStrength))
            {
                _signalStrength = SignalStrength;
                OnPropertyChanged("SignalStrength");
                OnPropertyChanged("SingalStrengthStatusImage");
            }
        }

        private string _name;
        private string _afeSerial;
        private string _macAddress;
        private string _sensorId;
        private bool? _paired;
        private double? _signalStrength;

        public string Name => NodeInfo.Name;
        public string AfeSerial => NodeInfo.AfeSerial;
        public string MacAddress => NodeInfo.MacAddress;
        public string SensorId => NodeInfo.SensorId;
        public bool Paired => NodeInfo.Paired ?? false;
        public ImageSource PairedStatusImage => Paired ? Images.Paired : Images.NotPaired;
        public double SignalStrength => NodeInfo.SignalStrength ?? 0;
        public ImageSource SingalStrengthStatusImage
        {
            get
            {
                if (Math.Abs(SignalStrength) < 0.001)
                    return Images.Signal0;
                if (SignalStrength < .3)
                    return Images.Signal1;
                if (SignalStrength < .6)
                    return Images.Signal2;
                if (SignalStrength <= 1)
                    return Images.Signal3;
                return Images.Signal0;
            }
        }
        
        private bool _connected;
        public bool Connected
        {
            get => _connected;
            // ReSharper disable once ExplicitCallerInfoArgument
            // DM: change for the bluetooth connected image
            // set => SetProperty(ref _connected, value, () => RaisePropertyChanged(nameof(ConnectedImage)));
            set => SetProperty(ref _connected, value, () =>
            {
                RaisePropertyChanged(nameof(ConnectedImage));
                RaisePropertyChanged(nameof(BluetoothConnectedImage));
            });
            // DM: end
        }
        public ImageSource ConnectedImage => Connected ? Images.GreenDot : Images.RedDot;
        // DM: add imagesource property
        public ImageSource BluetoothConnectedImage => Connected ? Images.BluetoothConnected : Images.Bluetooth;
        // DM: end

        public override bool Equals(object obj)
        {
            var tmp = obj as MetaSenseNodeViewModel;
            if (tmp != null && NodeInfo.Id != null)
                return NodeInfo.Id.Equals(tmp.NodeInfo.Id);
            var str = obj as string;
            return str != null && str.Equals(NodeInfo.Id);
        }
        public override int GetHashCode()
        {
            return NodeInfo.Id?.GetHashCode() ?? 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                Device.BeginInvokeOnMainThread(() => { handler(this, new PropertyChangedEventArgs(propertyName)); });
            }
        }
    }
}
