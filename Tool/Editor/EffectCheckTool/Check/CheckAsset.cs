using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Editor;
using Kuroha.Util.Release;
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
            "资源命名"
        };

        /// <summary>
        /// 资源通用检查类型
        /// </summary>
        public enum CheckOptions
        {
            AssetName
        }
        
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos)
        {
            if (itemData.path.StartsWith("Assets"))
            {
                var fullPath = Path.GetFullPath(itemData.path);
                var direction = new DirectoryInfo(fullPath);
                var files = direction.GetFiles("*", SearchOption.AllDirectories);
                for (var index = 0; index < files.Length; index++)
                {
                    ProgressBar.DisplayProgressBar("资源规则检查", $"排查中: {index + 1}/{files.Length}", index + 1, files.Length);
                    if (files[index].Name.EndsWith(".meta"))
                    {
                        continue;
                    }

                    var assetPath = PathUtil.GetAssetPath(files[index].FullName);

                    switch ((CheckOptions)itemData.checkType)
                    {
                        case CheckOptions.AssetName:
                            CheckAssetName(assetPath, itemData, ref reportInfos);
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
        /// 检测: 剔除模式资源命名
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckAssetName(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            // 文件名
            assetPath = assetPath.Replace('\\', '/');
            var assetName = assetPath.Split('/').GetLast();
            
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
    }
}