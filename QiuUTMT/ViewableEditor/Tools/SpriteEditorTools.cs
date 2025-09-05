using UndertaleModLib.Models;
using UTMTdrid;

namespace QiuUTMT.ViewableEditor.Tools;

public class SpriteEditorTools
{
    /// <summary>
    /// 导出所有图像的帧
    /// NOTICE: 必须在线程执行
    /// </summary>
    /// <param name="sprite"></param>
    public static void ExportAllSpine(UndertaleSprite sprite)
    {
        CommonTools.ShowWarning(
            "所有帧的图片都会被导出,包括描述帧关系的 .json 和 .atlas 文件。\n" +
            "文件将按原样导出，如果必要请同时更改附带的 .json 文件。",
            "提醒");
        var savePath = MAUIBridge.PickFolder(CancellationToken.None).Result;
        if (savePath != null)
        {
            try
            {
                string dir = Path.GetDirectoryName(savePath);
                string name = Path.GetFileNameWithoutExtension(savePath);
                string path = Path.Combine(dir, name);
                string ext = Path.GetExtension(savePath);

                if (sprite.SpineTextures.Count > 0)
                {
                    Directory.CreateDirectory(path);

                    // textures
                    if (sprite.SpineHasTextureData)
                    {
                        foreach (var tex in sprite.SpineTextures.Select((tex, id) => new { id, tex }))
                        {
                            try
                            {
                                File.WriteAllBytes(Path.Combine(path, tex.id + ext), tex.tex.TexBlob);
                            }
                            catch (Exception ex)
                            {
                                CommonTools.ShowError("导出失败： " + ex.Message,
                                    "导出错误");
                            }
                        }
                    }

                    // json and atlas
                    File.WriteAllText(Path.Combine(path, "spine.json"), sprite.SpineJSON);
                    File.WriteAllText(Path.Combine(path, "spine.atlas"), sprite.SpineAtlas);
                }
            }
            catch (Exception ex)
            {
                CommonTools.ShowError("导出错误: " + ex.Message, "无法导出精灵图");
            }
        }
    }
}