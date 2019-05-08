using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NodeLibrary
{
    public class WaitOnEvent<T>
    {
        private readonly SortedSet<CancellationTokenSource> _tokens = new SortedSet<CancellationTokenSource>();
        public event EventHandler<T> Event;
        public async Task<T> ReceiveEventAsync()
        {
            var canc = new CancellationTokenSource();
            var retValue = default(T);
            EventHandler<T> h = (o, val) => { retValue = val; canc.Cancel(); };
            Event += h;
            var t = Task.Delay(-1, canc.Token);
            _tokens.Add(canc);
            try
            {
                await t;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            Event -= h;
            _tokens.Remove(canc);
            return retValue; 
        }
        public void OnEvent(T value)
        {
            Event?.Invoke(this, value);
        }
        ~WaitOnEvent()
        {
            foreach (var t in _tokens)
                t.Cancel();
        }
    }
}