using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QiuUTMT.ViewableEditor.Tools;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using UTMTdrid;

namespace QiuUTMT.ViewableEditor.Editors;

public partial class SpriteEditor : EditorPage
{
    public SpriteEditor(object dataContext) : base(dataContext)
    {
        InitializeComponent();
    }

    private async void ExportAll_Click(object sender, EventArgs e)
    {
        UndertaleSprite sprite = this.DataContext as UndertaleSprite;

        var savePath = await MAUIBridge.SaveFile(sprite.Name.Content + ".png", CancellationToken.None);

        if (sprite.IsSpineSprite)
        {
            SpriteEditorTools.ExportAllSpine(sprite);
            if (sprite.SpineHasTextureData)
                return;
        }

        if (savePath != null)
        {
            try
            {
                bool includePadding = (CommonTools.ShowQuestion("要包括边界(Padding)吗?"));

                using TextureWorker worker = new();
                if (sprite.Textures.Count > 1)
                {
                    string dir = Path.GetDirectoryName(savePath);
                    string name = Path.GetFileNameWithoutExtension(savePath);
                    string path = Path.Combine(dir, name);
                    string ext = Path.GetExtension(savePath);

                    Directory.CreateDirectory(path);
                    foreach (var tex in sprite.Textures.Select((tex, id) => new { id, tex }))
                    {
                        try
                        {
                            worker.ExportAsPNG(tex.tex.Texture,
                                Path.Combine(path, sprite.Name.Content + "_" + tex.id + ext), null, includePadding);
                        }
                        catch (Exception ex)
                        {
                            CommonTools.ShowError("Failed to export file: " + ex.Message, "Failed to export file");
                        }
                    }
                }
                else if (sprite.Textures.Count == 1)
                {
                    try
                    {
                        worker.ExportAsPNG(sprite.Textures[0].Texture, savePath, null, includePadding);
                    }
                    catch (Exception ex)
                    {
                        CommonTools.ShowError("Failed to export file: " + ex.Message, "Failed to export file");
                    }
                }
                else
                {
                    CommonTools.ShowError("No frames to export", "Failed to export sprite");
                }
            }
            catch (Exception ex)
            {
                CommonTools.ShowError("Failed to export: " + ex.Message, "Failed to export sprite");
            }
        }
    }
}