using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.ItemListView;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Report;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Check {
    public static class CheckMesh {
        /// <summary>
        /// 网格资源检查类型
        /// </summary>
        public static readonly string[] checkOptions = {
            "网格 UV 信息",
        };

        /// <summary>
        /// 网格资源检查类型
        /// </summary>
        public enum CheckOptions {
            MeshUV,
        }

        /// <summary>
        /// 检查 MeshCompression 时的子检查项
        /// </summary>
        public static readonly string[] meshCompressionOptions = {
            "Off", "Low", "Medium", "High"
        };

        /// <summary>
        /// 对网格资源进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos) {
            if (itemData.path.StartsWith("Assets")) {
                var fullPath = System.IO.Path.GetFullPath(itemData.path);
                if (Directory.Exists(fullPath)) {
                    var direction = new DirectoryInfo(fullPath);
                    var searchType = itemData.isCheckSubFile? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var files = direction.GetFiles("*", searchType);
                    for (var index = 0; index < files.Length; index++) {
                        ProgressBar.DisplayProgressBar("特效检测工具", $"Mesh 排查中: {index + 1}/{files.Length}", index + 1, files.Length);
                        if (files[index].Name.EndsWith(".meta")) {
                            continue;
                        }

                        var assetPath = PathUtil.GetAssetPath(files[index].FullName);
                        var pattern = itemData.writePathRegex;
                        if (string.IsNullOrEmpty(pattern) == false) {
                            var regex = new Regex(pattern);
                            if (regex.IsMatch(assetPath)) {
                                continue;
                            }
                        }

                        switch ((CheckOptions)itemData.checkType) {
                            case CheckOptions.MeshUV:
                                CheckSkinnedMeshRenderer(assetPath, files[index], itemData, ref reportInfos);
                                CheckMeshFilter(assetPath, files[index], itemData, ref reportInfos);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            } else {
                DebugUtil.LogError("检测路径必须以 Assets 开头!");
            }
        }

        /// <summary>
        /// 检测: SkinnedMeshRenderer
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源文件</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckSkinnedMeshRenderer(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null) == false) {
                var skinnedMeshes = asset.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                if (ReferenceEquals(skinnedMeshes, null) == false) {
                    foreach (var skinnedMesh in skinnedMeshes) {
                        var mesh = skinnedMesh.sharedMesh;
                        var message = string.Empty;
                        var isError = false;

                        if (mesh.uv2.Length > 0) {
                            isError = true;
                            message += $"uv2 : {mesh.uv2.Length}";
                        }

                        if (mesh.uv3.Length > 0) {
                            isError = true;
                            message += $"uv3 : {mesh.uv3.Length}";
                        }

                        if (mesh.uv4.Length > 0) {
                            isError = true;
                            message += $"uv4 : {mesh.uv4.Length}";
                        }

                        if (mesh.colors.Length > 0) {
                            isError = true;
                            message += $"colors : {mesh.colors.Length}";
                        }

                        if (isError) {
                            var content = $"SkinnedMeshRenderer: mesh 顶点属性错误: {assetInfo.FullName} 子物件: {skinnedMesh.gameObject.name} 引用的 {mesh.name} 网格: {message} >>> 去除!";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.MeshUV, content, item));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检测: MeshFilter
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源文件</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckMeshFilter(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null) == false) {
                var meshFilters = asset.GetComponentsInChildren<MeshFilter>(true);
                if (ReferenceEquals(meshFilters, null) == false) {
                    foreach (var meshFilter in meshFilters) {
                        var mesh = meshFilter.sharedMesh;
                        var message = string.Empty;
                        var isError = false;

                        if (mesh.uv2.Length > 0) {
                            isError = true;
                            message += $"uv2 : {mesh.uv2.Length}";
                        }

                        if (mesh.uv3.Length > 0) {
                            isError = true;
                            message += $"uv3 : {mesh.uv3.Length}";
                        }

                        if (mesh.uv4.Length > 0) {
                            isError = true;
                            message += $"uv4 : {mesh.uv4.Length}";
                        }

                        if (mesh.colors.Length > 0) {
                            isError = true;
                            message += $"colors : {mesh.colors.Length}";
                        }

                        if (isError) {
                            var content = $"MeshFilter: mesh 顶点属性错误: {assetInfo.FullName} 子物件: {meshFilter.gameObject.name} 引用的 {mesh.name} 网格: {message} >>> 去除!";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.MeshUV, content, item));
                        }
                    }
                }
            }
        }
    }
}