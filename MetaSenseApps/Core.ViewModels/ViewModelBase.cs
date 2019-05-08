using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.ViewModels.Properties;
using Prism.Mvvm;
using Prism.Navigation;

namespace Core.ViewModels
{
    public abstract class ViewModelBase : BindableBase, INavigationAware
    {
        protected INavigationService NavigationService { get; }

        protected ViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }
        //public Command NavigationPop { protected set; get; }
        //public event EventHandler<T> NavigationEvent;
        //protected void OnNavigationEvent()
        //{
        //    NavigationEvent?.Invoke(this, default(T));
        //}
        //protected void OnNavigationEvent(T param)
        //{
        //    NavigationEvent?.Invoke(this, param);
        //}

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            // ReSharper disable once ExplicitCallerInfoArgument
            set => SetProperty(ref _isBusy, value, () => RaisePropertyChanged(nameof(IsNotBusy)));
        }

        public bool IsNotBusy => !IsBusy;

        //public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
            
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
            
        }

        public virtual void OnNavigatingTo(INavigationParameters parameters)
        {
            
        }
    }
}