using System.Threading.Tasks;

namespace NodeLibrary.Native
{
    public interface IBLEUtils
    {
        Task<MetaSenseNode> NodeFactory(NodeInfo devicInfo);
        Task<NodeInfo> DeviceInfoFromMac(string mac);
        Task<bool> PairDevice(NodeInfo dev);
    }
}