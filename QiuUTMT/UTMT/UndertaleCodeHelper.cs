using Underanalyzer.Decompiler;
using UndertaleModLib;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;

namespace QiuUTMT.UTMT;

public class UndertaleCodeHelper
{
    private UndertaleData GetData()
    {
        if (QiuFuncMainSingle.QiuFuncMain == null)
        {
            throw new Exception("QiuFuncMainSingle.QiuFuncMain尚未初始化，请加载正确的数据文件");
        }

        if (QiuFuncMainSingle.QiuFuncMain == null)
        {
            throw new Exception("QiuFuncMainSingle.QiuFuncMain.Data == null尚未初始化，请加载正确的数据文件");
        }

        return QiuFuncMainSingle.QiuFuncMain.Data;
    }

    public string GetDecompiledText(string codeName, GlobalDecompileContext context = null,
        IDecompileSettings settings = null)
    {
        return GetDecompiledText(GetData().Code.ByName(codeName), context, settings);
    }

    public string GetDecompiledText(UndertaleCode code, GlobalDecompileContext? context = null,
        IDecompileSettings? settings = null)
    {
        if (code.ParentEntry is not null)
            return $"// 本代码项是 \"{code.ParentEntry.Name.Content}\" 中的匿名函数, 请反编译它.";

        GlobalDecompileContext globalDecompileContext = context is null ? new(GetData()) : context;
        try
        {
            return code != null
                ? new Underanalyzer.Decompiler.DecompileContext(globalDecompileContext, code,
                    settings ?? GetData().ToolInfo.DecompilerSettings).DecompileToString()
                : "";
        }
        catch (Exception e)
        {
            return "/*\n反编译失败!\n\n" + e.ToString() + "\n*/";
        }
    }

    public string GetDisassemblyText(UndertaleCode code)
    {
        if (code.ParentEntry is not null)
            return $"// 本代码项是 \"{code.ParentEntry.Name.Content}\" 中的匿名函数, 请反编译它.";

        try
        {
            return code != null
                ? code.Disassemble(GetData().Variables, GetData().CodeLocals?.For(code), GetData().CodeLocals is null)
                : "";
        }
        catch (Exception e)
        {
            return "/*\n反编译失败!\n\n" + e.ToString() + "\n*/"; // Please don't
        }
    }

    public string GetDisassemblyText(string codeName)
    {
        return GetDisassemblyText(GetData().Code.ByName(codeName));
    }
}