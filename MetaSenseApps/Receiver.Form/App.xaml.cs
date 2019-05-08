using DryIoc;
using Prism.DryIoc;
using Prism.Mvvm;
using Receiver.ViewModels;
using Receiver.Views;
using System;
using System.Globalization;
using NodeLibrary;
using Prism.Ioc;
using Xamarin.Forms;

namespace Receiver
{
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : PrismApplication
    {
        public App() 
        {
            InitializeComponent();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
            {
                var viewName = viewType.FullName;
                viewName = viewName.Replace(".Views.", ".ViewModels.");
                var viewAssemblyName = viewType.GetAssembly().FullName;
                var suffix = viewName.EndsWith("View") ? "Model" : "ViewModel";
                var viewModelName = String.Format(CultureInfo.InvariantCulture, "{0}{1}", viewName, suffix);

                var assembly = viewType.GetAssembly();
                var type = assembly.GetType(viewModelName);

                return type;
            });
            ViewModelLocationProvider.SetDefaultViewModelFactory(viewModelType =>
            {
                return Container.Resolve(viewModelType);
            });
        }

        protected override void OnInitialized()
        {
            //NavigationService.NavigateAsync("/DataPage");// //?title=Hello%20from%20Xamarin.Forms
            // DM: make aqi page the first page
            // NavigationService.NavigateAsync(new Uri("/Index/Navigation/OverviewPage", UriKind.Absolute)); //?title=Hello%20from%20Xamarin.Forms
            NavigationService.NavigateAsync(new Uri("/Index/Navigation/AQIPage", UriKind.Absolute));
            //MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        protected override void RegisterTypes(IContainerRegistry container)
        {
            container.RegisterForNavigation<NavigationPage>("Navigation");
            container.RegisterForNavigation<MainPage>("Index");
            container.RegisterSingleton<MainPageViewModel>();
            container.RegisterForNavigation<BluetoothPage>();
            container.Register<BluetoothPageViewModel>();
            container.RegisterForNavigation<LocationPage>();
            container.RegisterSingleton<LocationPageViewModel>();
            //Container.RegisterTypeForNavigation<MenuPage>();
            container.RegisterForNavigation<OverviewPage>();
            container.RegisterSingleton<OverviewPageViewModel>();
            container.RegisterForNavigation<DataPage>();
            container.RegisterSingleton<DataPageViewModel>();
            container.RegisterForNavigation<GraphPage>();
            container.RegisterSingleton<GraphPageViewModel>();
            container.RegisterForNavigation<SettingsPage>();
            container.RegisterSingleton<SettingsPageViewModel>();
            /* DM: Register new UI pages for navigation */
            container.RegisterForNavigation<AQIPage>();
            container.RegisterSingleton<AQIPageViewModel>();
            container.RegisterForNavigation<PollutantHelpPage>();
            container.RegisterSingleton<PollutantHelpPageViewModel>();
            container.RegisterForNavigation<PollutantDetailsPage>();
            container.RegisterSingleton<PollutantDetailsPageViewModel>();
            /* DM: End registering */
            //var sharad_json_4 = //AFE 729
            //    "{\"O3\": {\"weights\": [0.0, 0.07230986418652653, 0.4160721301285209, 0.18579584421943768, -0.1643969201067098, 1.4324469991422648, -0.12546586464600012], \"intercept\": -186.41827760344097}, \"CO\": {\"weights\": [0.0, -0.0, 0.0, 0.0, -0.0, -8.728608466768063e-06, 1.4357179882804295e-05, -0.0, -3.962945202714216e-05, -3.867204518216653e-07, -2.060444498008262e-05, 1.083013338506562e-05, 0.0, -0.0, 4.694282215946266e-05], \"intercept\": -0.028363721106486328}, \"NO2\": {\"weights\": [0.0, -0.0, 0.0, 0.0, -0.0, 0.0, -0.0, 0.0, -0.0, 0.000860361445119428, 1.4103861512856326e-05, -7.285684231575184e-05, -0.00010504628804771831, -0.0005577372965488009, 0.00030074302236256207, 0.0001358903534236543, -0.00033488851573922464, -0.00012026336884163385, -5.734607438455616e-05, -7.658389153104979e-05, -0.0017058139025766337, 3.39026286174402e-05, 0.0011525399770816068, -0.0002680356334287563, -0.0008941111476869357, -0.00012858214114180736, -4.42014149122897e-05, -1.855884378895224e-05, -0.0003002812405121397, -0.00039001091852163737, -9.277183606044659e-05, 0.0037242866272783454, -0.0003546177050616209, -0.0008689149792320277, 8.465202639161321e-05, 0.00014718066030555113, 0.0008011101570634587, -0.009397348543152724, -0.0010362001034199006, -0.00027570645428680497, 0.004493717045613808, 0.0002641043755751236, 0.000553627470498921, -0.008797524873567079, 0.002613030082755843], \"intercept\": -15.116433013118854}}";
            //var sharad_json_8 = //AFE 007
            //    "{\"O3\": {\"weights\": [0.0, -1.1563595176534505, 0.5768605369552319, 0.27977488564128233, -0.0777923014663584, 0.0, -0.6729144109954817], \"intercept\": 254.0218731350004}, \"CO\": {\"weights\": [0.0, 0.0, 0.0, 0.0, -0.0, 3.1417249491587646e-06, -4.324624176446251e-06, 0.0, -9.921163610281978e-06, -7.026468601130439e-07, 9.21857360543496e-06, 5.78079270424121e-05, 0.0, -6.347547388190278e-05, 3.794286879176944e-05], \"intercept\": -0.3543508470976085}, \"NO2\": {\"weights\": [0.0, 0.0, -0.0, -0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.002538978262132088, -0.003121428967740682, -0.0004184132124288028, 0.00017222453882580245, 0.001885141335840755, 0.0004108135055299739, 0.0006849197673423578, -0.0025720597239259437, 0.0007242897640302858, -0.0028027322105233575, -0.0010758848990381736, -0.0003439817500832972, -0.0005213416903659827, 0.004236300211599229, -0.0010320268639009873, -0.0006349832111110764, 0.004806053855476336, 0.0014644998522670108, 0.0006281693676303309, -0.010726794340331981, 0.0010797091432756304, 0.0016447130829139614, -0.0009181264558453305, 2.7166017812942863e-05, 0.0006505160371262054, 9.309793421338685e-05, -0.0009765826473761864, -0.00014653678847105465, -0.0006468820384312948, -0.0009415788543265367, 1.110911519025407e-05, -2.1511709647921794e-05, 0.00019678605142289275, 0.0, 4.210264603330193e-05, 0.008642959372461137], \"intercept\": 210.72601925961888}}";
            //Container.UseInstance(typeof(IConversionFunctions), new ConversionFunctionsSharad(sharad_json_8));

            //var alphasense_json_729 = 
            //    "{\"NO2\":{\"WE_E0_mV\":306,\"WE_S0_mV\":1,\"WE_0_mV\":307,\"AE_E0_mV\":300,\"AE_S0_mV\":2,\"AE_0_mV\":302,\"Sensitivity_nA_ppb\":-0.382,\"Sensitivity_NO2_nA_ppb\":-0.382,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.278,\"Sensitivity_NO2_mV_ppb\":0.278},\"O3\":{\"WE_E0_mV\":424,\"WE_S0_mV\":8,\"WE_0_mV\":432,\"AE_E0_mV\":410,\"AE_S0_mV\":2,\"AE_0_mV\":412,\"Sensitivity_nA_ppb\":-0.559,\"Sensitivity_NO2_nA_ppb\":-0.380,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.408,\"Sensitivity_NO2_mV_ppb\":0.277},\"CO\":{\"WE_E0_mV\":269,\"WE_S0_mV\":23,\"WE_0_mV\":292,\"AE_E0_mV\":255,\"AE_S0_mV\":-1,\"AE_0_mV\":254,\"Sensitivity_nA_ppb\":0.348,\"Sensitivity_NO2_nA_ppb\":0,\"PCB_gain_mV_nA\":0.80,\"Sensitivity_mV_ppb\":0.278,\"Sensitivity_NO2_mV_ppb\":0}}";
            //var alphasense_json_034 =
            //    "{\"NO2\":{\"WE_E0_mV\":306,\"WE_S0_mV\":1,\"WE_0_mV\":307,\"AE_E0_mV\":300,\"AE_S0_mV\":2,\"AE_0_mV\":302,\"Sensitivity_nA_ppb\":-0.382,\"Sensitivity_NO2_nA_ppb\":-0.382,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.278,\"Sensitivity_NO2_mV_ppb\":0.278},\"O3\":{\"WE_E0_mV\":424,\"WE_S0_mV\":8,\"WE_0_mV\":432,\"AE_E0_mV\":410,\"AE_S0_mV\":2,\"AE_0_mV\":412,\"Sensitivity_nA_ppb\":-0.559,\"Sensitivity_NO2_nA_ppb\":-0.380,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.408,\"Sensitivity_NO2_mV_ppb\":0.277},\"CO\":{\"WE_E0_mV\":269,\"WE_S0_mV\":23,\"WE_0_mV\":292,\"AE_E0_mV\":255,\"AE_S0_mV\":-1,\"AE_0_mV\":254,\"Sensitivity_nA_ppb\":0.348,\"Sensitivity_NO2_nA_ppb\":0,\"PCB_gain_mV_nA\":0.80,\"Sensitivity_mV_ppb\":0.278,\"Sensitivity_NO2_mV_ppb\":0}}";
            //var alphasense_json_007 =
            //    "{\"NO2\":{\"WE_E0_mV\":297,\"WE_S0_mV\":6,\"WE_0_mV\":303,\"AE_E0_mV\":305,\"AE_S0_mV\":1,\"AE_0_mV\":306,\"Sensitivity_nA_ppb\":-0.392,\"Sensitivity_NO2_nA_ppb\":-0.392,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.286,\"Sensitivity_NO2_mV_ppb\":0.286},\"O3\":{\"WE_E0_mV\":410,\"WE_S0_mV\":10,\"WE_0_mV\":420,\"AE_E0_mV\":411,\"AE_S0_mV\":1,\"AE_0_mV\":412,\"Sensitivity_nA_ppb\":-0.436,\"Sensitivity_NO2_nA_ppb\":-0.438,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.318,\"Sensitivity_NO2_mV_ppb\":0.319},\"CO\":{\"WE_E0_mV\":273,\"WE_S0_mV\":43,\"WE_0_mV\":316,\"AE_E0_mV\":272,\"AE_S0_mV\":0,\"AE_0_mV\":272,\"Sensitivity_nA_ppb\":0.240,\"Sensitivity_NO2_nA_ppb\":0,\"PCB_gain_mV_nA\":0.80,\"Sensitivity_mV_ppb\":0.192,\"Sensitivity_NO2_mV_ppb\":0}}";
            //var alphasense_json_003 =
            //    "{\"NO2\":{\"WE_E0_mV\":300,\"WE_S0_mV\":6,\"WE_0_mV\":306,\"AE_E0_mV\":289,\"AE_S0_mV\":1,\"AE_0_mV\":290,\"Sensitivity_nA_ppb\":-0.392,\"Sensitivity_NO2_nA_ppb\":-0.392,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.286,\"Sensitivity_NO2_mV_ppb\":0.286},\"O3\":{\"WE_E0_mV\":411,\"WE_S0_mV\":7,\"WE_0_mV\":418,\"AE_E0_mV\":416,\"AE_S0_mV\":2,\"AE_0_mV\":418,\"Sensitivity_nA_ppb\":-0.388,\"Sensitivity_NO2_nA_ppb\":-0.377,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.283,\"Sensitivity_NO2_mV_ppb\":0.275},\"CO\":{\"WE_E0_mV\":270,\"WE_S0_mV\":55,\"WE_0_mV\":325,\"AE_E0_mV\":267,\"AE_S0_mV\":3,\"AE_0_mV\":270,\"Sensitivity_nA_ppb\":0.274,\"Sensitivity_NO2_nA_ppb\":0,\"PCB_gain_mV_nA\":0.80,\"Sensitivity_mV_ppb\":0.219,\"Sensitivity_NO2_mV_ppb\":0}}";
            //var alphasense_json_032 =
            //    "{\"NO2\":{\"WE_E0_mV\":301,\"WE_S0_mV\":-1,\"WE_0_mV\":300,\"AE_E0_mV\":282,\"AE_S0_mV\":-11,\"AE_0_mV\":271,\"Sensitivity_nA_ppb\":-0.387,\"Sensitivity_NO2_nA_ppb\":-0.387,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.282,\"Sensitivity_NO2_mV_ppb\":0.282},\"CO\":{\"WE_E0_mV\":280,\"WE_S0_mV\":26,\"WE_0_mV\":306,\"AE_E0_mV\":275,\"AE_S0_mV\":6,\"AE_0_mV\":281,\"Sensitivity_nA_ppb\":0.363,\"Sensitivity_NO2_nA_ppb\":0,\"PCB_gain_mV_nA\":0.80,\"Sensitivity_mV_ppb\":0.290,\"Sensitivity_NO2_mV_ppb\":0},\"03\":{\"WE_E0_mV\":413,\"WE_S0_mV\":7,\"WE_0_mV\":420,\"AE_E0_mV\":426,\"AE_S0_mV\":1,\"AE_0_mV\":427,\"Sensitivity_nA_ppb\":-0.590,\"Sensitivity_NO2_nA_ppb\":-0.371,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.489,\"Sensitivity_NO2_mV_ppb\":0.270}}";
            //var alphasense_json_033 =
            //    "{\"NO2\":{\"WE_E0_mV\":309,\"WE_S0_mV\":1,\"WE_0_mV\":310,\"AE_E0_mV\":305,\"AE_S0_mV\":1,\"AE_0_mV\":306,\"Sensitivity_nA_ppb\":-0.432,\"Sensitivity_NO2_nA_ppb\":-0.432,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.315,\"Sensitivity_NO2_mV_ppb\":0.315},\"CO\":{\"WE_E0_mV\":276,\"WE_S0_mV\":43,\"WE_0_mV\":319,\"AE_E0_mV\":277,\"AE_S0_mV\":13,\"AE_0_mV\":290,\"Sensitivity_nA_ppb\":0.338,\"Sensitivity_NO2_nA_ppb\":0,\"PCB_gain_mV_nA\":0.80,\"Sensitivity_mV_ppb\":0.270,\"Sensitivity_NO2_mV_ppb\":0},\"O3\":{\"WE_E0_mV\":410,\"WE_S0_mV\":7,\"WE_0_mV\":417,\"AE_E0_mV\":413,\"AE_S0_mV\":1,\"AE_0_mV\":414,\"Sensitivity_nA_ppb\":-0.587,\"Sensitivity_NO2_nA_ppb\":-0.425,\"PCB_gain_mV_nA\":-0.73,\"Sensitivity_mV_ppb\":0.428,\"Sensitivity_NO2_mV_ppb\":0.310}}";
            //Container.UseInstance(typeof(IConversionFunctions), new ConversionFunctionsAlphasense(alphasense_json_729));

            container.RegisterInstance(typeof(ISettingsData), SettingsData.Default);
            container.RegisterSingleton<IAppProperties, AppProperties>();
            //Container.Register<AppProperties>(Reuse.Singleton);
        }
    }
}
