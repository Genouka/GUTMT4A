using Android.Content;
using Android.OS;
using Android.Provider;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace QiuUTMT;
public static class CheckPermission
{
    public static bool CheckExternalStoragePermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            var result = Android.OS.Environment.IsExternalStorageManager;
            if (!result)
            {
                var manage = Settings.ActionManageAppAllFilesAccessPermission;
                Intent intent = new Intent(manage);
                Android.Net.Uri uri = Android.Net.Uri.Parse("package:" + AppInfo.Current.PackageName);
                intent.SetData(uri);
                Platform.CurrentActivity.StartActivity(intent);
            }
            return result;
        }

        return true;
    }

    public static void createLooper()
    {
        Android.OS.Looper.Prepare();
    }
}
