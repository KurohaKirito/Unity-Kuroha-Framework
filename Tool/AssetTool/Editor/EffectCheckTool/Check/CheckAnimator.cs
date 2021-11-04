using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Editor;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.EffectCheckTool.Check
{
    public static class CheckAnimator
    {
        /// <summary>
        /// 动画状态机检查类型
        /// </summary>
        public static readonly string[] checkOptions =
        {
            "状态机剔除模式"
        };

        /// <summary>
        /// 动画状态机检查类型
        /// </summary>
        public enum CheckOptions
        {
            CullMode
        }

        /// <summary>
        /// 检查 CullMode 时的子检查项
        /// </summary>
        public static readonly string[] cullModeOptions =
        {
            "Always Animate",
            "Cull Update Transforms",
            "Cull Completely"
        };

        /// <summary>
        /// 对动画状态机文件进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
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
                    var files = direction.GetFiles("*", searchType);
                    for (var index = 0; index < files.Length; index++)
                    {
                        ProgressBar.DisplayProgressBar("特效检测工具", $"Animator 排查中: {index + 1}/{files.Length}", index + 1, files.Length);
                        if (files[index].Name.EndsWith(".meta"))
                        {
                            continue;
                        }

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

                        switch ((CheckOptions)itemData.checkType)
                        {
                            case CheckOptions.CullMode:
                                CheckCullingMode(assetPath, itemData, ref reportInfos);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            else
            {
                DebugUtil.LogError("路径必须以 Assets 开头!");
            }
        }

        /// <summary>
        /// 检测: 剔除模式
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckCullingMode(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var fullName = System.IO.Path.GetFullPath(assetPath);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            
            if (ReferenceEquals(asset, null) == false)
            {
                var animators = asset.GetComponentsInChildren<Animator>(true);
                var cullingMode = (AnimatorCullingMode) Convert.ToInt32(item.parameter);

                foreach (var animator in animators)
                {
                    if (animator.cullingMode != cullingMode)
                    {
                        var gameObject = animator.gameObject;
                        var content = $"状态机剔除模式错误: {fullName} 子物件: {gameObject.name}, 当前动画剔除模式: {animator.cullingMode} >>> {cullingMode}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.AnimatorCullMode, content, item));
                    }
                }
            }
            else
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }
    }
}