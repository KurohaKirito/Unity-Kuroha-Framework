using System.Collections.Generic;
using System.Linq;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.AssetBatchTool;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

public static class MaterialShaderDetect
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

        // 获取相对目录下所有的材质球
        var guids = AssetDatabase.FindAssets("t:Material", new[] {"Assets/Art/Effects/Materials"});
        var assetPaths = new List<string>(guids.Select(AssetDatabase.GUIDToAssetPath));
        
        // 加载全部的材质球
        var shaderCheckerDataList = new List<ShaderChecker.ShaderCheckerData>();
        if (assetPaths.Count > 0)
        {
            for (var index = 0; index < assetPaths.Count; index++)
            {
                ProgressBar.DisplayProgressBar("指定 Shader 引用检测工具", $"加载材质中: {index + 1}/{assetPaths.Count}", index + 1, assetPaths.Count);

                var material = AssetDatabase.LoadAssetAtPath<Material>(assetPaths[index]);
                
                shaderCheckerDataList.Add(new ShaderChecker.ShaderCheckerData
                {
                    prefabPath = null,
                    subObjectName = null,
                    material = material
                });
            }
        }
        
        // 调用检测方法
        var detectResult = ShaderChecker.Detect(shaderCheckerDataList, "Lightweight Render Pipeline", true);
        
        // 整理数据
        foreach (var path in detectResult)
        {
            var result = new Dictionary<string, string>
            {
                {"错误名称", "材质球引用了 Lightweight Render Pipeline 着色器"},
                {"资源路径", path},
                {"错误等级", "Error"},
                {"负责人", "傅佳亿"},
                {"备注", "请仔细检查并修复" }
            };

            results.Add(result);
        }
        
        if (isExportFile)
        {
            AutoCheckTool.ExportResult(results);
            DebugUtil.Log("Material-Shader Check Completed!");
        }

        return results;
    }
}