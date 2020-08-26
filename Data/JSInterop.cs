using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace server.Data
{
    public static class JSInteropExtensions
    {
        public static async Task<FileDataModel> ReadFileAsTextAsync(this IJSRuntime js)
        {
            string fileInputId = "fileUpload";

            return await js.InvokeAsync<FileDataModel>("ReadUploadedFileAsText", fileInputId);
        }

        public static async Task SaveFileAsync(this IJSRuntime js, string text)
        {
            await js.InvokeAsync<object>("FileSaveAs", "", text);
        }
    }

    /*BEGIN MODELS*/

    public class FileDataModel
    {
        public string FileName { get; set; }
        public string Data { get; set; }
    }

    /*END MODELS*/
}