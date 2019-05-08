using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary
{
    /// <summary>
    /// Median filter that takes the median of individual measurements across previous
    /// MetaSenseMessages and combines the medians into one MetaSenseMessage.
    /// </summary>
    public class MedianFilter
    {
        private int _windowSize;
        private const int DEFAULT_WINDOW_SIZE = 5;
        public Queue<MetaSenseMessage> _prevMessages;

        /// <summary>
        /// Initialize the window size of the median filter to the default value.
        /// </summary>
        public MedianFilter() : this(DEFAULT_WINDOW_SIZE) { }

        /// <summary>
        /// Initialize the window size of the median filter to the specified value.
        /// </summary>
        /// <param name="windowSize">The amount of reads included when calculating the
        ///     median</param>
        public MedianFilter(int windowSize)
        {
            _windowSize = windowSize;
            _prevMessages = new Queue<MetaSenseMessage>();
        }

        /// <summary>
        /// Check whether there is a sufficient amount of previous reads to apply the median
        /// filter.
        /// </summary>
        /// <returns>True if the median filter can be applied; false otherwise.</returns>
        public bool CanApplyMedianFilter()
        {
            return _prevMessages.Count() == _windowSize;
        }

        /// <summary>
        /// Find the median of each measurement in each previous message within the window and
        /// combine them into one message serving as the median of the previous measurements.
        /// 
        /// Note: this median filter currently does not take the median of the Voc and Co2
        /// readings.
        /// </summary>
        /// <returns>The median of the previous messages or the previous message if there are not
        ///     enough previous messages.</returns>
        public MetaSenseMessage ApplyMedianFilter(MetaSenseMessage lastMessage)
        {
            // do not apply median filter on a duplicated message
            if (_prevMessages.Count() > 0 && lastMessage.Ts == _prevMessages.Last().Ts)
            {
                return lastMessage;
            }

            UpdatePreviousMessages(lastMessage);

            if (!CanApplyMedianFilter())
            {
                return lastMessage;
            }

            var prevMessagesCopy = new Queue<MetaSenseMessage>(_prevMessages);
            var prevRawGasReadingArrs = new List<double[]>();
            var prevHuPrReadingArrs = new List<double[]>();
            var mediansDict = new Dictionary<string, double>();
            
            int i = 0;
            while (i < _windowSize)
            {
                var previousMessage = prevMessagesCopy.Dequeue();
                prevRawGasReadingArrs.Add(previousMessage.Raw.ToDoubleArray());
                prevHuPrReadingArrs.Add(previousMessage.HuPr.ToArray());
                i++;
            }

            AddMediansToDict(mediansDict, MetaSenseRawGasReadings.GetArrayLabels(),
                prevRawGasReadingArrs);
            AddMediansToDict(mediansDict, MetaSenseRawHuPrReadings.GetArrayLabels(),
                prevHuPrReadingArrs);

            /* debug printing
            StringBuilder sb = new StringBuilder();
            sb.Append("MEDIANS: [" + Environment.NewLine);
            foreach (KeyValuePair<string, double> pair in mediansDict)
            {
                sb.Append($"{pair.Key}: {pair.Value}  ");
            }
            sb.Append(Environment.NewLine);
            Log.Trace(sb.ToString() + "]");
            */

            return CreateMedianMessage(mediansDict, lastMessage);
        }

        /// <summary>
        /// Add the median of each column in measurements to the specified dictionary.
        /// </summary>
        /// <param name="mediansDict">The dictionary to add medians to</param>
        /// <param name="keys">The key for the median of each column of measurements</param>
        /// <param name="measurements">Contains double arrays whose type of measurements align at
        ///     each index</param>
        private void AddMediansToDict(Dictionary<string, double> mediansDict, string[] keys,
                                      List<double[]> measurements)
        {
            var measurementsMat = Matrix<double>.Build.DenseOfRowArrays(measurements);
            var colEnumerator = measurementsMat.EnumerateColumns();

            var i = 0;
            foreach (var col in colEnumerator)
            {
                mediansDict.Add(keys[i], Statistics.Median(col));
                i++;
            }
        }

        /// <summary>
        /// Update the previous messages so that it contains the most recent message and removes
        /// the oldest message if necessary.
        /// </summary>
        /// <param name="lastMessage">The most recent message to be added to the previous
        ///     messages</param>
        private void UpdatePreviousMessages(MetaSenseMessage lastMessage)
        {
            if (_prevMessages.Count() >= _windowSize)
            {
                _prevMessages.Dequeue();
            }
            _prevMessages.Enqueue(lastMessage);
        }

        /// <summary>
        /// Create a MetaSenseMessage using the medians from the given dictionary.
        /// </summary>
        /// <param name="mediansDict">The dictionary containing the medians for
        ///     measurements</param>
        /// <param name="lastMessage">The most recent message</param>
        /// <returns></returns>
        private MetaSenseMessage CreateMedianMessage(Dictionary<string, double> mediansDict,
                                                     MetaSenseMessage lastMessage)
        {
            return new MetaSenseMessage
            {
                Raw = new MetaSenseRawGasReadings
                {
                    Rng = (int)Math.Round(mediansDict["Rng"]),
                    S1A = (int)Math.Round(mediansDict["S1A"]),
                    S1W = (int)Math.Round(mediansDict["S1W"]),
                    S2A = (int)Math.Round(mediansDict["S2A"]),
                    S2W = (int)Math.Round(mediansDict["S2W"]),
                    S3A = (int)Math.Round(mediansDict["S3A"]),
                    S3W = (int)Math.Round(mediansDict["S3W"]),
                    Temperature = (int)Math.Round(mediansDict["Temperature"]),
                    Voc = (int)Math.Round(mediansDict["Voc"])
                },
                HuPr = new MetaSenseRawHuPrReadings
                {
                    HumiditySensorTemperatureCelsius =
                        mediansDict["HumiditySensorTemperatureCelsius"],
                    HumiditySensorHumidityPercent =
                        mediansDict["HumiditySensorHumidityPercent"],
                    BarometricSensorPressureMilliBar =
                        mediansDict["BarometricSensorPressureMilliBar"],
                    BarometricSensorTemperatureCelsius =
                        mediansDict["BarometricSensorTemperatureCelsius"]
                },
                Co2 = lastMessage.Co2,
                Voc = lastMessage.Voc,
                Loc = lastMessage.Loc,
                Req = lastMessage.Req,
                Ts = lastMessage.Ts,

                SSd = lastMessage.SSd,
                SWifi = lastMessage.SWifi,
                StreamBLE = lastMessage.StreamBLE,
                WifiEn = lastMessage.WifiEn,
                SleepEn = lastMessage.SleepEn,
                UsbEn = lastMessage.UsbEn,
                UsbPass = lastMessage.UsbPass,
                Co2En = lastMessage.Co2En,
                VocEn = lastMessage.VocEn,

                Power = lastMessage.Power,
                SInter = lastMessage.SInter,

                FlagSD = lastMessage.FlagSD,
                FlagWifi = lastMessage.FlagWifi,
                FlagBLE = lastMessage.FlagBLE,

                Ssid = lastMessage.Ssid,
                Pass = lastMessage.Pass,
                NodeId = lastMessage.NodeId,
                MacAddr = lastMessage.MacAddr
            };
        }
    }
}
