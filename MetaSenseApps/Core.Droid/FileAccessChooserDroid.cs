using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Core.Droid;
using Core.Droid.Platform;
using NodeLibrary;
using NodeLibrary.Native;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileAccessChooserDroid))]
namespace Core.Droid
{
    internal sealed class FileAccessChooserDroid : IFileAccessChooser
    {
        private static int _requestCode;
        private readonly Dictionary<int, Task> _waitList = new Dictionary<int, Task>();
        private readonly Dictionary<int, CancellationTokenSource> _cancList = new Dictionary<int, CancellationTokenSource>();
        private readonly Dictionary<int, ActivityResultMessage> _resultList = new Dictionary<int, ActivityResultMessage>();
        private readonly object _thisLock = new object();
        public FileAccessChooserDroid()
        {
            MessagingCenter.Subscribe<ActivityResultMessage>(this, ActivityResultMessage.Key, message =>
            {
                lock (_thisLock)
                {
                    var rc = message.RequestCode;
                    if (_cancList.ContainsKey(rc))
                    {
                        CancellationTokenSource tokenSource = _cancList[rc];
                        _cancList.Remove(rc);
                        _waitList.Remove(rc);
                        _resultList[rc] = message;
                        tokenSource.Cancel();
                    }
                }
            });
        }
        private int RequestCode
        {
            get
            {
                lock (_thisLock)
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    var rc = _requestCode++;
                    _waitList.Add(rc, Task.Delay(-1, tokenSource.Token));
                    _cancList.Add(rc, tokenSource);
                    return rc;
                }
            }
        }
        private ActivityResultMessage GetResponse(int rc)
        {
            lock (_thisLock)
            {
                var result = _resultList[rc];
                _resultList.Remove(rc);
                return result;
            }
        }
        private async Task<ActivityResultMessage> StartActivityForResult(Intent intent)
        {
            int rc = RequestCode;
            ((Activity)Forms.Context).StartActivityForResult(intent, rc);
            try
            {
                await _waitList[rc];
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return GetResponse(rc);
        }
        public async Task<Stream> OpenFileForWrite()
        {
            var intent = new Intent(Intent.ActionCreateDocument);
            // Filter to only show results that can be "opened", such as
            // a file (as opposed to a list of contacts or timezones).
            intent.AddCategory(Intent.CategoryOpenable);
            // Create a file with the requested MIME type.
            intent.SetType("text/csv");
            var timestamp = DateTime.Now;
            intent.PutExtra(Intent.ExtraTitle, "MetaSenseReadings-"+timestamp.ToString("yyyyMMddHHmmss")+".csv");
            var res = await StartActivityForResult(intent);
            if (res.ResultCode != Result.Ok) return null;
            if (res.Data == null) return null;
            var act = Forms.Context as Activity;
            return act?.ContentResolver.OpenOutputStream(res.Data.Data, "w");
        }
        public async Task<Stream> OpenFileForRead(string mime)
        {
            var intent = new Intent(Intent.ActionOpenDocument);
            // Filter to only show results that can be "opened", such as
            // a file (as opposed to a list of contacts or timezones).
            intent.AddCategory(Intent.CategoryOpenable);
            // Create a file with the requested MIME type.
            intent.SetType(mime);
            //var timestamp = DateTime.Now;
            //intent.PutExtra(Intent.ExtraTitle, "MetaSenseReadings-" + timestamp.ToString("yyyyMMddHHmmss") + ".csv");
            try
            {
                var res = await StartActivityForResult(intent);
                if (res.ResultCode != Result.Ok) return null;
                if (res.Data == null) return null;
                var act = Forms.Context as Activity;
                return act?.ContentResolver.OpenInputStream(res.Data.Data);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}