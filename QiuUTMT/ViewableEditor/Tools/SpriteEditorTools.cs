using UndertaleModLib.Models;
using UTMTdrid;

namespace QiuUTMT.ViewableEditor.Tools;

public class SpriteEditorTools
{
    public static async void ExportAllSpine(UndertaleSprite sprite)
    {
        CommonTools.ShowWarning(
            "This seems to be a Spine sprite, .json and .atlas files will be exported together with the frames. " +
            "PLEASE EDIT THEM CAREFULLY! SOME MANUAL EDITING OF THE JSON MAY BE REQUIRED! THE DATA IS EXPORTED AS-IS.",
            "Spine warning");
        var savePath = await MAUIBridge.PickFolder(CancellationToken.None);
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
                                CommonTools.ShowError("Failed to export file: " + ex.Message, "Failed to export file");
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
                CommonTools.ShowError("Failed to export: " + ex.Message, "Failed to export sprite");
            }
        }
    }
}