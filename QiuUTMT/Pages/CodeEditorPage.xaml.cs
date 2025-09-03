using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Platform;
#if ANDROID
using IO.Github.Rosemoe.Sora.Widget;
#endif
namespace QiuUTMT;

public partial class CodeEditorPage : ContentPage
{
    private String codestring="";
    private bool firstinit = false;
#if  ANDROID
    private CodeEditor mCodeEditor;
#endif
    public CodeEditorPage()
    {
        InitializeComponent();
        App.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
    }

    public CodeEditorPage(String codestring):this()
    {
        this.codestring = codestring;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
#if  ANDROID
        if (!firstinit)
        {
            firstinit = true;
            mCodeEditor = (CodeEditor)SoraEditor.ToPlatform(SoraEditor.Handler.MauiContext);
            mCodeEditor.SetText(codestring);
        }
#endif
    }
}