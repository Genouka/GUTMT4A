using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiuUTMT;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private void OnHomepageBtnClicked(object? sender, EventArgs e)
    {
        try
        {
            Uri uri = new Uri("https://space.bilibili.com/3493116076100126");
            Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {}
    }

    private void OnProjectBtnClicked(object? sender, EventArgs e)
    {
        try
        {
            Uri uri = new Uri("https://github.com/Genouka/GUTMT4A");
            Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {}
    }

    private void OnNewVersionBtnClicked(object? sender, EventArgs e)
    {
        try
        {
            Uri uri = new Uri("https://github.com/Genouka/GUTMT4A/releases/latest");
            Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {}
    }

    private async void OnCodeEditorPageBtnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new CodeEditorPage());
    }
}