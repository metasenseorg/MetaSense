using System.Threading.Tasks;
using Android.Content;
using Core.Droid;
using Core.Droid.BLE;
using NodeLibrary;
using NodeLibrary.Native;
using Xamarin.Forms;

[assembly: Dependency(typeof(BLEUtilsDroid))]
namespace Core.Droid
{
    internal sealed class BLEUtilsDroid : IBLEUtils
    {
        private readonly BLEAdapterDroid _bleAdapter;
        public BLEUtilsDroid() : this(Forms.Context) { }
        public BLEUtilsDroid(Context context)
        {
            _bleAdapter = new BLEAdapterDroid(context);
        }
        public async Task<bool> PairDevice(NodeInfo devInfo)
        {
            var devInfoDroid = devInfo as BLENodeInfoDroid;
            if (devInfoDroid == null)
                return false;
            return await devInfoDroid.DoPair();
        }
        public Task<NodeInfo> DeviceInfoFromMac(string mac)
        {
            return Task.FromResult((NodeInfo)new BLENodeInfoDroid(mac, _bleAdapter));
        }
        public Task<MetaSenseNode> NodeFactory(NodeInfo devicInfo)
        {
            return Task.FromResult(new MetaSenseNode(devicInfo, new BLEMetaSenseNodeDroid(devicInfo.MacAddress, _bleAdapter)));
        }
    }
}