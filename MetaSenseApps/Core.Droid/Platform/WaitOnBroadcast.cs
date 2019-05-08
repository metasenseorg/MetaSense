using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using NodeLibrary;

namespace Core.Droid.Platform
{
    public class WaitOnBroadcastReceiver<T> : BroadcastReceiver
    {
        private Task _task;
        private CancellationTokenSource _tokenSource;
        private bool _done;
        public T ReturnValue;
        public WaitOnBroadcastReceiver()
        {
            _done = false;
            _tokenSource = new CancellationTokenSource();
            _task = Task.Delay(-1, _tokenSource.Token);
        }
        public async Task<T> Complete()
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
        public event EventHandler<Intent> ReceiveEvent;
        public override void OnReceive(Context context, Intent intent)
        {
            _done = true;
            ReceiveEvent?.Invoke(context, intent);
            _tokenSource.Cancel();
        }
    }

}