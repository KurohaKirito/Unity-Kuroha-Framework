using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kuroha.GUI.Editor;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemSetView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Check
{
    public static class CheckPrefab
    {
        /// <summary>
        /// 预制体资源检查类型
        /// </summary>
        public static readonly string[] checkOptions =
        {
            "命名",
            "禁止碰撞",
            "隐藏物体",
            "纹理大小"
        };

        /// <summary>
        /// 预制体资源检查类型
        /// </summary>
        public enum CheckOptions
        {
            ObjectName,
            ForbidCollision,
            DisableObject,
            TextureSize
        }

        /// <summary>
        /// 检查 TextureSize 时的子检查项
        /// </summary>
        public static readonly string[] textureSizeOptions =
        {
            "64",
            "128",
            "256",
            "512",
            "1024",
            "2048"
        };

        /// <summary>
        /// 对预制体进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos)
        {
            if (itemData.path.StartsWith("Assets"))
            {
                var assetGuids = AssetDatabase.FindAssets("t:Prefab", new[] { itemData.path });
                DebugUtil.Log($"CheckParticleSystem: 查询到了 {assetGuids.Length} 个预制体, 检测路径为: {itemData.path}");
                
                for (var index = 0; index < assetGuids.Length; index++)
                {
                    ProgressBar.DisplayProgressBar("特效检测工具", $"Prefab 排查中: {index + 1}/{assetGuids.Length}", index + 1, assetGuids.Length);
                    
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
                        case CheckOptions.ObjectName:
                            CheckObjectName(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.ForbidCollision:
                            CheckCollider(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.DisableObject:
                            CheckDisableObject(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.TextureSize:
                            CheckTextureSize(assetPath, itemData, ref reportInfos);
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
        /// 检测: 命名
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckObjectName(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var transforms = asset.GetComponentsInChildren<Transform>(true);
            var parameter = item.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
            var isCheckChinese = Convert.ToBoolean(parameter[0]);
            var isCheckSpace = Convert.ToBoolean(parameter[1]);

            foreach (var transform in transforms)
            {
                foreach (var cha in transform.name.ToCharArray())
                {
                    if (isCheckChinese && CharUtil.IsChinese(cha))
                    {
                        var content = $"预制体名称中不能有中文: {assetPath} 子物件: {transform.name}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.PrefabName, content, item));
                        break;
                    }

                    if (isCheckSpace && CharUtil.IsSpace(cha))
                    {
                        var content = $"预制体名称中不能有空格: {assetPath} 子物件: {transform.name}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.PrefabName, content, item));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 检测: 禁止碰撞体
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckCollider(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var transforms = asset.GetComponentsInChildren<Transform>(true);

            foreach (var transform in transforms)
            {
                var collider = transform.GetComponent<Collider>();
                var meshCollider = transform.GetComponent<MeshCollider>();

                if (collider == null && meshCollider == null)
                {
                    continue;
                }

                var content = $"预制体中不能有碰撞体: {assetPath} 中的子物体 {transform.name}";
                report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.PrefabForbidCollision, content, item));
            }
        }

        /// <summary>
        /// 检测: 隐藏物体
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckDisableObject(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var transforms = asset.GetComponentsInChildren<Transform>(true);

            foreach (var transform in transforms)
            {
                if (transform.gameObject.activeSelf == false)
                {
                    var content = $"预制体中有 Disable 的物体: {assetPath} 中的子物体 {transform.name}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.PrefabDisableObject, content, item));
                }
            }
        }

        /// <summary>
        /// 检测: 贴图大小
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckTextureSize(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var transforms = asset.GetComponentsInChildren<Transform>(true);

            // 获取最大尺寸, 用于比较
            var parameter = item.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
            var width = Convert.ToInt32(textureSizeOptions[Convert.ToInt32(parameter[0])]);
            var height = Convert.ToInt32(textureSizeOptions[Convert.ToInt32(parameter[1])]);

            foreach (var transform in transforms)
            {
                var renderer = transform.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterials != null)
                {
                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material != null)
                        {
                            var textureNames = material.GetTexturePropertyNames();
                            foreach (var textureName in textureNames)
                            {
                                var texture = material.GetTexture(textureName);
                                if (texture != null)
                                {
                                    if (texture.width > width || texture.height > height)
                                    {
                                        var content = $"纹理尺寸超出限制: {texture.width}X{texture.height}\t物体: {assetPath} 中的子物体 {transform.name}";
                                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.PrefabTextureSize, content, item));
                                    }
                                }
                            }
                        }
                        else
                        {
                            var content = $"预制体 {assetPath} 中的子物体 {transform.name} 上引用的 Material 为空!";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.PrefabTextureSize, content, item));
                        }
                    }
                }
            }
        }
    }
}