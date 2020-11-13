using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace server.Data
{
    public static class JSInteropExtensions
    {

        public static async Task SaveFileAsync(this IJSRuntime js, string text)
        {
            await js.InvokeAsync<object>("FileSaveAs", "newfile.als", text);
        }
    }
}