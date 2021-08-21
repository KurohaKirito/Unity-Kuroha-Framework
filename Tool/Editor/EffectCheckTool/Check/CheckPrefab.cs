using System;
using System.Collections.Generic;
using System.IO;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.Editor.EffectCheckTool.ItemSetView;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.EffectCheckTool.Check
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
            var fullPath = $"{Application.dataPath}/{itemData.path}";
            var direction = new DirectoryInfo(fullPath);
            var files = direction.GetFiles("*", SearchOption.AllDirectories);

            for (var index = 0; index < files.Length; index++)
            {
                ProgressBar.DisplayProgressBar("Prefab 资源排查", "排查中", index + 1, files.Length);
                if (!files[index].Name.EndsWith(".prefab"))
                {
                    continue;
                }

                var assetPath = files[index].FullName
                    .Substring(files[index].FullName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase));

                switch ((CheckOptions)itemData.checkType)
                {
                    case CheckOptions.ObjectName:
                        CheckObjectName(assetPath, files[index], itemData, ref reportInfos);
                        break;

                    case CheckOptions.ForbidCollision:
                        CheckForbidCollision(assetPath, files[index], itemData, ref reportInfos);
                        break;

                    case CheckOptions.DisableObject:
                        CheckDisableObject(assetPath, files[index], itemData, ref reportInfos);
                        break;

                    case CheckOptions.TextureSize:
                        CheckTextureSize(assetPath, files[index], itemData, ref reportInfos);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 检测: 命名
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckObjectName(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
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
                        var content = $"预制体名称中不能有中文: {assetInfo.FullName} 子物件: {transform.name}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath,
                            EffectCheckReportInfo.EffectCheckReportType.PrefabName, content, item));
                    }

                    if (isCheckSpace && CharUtil.IsSpace(cha))
                    {
                        var content = $"预制体名称中不能有空格: {assetInfo.FullName} 子物件: {transform.name}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath,
                            EffectCheckReportInfo.EffectCheckReportType.PrefabName, content, item));
                    }
                }
            }
        }

        /// <summary>
        /// 检测: 禁止碰撞体
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckForbidCollision(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
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

                var content = $"预制体中不能有碰撞体: {assetInfo.FullName} 中的子物体 {transform.name}";
                report.Add(EffectCheckReport.AddReportInfo(transform.gameObject, assetPath,
                    EffectCheckReportInfo.EffectCheckReportType.PrefabForbidCollision, content, item));
            }
        }

        /// <summary>
        /// 检测: 隐藏物体
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckDisableObject(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
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
                if (transform.gameObject.activeSelf)
                {
                    continue;
                }

                var content = $"预制体中有 Disable 物体: {assetInfo.FullName} 中的子物体 {transform.name}";
                report.Add(EffectCheckReport.AddReportInfo(transform.gameObject, assetPath,
                    EffectCheckReportInfo.EffectCheckReportType.PrefabDisableObject, content, item));
            }
        }

        /// <summary>
        /// 检测: 贴图大小
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckTextureSize(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var transforms = asset.GetComponentsInChildren<Transform>(true);

            var parameter = item.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
            var width = Convert.ToInt32(textureSizeOptions[Convert.ToInt32(parameter[0])]);
            var height = Convert.ToInt32(textureSizeOptions[Convert.ToInt32(parameter[1])]);

            foreach (var transform in transforms)
            {
                var meshRenderer = transform.GetComponent<MeshRenderer>();
                if (meshRenderer == null || meshRenderer.sharedMaterials == null)
                {
                    continue;
                }

                foreach (var material in meshRenderer.sharedMaterials)
                {
                    if (material.mainTexture == null)
                    {
                        continue;
                    }

                    if (material.mainTexture.width <= width && material.mainTexture.height <= height)
                    {
                        continue;
                    }

                    var content =
                        $"纹理尺寸为: {material.mainTexture.width}X{material.mainTexture.height}\t物体: {assetInfo.FullName} 中的子物体 {transform.name}";
                    report.Add(EffectCheckReport.AddReportInfo(transform.gameObject, assetPath,
                        EffectCheckReportInfo.EffectCheckReportType.PrefabTextureSize, content, item));
                }
            }
        }
    }
}