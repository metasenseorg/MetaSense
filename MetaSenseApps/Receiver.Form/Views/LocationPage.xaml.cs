using System.Diagnostics;
using NodeLibrary.Native;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Receiver.ViewModels;

namespace Receiver.Views
{
    public partial class LocationPage : ContentPage
    {
        public LocationPage()
        {
            InitializeComponent();
            var vm = BindingContext as LocationPageViewModel;
            Debug.Assert(vm != null, "vm != null");
            vm.MapRef = MyMap;
        }

        protected override void OnAppearing()
        {
            SetCurrentLocation();
        }

        private async void SetCurrentLocation()
        {
            var locservice = DependencyService.Get<ISensorLocation>();
            await locservice.WaitForConnected();
            if (!locservice.Connected) return;
            var loc = locservice.RequestLocation();
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(loc.Latitude, loc.Longitude),
                Distance.FromMeters(300)));
        }
    }
}
