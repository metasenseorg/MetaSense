using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeLibrary.Native;
using SQLite;
using Xamarin.Forms;

namespace NodeLibrary
{
    [Table("Logs")]
    public class LogElement
    {
        [PrimaryKey, AutoIncrement]
        public long Key { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
        public string Message { get; set; }
        public LogElement() { }
        public LogElement(string type, string tag, string message)
        {
            Type = type;
            Tag = tag;
            Message = message;
        }
    }
    [Table("Items")]
    public class Item
    {
        [PrimaryKey]
        public string Key { get; set; }
        public string Value { get; set; }
        public Item() { }
        public Item(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
    // DM: add counts
    /// <summary>
    /// Consists of a key associated with a long value intended for keeping track of counts, such
    /// as the cache clock.
    /// </summary>
    [Table("Counts")]
    public class Count
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        [Unique]
        public string Key { get; set; }
        public long Value { get; set; }
        public Count() { }
        /// <summary>
        /// Initialize the key and value to the specified values.
        /// </summary>
        /// <param name="key">The desired key for the count</param>
        /// <param name="value">The desired value for the count</param>
        public Count(string key, long value)
        {
            Key = key;
            Value = value;
        }
    }
    // DM: end
    [Table("Readings")]
    public class Read
    {
        [PrimaryKey, AutoIncrement]
        public long Key { get; set; }
        public long Ts { get; set; }
        public int Rng { get; set; }
        public int S1A { get; set; }
        public int S1W { get; set; }
        public int S2A { get; set; }
        public int S2W { get; set; }
        public int S3A { get; set; }
        public int S3W { get; set; }
        public int Pt { get; set; }
        public int Nc { get; set; }

        // ReSharper disable InconsistentNaming
        public double HT { get; set; }
        public double HH { get; set; }
        public double BP { get; set; }
        public double BT { get; set; }
        // ReSharper restore InconsistentNaming

        public double? Co2 { get; set; }
        public double? VPp { get; set; }
        public double? VIp { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public float? Accuracy { get; set; }
        public float? Bearing { get; set; }
        public float? Speed { get; set; }

        public Read() { }
        public Read(long ts, MetaSenseRawGasReadings gas, MetaSenseRawHuPrReadings humPr, MetaSenseCo2Readings co2, MetaSenseVocReadings voc, LocationInfo location)
        {
            Ts = ts;
            Nc = gas.Voc;
            Pt = gas.Temperature;
            Rng = gas.Rng;
            S1A = gas.S1A;
            S1W = gas.S1W;
            S2A = gas.S2A;
            S2W = gas.S2W;
            S3A = gas.S3A;
            S3W = gas.S3W;

            BP = humPr.BarometricSensorPressureMilliBar;
            BT = humPr.BarometricSensorTemperatureCelsius;
            HH = humPr.HumiditySensorHumidityPercent;
            HT = humPr.HumiditySensorTemperatureCelsius;

            if (co2 != null)
                Co2 = co2.Co2;

            if (voc != null)
            {
                VPp = voc.VPp;
                VIp = voc.VIp;
            }

            Latitude = location.Latitude;
            Longitude = location.Longitude;
            Altitude = location.Altitude;
            if (location.Radius != null) Accuracy = (float)location.Radius;
            if (location.Direction != null) Bearing = (float)location.Direction;
            if (location.Speed != null) Speed = (float)location.Speed;
        }
        public MetaSenseRawGasReadings GetGas()
        {
            var val = new MetaSenseRawGasReadings
            {
                Voc = Nc,
                Temperature = Pt,
                Rng = Rng,
                S1A = S1A,
                S1W = S1W,
                S2A = S2A,
                S2W = S2W,
                S3A = S3A,
                S3W = S3W
            };
            return val;
        }
        public MetaSenseRawHuPrReadings GetHuPr()
        {
            var val = new MetaSenseRawHuPrReadings
            {
                BarometricSensorPressureMilliBar = BP,
                BarometricSensorTemperatureCelsius = BT,
                HumiditySensorHumidityPercent = HH,
                HumiditySensorTemperatureCelsius = HT
            };
            return val;
        }
        public MetaSenseCo2Readings GetCo2()
        {
            if (!Co2.HasValue) return null;
            var val = new MetaSenseCo2Readings {Co2 = Co2.Value};
            return val;
        }
        public MetaSenseVocReadings GetVoc()
        {
            if (!VIp.HasValue || !VPp.HasValue) return null;
            var val = new MetaSenseVocReadings
            {
                VIp = VIp.Value,
                VPp = VPp.Value
            };
            return val;
        }
        public LocationInfo GetLocation()
        {
            var loc = new LocationInfo(
            Latitude ?? 0,
            Longitude ?? 0,
            Accuracy ?? 0,
            Altitude ?? 0,
            Speed ?? 0,
            Bearing ?? 0, 
            MetaSenseNode.UnixToDateTime(Ts));
            return loc;
        }
    }
    // DM: add neural network conversions
    /// <summary>
    /// Neural network input and output and the "relative" time (order) that it was inserted/last
    /// accessed in the cache.
    /// 
    /// Note: The timestamps should be monotonically increasing, since any NNConversion with a
    /// timestamp of x was inserted/accessed more recently than any NNConversion with a timestamp
    /// less than x.
    /// 
    /// Note #2: Currently not in use.
    /// </summary>
    [Table("NNConversions")]
    public class NNConversion
    {
        [PrimaryKey, AutoIncrement]
        public long Key { get; set; }
        [Indexed(Name = "FeaturesIndex", Order = 1, Unique = true)]
        public double NO2 { get; set; }
        [Indexed(Name = "FeaturesIndex", Order = 2, Unique = true)]
        public double O3 { get; set; }
        [Indexed(Name = "FeaturesIndex", Order = 3, Unique = true)]
        public double CO { get; set; }
        [Indexed(Name = "FeaturesIndex", Order = 4, Unique = true)]
        public double Temperature { get; set; }
        [Indexed(Name = "FeaturesIndex", Order = 5, Unique = true)]
        public double AbsoluteHumidity { get; set; }
        [Indexed(Name = "FeaturesIndex", Order = 6, Unique = true)]
        public double Pressure { get; set; }
        public double NO2ppb { get; set; }
        public double O3ppb { get; set; }
        public double COppm { get; set; }
        [Indexed(Name = "TimeIndex")]
        public long Ts { get; set; }

