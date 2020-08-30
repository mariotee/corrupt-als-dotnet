using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace server.Data.Util
{
    public static class ZipTool
    {
        public static async Task<byte[]> DecompressAsync(byte[] input)
        {
            using var inmem = new MemoryStream(input);
            using var output = new MemoryStream();
            using var gz = new GZipStream(inmem, CompressionMode.Decompress);

            await gz.CopyToAsync(output);

            return output.ToArray();
        }
    }
}