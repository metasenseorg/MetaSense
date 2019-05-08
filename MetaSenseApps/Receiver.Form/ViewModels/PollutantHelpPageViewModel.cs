using NodeLibrary;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Receiver.ViewModels
{
    public class PollutantHelpPageViewModel : NodeAwareViewModel
    {
        public PollutantHelpPageViewModel(IAppProperties appProperties, INavigationService navigationService) : base(appProperties, navigationService)
        {
            Uri epaInfoUri = new Uri("https://www.epa.gov/criteria-air-pollutants");
            EPAInfoButton = new DelegateCommand(() => Device.OpenUri(epaInfoUri));
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            Log.Trace("NAVIGATED TO POLLUTANT HELP");
        }

        public DelegateCommand EPAInfoButton { get; private set; }
    }
}