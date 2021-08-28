using System.Collections.Generic;
using Kuroha.GUI.Editor;
using Kuroha.Util.Editor;
using Kuroha.Util.Release;
using UnityEditor;

public static class SolidTextureDetect
{
    /// <summary>
    /// 自动检测使用
    /// </summary>
    public static void Detect()
    {
        Check();
    }
    
    /// <summary>
    /// 执行检测
    /// </summary>
    /// <param name="isExportFile">是否导出文件, 默认导出文件</param>
    public static List<Dictionary<string, string>> Check(bool isExportFile = true)
    {
        var results = new List<Dictionary<string, string>>();
        
        // 获取全部纹理
        TextureUtil.GetTexturesInPath(new[] { "Assets/Art/Effects/Textures" }, out var textures, out var texturePaths);
        
        // 检测每一张纹理
        for (var index = 0; index < textures.Count; index++)
        {
            ProgressBar.DisplayProgressBar("Texture", $"纹理检测中: {index + 1}/{textures.Count}", index + 1, textures.Count);
            
            if (texturePaths[index].EndsWith(".png") || texturePaths[index].EndsWith(".tga"))
            {
                // 检测纹理的导入类型必须为: Texture2D
                var textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePaths[index]);
                if (ReferenceEquals(textureImporter, null) == false)
                {
                    if (textureImporter.textureShape == TextureImporterShape.Texture2D)
                    {
                        // 检测纯色和尺寸
                        var isSolid = TextureUtil.IsSolidColor(textures[index]);
                        if (isSolid)
                        {
                            if (textures[index].width > 32 || textures[index].height > 32)
                            {
                                var result = new Dictionary<string, string>
                                {
                                    { "错误名称", "尺寸大于 32 X 32 的纯色纹理" },
                                    { "资源路径", texturePaths[index] },
                                    { "错误等级", "Error" },
                                    { "负责人", "傅佳亿" },
                                    { "备注", $"人工确认并修复, 当前尺寸: {textures[index].width} X {textures[index].height}" }
                                };
                                
                                results.Add(result);
                            }
                        }
                    }
                }
            }
        }
        
        if (isExportFile)
        {
            AutoCheckTool.ExportResult(results);
            DebugUtil.Log("Solid Color Texture Check Completed!");
        }

        return results;
    }
}
