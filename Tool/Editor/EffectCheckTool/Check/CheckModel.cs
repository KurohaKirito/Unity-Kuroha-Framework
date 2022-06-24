using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.ItemListView;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Report;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Check {
    public static class CheckModel {
        /// <summary>
        /// 模型资源检查类型
        /// </summary>
        public static readonly string[] checkOptions = {
            "读写权限设置", "法线导入设置", "网格优化", "网格压缩", "顶点焊接"
        };

        /// <summary>
        /// 模型资源检查类型
        /// </summary>
        public enum CheckOptions {
            ReadWriteEnable,
            Normals,
            OptimizeMesh,
            MeshCompression,
            WeldVertices,
        }

        /// <summary>
        /// 检查 MeshCompression 时的子检查项
        /// </summary>
        public static readonly string[] meshCompressionOptions = Enum.GetNames(typeof(ModelImporterMeshCompression));

        /// <summary>
        /// 对模型文件进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos) {
            if (itemData.path.StartsWith("Assets")) {
                var assetGuids = AssetDatabase.FindAssets("t:Model", new[] {
                    itemData.path
                });

                for (var index = 0; index < assetGuids.Length; index++) {
                    ProgressBar.DisplayProgressBar("特效检测工具", $"FBX 排查中: {index + 1}/{assetGuids.Length}", index + 1, assetGuids.Length);

                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[index]);
                    var pattern = itemData.assetWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false) {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(assetPath)) {
                            continue;
                        }
                    }

                    switch ((CheckOptions)itemData.checkType) {
                        case CheckOptions.ReadWriteEnable:
                            CheckReadWriteEnable(assetPath, itemData, ref reportInfos);
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
            } else {
                DebugUtil.LogError("路径必须以 Assets 开头!");
            }
        }

        /// <summary>
        /// 检测: 读写设置
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckReadWriteEnable(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (ReferenceEquals(modelImporter, null) == false) {
                if (modelImporter.isReadable) {
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    var content = $"模型的读写权限错误!\t路径: {assetPath}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXReadWriteEnable, content, item));
                }
            } else {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }

        /// <summary>
        /// 检测: 模型法线导入
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckNormals(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var regex = new System.Text.RegularExpressions.Regex("_[DE][\\d][\\d][\\d]");
            if (regex.IsMatch(assetPath)) {
                var assetImporter = AssetImporter.GetAtPath(assetPath);
                if (assetImporter is ModelImporter modelImporter) {
                    if (modelImporter.importNormals != ModelImporterNormals.None) {
                        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        var content = $"模型的法线设置错误!\t路径: {assetPath}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXNormals, content, item));
                    }
                }
            } else {
                DebugUtil.LogError($"资源命名中不含有 _D 以及 _E, 请检查资源命名! {assetPath}");
            }
        }

        /// <summary>
        /// 检测: OptimizeMesh
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckOptimizeMesh(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var model = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (ReferenceEquals(model, null) == false) {
                var set = bool.Parse(item.parameter);
                if (model.optimizeMesh != set)
                {
                    var content = $"模型的网格优化错误!\t路径: {assetPath} ({model.optimizeMesh}) >>> ({set})";
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
        private static void CheckMeshCompression(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var model = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (ReferenceEquals(model, null) == false) {
                // 翻译参数
                var parameter = ModelImporterMeshCompression.Off;
                switch (Convert.ToInt32(item.parameter)) {
                    case 0:
                        parameter = ModelImporterMeshCompression.Off;
                        break;
                    case 1:
                        parameter = ModelImporterMeshCompression.Low;
                        break;
                    case 2:
                        parameter = ModelImporterMeshCompression.Medium;
                        break;
                    case 3:
                        parameter = ModelImporterMeshCompression.High;
                        break;
                }
                if (model.meshCompression != parameter) {
                    var content = $"模型的网格压缩错误!\t路径: : {assetPath} ({model.meshCompression}) >>> ({parameter})";
                    report.Add(EffectCheckReport.AddReportInfo(model, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXMeshCompression, content, item));
                }
            }
        }

        /// <summary>
        /// 检测: WeldVertices
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckWeldVertices(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var model = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (ReferenceEquals(model, null) == false) {
                var set = bool.Parse(item.parameter);
                if (model.weldVertices != set) {
                    var content = $"模型的顶点焊接错误: {assetPath} ({model.weldVertices}) >>> ({set})";
                    report.Add(EffectCheckReport.AddReportInfo(model, assetPath, EffectCheckReportInfo.EffectCheckReportType.FBXWeldVertices, content, item));
                }
            }
        }
    }
}
