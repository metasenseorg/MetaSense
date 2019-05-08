using Core.ViewModels;
using NodeLibrary;
using Prism.Commands;
using Prism.Navigation;

namespace Receiver.ViewModels
{
    public abstract class NodeAwareViewModel : ViewModelBase
    {
        // DM: add median filter
        MedianFilter _medianFilter;

        protected NodeAwareViewModel(IAppProperties appProperties, INavigationService navigationService) : base(navigationService)
        {
            App = appProperties;
            App.MessageReceived += Node_MessageReceived;
            App.NodeSelectedChanged += OnNodeSelectedChanged;
            var lastMetaSenseMessage = App.LastMessageReceived;
            if (lastMetaSenseMessage?.Raw != null)
                _lastRead = lastMetaSenseMessage;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
            // DM: add median filter
            _medianFilter = new MedianFilter();
            // DM: end
        }

        protected virtual void OnNodeSelectedChanged(object sender, MetaSenseNodeViewModel e) { }

        ~NodeAwareViewModel()
        {
            App.MessageReceived -= Node_MessageReceived;
            App.NodeSelectedChanged -= OnNodeSelectedChanged;
        }
        private void Node_MessageReceived(object sender, MetaSenseMessage e)
        {
            if (e == null) return;
            if (e.Raw == null)
                LastControl = e;
            else
                LastRead = e;
        }
        protected virtual void ProcessLastRead() { }

        #region Sensor Node Properties

        protected IAppProperties App { get; }
    

        public bool NodeAvailable => App.Node != null;

        private MetaSenseMessage _lastControl;
        public MetaSenseMessage LastControl {
            get => _lastControl;
            private set => SetProperty(ref _lastControl, value);
        }

        private MetaSenseMessage _lastRead;
        public MetaSenseMessage LastRead
        {
            get => _lastRead;
            // DM: do not apply median filter currently since a duplicated read occurs when using it
            // private set => SetProperty(ref _lastRead, _medianFilter.ApplyMedianFilter(value), ProcessLastRead);
            private set => SetProperty(ref _lastRead, value, ProcessLastRead);
        }

        public DelegateCommand<string> NavigateCommand { get; }

        #endregion

        protected async void OnNavigateCommandExecuted(string path)
        {
            await NavigationService.NavigateAsync(path);
        }
    }
}
