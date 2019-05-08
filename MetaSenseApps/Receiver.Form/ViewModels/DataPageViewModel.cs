using System;
using NodeLibrary;
using Prism.Navigation;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Receiver.ViewModels
{
    public sealed class DataPageViewModel : NodeAwareViewModel
    {
        public DataPageViewModel(/*IConversionFunctions conversion,*/ IAppProperties appProperties, INavigationService navigationService) : base(appProperties, navigationService)
        {
            //_conversion = conversion;
            if (LastRead!=null)
                ProcessLastRead();
        }
        protected override void ProcessLastRead()
        {
            try {
                LastMessageReceivedAt = DateTime.Now;
                Rng = LastRead.Raw.Rng;
                S1A = LastRead.Raw.S1A;
                S1W = LastRead.Raw.S1W;
                S2A = LastRead.Raw.S2A;
                S2W = LastRead.Raw.S2W;
                S3A = LastRead.Raw.S3A;
                S3W = LastRead.Raw.S3W;
                PT = LastRead.Raw.Temperature;
                NC = LastRead.Raw.Voc;

                HT = LastRead.HuPr.HumiditySensorTemperatureCelsius;
                HH = LastRead.HuPr.HumiditySensorHumidityPercent;
                BP = LastRead.HuPr.BarometricSensorPressureMilliBar;
                BT = LastRead.HuPr.BarometricSensorTemperatureCelsius;

                var conv = new MetaSenseConverters(LastRead);

                var hupr = conv.HuPr();
                HumCelsius = hupr.HumCelsius;
                HumPercent = hupr.HumPercent;
                PresCelsius = hupr.PresCelsius;
                PresBar = hupr.PresMilliBar;

                var gas = conv.Gas();
                No2AVolt = gas.No2A;
                No2WVolt = gas.No2W;
                OxAVolt = gas.OxA;
                OxWVolt = gas.OxW;
                CoAVolt = gas.CoA;
                CoWVolt = gas.CoW;
                TempVolt = gas.Temp;
                NcVolt = gas.Nc;

                if (App.Conversion != null)
                {
                    var gasConcentration = App.Conversion.Convert(LastRead);
                    //gas.CoA*1000, gas.CoW * 1000, gas.OxA * 1000, gas.OxW * 1000, gas.No2A * 1000, gas.No2W * 1000, gas.Temp * 1000, hupr.HumPercent, hupr.PresMilliBar*1000, hupr.HumCelsius);
                    COppm = gasConcentration.COppm;
                    O3ppb = gasConcentration.O3ppb;
                    NO2ppb = gasConcentration.NO2ppb;
                    var AQITuple = AQICalculator.CalculateAQI(gasConcentration);
                    AQI = AQITuple.Item1;
                }
                if (LastRead.Voc!=null)
                    VOC = LastRead.Voc.VIp;
                if (LastRead.Co2 != null)
                    CO2 = LastRead.Co2.Co2;

                Latitude = LastRead.Loc.Latitude;
                Longitude = LastRead.Loc.Longitude;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        private DateTime _lastMessageReceivedAt;

        public DateTime LastMessageReceivedAt
        {
            get => _lastMessageReceivedAt;
            set => SetProperty(ref _lastMessageReceivedAt, value);
        }

        private double _aqi;
        public double AQI
        {
            get => _aqi;
            set => SetProperty(ref _aqi, value);
        }
        //private IConversionFunctions _conversion;
        private double _COppm;
        public double COppm
        {
            get => _COppm;
            set => SetProperty(ref _COppm, value);
        }
        private double _o3Ppb;
        public double O3ppb
        {
            get => _o3Ppb;
            set => SetProperty(ref _o3Ppb, value);
        }
        private double _no2Ppb;
        public double NO2ppb
        {
            get => _no2Ppb;
            set => SetProperty(ref _no2Ppb, value);
        }

        private double _co2;
        private double _voc;
        public double CO2
        {
            get => _co2;
            set => SetProperty(ref _co2, value);
        }
        public double VOC
        {
            get => _voc;
            set => SetProperty(ref _voc, value);
        }

        #region Values 

        private int _rng;
        private int _s1A;
        private int _s1W;
        private int _s2A;
        private int _s2W;
        private int _s3A;
        private int _s3W;
        private int _pt;
        private int _nc;

        public int Rng
        {
            get => _rng;
            set => SetProperty(ref _rng, value);

        }
        public int S1A
        {
            get => _s1A;
            set => SetProperty(ref _s1A, value);

        }
        public int S1W
        {
            get => _s1W;
            set => SetProperty(ref _s1W, value);

        }
        public int S2A
        {
            get => _s2A;
            set => SetProperty(ref _s2A, value);

        }
        public int S2W
        {
            get => _s2W;
            set => SetProperty(ref _s2W, value);

        }
        public int S3A
        {
            get => _s3A;
            set => SetProperty(ref _s3A, value);

        }
        public int S3W
        {
            get => _s3W;
            set => SetProperty(ref _s3W, value);

        }
        public int PT
        {
            get => _pt;
            set => SetProperty(ref _pt, value);

        }
        public int NC
        {
            get => _nc;
            set => SetProperty(ref _nc, value);

        }

        private double _ht;
        private double _hh;
        private double _bp;
        private double _bt;

        public double HT
        {
            get => _ht;
            set => SetProperty(ref _ht, value);

        }
        public double HH
        {
            get => _hh;
            set => SetProperty(ref _hh, value);

        }
        public double BP
        {
            get => _bp;
            set => SetProperty(ref _bp, value);

        }
        public double BT
        {
            get => _bt;
            set => SetProperty(ref _bt, value);

        }


        private double _humCelsius;
        private double _humPercent;
        private double _presCelsius;
        private double _presBar;

        public double HumCelsius
        {
            get => _humCelsius;
            set => SetProperty(ref _humCelsius, value);

        }
        public double HumPercent
        {
            get => _humPercent;
            set => SetProperty(ref _humPercent, value);

        }
        public double PresCelsius
        {
            get => _presCelsius;
            set => SetProperty(ref _presCelsius, value);

        }
        public double PresBar
        {
            get => _presBar;
            set => SetProperty(ref _presBar, value);

        }

        private double _no2AVolt;
        private double _no2WVolt;
        private double _oxAVolt;
        private double _oxWVolt;
        private double _coAVolt;
        private double _coWVolt;
        private double _tempVolt;
        private double _ncVolt;

        public double No2AVolt
        {
            get => _no2AVolt;
            set => SetProperty(ref _no2AVolt, value);

        }
        public double No2WVolt
        {
            get => _no2WVolt;
            set => SetProperty(ref _no2WVolt, value);

        }
        public double OxAVolt
        {
            get => _oxAVolt;
            set => SetProperty(ref _oxAVolt, value);

        }
        public double OxWVolt
        {
            get => _oxWVolt;
            set => SetProperty(ref _oxWVolt, value);

        }
        public double CoAVolt
        {
            get => _coAVolt;
            set => SetProperty(ref _coAVolt, value);

        }
        public double CoWVolt
        {
            get => _coWVolt;
            set => SetProperty(ref _coWVolt, value);

        }
        public double TempVolt
        {
            get => _tempVolt;
            set => SetProperty(ref _tempVolt, value);

        }
        public double NcVolt
        {
            get => _ncVolt;
            set => SetProperty(ref _ncVolt, value);
            
        }

        private double _latitude;
        private double _longitude;

        public double Latitude
        {
            get => _latitude;
            set => SetProperty(ref _latitude, value);

        }
        public double Longitude
        {
            get => _longitude;
            set => SetProperty(ref _longitude, value);
        }

   

        #endregion

    }
}
