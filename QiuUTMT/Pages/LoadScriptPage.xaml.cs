using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTMTdrid;

namespace QiuUTMT;

public partial class LoadScriptPage : ContentPage
{

    public LoadScriptPage()
    {
        InitializeComponent();
        LabelOutput.FontFamily="monospace";
    }

    private void SetButtonsEnabled(bool isEnabled)
    {
        OpenFileBtn.IsEnabled = isEnabled;
        OpenRawFileBtn.IsEnabled = isEnabled;
    }
    private async void OpenFileBtn_OnClicked(object? sender, EventArgs e)
    {
        LabelOutput.Text = "";
        SetButtonsEnabled(false);
        var result = await OpenfileUtils.PickAndShow(PickOptions.Default);
        if (result != null)
        {
            await Task.Run(async () =>
            {
                bool isSuccessful = QiuFuncMainSingle.QiuFuncMain.RunCSharpFilePublic(result.FullPath,
                    line => { MainThread.BeginInvokeOnMainThread(() => { LabelOutput.Text += line; }); }, this);
            });
        }
        SetButtonsEnabled(true);
    }

    private async void OpenRawFileBtn_OnClicked(object? sender, EventArgs e)
    {
        LabelOutput.Text = "";
        SetButtonsEnabled(false);
        using var stream1 = await FileSystem.OpenAppPackageFileAsync("scriptpath.txt");
        using var reader1 = new StreamReader(stream1);
        var contents1 = reader1.ReadToEnd();
        var list = contents1.Split(";;");
        string action = await DisplayActionSheet("选择要运行的脚本", "取消", null, list);
        if (action != null && action != "" && action != "取消")
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(action);
            using var reader = new StreamReader(stream);
            var contents = reader.ReadToEnd();
            await Task.Run(async () =>
            {
                QiuFuncMainSingle.QiuFuncMain.RunCSharpCodePublic2(line =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LabelOutput.Text += line;
                    });
                },this, contents);
            });
        }
        SetButtonsEnabled(true);
    }
}