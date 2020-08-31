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

            try
            {
                await gz.CopyToAsync(output);
            }
            catch (InvalidDataException invalidDataEx)
            {
                if (invalidDataEx.Message == "The archive entry was compressed using an unsupported compression method.")
                {
                    throw new UnsupportedCompressionAlgorithmException();
                }
            }

            return output.ToArray();
        }
    }
}