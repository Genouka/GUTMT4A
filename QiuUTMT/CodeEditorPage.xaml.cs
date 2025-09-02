using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;

namespace QiuUTMT;

public partial class CodeEditorPage : ContentPage
{
    private String codestring="";

    public CodeEditorPage()
    {
        InitializeComponent();
        //SoraEditor.Text = codestring;
    }

    public CodeEditorPage(String codestring)
    {
        this.codestring = codestring;
        this();
    }
    
}