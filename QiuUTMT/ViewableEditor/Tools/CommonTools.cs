using UTMTdrid;

namespace QiuUTMT.ViewableEditor.Tools;

public class CommonTools
{
    public static void ShowWarning(string msg, string? caption = null)
    {
        //此方法是异步的
#if ANDROID
        Bindme.dMsgDialog(caption ?? "警告", msg);
#endif
    }

    public static void ShowError(string msg, string? caption = null)
    {
        //此方法是异步的
#if ANDROID
        Bindme.dMsgDialog(caption ?? "错误", msg);
#endif
    }
    
    public static void ShowMessage(string msg, string? caption = null)
    {
        //此方法是异步的
#if ANDROID
        Bindme.dMsgDialog(caption ?? "信息", msg);
#endif
    }

    public static bool ShowQuestion(string msg, string? caption = null)
    {
        return MAUIBridge.AskDialog(caption ?? "质询", msg).Result;
    }
}