using System;

namespace BackendAPI.Data
{
    public class TimeManagementUtils
    {
        public static DateTime UnixToDateTime(double ts)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(ts).ToLocalTime();
            return dtDateTime;
        }

        public static double DateTimeToUnix(DateTime ts)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return ts.ToUniversalTime().Subtract(dtDateTime).TotalSeconds;
        }
    }
}