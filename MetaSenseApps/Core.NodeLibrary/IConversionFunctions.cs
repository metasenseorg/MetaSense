using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary
{
    public class GasReading
    {
        public double COppm;
        public double O3ppb;
        public double NO2ppb;
    }
    public interface IConversionFunctions
    {
        double COppm(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C);

        double NO2ppb(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C);

        double O3ppb(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C);

        GasReading Convert(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV,
            double temperature_mV, double humidity_pc, double pressure_millibar, double temp_C);

        GasReading Convert(Read read);
        GasReading Convert(MetaSenseMessage message);
    }
}