        public NNConversion() { }

        /// <summary>
        /// Initialize the input and output of the conversion to the specified values.
        /// </summary>
        /// <param name="no2">The voltage difference in mV of the NO2 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="o3">The voltage difference of the O3 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="co">The voltage difference of the CO electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="temp">Temperature (degrees Celsius)</param>
        /// <param name="absHumidity">Absolute humidity</param>
        /// <param name="pres">Pressure (mbar)</param>
        /// <param name="no2PPB">The output NO2 (ppb) of the neural network</param>
        /// <param name="o3PPB">The output O3 (ppb) of the neural network</param>
        /// <param name="coPPM">The output CO (ppm) of the neural network</param>
        /// <param name="ts">The "relative" time (order) that the conversion was inserted/last
        ///     accessed</param>
        public NNConversion(double no2, double o3, double co, double temp, double absHumidity, double pres,
            double no2PPB, double o3PPB, double coPPM, long ts)
        {
            NO2 = no2;
            O3 = o3;
            CO = co;
            Temperature = temp;
            AbsoluteHumidity = absHumidity;
            Pressure = pres;
            NO2ppb = no2PPB;
            O3ppb = o3PPB;
            COppm = coPPM;
            Ts = ts;
        }

        /// <summary>
        /// Returns the amount of each pollutant gas output by the neural network.
        /// </summary>
        /// <returns>A double array of the amount of each pollutant gas in the order of:
        ///     NO2 (ppb), O3 (ppb), CO (ppm).</returns>
        public double[] GetConversions()
        {
            return new double[] { NO2ppb, O3ppb, COppm };
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string representing the input and output of the conversion.</returns>
        public override string ToString()
        {
            return $"no2: {NO2}, o3: {O3}, co: {CO}, temp: {Temperature}, "
                + $"absHumidity: {AbsoluteHumidity}, pres: {Pressure}, no2_ppb: {NO2ppb}, "
                + $"o3_ppb: {O3ppb}, co_ppm: {COppm}, ts: {Ts}, key: {Key}";
        }
    }
    // DM: end
    // DM: add table of conversion outputs for the pollutant details graph to use
    /// <summary>
    /// The amount of each pollutant gas output by the neural network and the absolute time (in
    /// universal unix time) that the conversion occurred.
    /// 
    /// Note: this is used so that the pollutant details graph does not have to convert all
    /// previous reads again in order to get the hourly max AQIs.
    /// </summary>
    [Table("NNConversionOutputs")]
    public class NNConversionOutput
    {
        [PrimaryKey, AutoIncrement]
        public long Key { get; set; }
        public double NO2ppb { get; set; }
        public double O3ppb { get; set; }
        public double COppm { get; set; }
        [Indexed(Name = "TimeIndex")]
        public long Ts { get; set; }

