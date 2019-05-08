using Android.Content.PM;

namespace Core.Droid.Platform
{
    public class RequestPermissionsResultMessage
    {
        public static string Key = "rpr";

        public int RequestCode { get; set; }
        public string[] Permissions { get; set; }
        public Permission GrantResult { get; set; }
    }
}