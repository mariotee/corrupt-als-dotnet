using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorInputFile;

namespace server.Data.Util
{
    public static class ZipTool
    {
        public static async Task<byte[]> ZipString(string textToZip)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = zipArchive.CreateEntry("zipped.txt");
            
                    using (var entryStream = demoFile.Open())
                    {
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            await streamWriter.WriteAsync(textToZip);
                        }
                    }
                }
            
                return memoryStream.ToArray();
            }
        }

        public static async Task<string> UnzipBytes(byte[] zippedBuffer)
        {
            using (var zippedStream = new MemoryStream(zippedBuffer))
            {
                using (var archive = new ZipArchive(zippedStream))
                {
                    var entry = archive.Entries.FirstOrDefault();
            
                    if (entry != null)
                    {
                        using (var unzippedEntryStream = entry.Open())
                        {
                            using (var ms = new MemoryStream())
                            {
                                await unzippedEntryStream.CopyToAsync(ms);
                                var unzippedArray = ms.ToArray();
                    
                                return Encoding.Default.GetString(unzippedArray);
                            }
                        }
                    }
            
                    return null;
                }
            }
        }
    }
}