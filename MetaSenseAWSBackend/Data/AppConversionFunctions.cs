using System;

namespace BackendAPI.Data
{
    public class AppConversionFunctions
    {
        private readonly ISettingsData _settingsData;
        public AppConversionFunctions(ISettingsData settingsData)
        {
            _settingsData = settingsData;
            InitializeConversionFromDatabase();
        }

        private void InitializeConversionFromDatabase()
        {
            var function = _settingsData.Get("ConversionFunction");
            if ("alphasense".Equals(function)) ConversionType = ConversionType.Alphasense;
            else if ("sharad".Equals(function)) ConversionType = ConversionType.Sharad;
            else ConversionType = ConversionType.None;
        }
        public bool IsAlphasenseJsonValid(string json)
        {
            var conv = new ConversionFunctionsAlphasense(json);
            return conv.ValidCalibrationFile;
        }
        public bool IsSharadJsonValid(string json)
        {
            var conv = new ConversionFunctionsSharad(json);
            return conv.ValidCalibrationFile;
        }
        private ConversionType _conversionType;
        public ConversionType ConversionType
        {
            get => _conversionType;
            set
            {
                //if (_conversionType ==value) return;

                if (value == ConversionType.Alphasense)
                {
                    var json = AlphasenseJson;
                    var conv = new ConversionFunctionsAlphasense(json);
                    if (conv.ValidCalibrationFile)
                    {
                        Conversion = conv;
                        _settingsData.Set("ConversionFunction", "alphasense");
                        _conversionType = ConversionType.Alphasense;
                        return;
                    }
                }
                else if (value == ConversionType.Sharad)
                {
                    var json = SharadJson;
                    var conv = new ConversionFunctionsSharad(json);
                    if (conv.ValidCalibrationFile)
                    {
                        Conversion = conv;
                        _settingsData.Set("ConversionFunction", "sharad");
                        _conversionType = ConversionType.Sharad;
                        return;
                    }
                }
                _conversionType = ConversionType.None;
                Conversion = null;
            }
        }
        public string AlphasenseJson
        {
            get => _settingsData.Get("ConversionAlphasenseJson");
            set
            {
                var conv = new ConversionFunctionsAlphasense(value);
                if (conv.ValidCalibrationFile)
                    _settingsData.Set("ConversionAlphasenseJson", value);
                else
                    throw new Exception("JSON File Validation Failed");
            }
        }
        public string SharadJson
        {
            get => _settingsData.Get("ConversionSharadJson");
            set
            {
                var conv = new ConversionFunctionsSharad(value);
                if (conv.ValidCalibrationFile)
                    _settingsData.Set("ConversionSharadJson", value);
                else
                    throw new Exception("JSON File Validation Failed");
            }
        }
        public IConversionFunctions Conversion { get; set; }

    }
}