        public NNConversionOutput() { }

        /// <summary>
        /// Initialize the output of the conversion to the specified values.
        /// </summary>
        /// <param name="no2PPB">The output NO2 (ppb) of the neural network</param>
        /// <param name="o3PPB">The output O3 (ppb) of the neural network</param>
        /// <param name="coPPM">The output CO (ppm) of the neural network</param>
        /// <param name="ts">The absolute time (in universal unix time) that the conversion
        ///     occurred</param>
        public NNConversionOutput(double no2PPB, double o3PPB, double coPPM, long ts)
        {
            NO2ppb = no2PPB;
            O3ppb = o3PPB;
            COppm = coPPM;
            Ts = ts;
        }

        /// <summary>
        /// Returns a string representing the current object.
        /// </summary>
        /// <returns>A string representing the output of the neural network.</returns>
        public override string ToString()
        {
            return $"NO2(ppb): {NO2ppb}  O3(ppb): {O3ppb}  CO(ppm): {COppm}  Ts: {Ts}";
        }

        /// <summary>
        /// Returns a GasReading representing the current object.
        /// </summary>
        /// <returns>A GasReading that contains the NO2 (ppb), O3 (ppb), and CO (ppm)
        ///     of the conversion output.</returns>
        public GasReading ToGasReading()
        {
            GasReading reading = new GasReading();
            reading.NO2ppb = NO2ppb;
            reading.O3ppb = O3ppb;
            reading.COppm = COppm;
            return reading;
        }
    }
    public interface ISettingsData
    {
        string Get(string key);
        TableQuery<Read> Readings();
        Task ExportReads(StreamWriter stream);
        int Delete(string key);
        int Set(string key, string value);
        void AddLog(string type, string tag, string message);
        int ClearLogs();
        IEnumerable<LogElement> ReadLogs();
        int AddRead(Read read);
        int ClearReads();
        IEnumerable<Tuple<DateTime,double>> LastHours(int hours, Func<Read,double> selector);
        // DM: add conversion output methods
        IEnumerable<NNConversionOutput> ReadNNConversionOutputs();
        int AddNNConversionOutput(double no2PPB, double o3PPB, double coPPM, long ts);
        int ClearNNConversionOutputs();
        // DM: end

        /* DM: add neural network and count methods
        double[] GetNNConversionResults(double no2, double o3, double co, double temp,
            double absHumidity, double pres);
        NNConversion Get(double no2, double o3, double co, double temp, double absHumidity, 
            double pres);
        IEnumerable<NNConversion> ReadNNConversions();
        int AddNNConversion(double no2, double o3, double co, double temp, double absHumidity,
            double pres, double no2PPB, double o3PPB, double coPPM);
        int UpdateNNConversionTime(double no2, double o3, double co, double temp,
            double absHumidity, double pres);
        int DeleteNNConversion(NNConversion conv);
        int DeleteNNConversionsLRU(int numToDelete);
        int ClearNNConversions();
        Count GetCount(string key);
        long GetCountValue(string key);
        int AddCount(string key);
        int IncrementCount(string key);
        IEnumerable<Count> ReadCounts();
        int DeleteCount(string key);
        int ClearCounts();
       */
    }

    public class SettingsData : ISettingsData
    {
        private readonly SQLiteConnection _database;
        public const int NN_CONVERSION_CACHE_SIZE = 1000;
        
