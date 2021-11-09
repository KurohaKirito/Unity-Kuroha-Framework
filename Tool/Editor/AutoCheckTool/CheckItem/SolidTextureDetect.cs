using System.Collections.Generic;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using UnityEditor;

public static class SolidTextureDetect {
    /// <summary>
    /// 自动检测使用
    /// </summary>
    public static void Detect() {
        Check("Assets/Art/Effects/Textures", "傅佳亿");
    }

    /// <summary>
    /// 执行检测
    /// </summary>
    /// <param name="path">检测路径</param>
    /// <param name="principal">负责人</param>
    /// <param name="isExportFile">是否导出文件, 默认导出文件</param>
    public static List<Dictionary<string, string>> Check(string path, string principal, bool isExportFile = true) {
        var results = new List<Dictionary<string, string>>();

        // 获取全部纹理
        TextureUtil.GetTexturesInPath(new[] {
            path
        }, out var textures, out var texturePaths);

        // 检测每一张纹理
        for (var index = 0; index < textures.Count; index++) {
            ProgressBar.DisplayProgressBar("纯色纹理检测工具", $"纹理检测中: {index + 1}/{textures.Count}", index + 1, textures.Count);

            if (texturePaths[index].EndsWith(".png") || texturePaths[index].EndsWith(".tga")) {
                // 检测纹理的导入类型必须为: Texture2D
                var textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePaths[index]);
                if (ReferenceEquals(textureImporter, null) == false) {
                    if (textureImporter.textureShape == TextureImporterShape.Texture2D) {
                        // 检测纯色和尺寸
                        var isSolid = TextureUtil.IsSolidColor(textures[index]);
                        if (isSolid) {
                            if (textures[index].width > 32 || textures[index].height > 32) {
                                var result = new Dictionary<string, string> {
                                    {
                                        "错误名称", "尺寸大于 32 X 32 的纯色纹理"
                                    }, {
                                        "资源路径", texturePaths[index]
                                    }, {
                                        "错误等级", "Error"
                                    }, {
                                        "负责人", principal
                                    }, {
                                        "备注", $"人工确认并修复, 当前尺寸: {textures[index].width} X {textures[index].height}"
                                    }
                                };

                                results.Add(result);
                            }
                        }
                    }
                }
            }
        }

        if (isExportFile) {
            AutoCheckToolGUI.ExportResult(results);
            DebugUtil.Log("Solid Color Texture Check Completed!");
        }

        return results;
    }
}