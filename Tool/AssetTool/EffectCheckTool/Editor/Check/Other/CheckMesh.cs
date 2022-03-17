using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Kuroha.Framework.GUI.Editor;
using Kuroha.Framework.Utility.RunTime;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.ItemListView;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.Report;
using Kuroha.Util.Editor;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.EffectCheckTool.Editor.Check.Other
{
    public static class CheckMesh
    {
        /// <summary>
        /// 网格资源检查类型
        /// </summary>
        public static readonly string[] checkOptions =
        {
            "网格 UV 信息",
        };

        /// <summary>
        /// 网格资源检查类型
        /// </summary>
        public enum CheckOptions
        {
            MeshUV,
        }

        /// <summary>
        /// 对网格资源进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos)
        {
            if (itemData.checkPath.StartsWith("Assets"))
            {
                var fullPath = System.IO.Path.GetFullPath(itemData.checkPath);
                if (Directory.Exists(fullPath))
                {
                    var direction = new DirectoryInfo(fullPath);
                    var searchType = itemData.isCheckSubFile
                        ? SearchOption.AllDirectories
                        : SearchOption.TopDirectoryOnly;
                    var files = direction.GetFiles("*", searchType);
                    for (var index = 0; index < files.Length; index++)
                    { 
                        ProgressBar.DisplayProgressBar("特效检测工具", $"Mesh 排查中: {index + 1}/{files.Length}", index + 1, files.Length);
                        if (files[index].Name.EndsWith(".meta"))
                        {
                            continue;
                            
                        }
    
                        var assetPath = PathUtil.GetAssetPath(files[index].FullName);
                        var pattern = itemData.assetWhiteRegex;
                        if (string.IsNullOrEmpty(pattern) == false)
                        { 
                            var regex = new Regex(pattern);
                            if (regex.IsMatch(assetPath))
                            {
                                continue;
                            }
                        }
                        
                        switch ((CheckOptions)itemData.checkOption)
                        {
                            case CheckOptions.MeshUV:
                                CheckSkinnedMeshRenderer(assetPath, files[index], itemData, ref reportInfos);
                                CheckMeshFilter(assetPath, files[index], itemData, ref reportInfos);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            else
            {
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
        private static void CheckSkinnedMeshRenderer(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset != null)
            {
                var skinnedMeshes = asset.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var skinnedMesh in skinnedMeshes)
                {
                    // 物体正则白名单
                    var pattern = item.objectWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false)
                    {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(skinnedMesh.gameObject.name))
                        {
                            continue;
                        }
                    }
                    
                    var mesh = skinnedMesh.sharedMesh;
                    var message = string.Empty;
                    var isError = false;

                    if (mesh.uv2.Length > 0)
                    {
                        isError = true;
                        message += $"uv2 : {mesh.uv2.Length}";
                    }
                    if (mesh.uv3.Length > 0)
                    {
                        isError = true;
                        message += $"uv3 : {mesh.uv3.Length}";
                    }
                    if (mesh.uv4.Length > 0)
                    {
                        isError = true;
                        message += $"uv4 : {mesh.uv4.Length}";
                    }
                    if (mesh.colors.Length > 0)
                    {
                        isError = true;
                        message += $"colors : {mesh.colors.Length}";
                    }

                    if (isError)
                    {
                        var content = $"网格的顶点属性错误!\t物体: {assetInfo.FullName} 子物体: {skinnedMesh.gameObject.name} 引用的 {mesh.name} 网格: {message} >>> 去除!";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.MeshUV, content, item));
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
        private static void CheckMeshFilter(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset != null)
            {
                var meshFilters = asset.GetComponentsInChildren<MeshFilter>(true);
                foreach (var meshFilter in meshFilters)
                {
                    // 物体正则白名单
                    var pattern = item.objectWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false)
                    {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(meshFilter.gameObject.name))
                        {
                            continue;
                        }
                    }
                    
                    var mesh = meshFilter.sharedMesh;
                    var message = string.Empty;
                    var isError = false;

                    if (mesh.uv2.Length > 0)
                    {
                        isError = true;
                        message += $"uv2 : {mesh.uv2.Length}";
                    }
                    if (mesh.uv3.Length > 0)
                    {
                        isError = true;
                        message += $"uv3 : {mesh.uv3.Length}";
                    }
                    if (mesh.uv4.Length > 0)
                    {
                        isError = true;
                        message += $"uv4 : {mesh.uv4.Length}";
                    }
                    if (mesh.colors.Length > 0)
                    {
                        isError = true;
                        message += $"colors : {mesh.colors.Length}";
                    }

                    if (isError)
                    {
                        var content = $"网格的顶点属性错误!\t物体: {assetInfo.FullName} 子物体: {meshFilter.gameObject.name} 引用的 {mesh.name} 网格: {message} >>> 去除!";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.MeshUV, content, item));
                    }
                }
            }
        }
    }
}