        protected SettingsData()
        {
            _database = DependencyService.Get<IDatabase>().Connection;

            try
            {
                _database.CreateTable<Item>();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            try
            {
                _database.CreateTable<Read>();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            // DM: create conversion output table
            _database.CreateTable<NNConversionOutput>();
            // DM: end
        }
        public static SettingsData Default { get; } = new SettingsData();

        public string Get(string key)
        {
            try
            {
                return _database.Table<Item>().FirstOrDefault(x => x.Key == key).Value;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return null;
        }

        public TableQuery<Read> Readings()
        {
            return _database.Table<Read>();
        }
        public async Task ExportReads(StreamWriter stream)
        {
            StringBuilder sb = new StringBuilder();
            //IEnumerable<string> columnNames = database.GetMapping<Read>().Columns.
            //                                  Select(column => column.Name);
            //IEnumerable<string> propertyNames = database.GetMapping<Read>().Columns.
            //                                  Select(column => column.PropertyName);
            sb
                .Append("ts").Append(",")
                .Append("Rng").Append(",")
                .Append("S1A").Append(",")
                .Append("S1W").Append(",")
                .Append("S2A").Append(",")
                .Append("S2W").Append(",")
                .Append("S3A").Append(",")
                .Append("S3W").Append(",")
                .Append("PT").Append(",")
                .Append("NC").Append(",")
                .Append("BarometricSensorPressureMilliBar").Append(",")
                .Append("BarometricSensorTemperatureCelsius").Append(",")
                .Append("HumiditySensorHumidityPercent").Append(",")
                .Append("HumiditySensorTemperatureCelsius").Append(",")
                .Append("CO2").Append(",")
                .Append("vIP").Append(",")
                .Append("vPP").Append(",")
                .Append("latitude").Append(",")
                .Append("longitude").Append(",")
                .Append("altitude").Append(",")
                .Append("accuracy").Append(",")
                .Append("bearing").Append(",")
                .Append("speed").Append(",");
            await stream.WriteLineAsync(sb.ToString());
            foreach (Read row in _database.Table<Read>())
            {
                try
                {
                    sb.Clear();
                    sb
                        .Append(row.Ts).Append(",")
                        .Append(row.Rng).Append(",")
                        .Append(row.S1A).Append(",")
                        .Append(row.S1W).Append(",")
                        .Append(row.S2A).Append(",")
                        .Append(row.S2W).Append(",")
                        .Append(row.S3A).Append(",")
                        .Append(row.S3W).Append(",")
                        .Append(row.Pt).Append(",")
                        .Append(row.Nc).Append(",")
                        .Append(row.BP).Append(",")
                        .Append(row.BT).Append(",")
                        .Append(row.HH).Append(",")
                        .Append(row.HT).Append(",")
                        .Append(row.Co2).Append(",")
                        .Append(row.VIp).Append(",")
                        .Append(row.VPp).Append(",")
                        .Append(row.Latitude).Append(",")
                        .Append(row.Longitude).Append(",")
                        .Append(row.Altitude).Append(",")
                        .Append(row.Accuracy).Append(",")
                        .Append(row.Bearing).Append(",")
                        .Append(row.Speed).Append(",");
                    await stream.WriteLineAsync(sb.ToString());

                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }                
        }
        public int Delete(string key)
        {
            return _database.Table<Item>().Delete(x => x.Key == key);
        }
        public int Set(string key, string value)
        {
            var item = new Item(key, value);
            try
            {
                return _database.Insert(item);
            } catch(SQLiteException e)
            {
                if (e.Result==SQLite3.Result.Constraint)
                {
                    //May already exist try to update
                    return _database.Update(item);
                }
                return 0;
            }
        }

        public void AddLog(string type, string tag, string message)
        {
            var element = new LogElement
            {
                Type = type,
                Tag = tag,
                Message = message
            };
            try
            {
                _database.Insert(element);
            }
            catch (SQLiteException)
            {
            }
        }
        public int ClearLogs()
        {
            return _database.DeleteAll<LogElement>();
        }
        public IEnumerable<LogElement> ReadLogs()
        {
            return _database.Table<LogElement>();
        }

        public int AddRead(Read read)
        {
            return _database.Insert(read);
        }
        public int ClearReads()
        {
            return _database.DeleteAll<Read>();
        }
        public IEnumerable<Tuple<DateTime,double>> LastHours(int hours, Func<Read,double> selector)
        {
            Tuple<DateTime, double> t = new Tuple<DateTime, double>(DateTime.Now, 0.0);
            var nowTime = (long)MetaSenseNode.DateTimeToUnix(DateTime.Now);
            var interval = (long)TimeSpan.FromHours(hours).TotalSeconds;
            var oldest = nowTime - interval;
            var vals = _database.Table<Read>().Where(v => v.Ts > oldest);
            var ret = vals.AsEnumerable().Select((v) =>
                {
                    return new Tuple<DateTime, double>(MetaSenseNode.UnixToDateTime(v.Ts), selector(v));
                });
                
            return ret;
            //var minGroups = from read in vals group selector(read) by read.Ts/60*60;
            //return from gr in minGroups let min = MetaSenseNode.UnixToDateTime(gr.Key) let val = gr.Average() select new Tuple<DateTime, double>(min, val);
        }

        // DM: add conversion output methods
        /// <summary>
        /// Find all stored conversion outputs from conversion functions that used a neural network.
        /// </summary>
        /// <returns>A TableQuery containing all stored conversion outputs from conversion
        ///     functions that used a neural network.</returns>
        public IEnumerable<NNConversionOutput> ReadNNConversionOutputs()
        {
            return _database.Table<NNConversionOutput>();
        }

        /// <summary>
        /// Insert a conversion output from a conversion fucntion that used a neural network.
        /// 
        /// Note: allows duplicates in case that the same output naturally occurs twice.
        /// </summary>
        /// <param name="no2PPB">The output NO2 (ppb) of the neural network</param>
        /// <param name="o3PPB">The output O3 (ppb) of the neural network</param>
        /// <param name="coPPM">The output CO (ppm) of the neural network</param>
        /// <param name="ts">The absolute time (in universal unix time) that the conversion
        ///     occurred</param>
        /// <returns>The amount of rows inserted into the table.</returns>
        public int AddNNConversionOutput(double no2PPB, double o3PPB, double coPPM, long ts)
        {
            NNConversionOutput convOutput = new NNConversionOutput(no2PPB, o3PPB, coPPM, ts);
            return _database.Insert(convOutput);
        }

        /// <summary>
        /// Delete all conversion outputs from conversion functions that used a neural network.
        /// </summary>
        /// <returns>The amount of rows deleted from the table.</returns>
        public int ClearNNConversionOutputs()
        {
            return _database.DeleteAll<NNConversionOutput>();
        }

        /// <summary>
        /// Delete all conversion outputs which are older than the specified unix time and are from
        /// conversion functions that used a neural network.
        /// </summary>
        /// <param name="time">The universal unix time that determines which conversion outputs are
        ///     deleted.</param>
        /// <returns>The amount of rows deleted from the table.</returns>
        public int DeleteNNConversionOutputs(long time)
        {
            var oldConvOutputs = _database.Query<NNConversionOutput>("select * from NNConversionOutputs " +
                $"indexed by TimeIndex where Ts < ?", time);
            var rowsDeleted = 0;

            foreach (var convOutput in oldConvOutputs)
            {
                rowsDeleted += _database.Delete(convOutput);
            }

            return rowsDeleted;
        }
        // DM: end
        // DM: add neural net conversion methods and count methods (currently not in use)
        /// <summary>
        /// Returns the amount of each pollutant output by the neural network when the given inputs
        /// were used.
        /// 
        /// Note: currently not in use.
        /// </summary>
        /// <param name="no2">The voltage difference in mV of the NO2 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="o3">The voltage difference of the O3 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="co">The voltage difference of the CO electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="temp">Temperature (degrees Celsius)</param>
        /// <param name="absHumidity">Absolute humidity</param>
        /// <param name="pres">Pressure (mbar)</param>
        /// <returns>A double array of the amount of each pollutant gas in the order of:
        ///     NO2 (ppb), O3 (ppb), CO (ppm).</returns>
        public double[] GetNNConversionResults(double no2, double o3, double co, double temp,
            double absHumidity, double pres)
        {
            var match = Get(no2, o3, co, temp, absHumidity, pres);

            if (match == null)
            {
                return null;
            }

            return match.GetConversions();
        }

        /// <summary>
        /// Find the stored NNConversion in the database that matches the specified inputs.
        /// 
        /// Note: currently not in use.
        /// </summary>
        /// <param name="no2">The voltage difference in mV of the NO2 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="o3">The voltage difference of the O3 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="co">The voltage difference of the CO electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="temp">Temperature (degrees Celsius)</param>
        /// <param name="absHumidity">Absolute humidity</param>
        /// <param name="pres">Pressure (mbar)</param>
        /// <returns>The stored NNConversion in the database that matches the specified inputs or
        ///     null if it was not found or more than one was found.</returns>
        public NNConversion Get(double no2, double o3, double co, double temp,
            double absHumidity, double pres)
        {
            var matches = _database.Query<NNConversion>("select * from NNConversions indexed " + 
                "by FeaturesIndex where NO2=? and O3=? and CO=? and Temperature=? and " +
                "AbsoluteHumidity=? and Pressure=?", no2, o3, co, temp, absHumidity, pres);
            
            // advance the clock one tick due to cache access
            IncrementCount("NNConversionCacheClock");

            if (matches == null || matches.Count() != 1)
            {
                return null;
            }

            return matches.First();
        }

        /// <summary>
        /// Find all stored conversions.
        /// 
        /// Note: currently not in use.
        /// </summary>
        /// <returns>A TableQuery containing all stored conversions.</returns>
        public IEnumerable<NNConversion> ReadNNConversions()
        {
            return _database.Table<NNConversion>();
        }

        /// <summary>
        /// Insert the input and output of the neural network conversion into the database.
        /// 
        /// Note: assumes that it is okay to insert the NNConversion and does not check if it
        /// is a duplicate.
        /// 
        /// Note #2: currently not in use.
        /// </summary>
        /// <param name="no2">The voltage difference in mV of the NO2 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="o3">The voltage difference of the O3 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="co">The voltage difference of the CO electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="temp">Temperature (degrees Celsius)</param>
        /// <param name="absHumidity">Absolute humidity</param>
        /// <param name="pres">Pressure (mbar)</param>
        /// <param name="no2PPB">The output NO2 (ppb) of the neural network</param>
        /// <param name="o3PPB">The output O3 (ppb) of the neural network</param>
        /// <param name="coPPM">The output CO (ppm) of the neural network</param>
        /// <returns>The amount of rows inserted into the table.</returns>
        public int AddNNConversion(double no2, double o3, double co, double temp,
            double absHumidity, double pres, double no2PPB, double o3PPB, double coPPM)
        {
            int numToDeleteAfterAdd = _database.Table<NNConversion>().Count() + 1 - NN_CONVERSION_CACHE_SIZE;
            DeleteNNConversionsLRU(numToDeleteAfterAdd);

            var nnConversion = new NNConversion(no2, o3, co, temp, absHumidity, pres, no2PPB, o3PPB,
                coPPM, GetCountValue("NNConversionCacheClock"));

            // advance the clock one tick due to insertion
            IncrementCount("NNConversionCacheClock");

            return _database.Insert(nnConversion);
        }

        /// <summary>
        /// Update the time that the conversion was last accessed to the current cache clock tick.
        /// 
        /// Note: assumes that the NNConversion in the database already exists.
        /// 
        /// Note #2: currently not in use.
        /// </summary>
        /// <param name="no2">The voltage difference in mV of the NO2 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="o3">The voltage difference of the O3 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="co">The voltage difference of the CO electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="temp">Temperature (degrees Celsius)</param>
        /// <param name="absHumidity">Absolute humidity</param>
        /// <param name="pres">Pressure (mbar)</param>
        /// <returns>The amount of rows modified in the table.</returns>
        public int UpdateNNConversionTime(double no2, double o3, double co, double temp,
            double absHumidity, double pres)
        {
            var databaseConv = Get(no2, o3, co, temp, absHumidity, pres);

            // update the last time and amount of times it was accessed since it already exists
            databaseConv.Ts = GetCountValue("NNConversionCacheClock");

            // advance the clock one tick due to update
            IncrementCount("NNConversionCacheClock");

            return _database.Update(databaseConv);
        }

        /// <summary>
        /// Delete the NNConversion stored in the database that has the same inputs as the
        /// specified conversion.
        /// 
        /// Note: currently not in use.
        /// </summary>
        /// <param name="conv">The conversion containing the matching inputs to delete</param>
        /// <returns>The amount of rows deleted from the table.</returns>
        public int DeleteNNConversion(NNConversion conv)
        {
            return _database.Table<NNConversion>().Delete(c => c.NO2 == conv.NO2 &&
                c.O3 == conv.O3 && c.CO == conv.CO && c.Temperature == conv.Temperature &&
                c.AbsoluteHumidity == conv.AbsoluteHumidity && c.Pressure == conv.Pressure);
        }

        /// <summary>
        /// Delete the least recently used NNConversions from the database.
        /// 
        /// Note: currently not in use.
        /// </summary>
        /// <param name="numToDelete">Amount of NNConversions to delete from the database</param>
        /// <returns>The amount of rows deleted from the table.</returns>
        public int DeleteNNConversionsLRU(int numToDelete)
        {
            if (numToDelete <= 0)
            {
                return 0;
            }

            var leastRecentConvs = _database.Query<NNConversion>("select * from NNConversions " +
                "order by Ts limit " + numToDelete);
            var rowsDeleted = 0;

            if (leastRecentConvs == null)
            {
                return -1;
            }
            
            foreach (var conv in leastRecentConvs)
            {
                rowsDeleted += DeleteNNConversion(conv);
            }

            return rowsDeleted;
        }

        /// <summary>
        /// Delete all stored NNConversions in the database.
        /// 
        /// Note: currently not in use.
        /// </summary>
        /// <returns>The amount of rows deleted from the table.</returns>
        public int ClearNNConversions()
        {
            return _database.DeleteAll<NNConversion>();
        }

        /// <summary>
        /// Find the count with the matching key.
        /// </summary>
        /// <param name="key">The key of the count to find</param>
        /// <returns>The count with the matching key or null if it wasn't found or if more than
        ///     one count matched the key.</returns>
        public Count GetCount(string key)
        {
            var match = _database.Table<Count>().Where(c => c.Key.Equals(key));

            if (match == null || match.Count() != 1)
            {
                return null;
            }

            return match.First();
        }

        /// <summary>
        /// Find the value associated with the count that matches the key.
        /// 
        /// Note: assumes that count values are nonnegative (or at least never equal to -1).
        /// 
        /// Note #2: currently not in use.
        /// </summary>
        /// <param name="key">The key of the count to find</param>
        /// <returns>The value associated with the count that matches the key or -1 if it wasn't
        ///     found or if more than one count matched the key.</returns>
        public long GetCountValue(string key)
        {
            var search = GetCount(key);

            if (search == null)
            {
                return -1;
            }

            return search.Value;
        }

        /// <summary>
        /// Insert a count with the specified key that starts at 0 into the database.
        /// </summary>
        /// <param name="key">The key of the count to insert</param>
        /// <returns>The amount of rows inserted into the table.</returns>
        public int AddCount(string key)
        {
            Count count = new Count(key, 0);
            return _database.Insert(count);
        }

        /// <summary>
        /// Increment the count with the specified key.
        /// </summary>
        /// <param name="key">The key of the count to increment</param>
        /// <returns>The amount of rows modified in the table if the count with the matching key
        ///     was found or the amount of rows inserted in the table if it was not
        ///     found.</returns>
        public int IncrementCount(string key)
        {
            var storedCount = GetCount(key);

            if (storedCount == null)
            {
                return AddCount(key);
            }

            storedCount.Value += 1;
            return _database.Update(storedCount);
        }

        /// <summary>
        /// Find all counts stored in the database.
        /// </summary>
        /// <returns>A TableQuery containing all the counts stored in the database.</returns>
        public IEnumerable<Count> ReadCounts()
        {
            return _database.Table<Count>();
        }

        /// <summary>
        /// Delete the count that matches the specified key.
        /// </summary>
        /// <param name="key">The key of the count ot delete</param>
        /// <returns>The amount of rows deleted from the table.</returns>
        public int DeleteCount(string key)
        {
            return _database.Table<Count>().Delete(c => c.Key.Equals(key));
        }

        /// <summary>
        /// Delete all counts stored in the database.
        /// </summary>
        /// <returns>The amount of rows deleted from the table.</returns>
        public int ClearCounts()
        {
            return _database.DeleteAll<Count>();
        }
        // DM: end
    }
}
