using UTMTdrid;

namespace QiuUTMT;

public class OpenfileUtils
{
    public static async Task<FileResult> PickAndShow(PickOptions options)
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
}

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        var status =await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            status=await Permissions.RequestAsync<Permissions.StorageRead>();
            //判断是否获取到权限
        }
#if ANDROID
        if (!CheckPermission.CheckExternalStoragePermission())
        {
            return;
        }
#endif
        DataEditBtn.IsEnabled = false;
        RunScriptBtn.IsEnabled = false;
        var result = await OpenfileUtils.PickAndShow(PickOptions.Default);
        if(result != null)
        {
            LabelProjectName.Text = result.FullPath;
            try
            {
                CliMainSingle.cliMain = new CliMain(new FileInfo(result.FullPath), null, null, true, false);
                LabelInfo.Text = CliMainSingle.cliMain.getQuickInfo();
                DataEditBtn.IsEnabled = true;
                RunScriptBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                LabelInfo.Text = ex.Message;
            }

            
        }
    }

    private async void OnInfoShowClicked(object? sender, EventArgs e)
    {
        if (CliMainSingle.cliMain != null)
        {
            await Navigation.PushAsync(new DetailInfoInternative());
        }
    }

    private async void OnRunScriptClicked(object? sender, EventArgs e)
    {
        if (CliMainSingle.cliMain != null)
        {
            await Navigation.PushAsync(new LoadScriptPage());
        }
    }
    
    
}