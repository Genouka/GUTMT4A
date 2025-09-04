using System.Text.RegularExpressions;

namespace QiuUTMT;

public class FuzzyMatch
{
    public static bool IsMatch(string input, string pattern)
    {
        // 将通配符转换为正则表达式
        string regexPattern = "^" + Regex.Escape("*"+pattern+"*").Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(input, regexPattern);
    }
}