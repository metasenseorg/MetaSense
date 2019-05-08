using System.Collections.Generic;
using System.Threading.Tasks;

namespace NodeLibrary.Native
{
    public interface IWiFiUtils
    {
        Task<IList<SSIDInfo>> NearbySSID();
    }
}