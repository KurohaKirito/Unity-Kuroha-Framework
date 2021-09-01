using System.Collections.Generic;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.TextureAnalysisTool;
using Kuroha.Util.Editor;
using Kuroha.Util.Release;

public static class RepeatTextureDetect
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
            ProgressBar.DisplayProgressBar("重复纹理检测工具", $"纹理检测中: {index + 1}/{textures.Count}", index + 1, textures.Count);
            
            if (texturePaths[index].EndsWith(".png") || texturePaths[index].EndsWith(".tga"))
            {
                // 重复纹理检测
                var isBegin = index == 0;
                TextureRepeatChecker.CheckOneTexture(texturePaths[index], isBegin);
            }
        }
        
        // 处理重复纹理的检测结果数据
        var repeatTextureInfos = TextureRepeatChecker.GetResult();
        foreach (var repeatTextureInfo in repeatTextureInfos)
        {
            var result = new Dictionary<string, string>
            {
                { "错误名称", "存在一组重复纹理" },
                { "资源路径", repeatTextureInfo.assetPaths[0] },
                { "错误等级", "Error" },
                { "负责人", "傅佳亿" },
                { "备注", "请使用贴图统计分析工具查看详细的检测结果, 进行人工确认并修复!" }
            };
            
            results.Add(result);
        }
        
        if (isExportFile)
        {
            AutoCheckTool.ExportResult(results);
            DebugUtil.Log("Repeat Texture Check Completed!");
        }

        return results;
    }
}
