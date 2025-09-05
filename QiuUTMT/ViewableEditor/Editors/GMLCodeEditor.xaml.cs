using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Platform;
using QiuUTMT.ViewableEditor.Tools;
using UndertaleModLib;
using UndertaleModLib.Compiler;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using UTMTdrid;

#if ANDROID
using IO.Github.Rosemoe.Sora.Widget;
#endif

namespace QiuUTMT.ViewableEditor.Editors;

public partial class GMLCodeEditor : EditorPage
{
    private bool firstinited = false;
    private bool decodeMode = false;
#if ANDROID
    private CodeEditor mCodeEditor;
#endif

    public GMLCodeEditor(object dataContext) : base(dataContext)
    {
        InitializeComponent();
        //修复输入法问题
        App.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
    }

    private string DisassembleCode(UndertaleCode code)
    {
        string text;
        if (code.ParentEntry != null)
        {
            text = "; 这是 " + code.ParentEntry.Name.Content + "中的匿名代码，请前往对应对象查看.";
        }
        else
        {
            try
            {
                var data = QiuFuncMainSingle.QiuFuncMain.Data;
                text = code.Disassemble(data.Variables, data.CodeLocals?.For(code), data.CodeLocals is null);
            }
            catch (Exception ex)
            {
                string exStr = ex.ToString();
                exStr = String.Join("\n;", exStr.Split('\n'));
                text = $";  反编译异常\n;   {exStr}\n";
            }
        }

        return text;
    }

    private string DecompileCode(UndertaleCode code)
    {
        string text;
        if (code.ParentEntry != null)
        {
            text = "// 这是 " + code.ParentEntry.Name.Content + "中的匿名代码，请前往对应对象查看.";
        }
        else
        {
            try
            {
                var dataa = QiuFuncMainSingle.QiuFuncMain.Data;
                GlobalDecompileContext context = new(dataa);
                text =
                    new Underanalyzer.Decompiler.DecompileContext(context, code, dataa.ToolInfo.DecompilerSettings)
                        .DecompileToString();
            }
            catch (Exception e)
            {
                text = "/* 反编译异常!\n   " + e.ToString() + "\n*/";
            }
        }

        return text;
    }

    private void AssemableCode(string assemableCode)
    {
        try
        {
            var data = QiuFuncMainSingle.QiuFuncMain.Data;
            var instructions = Assembler.Assemble(assemableCode, data);
            ((UndertaleCode)DataContext).Replace(instructions);
            CommonTools.ShowMessage("汇编成功，已保存更改");
        }
        catch (Exception ex)
        {
            CommonTools.ShowError(ex.ToString(), "汇编错误");
        }
    }

    private void CompileCode(string code)
    {
        var data = QiuFuncMainSingle.QiuFuncMain.Data;
        try
        {
            CompileGroup group = new(data);
            //group.MainThreadAction = (f) => { dispatcher.Invoke(() => f()); };
            group.QueueCodeReplace((UndertaleCode)DataContext, code);
            var compileResult = group.Compile();
            if (compileResult.Successful)
            {
                CommonTools.ShowMessage("编译成功，已保存更改");
            }
            else
            {
                CommonTools.ShowError(compileResult.PrintAllErrors(false), "编译错误");
            }
        }
        catch (Exception ex)
        {
            CommonTools.ShowError(ex.Message, "编译器异常");
            //rootException = ex.ToString();
        }
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
#if ANDROID
        if (!firstinited)
        {
            firstinited = true;
            mCodeEditor = (CodeEditor)SoraEditor.ToPlatform(SoraEditor.Handler.MauiContext);
            SetupDecodeCode();
        }
#endif
    }

    private void Switch_DecodeMode_OnToggled(object? sender, ToggledEventArgs e)
    {
        decodeMode = e.Value;
#if ANDROID
        if (firstinited)
        {
            mCodeEditor.SetText(DecompileCode((UndertaleCode)DataContext));
            SetupDecodeCode();
        }
#endif
    }

    private void SetupDecodeCode()
    {
#if ANDROID
        mCodeEditor.SetText(
            decodeMode ? DisassembleCode((UndertaleCode)DataContext) : DecompileCode((UndertaleCode)DataContext)
        );
#endif
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    private void Button_SaveCode_OnClicked(object? sender, EventArgs e)
    {
        if (decodeMode)
        {
            AssemableCode(GetCurrentCode());
        }
        else
        {
            CompileCode(GetCurrentCode());
        }
        
    }

    private string GetCurrentCode()
    {
#if ANDROID
        if (firstinited)
        {
            return mCodeEditor.Text.ToString();
        }
        else
        {
            throw new NotImplementedException("执行到此处必须先初始化mCodeEditor");
        }
#endif
        throw new NotImplementedException();
    }
}