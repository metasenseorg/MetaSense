using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using NodeLibrary;
using Xamarin.Forms;

namespace Core.Droid.Platform
{
    public class WaitOnActivity 
    {
        private static readonly object LockGenerator= new object();
        private static int _messageIdGenerator=999;
        private readonly Task _task;
        private readonly CancellationTokenSource _tokenSource;
        private bool _done;

        public int RequestId { get; }
        public ActivityResultMessage ReturnValue { get; private set; }

        public WaitOnActivity()
        {
            lock (LockGenerator)
            {
                RequestId = _messageIdGenerator++;
            }
            _done = false;
            _tokenSource = new CancellationTokenSource();
            _task = Task.Delay(-1, _tokenSource.Token);
            MessagingCenter.Subscribe<ActivityResultMessage>(this, $"#{RequestId}", message =>
            {
                if (message.RequestCode != RequestId) return;
                ReturnValue = message;
                _done = true;
                _tokenSource.Cancel();
                ReceiveEvent?.Invoke(this, ReturnValue);
                MessagingCenter.Unsubscribe<ActivityResultMessage>(this, $"#{RequestId}");
            });
        }
        ~WaitOnActivity()
        {
            if (!_done)
            {
                MessagingCenter.Unsubscribe<ActivityResultMessage>(this, $"#{RequestId}");
            }
            _tokenSource?.Cancel();
        }

        public static void ForwardActivityResult(int requestCode, Result resultCode, Intent data)
        {
            MessagingCenter.Send(
                new ActivityResultMessage { RequestCode = requestCode, ResultCode = resultCode, Data = data }, $"#{requestCode}");
        }
        public event EventHandler<ActivityResultMessage> ReceiveEvent;
        public async Task<ActivityResultMessage> CompleteActivity()
        {
            try
            {
                if (!_done)
                    await _task;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return ReturnValue;
        }
    }

}