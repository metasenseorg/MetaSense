using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary
{
    public class Range
    {

        // Comparison uses Double.compare, due to NaN
        private double lo, hi;

        public Range(double a, double b)
        {
            if (a >= b)
            {
                hi = a;
                lo = b;
            }
            else
            {
                hi = b;
                lo = a;
            }
        }

        public double Hi()
        {
            return hi;
        }

        public double Lo()
        {
            return lo;
        }

        public double Delta()
        {
            if (Double.IsNaN(lo) || Double.IsNaN(hi))
            {
                return Double.MaxValue;
            }
            else
            {
                return hi - lo;
            }
        }

        public bool Contains(double value)
        {
            // DM: accounted for edge case where value is equal to hi
            // return (lo <= value) && (value < hi);
            return (lo <= value) && (value <= hi);
        }
    }
    public class AqiBreakpointEntry
    {
        public Range breakpoints;
        public Range aqiValues;
    }
    public class AqiBreakpointTable
    {
        //private String m_pollutantName;
        private Dictionary<Range, Range> m_table;

        //public String GetPollutantName() { return m_pollutantName; }

        public AqiBreakpointTable(/*String pollutantName*/)
        {
            //m_pollutantName = pollutantName;
            // SDH: giving a 10 value startup size to ensure allocation
            m_table = new Dictionary<Range, Range>(10);
        }

        public void AddEntry(Range breakpoints, Range aqiValues)
        {
            m_table.Add(breakpoints, aqiValues);
        }
        public void AddEntry(double bplo, double bphi, double aqilo, double aqihi)
        {
            Range bp = new Range(bplo, bphi);
            Range aqi = new Range(aqilo, aqihi);
            AddEntry(bp, aqi);
        }
        public AqiBreakpointEntry GetEntryByConcentration(double concentration)
        {
            AqiBreakpointEntry entry = new AqiBreakpointEntry();
            foreach (Range breakpoints in m_table.Keys)
            {
                if (breakpoints.Contains(concentration))
                {
                    entry.breakpoints = breakpoints;
                    entry.aqiValues = m_table[breakpoints];
                    break;
                }
            }

            return entry;
        }
    }
    public class AQICalculator
    {
        public static readonly int UNKNOWN_AQI = -1;


        private static AqiBreakpointTable coTable;
        private static AqiBreakpointTable no2Table;
        private static AqiBreakpointTable ozone8hourTable;
        private static Dictionary<Range, string> AQINameTable;
        static AQICalculator()
        {
            coTable = new AqiBreakpointTable();
            coTable.AddEntry(0.0, 4.4, 0.0, 50.0);
            coTable.AddEntry(4.5, 9.4, 51.0, 100.0);
            coTable.AddEntry(9.5, 12.4, 101.0, 150.0);
            coTable.AddEntry(12.5, 15.4, 151.0, 200.0);
            coTable.AddEntry(15.5, 30.4, 201.0, 300.0);
            coTable.AddEntry(30.5, 40.4, 301.0, 400.0);
            coTable.AddEntry(40.5, 50.4, 401.0, 500.0);

            no2Table = new AqiBreakpointTable();
            no2Table.AddEntry(0.0, 0.64, 0.0, 0.0); // while not documented for AQI, we need to return a value to be consistent
            no2Table.AddEntry(0.65, 1.24, 201.0, 300.0);
            no2Table.AddEntry(1.25, 1.64, 301.0, 400.0);
            no2Table.AddEntry(1.65, 2.04, 401.0, 500.0);
            
            ozone8hourTable = new AqiBreakpointTable();
            ozone8hourTable.AddEntry(0.000, 0.059, 0.0, 50.0);
            ozone8hourTable.AddEntry(0.060, 0.075, 51.0, 100.0);
            ozone8hourTable.AddEntry(0.076, 0.095, 101.0, 150.0);
            ozone8hourTable.AddEntry(0.096, 0.115, 151.0, 200.0);
            ozone8hourTable.AddEntry(0.116, 0.374, 201.0, 300.0);
            ozone8hourTable.AddEntry(0.405, 0.504, 301.0, 400.0);
            ozone8hourTable.AddEntry(0.505, 0.604, 401.0, 500.0);

            AQINameTable = new Dictionary<Range, string>();
            AQINameTable[new Range(0, 50)] = "Good";
            AQINameTable[new Range(51,100)] = "Moderate";
            AQINameTable[new Range(101, 150)] = "Unhealthy for Sensitive Groups";
            AQINameTable[new Range(151, 200)] = "Unhealthy";
            AQINameTable[new Range(201, 300)] = "Very Unhealthy";
            AQINameTable[new Range(301, 500)] = "Hazardous";

        }

        public static string AQICategory(double aqi)
        {
            foreach (var key in AQINameTable.Keys)
            {
                if (key.Contains(Math.Truncate(aqi)))
                    return AQINameTable[key];
            }
            return "Unknown";
        }
        private static int CalculateAQIElement(double concentration, AqiBreakpointTable table)
        {
            AqiBreakpointEntry entry = table.GetEntryByConcentration(concentration);

            if (entry == null || entry.aqiValues == null
                || entry.breakpoints == null)
                return UNKNOWN_AQI;

            return CalculateAqiInRange(entry.aqiValues, entry.breakpoints,
                concentration);
        }

        private static int CO_CalculateAQI(GasReading reading)
        {
            var oneDec = Math.Truncate(reading.COppm * 10) / 10.0;
            return CalculateAQIElement(oneDec, coTable);
        }
        private static int NO2_CalculateAQI(GasReading reading)
        {
            var twoDec = Math.Truncate(reading.NO2ppb /10) / 100.0;
            return CalculateAQIElement(twoDec, no2Table);
        }
        private static int O3_CalculateAQI(GasReading reading)
        {
            var threeDec = Math.Truncate(reading.O3ppb) / 1000.0;
            return CalculateAQIElement(threeDec, ozone8hourTable);
        }

        private static int CalculateAqiInRange(Range aqiRange, Range breakpoint,
            double concentration)
        {
            return (int)Math.Round((aqiRange.Delta() / breakpoint.Delta())
                                   * (concentration - breakpoint.Lo()) + aqiRange.Lo());
        }
        public static Tuple<int,string> CalculateAQI(GasReading reading)
        {
            var co = CO_CalculateAQI(reading);
            var no2 = NO2_CalculateAQI(reading);
            var o3 = O3_CalculateAQI(reading);

            var maxValue = Math.Max(Math.Max(co, no2), o3);
            var respoinsible = "O3";
            if (maxValue == co) respoinsible = "CO";
            if (maxValue == no2) respoinsible = "NO2";
            return new Tuple<int, string>(maxValue, respoinsible);
        }
        // DM: add new methods to allow for filtering
        public static Tuple<int, int, int> CalculatePollutantAQIs(GasReading reading)
        {
            var co = CO_CalculateAQI(reading);
            var no2 = NO2_CalculateAQI(reading);
            var o3 = O3_CalculateAQI(reading);

            return Tuple.Create(co, no2, o3);
        }

        public static Tuple<int, string> CalculateAQI(Tuple<int, int, int> pollutantAQIs)
        {
            var co = pollutantAQIs.Item1;
            var no2 = pollutantAQIs.Item2;
            var o3 = pollutantAQIs.Item3;

            var maxValue = Math.Max(Math.Max(co, no2), o3);
            var respoinsible = "O3";
            if (maxValue == co) respoinsible = "CO";
            if (maxValue == no2) respoinsible = "NO2";
            return new Tuple<int, string>(maxValue, respoinsible);
        }
        // DM: end
    }
}
