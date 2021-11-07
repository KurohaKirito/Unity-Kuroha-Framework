using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kuroha.GUI.Editor;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Check
{
    public static class CheckModel
    {
        /// <summary>
        /// 模型资源检查类型
        /// </summary>
        public static readonly string[] checkOptions =
        {
            "读写权限设置",
            "投射阴影设置",
            "法线导入设置",
            "网格优化",
            "网格压缩",
            "顶点焊接"
        };

        /// <summary>
        /// 模型资源检查类型
        /// </summary>
        public enum CheckOptions
        {
            ReadWriteEnable,
            RendererCastShadow,
            Normals,
            OptimizeMesh,
            MeshCompression,
            WeldVertices,
        }

        /// <summary>
        /// 对模型文件进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos)
        {
            if (itemData.path.StartsWith("Assets"))
            {
                var assetGuids = AssetDatabase.FindAssets("t:Model", new[] { itemData.path });
                
                for (var index = 0; index < assetGuids.Length; index++)
                {
                    ProgressBar.DisplayProgressBar("特效检测工具", $"FBX 排查中: {index + 1}/{assetGuids.Length}", index + 1, assetGuids.Length);
                    
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[index]);
                    var pattern = itemData.writePathRegex;
                    if (string.IsNullOrEmpty(pattern) == false)
                    {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(assetPath))
                        {
                            continue;
                        }
                    }
                    
                    switch ((CheckOptions)itemData.checkType)
                    {
                        case CheckOptions.ReadWriteEnable:
                            CheckReadWriteEnable(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.RendererCastShadow:
                            CheckCastShadows(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.Normals:
                            CheckNormals(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.OptimizeMesh:
                            CheckOptimizeMesh(assetPath, itemData, ref reportInfos);
                            break;
                            
                        case CheckOptions.MeshCompression:
                            CheckMeshCompression(assetPath, itemData, ref reportInfos);
                            break;
                            
                        case CheckOptions.WeldVertices:
                            CheckWeldVertices(assetPath, itemData, ref reportInfos);
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                DebugUtil.LogError("路径必须以 Assets 开头!");
            }
        }

        /// <summary>
        /// 检测: 读写设置
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckReadWriteEnable(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (ReferenceEquals(modelImporter, null) == false)
            {
                if (modelImporter.isReadable)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    var content = $"模型读写权限设置错误, 应关闭读写权限! 资源路径为: {assetPath}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXReadWriteEnable, content, item));
                }
            }
            else
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }

        /// <summary>
        /// 检测: MeshRenderer 组件的阴影投射
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckCastShadows(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null) == false)
            {
                var renderers = asset.GetComponentsInChildren<Renderer>(true);
                if (renderers != null && renderers.Length > 0)
                {
                    foreach (var renderer in renderers)
                    {
                        if (renderer.shadowCastingMode != ShadowCastingMode.Off)
                        {
                            var content = $"模型的阴影投射 (Cast Shadows) 未关闭! 资源路径为: {assetPath}";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXMeshRendererCastShadows, content, item));
                        }
                    }
                }
            }
            else
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }
        
        /// <summary>
        /// 检测: 模型法线导入
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckNormals(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            if (assetPath.IndexOf("collider", StringComparison.OrdinalIgnoreCase) >= 0 || assetPath.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var assetImporter = AssetImporter.GetAtPath(assetPath);
                if (assetImporter is ModelImporter modelImporter)
                {
                    if (modelImporter.importNormals != ModelImporterNormals.None)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        var content = $"模型未关闭 Normals! 资源路径为: {assetPath}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXNormals, content, item));
                    }
                }
            }
            else
            {
                DebugUtil.LogError($"资源命名中不含有 collider 以及 virtual, 请检查资源命名! {assetPath}");
            }
        }
        
        /// <summary>
        /// 检测: OptimizeMesh
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckOptimizeMesh(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var model = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (ReferenceEquals(model, null) == false)
            {
                var set = bool.Parse(item.parameter);
#pragma warning disable 618
                if (model.optimizeMesh != set)
#pragma warning restore 618
                {
#pragma warning disable 618
                    var content = $"OptimizeMesh 未开启: {assetPath} ({model.optimizeMesh}) >>> ({set})";
#pragma warning restore 618
                    report.Add(EffectCheckReport.AddReportInfo(model, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXOptimizeMesh, content, item));
                }
            }
        }
        
        /// <summary>
        /// 检测: MeshCompression
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckMeshCompression(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var model = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (ReferenceEquals(model, null) == false)
            {
                var set = (ModelImporterMeshCompression)int.Parse(item.parameter);
                if (model.meshCompression != set)
                {
                    var content = $"MeshCompression 设置错误: {assetPath} ({model.meshCompression}) >>> ({set})";
                    report.Add(EffectCheckReport.AddReportInfo(model, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXMeshCompression, content, item));
                }
            }
        }
        
        /// <summary>
        /// 检测: OptimizeMesh
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckWeldVertices(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var model = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (ReferenceEquals(model, null) == false)
            {
                var set = bool.Parse(item.parameter);
                if (model.weldVertices != set)
                {
                    var content = $"WeldVertices 未开启: {assetPath} ({model.weldVertices}) >>> ({set})";
                    report.Add(EffectCheckReport.AddReportInfo(model, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXWeldVertices, content, item));
                }
            }
        }
    }
}