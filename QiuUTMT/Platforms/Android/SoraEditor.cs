using Android.Graphics;
using Android.OS;
using IO.Github.Rosemoe.Sora.Langs.Textmate;
using IO.Github.Rosemoe.Sora.Langs.Textmate.Registry;
using IO.Github.Rosemoe.Sora.Langs.Textmate.Registry.Model;
using IO.Github.Rosemoe.Sora.Langs.Textmate.Registry.Provider;
using IO.Github.Rosemoe.Sora.Widget;
using Microsoft.Maui.Handlers;
using Org.Eclipse.Tm4e.Core.Registry;

namespace QiuUTMT;

public partial class SoraEditorHandler : ViewHandler<ISoraEditor,CodeEditor>
{
    
    private CodeEditor codeEditor;
    internal static Bundle Bundle { get; set; }

    public SoraEditorHandler(IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
    {
    }

    private static bool _doMeOnlyOnceFlag;
    public static  void DoMeOnlyOnce()
    {
        if (!_doMeOnlyOnceFlag)
        {
            _doMeOnlyOnceFlag = true;
            FileProviderRegistry.Instance.AddFileProvider(
                new AssetsFileResolver(
                    Platform.AppContext.Assets // 使用应用上下文
                )
            );
            var themeRegistry = ThemeRegistry.Instance;
            var themeName = "solarized_dark"; // 主题名称
            var themeAssetsPath = "textmate/" + themeName + ".json";
            var themeModel = new ThemeModel(
                IThemeSource.FromInputStream(
                    FileProviderRegistry.Instance.TryGetInputStream(themeAssetsPath), themeAssetsPath, null
                ),
                themeName
            );
// 如果主题是适用于暗色模式的，请额外添加以下内容
// model.setDark(true);
            themeModel.Dark=true;
            themeRegistry.LoadTheme(themeModel);
            themeRegistry.SetTheme(themeName);
            GrammarRegistry.Instance.LoadGrammars("textmate/languages.json");
        }
    }


    protected override CodeEditor CreatePlatformView()
    {
        DoMeOnlyOnce();
        codeEditor = new CodeEditor(Context);
        codeEditor.TypefaceText=Typeface.Monospace;
        codeEditor.NonPrintablePaintingFlags=(
            CodeEditor.FlagDrawWhitespaceLeading | CodeEditor.FlagDrawLineSeparator | CodeEditor.FlagDrawWhitespaceInSelection); // Show Non-Printable Characters
        codeEditor.ColorScheme = TextMateColorScheme.Create(ThemeRegistry.Instance);
        var languageScopeName = "source.gml"; // 您目标语言的作用域名称
        var language = TextMateLanguage.Create(
            languageScopeName, true /* true表示启用自动补全 */
        );
        codeEditor.EditorLanguage=language;
        return codeEditor;
    }

    protected override void ConnectHandler(CodeEditor platformView)
    {
        base.ConnectHandler(platformView);
    }
}