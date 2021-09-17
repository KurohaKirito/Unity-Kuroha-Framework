using System.Collections.Generic;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.AssetBatchTool;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

public static class RedundantTextureReferencesDetect
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
        
        // 获取相对目录下所有的材质球文件
        var guids = AssetDatabase.FindAssets("t:Material", new []{"Assets/Art/Effects/Materials"});
        var materials = new List<Material>(guids.Length);
        for (var index = 0; index < guids.Length; index++)
        {
            ProgressBar.DisplayProgressBar("材质球冗余纹理检测工具", $"加载材质球: {index + 1}/{guids.Length}", index + 1, guids.Length);
            var assetPath = AssetDatabase.GUIDToAssetPath(guids[index]);
            materials.Add(AssetDatabase.LoadAssetAtPath<Material>(assetPath));
        }

        // 遍历材质
        var materialCount = materials.Count;
        for (var index = 0; index < materialCount; index++)
        {
            ProgressBar.DisplayProgressBar("材质球冗余纹理检测工具", $"检测材质球: {index + 1}/{materialCount}", index + 1, materialCount);

            var material = materials[index];
            if (RedundantTextureReferencesCleaner.Detect(material, false))
            {
                var result = new Dictionary<string, string>
                {
                    {"错误名称", "材质球中存在冗余的纹理引用"},
                    {"资源路径", AssetDatabase.GetAssetPath(material)},
                    {"错误等级", "Error"},
                    {"负责人", "傅佳亿"},
                    {"备注", "可使用资源检测工具批处理标签页面中的材质球冗余引用清除工具自动清除" }
                };

                results.Add(result);
            }
        }

        if (isExportFile)
        {
            AutoCheckToolGUI.ExportResult(results);
            DebugUtil.Log("RedundantTextureReferences Check Completed!");
        }

        return results;
    }
}
