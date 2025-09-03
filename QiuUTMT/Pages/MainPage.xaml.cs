using System.Reflection;
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
        LabelProjectName.Text = "按[打开]按钮选择Gamemaker数据文件吧！\n"+
                                "常见文件格式:(*.win,*.unx,*.droid,*.ios,audiogroup*.dat)\n" +
                                "当前应用版本:" + AppInfo.Current.VersionString + "\n" +
                                "爱来自Bilibili:@秋冥散雨_GenOuka，要不要考虑打赏一下？\n" +
                                "免责声明：您应当确保您拥有对您打开数据文件的知识产权，我们坚决禁止侵权行为，如有任何由于此工具引起的纠纷，责任均由您承担，开发者免除所有责任。\n" +
                                "本程序可能存在缺陷，不具有质量保证，由于本程序导致的任何损失由您承担，开发者不负任何责任。";
    }

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
            return;

#if ANDROID
    if (!CheckPermission.CheckExternalStoragePermission())
        return;
#endif

        LabelInfo.Text = "";

        OpenBtn.IsEnabled = DataEditBtn.IsEnabled = RunScriptBtn.IsEnabled = SaveBtn.IsEnabled = false;

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var fileResult = await OpenfileUtils.PickAndShow(PickOptions.Default);
            if (fileResult == null) return;

            await Task.Run(() =>
            {
                QiuFuncMainSingle.QiuFuncMain = new QiuFuncMain(
                    new FileInfo(fileResult.FullPath),
                    null, null, true, false);
            });

            LabelProjectName.Text = fileResult.FullPath;
            LabelInfo.Text = QiuFuncMainSingle.QiuFuncMain.getQuickInfo();
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                LabelInfo.Text = ex.Message);
        }
        finally
        {
            // 隐藏 Loading、恢复按钮
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                OpenBtn.IsEnabled = DataEditBtn.IsEnabled = RunScriptBtn.IsEnabled = SaveBtn.IsEnabled = true;
            });
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
        var result = await MAUIBridge.SaveFile("data.win", CancellationToken.None);
        if (result is not null && QiuFuncMainSingle.QiuFuncMain != null)
        {
            QiuFuncMainSingle.QiuFuncMain.SaveDataFile(result);
#if ANDROID
            Bindme.dMsgDialog("导出成功","文件已保存至:"+result);
#endif
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (QiuFuncMainSingle.QiuFuncMain != null)
        {
            LabelInfo.Text = QiuFuncMainSingle.QiuFuncMain.getQuickInfo();
        }
    }
}