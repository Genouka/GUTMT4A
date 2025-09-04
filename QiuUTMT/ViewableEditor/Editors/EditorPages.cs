using UndertaleModLib;
using UndertaleModLib.Models;

namespace QiuUTMT.ViewableEditor.Editors;

public class EditorPages
{
    public delegate TResult FuncWithArg1<out TResult>(object a) where TResult : allows ref struct;
    public static readonly Dictionary<Type,FuncWithArg1<EditorPage>> EditorPagesConstructors = new()
    {
        {typeof(UndertaleSprite), (d)=>new SpriteEditor(d)}
    };
}