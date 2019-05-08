using Android.App;
using Android.Content;

namespace Core.Droid.Platform
{
    public class ActivityResultMessage
    {
        public static string Key = "arm";

        public int RequestCode { get; set; }

        public Result ResultCode { get; set; }

        public Intent Data { get; set; }
    }
}