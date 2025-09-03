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
        LabelProjectName.Text = "按[打开]按钮选择你心仪的GM数据文件吧~\n当前应用版本:"+AppInfo.Current.VersionString+"\n爱来自Bilibili:@秋冥散雨_GenOuka，要不要考虑打赏一下？\n免责声明：您保证您拥有对您打开数据文件的知识产权，我们坚决禁止侵权行为，如有任何由于此工具引起的纠纷，责任均由您承担，开发者免除所有责任。\n本程序可能存在缺陷，不具有质量保证，由于本程序导致的任何损失，开发者不负任何责任。";
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
        SaveBtn.IsEnabled = false;
        var result = await OpenfileUtils.PickAndShow(PickOptions.Default);
        if(result != null)
        {
            LabelProjectName.Text = result.FullPath;
            try
            {
                QiuFuncMainSingle.QiuFuncMain = new QiuFuncMain(new FileInfo(result.FullPath), null, null, true, false);
                LabelInfo.Text = QiuFuncMainSingle.QiuFuncMain.getQuickInfo();
                DataEditBtn.IsEnabled = true;
                RunScriptBtn.IsEnabled = true;
                SaveBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                LabelInfo.Text = ex.Message;
            }
        }
    }

    private async void OnInfoShowClicked(object? sender, EventArgs e)
    {
        if (QiuFuncMainSingle.QiuFuncMain != null)
        {
            await Navigation.PushAsync(new DataTreePage());
        }
    }

    private async void OnRunScriptClicked(object? sender, EventArgs e)
    {
        if (QiuFuncMainSingle.QiuFuncMain != null)
        {
            await Navigation.PushAsync(new LoadScriptPage());
        }
    }


    private async void OnAboutBtnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutPage());
    }

    private async void OnSaveBtnClicked(object? sender, EventArgs e)
    {
        QiuFuncMain.clearCallbacks();
        var result=await MAUIBridge.SaveFile("data.win",CancellationToken.None);
        if (result is not null)
        {
            QiuFuncMainSingle.QiuFuncMain.SaveDataFile(result);
#if ANDROID
            Bindme.dMsgDialog("导出成功","文件已保存至:"+result);
#endif
        }
    }
}