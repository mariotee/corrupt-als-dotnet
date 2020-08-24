using System.IO;
using System.Threading.Tasks;
using BlazorInputFile;

namespace server.Data
{
    public interface IFileUpload
    {
         Task<Stream> UploadAsync(IFileListEntry file);
    }
}