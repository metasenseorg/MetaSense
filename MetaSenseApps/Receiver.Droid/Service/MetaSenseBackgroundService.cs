using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Core.Droid;
using NodeLibrary;
using Log = NodeLibrary.Log;
using Xamarin.Forms;
using NodeLibrary.Native;
using Receiver.ViewModels;

namespace Receiver.Droid.Service
{
    [Service (Name ="metasense.BLEDataReceiverService", Label ="BLE Data Receiver Service")]
    internal class MetaSenseBackgroundService : Android.App.Service
    {
        private volatile HandlerThread _mHandlerThread;
        private Handler _mServiceHandler;

        private readonly object _inProgressLock = new object();

        private MetaSenseMainDataModel _dataModel;
        private bool _inProgress;

        private const int OngoingNotificationId = 111;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            _dataModel = new MetaSenseMainDataModel(new SensorLocationDroid(this), DependencyService.Get<IBLEUtils>());
            _mHandlerThread = new HandlerThread("MetaSenseBackgroundService.HandlerThread");
            _mHandlerThread.Start();
            // An Android service handler is a handler running on a specific background thread.
            _mServiceHandler = new Handler(_mHandlerThread.Looper);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            _dataModel?.StopCurrentNode();
            _mHandlerThread.Quit();
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            _mServiceHandler.Post(() => {
                lock (_inProgressLock)
                {
                    if (_inProgress)
                    {
                        return;
                    }
                    _inProgress = true;
                }
                try
                {
                    var nodeMac = intent == null ? SettingsData.Default.Get("node.mac") : intent.GetStringExtra("MAC");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    _dataModel.StartNode(nodeMac);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    StartForegroundMetasense();
                    return;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                lock (_inProgressLock)
                {
                    Log.Debug("Failed, could not find any mac to connect to");
                    _inProgress = false;
                    StopSelf();
                }
            });
            return StartCommandResult.Sticky;
        }

        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }
        //private void NotifyMessage(string msg)
        //{
        //    var nMgr = (NotificationManager)GetSystemService(NotificationService);
        //    var builder = new NotificationCompat.Builder(this);
        //    var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)), 0);
        //    var notification = builder.SetContentIntent(pendingIntent)
        //            .SetSmallIcon(Resource.Drawable.icon).SetContentTitle("MetaSense Data Reciever")
        //            .SetContentText(msg).Build();
        //    nMgr.Notify(0, notification);
        //}
        private void StartForegroundMetasense()
        {
            var builder = new NotificationCompat.Builder(this);
            var notificationIntent = new Intent(this, typeof(MainActivity));
            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, 0);
            var notification = builder.SetContentIntent(pendingIntent)
                    .SetSmallIcon(Resource.Id.icon).SetTicker("MetaSense").SetWhen(CurrentTimeMillis())
                    .SetContentTitle("MetaSense Data Reciever")
                    .SetContentText("MetaSense is actively receiving BLE data").Build();
            StartForeground(OngoingNotificationId, notification);
        }
    }
}