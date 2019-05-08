namespace BackendAPI.Data
{
    public enum ConversionType
    {
        None,
        Alphasense,
        Sharad
    }

    public interface IConverter
    {
        ConversionType ConversionType { get; set; }
        string AlphasenseJson { get; set; }
        string SharadJson { get; set; }
        bool IsAlphasenseJsonValid(string json);
        bool IsSharadJsonValid(string json);
        IConversionFunctions Conversion { get; set; }
    }
}