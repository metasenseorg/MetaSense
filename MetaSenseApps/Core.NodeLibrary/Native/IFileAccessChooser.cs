using System.IO;
using System.Threading.Tasks;

namespace NodeLibrary.Native
{
    public interface IFileAccessChooser
    {
        Task<Stream> OpenFileForWrite();
        Task<Stream> OpenFileForRead(string mime);
    }
}
