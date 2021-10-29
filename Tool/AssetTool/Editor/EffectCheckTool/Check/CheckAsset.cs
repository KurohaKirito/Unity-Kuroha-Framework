﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Editor;
using Kuroha.Util.RunTime;
using UnityEditor;

namespace Kuroha.Tool.Editor.EffectCheckTool.Check
{
    public static class CheckAsset
    {
        /// <summary>
        /// 资源通用检查类型-文本
        /// </summary>
        public static readonly string[] checkOptions =
        {
            "资源命名",
            "文件夹命名"
        };

        /// <summary>
        /// 资源通用检查类型
        /// </summary>
        public enum CheckOptions
        {
            AssetName,
            FolderName
        }
        
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos)
        {
            if (itemData.path.StartsWith("Assets"))
            {
                var fullPath = Path.GetFullPath(itemData.path);
                if (Directory.Exists(fullPath))
                {
                    var direction = new DirectoryInfo(fullPath);
                    var searchType = itemData.isCheckSubFile
                        ? SearchOption.AllDirectories
                        : SearchOption.TopDirectoryOnly;
                    
                    switch ((CheckOptions)itemData.checkType)
                    {
                        case CheckOptions.AssetName:
                        {
                            var files = direction.GetFiles("*", searchType);
                            for (var index = 0; index < files.Length; index++)
                            {
                                ProgressBar.DisplayProgressBar("特效检测工具", $"资源命名规则排查中: {index + 1}/{files.Length}", index + 1, files.Length);
                                if (files[index].Name.EndsWith(".meta") == false)
                                {
                                    var assetPath = PathUtil.GetAssetPath(files[index].FullName);
                                    var pattern = itemData.writePathRegex;
                                    if (string.IsNullOrEmpty(pattern) == false)
                                    {
                                        var regex = new Regex(pattern);
                                        if (regex.IsMatch(assetPath))
                                        {
                                            continue;
                                        }
                                    }

                                    CheckAssetName(assetPath, itemData, ref reportInfos);
                                }
                            }
                        }
                            break;

                        case CheckOptions.FolderName:
                        {
                            var folders = direction.GetDirectories("*", searchType);
                            for (var index = 0; index < folders.Length; index++)
                            {
                                ProgressBar.DisplayProgressBar("特效检测工具", $"目录命名规则排查中: {index + 1}/{folders.Length}", index + 1, folders.Length);
                                var assetPath = PathUtil.GetAssetPath(folders[index].FullName);
                                var pattern = itemData.writePathRegex;
                                if (string.IsNullOrEmpty(pattern) == false)
                                {
                                    var regex = new Regex(pattern);
                                    if (regex.IsMatch(assetPath))
                                    {
                                        continue;
                                    }
                                }

                                CheckFolderName(assetPath, itemData, ref reportInfos);
                            }
                        }
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
        /// 检测: 资源命名
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckAssetName(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            // 文件名
            assetPath = assetPath.Replace('\\', '/');
            var assetName = assetPath.Split('/').Last();
            assetName = assetName.Substring(0, assetName.Length - 4);
            
            // 正则
            var pattern = item.parameter;
            var regex = new Regex(pattern);
            
            if (regex.IsMatch(assetName) == false)
            {
                var fullName = System.IO.Path.GetFullPath(assetPath);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                
                var content = $"资源命名错误! 资源路径: {fullName}";
                report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.AssetName, content, item));
            }
        }
        
        /// <summary>
        /// 检测: 文件夹命名
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckFolderName(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            // 文件名
            assetPath = assetPath.Replace('\\', '/');
            var assetName = assetPath.Split('/').Last();
            
            // 正则
            var pattern = item.parameter;
            var regex = new Regex(pattern);
            
            if (regex.IsMatch(assetName) == false)
            {
                var fullName = System.IO.Path.GetFullPath(assetPath);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                
                var content = $"文件夹命名错误! 文件夹路径: {fullName}";
                report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.FolderName, content, item));
            }
        }
    }
}