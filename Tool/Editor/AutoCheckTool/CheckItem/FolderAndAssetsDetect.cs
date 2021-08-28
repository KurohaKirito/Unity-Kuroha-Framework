using System.Collections.Generic;
using System.Linq;
using Kuroha.Tool.Editor.AssetBatchTool;
using Kuroha.Util.Editor;
using Kuroha.Util.Release;

public static class FolderAndAssetsDetect
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

        var countResultList = BundleAssetCounter.Count("Assets/Art/Effects/Textures", 50);

        // 删除掉名为 .git 的文件夹
        countResultList = countResultList.Where(t => t.currentFolder.Name.Equals(".git") == false).ToList();
        
        // 仅输出资源与文件夹同级的结果
        foreach (var countResult in countResultList)
        {
            if (countResult.isFoldersAndAssets)
            {
                foreach (var asset in countResult.assets)
                {
                    var assetPath = PathUtil.GetAssetPath(asset.FullName);
                    var result = new Dictionary<string, string>
                    {
                        {"错误信息", "资源与文件夹同级"},
                        {"资源路径", assetPath},
                        {"错误等级", "Error"},
                        {"负责人", "傅佳亿"},
                        {"备注", "请仔细检查并修正资源路径!" }
                    };

                    results.Add(result);
                }
            }
        }
        
        if (isExportFile)
        {
            AutoCheckTool.ExportResult(results);
            DebugUtil.Log("FolderAndAssets Check Completed!");
        }
        
        return results;
    }
}
