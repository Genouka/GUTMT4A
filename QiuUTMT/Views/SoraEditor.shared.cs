using Microsoft.Maui.Handlers;

namespace QiuUTMT;

public interface ISoraEditor : IView
{
}

public class SoraEditor : View, ISoraEditor
{
}
#if ANDROID
partial class SoraEditorHandler
{
    public static IPropertyMapper<SoraEditor, SoraEditorHandler> MapMapper =
        new PropertyMapper<SoraEditor, SoraEditorHandler>(ViewHandler.ViewMapper)
        {
        };

    public SoraEditorHandler() : base(MapMapper)
    {
    }
}
#endif