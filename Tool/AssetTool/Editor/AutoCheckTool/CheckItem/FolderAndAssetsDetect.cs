using System.Collections.Generic;
using System.Linq;
using Kuroha.Tool.AssetTool.Editor.AssetBatchTool;
using Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchItem;
using Kuroha.Tool.AssetTool.Editor.AutoCheckTool.GUI;
using Kuroha.Util.Editor;
using Kuroha.Util.RunTime;

namespace Kuroha.Tool.AssetTool.Editor.AutoCheckTool.CheckItem
{
    public static class FolderAndAssetsDetect
    {
        /// <summary>
        /// 执行检测
        /// </summary>
        /// <param name="path">检测路径</param>
        /// <param name="max">资源最大数量</param>
        /// <param name="principal">负责人</param>
        /// <param name="isExportFile">是否导出文件, 默认导出文件</param>
        public static AutoCheckToolCheckResult Check(string path, int max, string principal, bool isExportFile = true)
        {
            var results = new AutoCheckToolCheckResult();

            var countResultList = BundleAssetCounter.Count(path, max);

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
                            {"错误名称", "资源与文件夹同级"},
                            {"资源路径", assetPath},
                            {"错误等级", "Error"},
                            {"负责人", principal},
                            {"备注", "请仔细检查并修正资源路径!" }
                        };

                        results.resultList.Add(result);
                    }
                }
            }
        
            if (isExportFile)
            {
                AutoCheckToolGUI.ExportResult(results);
                DebugUtil.Log("FolderAndAssets Check Completed!");
            }
        
            return results;
        }
    }
}
