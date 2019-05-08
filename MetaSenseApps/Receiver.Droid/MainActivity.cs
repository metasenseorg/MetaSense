using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Core.Droid;
using Core.Droid.Platform;
using NodeLibrary;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Receiver.Droid
{
    [Activity(Label = "MetaSenseDataReceiver", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.FormsMaps.Init(this, bundle);

            Log.AdditionalLoggers.Add(new Logger());

            LoadApplication(new App());
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // Send trough the messaging sender (maybe someone subscribes to it)
            WaitOnActivity.ForwardActivityResult(requestCode, resultCode, data);
            MessagingCenter.Send(
                new ActivityResultMessage { RequestCode = requestCode, ResultCode = resultCode, Data = data }, ActivityResultMessage.Key);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            //base.OnRequestPermissionsResult(requestCode, permissions);
            MessagingCenter.Send(
               new RequestPermissionsResultMessage { RequestCode = requestCode, Permissions = permissions }, RequestPermissionsResultMessage.Key);
        }
    }

 
}

