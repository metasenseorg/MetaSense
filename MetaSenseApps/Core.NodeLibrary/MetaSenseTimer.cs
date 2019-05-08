using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NodeLibrary
{
    /// <summary>
    /// Timer for calculating relative time (how long ago something occurred).
    /// </summary>
    public class MetaSenseTimer
    {
        public static readonly String ABSOLUTE_TIME_NEEDED = "abs_time";
        public static readonly String ACTIVE_RECENT = "just now";
        readonly Action _action;
        readonly TimeSpan _interval;
        private bool _continueTimer;
        public static bool _dictIsInitialized;
        public static Dictionary<int, string> _relativeTimesDict;

        /// <summary>
        /// Initializes the interval and action to the specified objects.
        /// </summary>
        /// <param name="interval">The interval for how often the callback should fire</param>
        /// <param name="action">The action to do when the callback fires</param>
        public MetaSenseTimer(TimeSpan interval, Action action)
        {
            _action = action;
            _interval = interval;
            _dictIsInitialized = false;
        }

        /// <summary>
        /// Start the timer.
        /// </summary>
        public void StartTimer()
        {
            _continueTimer = true;

            Device.StartTimer(_interval, () =>
            {
                if (_continueTimer)
                {
                    _action();
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Stop the timer from continuing.
        /// </summary>
        public void StopTimer()
        {
            _continueTimer = false;
        }

        /// <summary>
        /// Initialize the dictionary for amount of time passed corresponding to relative time
        /// strings.
        /// </summary>
        public static void InitializeDict()
        {
            if (_dictIsInitialized)
            {
                return;
            }

            // if the amount of minutes is less than the key, then the value describes the time
            _relativeTimesDict = new Dictionary<int, string>
            {
                { 5, ACTIVE_RECENT },
                { 15, "5 minutes ago" },
                { 30, "15 minutes ago" },
                { 60, "30 minutes ago" },
                { 180, "1 hour ago" },
                { 360, "3 hours ago" },
                { 720, "6 hours ago" },
                { 1440, "today" },
                { 2880, "yesterday" },
                { int.MaxValue, ABSOLUTE_TIME_NEEDED }
            };

            _dictIsInitialized = true;
        }

        /// <summary>
        /// Determine the amount of time that has passed relatively.
        /// </summary>
        /// <param name="minutes">The amount of minutes that has passed</param>
        /// <returns>A string representing the amount of time that has passed relatively.</returns>
        public static string CalculateRelativeTime(int minutes)
        {
            List<int> dictKeys = new List<int>(_relativeTimesDict.Keys);
            dictKeys.Sort();

            foreach (var key in dictKeys)
            {
                if (minutes < key)
                {
                    return _relativeTimesDict[key];
                }
            }

            if (minutes == int.MaxValue)
            {
                return "abs_time";
            }
            else
            {
                return "unknown";
            }
        }

        /// <summary>
        /// Determine the amount of time that has passed between two times relatively.
        /// </summary>
        /// <param name="end">The ending DateTime of the time period</param>
        /// <param name="start">The starting DateTime of the time period</param>
        /// <returns>The amount of time that has passed between two times relatively.</returns>
        public static string CalculateRelativeTimeDiff(DateTime end, DateTime start)
        {
            var span = end.Subtract(start);
            int minutesDiff = (int) Math.Floor(span.TotalMinutes);
            return CalculateRelativeTime(minutesDiff);
        }
    }
}
