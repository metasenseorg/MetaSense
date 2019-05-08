using Xamarin.Forms;

namespace NodeLibrary.Native
{
    public interface IBLEDeviceImages
    {
        ImageSource Paired { get; }
        ImageSource NotPaired { get; }
        ImageSource Signal0 { get; }
        ImageSource Signal1 { get; }
        ImageSource Signal2 { get; }
        ImageSource Signal3 { get; }
        ImageSource RedDot { get; }
        ImageSource GreenDot { get; }
        // DM: add bluetooth imagesources
        ImageSource Bluetooth { get; }
        ImageSource BluetoothConnected { get; }
        // DM: end
    }
}