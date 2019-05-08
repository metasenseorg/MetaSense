using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Core.ViewModels;
using NodeLibrary.Native;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;

namespace Receiver.ViewModels
{
    public sealed class BluetoothPageViewModel : ViewModelBase
    {
        #region Properties

        public IAppProperties App { get; }
        private MetaSenseNodeViewModel _nodeViewModel;
        public MetaSenseNodeViewModel NodeViewModel {
            get => _nodeViewModel; 
            set => SetProperty(ref _nodeViewModel, value);
        }
        public ObservableCollection<MetaSenseNodeViewModel> SensorsList { get; }

        #endregion
        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand ItemTapped { get; }
        public DelegateCommand<ListView> Refreshing { get; }

        private readonly IBLEDevicesList _bleDevicesList;
        private readonly IBLEUtils _bleUtils;

        public BluetoothPageViewModel(IBLEUtils bleUtils, IAppProperties appProperties, INavigationService navigationService) : base(navigationService)
        {
            App = appProperties;
            _bleDevicesList = DependencyService.Get<IBLEDevicesList>();
            _bleUtils = bleUtils;

            SensorsList = new ObservableCollection<MetaSenseNodeViewModel>(
                _bleDevicesList.Devices.Select(el => new MetaSenseNodeViewModel(el)));

            RefreshCommand = new DelegateCommand(() => _bleDevicesList.StartScanDevices(30));
            ItemTapped = new DelegateCommand(ItemTappedImp);
            Refreshing = new DelegateCommand<ListView>(async lv =>
            {
                await Task.Delay(1000);
                lv?.EndRefresh();
            });
        }

        private async void ItemTappedImp()
        {
            if (!(NodeViewModel.NodeInfo.Paired.HasValue && NodeViewModel.NodeInfo.Paired.Value))
                await _bleUtils.PairDevice(NodeViewModel.NodeInfo);
            else
                await NavigationService.GoBackAsync();
        }

        public async void InitScanning()
        {
            if (App.NodeViewModel != null && App.NodeViewModel.Node != null)
                await App.NodeViewModel.Node.DisconnectAsync();
            _bleDevicesList.AddDevice += AddDeviceList;
            _bleDevicesList.RemoveDevice += RemoveDeviceList;
            _bleDevicesList.StartScanDevices(30);
        }
        public void EndScanning()
        {
            _bleDevicesList.StopScanDevices();
            _bleDevicesList.AddDevice -= AddDeviceList;
            _bleDevicesList.RemoveDevice -= RemoveDeviceList;
        }

        private void AddDeviceList(NodeInfo drv)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lock (SensorsList)
                {
                    //EventCounter++;
                    SensorsList.Add(new MetaSenseNodeViewModel(drv));
                }
            });
        }
        private void RemoveDeviceList(NodeInfo drv)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lock (SensorsList)
                {
                    //EventCounter++;
                    SensorsList.Remove(new MetaSenseNodeViewModel(drv));
                }
            });
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            InitScanning();
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            EndScanning();
            if (!Equals(App.NodeViewModel, NodeViewModel))
                App.NodeViewModel = NodeViewModel;
        }
    }
}
