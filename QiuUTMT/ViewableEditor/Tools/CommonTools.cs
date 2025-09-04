using UTMTdrid;

namespace QiuUTMT.ViewableEditor.Tools;

public class CommonTools
{
    public static void ShowWarning(string msg,string? caption=null)
    {
        //TODO
    }
    public static void ShowError(string msg,string? caption=null)
    {
        //TODO
    }

    public static bool ShowQuestion(string msg, string? caption = null)
    {
        return MAUIBridge.AskDialog(caption??"质询", msg).Result;
    }
}