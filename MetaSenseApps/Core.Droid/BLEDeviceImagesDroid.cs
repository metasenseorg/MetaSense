using Core.Droid;
using NodeLibrary.Native;
using Xamarin.Forms;

[assembly: Dependency(typeof(BLEDeviceImagesDroid))]
namespace Core.Droid
{
    internal sealed class BLEDeviceImagesDroid : IBLEDeviceImages
    {
        public BLEDeviceImagesDroid()
        {
            Paired = ImageSource.FromFile("bluetooth_icon.png");
            NotPaired = ImageSource.FromFile("bluetooth_icon_d.png");
            Signal0 = ImageSource.FromFile("wireless_icon_0.png");
            Signal1 = ImageSource.FromFile("wireless_icon_30.png");
            Signal2 = ImageSource.FromFile("wireless_icon_60.png");
            Signal3 = ImageSource.FromFile("wireless_icon_100.png");
            RedDot = ImageSource.FromFile("red_dot.png");
            GreenDot = ImageSource.FromFile("green_dot.png");
            // DM: add bluetooth images
            Bluetooth = ImageSource.FromFile("bluetooth");
            BluetoothConnected = ImageSource.FromFile("bluetooth_connected");
            // DM: end
        }
        public ImageSource Paired { get; }
        public ImageSource NotPaired { get; }
        public ImageSource Signal0 { get; }
        public ImageSource Signal1 { get; }
        public ImageSource Signal2 { get; }
        public ImageSource Signal3 { get; }
        public ImageSource RedDot { get; }
        public ImageSource GreenDot { get; }
        // DM: add bluetooth properties
        public ImageSource Bluetooth { get; }
        public ImageSource BluetoothConnected { get; }
        // DM: end
    }
}