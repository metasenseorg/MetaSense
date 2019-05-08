namespace BackendAPI.Data
{
    public class GasReading
    {
        public double COppm;
        public double O3Ppb;
        public double NO2Ppb;
    }
    public interface IConversionFunctions
    {
        double COppm(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC);

        double NO2Ppb(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC);

        double O3Ppb(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC);

        GasReading Convert(double coAMv, double coWMv, double o3AMv, double o3WMv, double no2AMv, double no2WMv,
            double temperatureMv, double humidityPc, double pressureMillibar, double tempC);

        GasReading Convert(Read read);
        GasReading Convert(MetaSenseMessage message);
    }
}
