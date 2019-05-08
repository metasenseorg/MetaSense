using Xamarin.Forms;
using Prism.Navigation;

namespace Receiver.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainPage : MasterDetailPage, IMasterDetailPageOptions
    {
        //private MainViewModel vm;
        public MainPage()
        {
            InitializeComponent();
            //vm= BindingContext as MainViewModel;
            //if (vm!=null) vm.NavigationEvent += NavigationEventHandler;
        }

        public bool IsPresentedAfterNavigation => Device.Idiom != TargetIdiom.Phone;
        //~MainPage()
        //{
        //    if (vm != null) vm.NavigationEvent -= NavigationEventHandler;
        //}
        //protected void NavigationEventHandler(object source, string args)
        //{
        //    try
        //    {
        //        //if (args.Equals("bleSelect"))
        //        //    ChangeDetail(new BluetoothPage());
        //        //if (args.Equals("GraphPage"))
        //        //    ChangeDetail(new Graph());
        //        //if (args.Equals("DataPage"))
        //        //    ChangeDetail(new Data());
        //        //if (args.Equals("LocationPage"))
        //        //    ChangeDetail(new LocationPage());
        //        //if (args.Equals("DeleteDB"))
        //        //    ClearDb();
        //        //if (args.Equals("ExportDB"))
        //        //    ExportDb();
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Trace(e.Message);
        //        // ignored
        //    }
        //    finally
        //    {
        //        IsPresented = false;
        //    }
        //}
        //private void ChangeDetail(Page page)
        //{
        //    var navigationPage = Detail as NavigationPage;
        //    if (navigationPage != null)
        //    {
        //        navigationPage.PushAsync(page);
        //        return;
        //    }
        //    Detail = new NavigationPage(page);
        //}


    }
}
