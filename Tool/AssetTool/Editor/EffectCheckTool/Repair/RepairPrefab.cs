using System;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Repair
{
    public static class RepairPrefab
    {
        /// <summary>
        /// 自动修复问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">问题项</param>
        public static void Repair(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modeType = (CheckPrefab.CheckOptions)effectCheckReportInfo.modeType;

            switch (modeType)
            {
                case CheckPrefab.CheckOptions.ObjectName:
                    break;

                case CheckPrefab.CheckOptions.ForbidCollision:
                    break;

                case CheckPrefab.CheckOptions.DisableObject:
                    break;

                case CheckPrefab.CheckOptions.TextureSize:
                    break;

                case CheckPrefab.CheckOptions.MotionVectors:
                    RepairMotionVectors(effectCheckReportInfo);
                    break;

                case CheckPrefab.CheckOptions.DynamicOcclusion:
                    RepairDynamicOcclusion(effectCheckReportInfo);
                    break;

                case CheckPrefab.CheckOptions.ForbidParticleSystem:
                    break;

                case CheckPrefab.CheckOptions.CastShadows:
                    RepairCastShadows(effectCheckReportInfo);
                    break;

                case CheckPrefab.CheckOptions.LightProbes:
                    RepairLightProbes(effectCheckReportInfo);
                    break;

                case CheckPrefab.CheckOptions.ReflectionProbes:
                    RepairReflectionProbes(effectCheckReportInfo);
                    break;

                case CheckPrefab.CheckOptions.AnimatorCullMode:
                    RepairAnimatorCullingMode(effectCheckReportInfo);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// MotionVectors
        /// </summary>
        private static void RepairMotionVectors(EffectCheckReportInfo effectCheckReportInfo)
        {
            if (ReferenceEquals(effectCheckReportInfo.asset, null))
            {
                DebugUtil.LogError("执行自动修复时, 预制为空!");
                return;
            }

            if (effectCheckReportInfo.asset is GameObject topObj)
            {
                var top = topObj.transform;
                var child = top.Find(effectCheckReportInfo.assetPath);
                if (ReferenceEquals(child, null) == false)
                {
                    if (child.TryGetComponent<SkinnedMeshRenderer>(out var renderer))
                    {
                        renderer.skinnedMotionVectors = false;
                        EditorUtility.SetDirty(topObj);
                        EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                    }
                }
            }
        }

        /// <summary>
        /// DynamicOcclusion
        /// </summary>
        private static void RepairDynamicOcclusion(EffectCheckReportInfo effectCheckReportInfo)
        {
            if (ReferenceEquals(effectCheckReportInfo.asset, null))
            {
                DebugUtil.LogError("执行自动修复时, 预制为空!");
                return;
            }

            if (effectCheckReportInfo.asset is GameObject topObj)
            {
                var top = topObj.transform;
                var child = top.Find(effectCheckReportInfo.assetPath);
                if (ReferenceEquals(child, null) == false)
                {
                    if (child.TryGetComponent<Renderer>(out var renderer))
                    {
                        renderer.allowOcclusionWhenDynamic = false;
                        EditorUtility.SetDirty(topObj);
                        EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                    }
                }
            }
        }

        /// <summary>
        /// CastShadows
        /// </summary>
        private static void RepairCastShadows(EffectCheckReportInfo effectCheckReportInfo)
        {
            if (ReferenceEquals(effectCheckReportInfo.asset, null))
            {
                DebugUtil.LogError("执行自动修复时, 预制为空!");
                return;
            }

            if (effectCheckReportInfo.asset is GameObject topObj)
            {
                var top = topObj.transform;
                var child = top.Find(effectCheckReportInfo.assetPath);
                if (ReferenceEquals(child, null) == false)
                {
                    if (child.TryGetComponent<SkinnedMeshRenderer>(out var renderer))
                    {
                        renderer.shadowCastingMode = ShadowCastingMode.On;
                        EditorUtility.SetDirty(topObj);
                        EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                    }
                }
            }
        }

        /// <summary>
        /// LightProbes
        /// </summary>
        private static void RepairLightProbes(EffectCheckReportInfo effectCheckReportInfo)
        {
            if (ReferenceEquals(effectCheckReportInfo.asset, null))
            {
                DebugUtil.LogError("执行自动修复时, 预制为空!");
                return;
            }

            if (effectCheckReportInfo.asset is GameObject topObj)
            {
                var top = topObj.transform;
                var child = top.Find(effectCheckReportInfo.assetPath);
                if (ReferenceEquals(child, null) == false)
                {
                    if (child.TryGetComponent<Renderer>(out var renderer))
                    {
                        renderer.lightProbeUsage = LightProbeUsage.Off;
                        EditorUtility.SetDirty(topObj);
                        EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                    }
                }
            }
        }

        /// <summary>
        /// ReflectionProbes
        /// </summary>
        private static void RepairReflectionProbes(EffectCheckReportInfo effectCheckReportInfo)
        {
            if (ReferenceEquals(effectCheckReportInfo.asset, null))
            {
                DebugUtil.LogError("执行自动修复时, 预制为空!");
                return;
            }

            if (effectCheckReportInfo.asset is GameObject topObj)
            {
                var top = topObj.transform;
                var child = top.Find(effectCheckReportInfo.assetPath);
                if (ReferenceEquals(child, null) == false)
                {
                    if (child.TryGetComponent<Renderer>(out var renderer))
                    {
                        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                        EditorUtility.SetDirty(topObj);
                        EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                    }
                }
            }
        }

        /// <summary>
        /// AnimatorCullingMode
        /// </summary>
        private static void RepairAnimatorCullingMode(EffectCheckReportInfo effectCheckReportInfo)
        {
            if (ReferenceEquals(effectCheckReportInfo.asset, null))
            {
                DebugUtil.LogError("执行自动修复时, 预制为空!");
                return;
            }

            if (effectCheckReportInfo.asset is GameObject topObj)
            {
                var top = topObj.transform;
                var child = top.Find(effectCheckReportInfo.assetPath);
                if (ReferenceEquals(child, null) == false)
                {
                    if (child.TryGetComponent<Animator>(out var animator))
                    {
                        animator.cullingMode = AnimatorCullingMode.CullCompletely;
                        EditorUtility.SetDirty(topObj);
                        EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                    }
                }
            }
        }
    }
}