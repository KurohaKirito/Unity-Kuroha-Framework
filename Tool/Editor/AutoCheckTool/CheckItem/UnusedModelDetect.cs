using System.Collections.Generic;
using Kuroha.Tool.Editor.AssetBatchTool;
using Kuroha.Util.Release;

public static class UnusedModelDetect
{
    /// <summary>
    /// 自动检测使用
    /// </summary>
    public static void Detect()
    {
        Check();
    }

    /// <summary>
    /// 检测无引用的纹理 (废弃纹理)
    /// </summary>
    /// <param name="isExportFile">是否导出文件, 默认导出文件</param>
    public static List<Dictionary<string, string>> Check(bool isExportFile = true)
    {
        var results = new List<Dictionary<string, string>>();
        
        // 执行检测
        var paths = UnusedAssetCleaner.Detect(
            UnusedAssetCleaner.UnusedAssetType.Model,
            "Assets/Art/Effects/Models",
            true);
    
        foreach (var path in paths)
        {
            var result = new Dictionary<string, string> 
            {
                {"错误名称", "无引用的模型资源"},
                {"资源路径", path},
                {"错误等级", "Error"},
                {"负责人", "傅佳亿"},
                {"备注", "可使用批量删除工具进行批量删除, 删除前请仔细确认."}
            };
            
            results.Add(result);
        }
        
        if (isExportFile)
        {
            AutoCheckTool.ExportResult(results);
            DebugUtil.Log("Unused Model Check Completed!");
        }

        return results;
    }
}
