using System;
using Android.Content;
using NodeLibrary;
using NodeLibrary.Native;
using Receiver.Droid;
using Receiver.Droid.Service;
using Xamarin.Forms;
using Log = NodeLibrary.Log;

[assembly: Dependency(typeof(BLEBackgorundReaderDroid))]
namespace Receiver.Droid
{
    internal sealed class BLEBackgorundReaderDroid : IBLEBackgroundReader
    {
        public readonly string Tag = "BLEBackgorundReaderDroid";
        public event EventHandler<MetaSenseMessage> BackgroundBLEServiceMessageReceived;
        private readonly Context _context;
        public BLEBackgorundReaderDroid() : this(Forms.Context) { }
        public BLEBackgorundReaderDroid(Context context)
        {
            _context = context;
        }
        private void OnBackgroundBLEServiceMessageReceived(MetaSenseMessage msg)
        {
            BackgroundBLEServiceMessageReceived?.Invoke(this, msg);
        }

        public void StartBackgroundBLEService(string mac)
        {
            MessagingCenter.Subscribe<MetaSenseMessage>(this, "MetaSenseRead", (message) =>
            {
                try
                {
                    OnBackgroundBLEServiceMessageReceived(message);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            });
            var intent = new Intent(_context, typeof(MetaSenseBackgroundService));
            intent.PutExtra("MAC", mac);
            var res = _context.StartService(intent);
            Log.Debug(res.ToString());
        }
        public void StopBackgroundBLEService()
        {
            var intent = new Intent(_context, typeof(MetaSenseBackgroundService));
            //intent.PutExtra("MAC", mac);
            var res = _context.StopService(intent);
            Log.Debug(res.ToString());
            MessagingCenter.Unsubscribe<MetaSenseMessage>(this, "MetaSenseRead");
        }
    }
}