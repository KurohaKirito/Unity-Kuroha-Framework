using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kuroha.GUI.Editor;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemSetView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Editor;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

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
            "纹理大小",
            "运动向量",
            "动态遮挡剔除",
            "禁用粒子特效",
            "阴影投射",
            "光照探针",
            "反射探针",
            "动画状态机剔除模式",
            "LOD渲染层级设置",
        };

        /// <summary>
        /// 预制体资源检查类型
        /// </summary>
        public enum CheckOptions
        {
            /// <summary>
            /// 命名
            /// </summary>
            ObjectName,

            /// <summary>
            /// 禁用碰撞体
            /// </summary>
            ForbidCollision,

            /// <summary>
            /// 隐藏物体
            /// </summary>
            DisableObject,

            /// <summary>
            /// 纹理大小
            /// </summary>
            TextureSize,

            /// <summary>
            /// 运动向量
            /// </summary>
            MotionVectors,

            /// <summary>
            /// 动态遮挡剔除
            /// </summary>
            DynamicOcclusion,

            /// <summary>
            /// 禁用粒子特效
            /// </summary>
            ForbidParticleSystem,

            /// <summary>
            /// 阴影投射
            /// </summary>
            CastShadows,

            /// <summary>
            /// 光照探针
            /// </summary>
            LightProbes,

            /// <summary>
            /// 反射探针
            /// </summary>
            ReflectionProbes,

            /// <summary>
            /// 动画状态机
            /// </summary>
            AnimatorCullMode,
            
            /// <summary>
            /// LOD Renderer 设置
            /// </summary>
            LODGroupRenderers,
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
        /// 检测 CastShadows 时的子检查项
        /// </summary>
        public static readonly string[] castShadowsOptions = Enum.GetNames(typeof(ShadowCastingMode));

        /// <summary>
        /// 检测 LightProbes 时的子检查项
        /// </summary>
        public static readonly string[] lightProbesOptions = Enum.GetNames(typeof(LightProbeUsage));

        /// <summary>
        /// 检测 ReflectionProbes 时的子检查项
        /// </summary>
        public static readonly string[] reflectionProbesOptions = Enum.GetNames(typeof(ReflectionProbeUsage));

        /// <summary>
        /// 检查 AnimatorCullMode 时的子检查项
        /// </summary>
        public static readonly string[] animatorCullModeOptions = Enum.GetNames(typeof(AnimatorCullingMode));

        /// <summary>
        /// 对预制体进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos)
        {
            if (itemData.path.StartsWith("Assets"))
            {
                var assetGuids = AssetDatabase.FindAssets("t:Prefab", new[]
                {
                    itemData.path
                });

                for (var index = 0; index < assetGuids.Length; index++)
                {
                    ProgressBar.DisplayProgressBar("特效检测工具", $"Prefab 排查中: {index + 1}/{assetGuids.Length}", index + 1, assetGuids.Length);

                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[index]);
                    var pattern = itemData.assetWhiteRegex;
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
                            CheckForbidCollider(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.DisableObject:
                            CheckDisableObject(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.TextureSize:
                            CheckTextureSize(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.MotionVectors:
                            CheckMotionVectors(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.DynamicOcclusion:
                            CheckDynamicOcclusion(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.ForbidParticleSystem:
                            CheckForbidParticleSystem(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.CastShadows:
                            CheckCastShadows(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.LightProbes:
                            CheckLightProbes(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.ReflectionProbes:
                            CheckReflectionProbes(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.AnimatorCullMode:
                            CheckAnimatorCullMode(assetPath, itemData, ref reportInfos);
                            break;
                        
                        case CheckOptions.LODGroupRenderers:
                            CheckLODGroupRenderers(assetPath, itemData, ref reportInfos);
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
                        var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                        var content = $"预制体名不能有中文!\t预制体: {assetPath} 子物件: {childPath}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabName, content, item));
                        break;
                    }

                    if (isCheckSpace && CharUtil.IsSpace(cha))
                    {
                        var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                        var content = $"预制体名不能有空格!\t预制体: {assetPath} 子物件: {childPath}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabName, content, item));
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
        private static void CheckForbidCollider(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
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
                // 物体正则白名单
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                if (transform.TryGetComponent<Collider>(out _))
                {
                    var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                    var content = $"预制体不能有碰撞体!\t预制体: {assetPath} 中的子物体 {childPath}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabForbidCollider, content, item));
                }
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
                // 物体正则白名单
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                if (transform.gameObject.activeSelf == false)
                {
                    var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                    var content = $"预制体中有隐藏物体!\t预制体: {assetPath} 中的子物体 {childPath}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabDisableObject, content, item));
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
                // 物体正则白名单
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                if (transform.TryGetComponent<Renderer>(out var renderer))
                {
                    if (renderer.sharedMaterials != null)
                    {
                        foreach (var material in renderer.sharedMaterials)
                        {
                            if (ReferenceEquals(material, null) == false)
                            {
                                TextureUtil.GetTexturesInMaterial(material, out var textureDataList);
                                foreach (var textureData in textureDataList)
                                {
                                    var textureWidth = textureData.asset.width;
                                    var textureHeight = textureData.asset.height;
                                    if (textureWidth > width || textureHeight > height)
                                    {
                                        var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                                        var content = $"纹理的尺寸超出限制!\t预制体: {assetPath} 中的子物体 {childPath}, {textureWidth}X{textureHeight} => {width}X{height}";
                                        report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabTextureSize, content, item));
                                    }
                                }
                            }
                            else
                            {
                                var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                                var content = $"渲染器引用材质为空!\t预制体: {assetPath} 中的子物体 {childPath} 上引用的 Material 为空!";
                                report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabTextureSize, content, item));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检测: 运动向量
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckMotionVectors(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到预制体资源, 路径为: {assetPath}");
                return;
            }

            // 得到预制上全部的游戏物体
            var transforms = asset.GetComponentsInChildren<Transform>(true);

            // 获取检测参数
            var isOpen = Convert.ToBoolean(item.parameter);

            // 遍历检测
            foreach (var transform in transforms)
            {
                // 物体正则白名单
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                if (transform.TryGetComponent<SkinnedMeshRenderer>(out var renderer))
                {
                    if (ReferenceEquals(renderer, null) == false)
                    {
                        if (renderer.skinnedMotionVectors != isOpen)
                        {
                            var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                            var content = $"运动向量设置不规范!\t预制体: {assetPath} 子物体: {childPath} : ({!isOpen}) => ({isOpen})!";
                            report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabMotionVectors, content, item));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检测: 动态遮挡剔除
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckDynamicOcclusion(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到预制体资源, 路径为: {assetPath}");
                return;
            }

            // 得到预制上全部的游戏物体
            var transforms = asset.GetComponentsInChildren<Transform>(true);

            // 获取检测参数
            var isOpen = Convert.ToBoolean(item.parameter);

            // 遍历检测
            foreach (var transform in transforms)
            {
                // 物体正则白名单
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                if (transform.TryGetComponent<Renderer>(out var renderer))
                {
                    if (renderer.allowOcclusionWhenDynamic != isOpen)
                    {
                        var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                        var content = $"动态遮挡剔除不规范!\t预制体: {assetPath} 子物体: {childPath} : ({!isOpen}) => ({isOpen})!";
                        report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabDynamicOcclusion, content, item));
                    }
                }
            }
        }

        /// <summary>
        /// 检测: 禁用粒子特效
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckForbidParticleSystem(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到预制体资源, 路径为: {assetPath}");
                return;
            }

            // 得到预制上全部的游戏物体
            var transforms = asset.GetComponentsInChildren<Transform>(true);

            // 获取检测参数
            var isForbid = Convert.ToBoolean(item.parameter);

            // 遍历检测
            foreach (var transform in transforms)
            {
                // 物体正则白名单
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                if (transform.TryGetComponent<ParticleSystem>(out _))
                {
                    var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                    var content = isForbid
                        ? $"预制体中不能有粒子系统: {assetPath} 中的子物体 {childPath}"
                        : $"预制体中缺少必要的粒子系统: {assetPath} 中的子物体 {childPath}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabForbidParticleSystem, content, item));
                }
            }
        }

        /// <summary>
        /// 检测: 阴影投射
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckCastShadows(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到预制体资源, 路径为: {assetPath}");
                return;
            }

            // 得到预制上全部的游戏物体
            var transforms = asset.GetComponentsInChildren<Transform>(true);

            // 获取检测参数
            var parameter = Convert.ToInt32(item.parameter) switch
            {
                0 => ShadowCastingMode.Off,
                1 => ShadowCastingMode.On,
                2 => ShadowCastingMode.TwoSided,
                3 => ShadowCastingMode.ShadowsOnly,
                _ => ShadowCastingMode.Off
            };

            // 遍历检测
            foreach (var transform in transforms)
            {
                // 物体正则白名单
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                if (transform.TryGetComponent<Renderer>(out var renderer))
                {
                    if (ReferenceEquals(renderer, null) == false)
                    {
                        if (renderer.shadowCastingMode != parameter)
                        {
                            var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                            var content = $"预制体阴影投射错误!\t预制体: {assetPath} 子物体 : {childPath}: ({renderer.shadowCastingMode}) => ({parameter})";
                            report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabCastShadows, content, item));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检测: 光照探针
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckLightProbes(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到预制体资源, 路径为: {assetPath}");
                return;
            }

            // 得到预制上全部的游戏物体
            var transforms = asset.GetComponentsInChildren<Transform>(true);

            // 获取检测参数
            var parameter = Convert.ToInt32(item.parameter) switch
            {
                0 => LightProbeUsage.Off,
                1 => LightProbeUsage.BlendProbes,
                2 => LightProbeUsage.UseProxyVolume,
                3 => LightProbeUsage.CustomProvided,
                _ => LightProbeUsage.Off
            };

            // 遍历检测
            foreach (var transform in transforms)
            {
                // 物体正则白名单
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                if (transform.TryGetComponent<Renderer>(out var renderer))
                {
                    if (renderer.lightProbeUsage != parameter)
                    {
                        var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                        var content = $"预制体光照探针错误!\t预制体: {assetPath} 子物体: {childPath}: ({renderer.lightProbeUsage}) => ({parameter})";
                        report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabLightProbes, content, item));
                    }
                }
            }
        }

        /// <summary>
        /// 检测: 反射探针
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckReflectionProbes(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到预制体资源, 路径为: {assetPath}");
                return;
            }

            // 得到预制上全部的游戏物体
            var transforms = asset.GetComponentsInChildren<Transform>(true);

            // 获取检测参数
            var parameter = Convert.ToInt32(item.parameter) switch
            {
                0 => ReflectionProbeUsage.Off,
                1 => ReflectionProbeUsage.BlendProbes,
                2 => ReflectionProbeUsage.BlendProbesAndSkybox,
                3 => ReflectionProbeUsage.Simple,
                _ => ReflectionProbeUsage.Off
            };

            // 遍历检测
            foreach (var transform in transforms)
            {
                // 物体正则白名单
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                if (transform.TryGetComponent<Renderer>(out var renderer))
                {
                    if (renderer.reflectionProbeUsage != parameter)
                    {
                        var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                        var content = $"预制体反射探针错误!\t预制体: {assetPath} 子物体: {childPath}: ({renderer.reflectionProbeUsage}) => ({parameter})";
                        report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabReflectionProbes, content, item));
                    }
                }
            }
        }

        /// <summary>
        /// 检测: 动画状态机剔除模式
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckAnimatorCullMode(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到预制体资源, 路径为: {assetPath}");
                return;
            }

            // 得到预制上全部的游戏物体
            var transforms = asset.GetComponentsInChildren<Transform>(true);

            // 获取检测参数
            var parameter = Convert.ToInt32(item.parameter) switch
            {
                0 => AnimatorCullingMode.AlwaysAnimate,
                1 => AnimatorCullingMode.CullUpdateTransforms,
                2 => AnimatorCullingMode.CullCompletely,
                _ => AnimatorCullingMode.CullCompletely
            };

            // 遍历检测
            foreach (var transform in transforms)
            {
                // 正则
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                // 检测
                if (transform.TryGetComponent<Animator>(out var animator))
                {
                    if (animator.cullingMode != parameter)
                    {
                        var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                        var content = $"动画状态机剔除错误!\t预制体: {assetPath} 子物体: {childPath}: ({animator.cullingMode}) => ({parameter})";
                        report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabReflectionProbes, content, item));
                    }
                }
            }
        }
        
        /// <summary>
        /// 检测: LOD渲染层级设置
        /// </summary>
        private static void CheckLODGroupRenderers(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到预制体资源, 路径为: {assetPath}");
                return;
            }

            // 得到预制上全部的游戏物体
            var transforms = asset.GetComponentsInChildren<Transform>(true);

            // 遍历检测
            foreach (var transform in transforms)
            {
                // 正则
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false)
                {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(transform.gameObject.name))
                    {
                        continue;
                    }
                }

                // 检测
                if (transform.TryGetComponent<LODGroup>(out var lodGroup))
                {
                    var lods = lodGroup.GetLODs();
                    
                    // LODs 的层级数不包含 Cull 层, 例如: LOD0 + Cull 的层数为: 1
                    if (lods[0].renderers == null || lods[0].renderers.Length <= 0)
                    {
                        DebugUtil.Log($"{transform.name} 物体的 LOD 层数为: {lods.Length}", null, "red");
                        var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                        if (childPath.IsNullOrEmpty())
                        {
                            var content = $"LODGroups设置错误!\t预制体: {assetPath}";
                            report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabLODGroupRenderers, content, item));
                        }
                        else
                        {
                            var content = $"LODGroups设置错误!\t预制体: {assetPath} 子物体: {childPath}";
                            report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabLODGroupRenderers, content, item));
                        }
                    }
                    else
                    {
                        // 检查 LOD0
                        var isError = false;
                        foreach (var renderer in lods[0].renderers)
                        {
                            if (renderer == null)
                            {
                                isError = true;
                            }
                        }

                        if (isError)
                        {
                            DebugUtil.Log($"{transform.name} 物体的 LOD 层数为: {lods.Length}", null, "red");
                            var childPath = PrefabUtil.GetHierarchyPath(transform, false);
                            if (childPath.IsNullOrEmpty())
                            {
                                var content = $"LOD0存在空物体!\t预制体: {assetPath}";
                                report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabLODGroupRenderers, content, item));
                            }
                            else
                            {
                                var content = $"LOD0存在空物体!\t预制体: {assetPath} 子物体: {childPath}";
                                report.Add(EffectCheckReport.AddReportInfo(asset, childPath, EffectCheckReportInfo.EffectCheckReportType.PrefabLODGroupRenderers, content, item));
                            }
                        }
                    }
                }
            }
        }
    }
}
