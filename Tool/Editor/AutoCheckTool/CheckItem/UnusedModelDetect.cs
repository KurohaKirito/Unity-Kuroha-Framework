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
        var errorInfos = UnusedAssetCleaner.Detect(
            UnusedAssetCleaner.UnusedAssetType.Model,
            "Assets/Art/Effects/Models",
            true);
    
        foreach (var info in errorInfos)
        {
            if (info.type == UnusedAssetCleaner.ErrorType.NoneReference)
            {
                var result = new Dictionary<string, string>
                {
                    {"错误名称", "未放置在无引用文件夹下的无引用模型资源"},
                    {"资源路径", info.assetPath},
                    {"错误等级", "Error"},
                    {"负责人", "傅佳亿"},
                    {"备注", "请确认是否需要移动到无引用文件夹"}
                };
                results.Add(result);
            }
            else if (info.type == UnusedAssetCleaner.ErrorType.HadReference)
            {
                var result = new Dictionary<string, string>
                {
                    {"错误名称", "放置在无引用文件夹下的有引用模型资源"},
                    {"资源路径", info.assetPath},
                    {"错误等级", "Error"},
                    {"负责人", "傅佳亿"},
                    {"备注", "请确认是否需要移出无引用文件夹"}
                };
                results.Add(result);
            }
        }
        
        if (isExportFile)
        {
            AutoCheckToolGUI.ExportResult(results);
            DebugUtil.Log("Unused Model Check Completed!");
        }

        return results;
    }
}
