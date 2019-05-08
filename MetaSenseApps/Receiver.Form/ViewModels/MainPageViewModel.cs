using System;
using System.IO;
using NodeLibrary;
using NodeLibrary.Native;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using Core.ViewModels;

namespace Receiver.ViewModels
{
    public class MainPageViewModel : ViewModelBase {

        //public static MainViewModel Instance { get; } = new MainViewModel();
        //public MetaSenseNodeViewModel SelectedNode
        //{
        //    get { return _selectedNode; }
        //    set
        //    {
        //        if (Equals(_selectedNode, value)) return;
        //        _selectedNode = value;
        //        Node = _selectedNode?.Node;
        //    }
        //}
        //public bool CanRefreshStatus => App.Node != null;
        //public void NodeUpdated()
        //{
        //    OnPropertyChanged(nameof(SelectedNode));
        //    if (!ConnectButtonName.Equals("Disconnect")) return;
        //    DiconnectSelectedNodeFunc();
        //    ConnectSelectedNodeFunc();
        //}
        //public string ConnectButtonName => App.Connected ? "Disconnect" : "Connect";

        private readonly ISettingsData _settings;
        private readonly IAppProperties _appProperties;
        private readonly IPageDialogService _dialogService;
        private readonly IBLEBackgroundReader _backgroundReader;
        private readonly IFileAccessChooser _fileAccessChooser;

        public MainPageViewModel(IBLEBackgroundReader backgroundReader,
            IFileAccessChooser fileAccessChooser,
            ISettingsData settings, 
            IAppProperties appProperties, 
            INavigationService navigationService, 
            IPageDialogService dialogService) : base(navigationService)
        {
            _appProperties = appProperties;
            _fileAccessChooser = fileAccessChooser;
            _backgroundReader = backgroundReader;
            _dialogService = dialogService;
            _settings = settings;
            //App.NodeSelectedChanged += AppPropertiesOnNodSelectedChanged;

            //SelectedNode = AppProperties.Instance.NodeViewModel;

            //var serviceRunning = SettingsData.Default.Get("service.running");
            //if ("running".Equals(serviceRunning))
            //{
            //    ConnectSelectedNodeFunc();
            //    ConnectButtonName = "Disconnect";
            //}
            //else
            //    ConnectButtonName = "Connect";
            //ConnectButton = new DelegateCommand(() =>
            //{
            //    if (App.Connected)
            //        App.DiconnectSelectedNode();
            //    else
            //        App.ConnectSelectedNode();
            //    RaisePropertyChanged();
            //});

            //SensorSelectorButton = new DelegateCommand(() => _navigationService.NavigateAsync("BluetoothPage"));
            //DataPageButton = new DelegateCommand(() => _navigationService.NavigateAsync("Data"));
            //GraphPageButton = new DelegateCommand(() => _navigationService.NavigateAsync("Graph"));
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
            ClearDbButton = new DelegateCommand(ClearDb);
            ExportDbButton = new DelegateCommand(ExportDb);
            //LocationButton = new DelegateCommand(() => _navigationService.NavigateAsync("LocationPage"));
        }

        private async void OnNavigateCommandExecuted(string path)
        {
            await NavigationService.NavigateAsync(new Uri(path, UriKind.Relative));
        }
        private async void ClearDb()
        {
            var answer = await _dialogService.DisplayAlertAsync("Warning", "Are you sure you you want to delete all sensor data stored on the phone?", "Yes", "No");
            //Debug.WriteLine("Answer: " + answer);
            if (answer)
                _settings.ClearReads();
        }
        private async void ExportDb()
        {
            try
            {
                using (var output = await _fileAccessChooser.OpenFileForWrite())
                {
                    var outWriter = new StreamWriter(output);
                    await _settings.ExportReads(outWriter);
                    await outWriter.FlushAsync();
                }
                var answer = await _dialogService.DisplayAlertAsync("Done", "Do you want to delete all sensor data stored on the app database?\n[It will not delete the file you have exported]", "Yes", "No");
                //Debug.WriteLine("Answer: " + answer);
                if (answer)
                    _settings.ClearReads();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        //protected override void OnNodeSelectedChanged(object sender, MetaSenseNodeViewModel metaSenseNodeViewModel)
        //{
        //    //var service = DependencyService.Get<IBLEBackgroundReader>();
        //    //var serviceRunning = SettingsData.Default.Get("service.running");
        //    //if ("running".Equals(serviceRunning))
        //    //{
        //    //    service.StopBackgroundBLEService();
        //    //}
        //    //else
        //    //{
                
        //    //}
        //}

        ~MainPageViewModel()
        {
            try
            {
                //if (App.Node == null) return;
                //_backgroundReader.StopBackgroundBLEService();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }


        //protected override void ProcessLastRead()
        //{
        //    LastTimestampReceived = App.LastTimestampReceived ?? default(DateTime);
        //}

        //private DateTime _lastTimestampReceived;

        //public DateTime LastTimestampReceived
        //{
        //    get => _lastTimestampReceived;
        //    private set => SetProperty(ref _lastTimestampReceived, value, ProcessLastRead);
        //}

        #region Commands
        //private void ConnectSelectedNodeFunc()
        //{
        //    try
        //    {
        //        var info = App.Node?.Info;
        //        if (info == null) return;
        //        DependencyService.Get<IBLEBackgroundReader>().StartBackgroundBLEService(info.MacAddress);
        //        SettingsData.Default.Set("service.running", "running");
        //        ConnectButtonName = "Disconnect";
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e);
        //    }
        //}
        //private void DiconnectSelectedNodeFunc()
        //{
        //    try
        //    {
        //        if (App.Node == null) return;
        //        DependencyService.Get<IBLEBackgroundReader>().StopBackgroundBLEService();
        //        SettingsData.Default.Delete("service.running");
        //        ConnectButtonName = "Connect";
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e);
        //    }
        //}
        //public DelegateCommand GraphPageButton { protected set; get; }
        //public DelegateCommand DataPageButton { protected set; get; }
        //public DelegateCommand SensorSelectorButton { protected set; get; }
        //public DelegateCommand ConnectSelectedNode { protected set; get; }
        //public DelegateCommand DisconnectSelectedNode { protected set; get; }
        //public DelegateCommand LocationButton { get; protected set; }
        public DelegateCommand<string> NavigateCommand { get; }
        public DelegateCommand ExportDbButton { protected set; get; }
        public DelegateCommand ClearDbButton { protected set; get; }
        //public DelegateCommand ConnectButton { get; protected set; }
        #endregion

    }
}
