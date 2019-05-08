using System;
using System.Threading;
using System.Threading.Tasks;

namespace NodeLibrary
{
    public class WaitableResponse<T>
    {
        private T _response;
        private readonly Task _completed;
        readonly CancellationTokenSource _tokenSource;
        //public WaitableResponse() : this(-1) { }
        public WaitableResponse(int delayMs)
        {
            _tokenSource = new CancellationTokenSource();
            _completed = Task.Delay(delayMs, _tokenSource.Token);
        }
        public void ResolveWaitOn(T result)
        {
            _response = result;
            _tokenSource.Cancel();
        }
        public async Task<T> WaitOn()
        {
            try
            {
                await _completed;
            }
            catch (Exception)
            {
                return _response;
            }
            return default(T);
        }
    }
}
