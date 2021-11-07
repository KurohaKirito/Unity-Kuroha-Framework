using System.Collections.Generic;
using Kuroha.Tool.AssetTool.Editor.AssetBatchTool;
using Kuroha.Util.RunTime;

public static class UnusedMaterialDetect
{
    /// <summary>
    /// 自动检测使用
    /// </summary>
    public static void Detect()
    {
        Check("Assets/Art/Effects/Materials", "傅佳亿");
    }

    /// <summary>
    /// 检测无引用的纹理 (废弃纹理)
    /// </summary>
    /// <param name="path">检测路径</param>
    /// <param name="principal">负责人</param>
    /// <param name="isExportFile">是否导出文件, 默认导出文件</param>
    public static List<Dictionary<string, string>> Check(string path, string principal, bool isExportFile = true)
    {
        var results = new List<Dictionary<string, string>>();
        
        // 执行检测
        var errorInfos = UnusedAssetCleaner.Detect(UnusedAssetCleaner.UnusedAssetType.Material, path, true);
        
        foreach (var info in errorInfos)
        {
            if (info.type == UnusedAssetCleaner.ErrorType.NoneReference)
            {
                var result = new Dictionary<string, string>
                {
                    {"错误名称", "未放置在无引用文件夹下的无引用材质资源"},
                    {"资源路径", info.assetPath},
                    {"错误等级", "Error"},
                    {"负责人", principal},
                    {"备注", "请确认是否需要移动到无引用文件夹"}
                };
                results.Add(result);
            }
            else if (info.type == UnusedAssetCleaner.ErrorType.HadReference)
            {
                var result = new Dictionary<string, string>
                {
                    {"错误名称", "放置在无引用文件夹下的有引用材质资源"},
                    {"资源路径", info.assetPath},
                    {"错误等级", "Error"},
                    {"负责人", principal},
                    {"备注", "请确认是否需要移出无引用文件夹"}
                };
                results.Add(result);
            }
        }
        
        if (isExportFile)
        {
            AutoCheckToolGUI.ExportResult(results);
            DebugUtil.Log("Unused Material Check Completed!");
        }

        return results;
    }
}
