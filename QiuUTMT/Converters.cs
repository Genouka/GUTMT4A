namespace QiuUTMT;

using System.Globalization;

public class IndentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int indentLevel && parameter is string paramStr && int.TryParse(paramStr, out int baseIndent))
        {
            return $"{indentLevel * baseIndent}, Auto, *";
        }

        return "Auto, Auto, *";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ExpandableIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isExpandable && isExpandable)
        {
            return "arrow_right.png"; // 需要添加这个图标到项目中
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}