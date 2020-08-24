using BlazorInputFile;
using Microsoft.AspNetCore.Hosting;
using server.Data;
using System.IO;
using System.Threading.Tasks;
namespace FileUploadBlazor.Services
{
    public class FileUpload : IFileUpload
    {
        private readonly IWebHostEnvironment _environment;
        public FileUpload(IWebHostEnvironment env)
        {
            _environment = env;
        }
        public async Task<Stream> UploadAsync(IFileListEntry fileEntry)
        {
            var path = Path.Combine(_environment.ContentRootPath, "Upload", fileEntry.Name);
            var ms = new MemoryStream();
            await fileEntry.Data.CopyToAsync(ms);
            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                ms.WriteTo(file);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}