using System.Collections.Generic;
using Kuroha.Tool.Editor.AssetBatchTool;

public static class UnusedMaterialDetect
{
    public static void Detect()
    {
        var paths = UnusedAssetCleaner.Detect(UnusedAssetCleaner.UnusedAssetType.Material, "Assets/Art/Effects/Materials", true);
        
        foreach (var path in paths)
        {
            var result = new Dictionary<string, string>
            {
                {"错误名称", "未使用的材质资源"},
                {"资源路径", path},
                {"错误等级", "Error"},
                {"负责人", "傅佳亿"},
                {"备注", "可用工具批量删除(删除前请仔细确认)"}
            };
            //AutoCheckTool.results.Add(result);
        }
    }
}
