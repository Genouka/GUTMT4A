using System.Text;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace UTMTdrid;

public class MAUIBridge
{
    public static async Task<FileResult?> PickAndShow(PickOptions options)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = await result.OpenReadAsync();
                    var image = ImageSource.FromStream(() => stream);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
        }

        return null;
    }
    // public static async Task<string?> SaveFile(CancellationToken cancellationToken)
    // {
    //     var fileSaverResult = await FileSaver.Default.SaveAsync("test.txt", stream, cancellationToken);
    //     if (fileSaverResult.IsSuccessful)
    //     {
    //         return fileSaverResult.FilePath;
    //     }
    //     else
    //     {
    //         return null;
    //     }
    // }
    public static async Task<string?> PickFolder(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<string>();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var result = await FolderPicker.Default.PickAsync(cancellationToken);
            if (result.IsSuccessful){
                tcs.SetResult(result.Folder.Path);
            }
            else
            {
                tcs.SetResult(null);
            }
        });
        return await tcs.Task;
    }
}