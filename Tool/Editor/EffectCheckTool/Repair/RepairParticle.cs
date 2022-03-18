using System;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Check;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Report;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Repair {
    public static class RepairParticle {
        /// <summary>
        /// 自动修复问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">问题项</param>
        public static void Repair(EffectCheckReportInfo effectCheckReportInfo) {
            var modeType = (CheckParticleSystem.CheckOptions)effectCheckReportInfo.modeType;

            switch (modeType) {
                case CheckParticleSystem.CheckOptions.RenderMode:
                    break;

                case CheckParticleSystem.CheckOptions.CastShadows:
                    break;

                case CheckParticleSystem.CheckOptions.ReceiveShadows:
                    break;

                case CheckParticleSystem.CheckOptions.MeshTrisLimit:
                    break;

                case CheckParticleSystem.CheckOptions.MeshUV:
                    break;

                case CheckParticleSystem.CheckOptions.CollisionAndTrigger:
                    break;

                case CheckParticleSystem.CheckOptions.Prewarm:
                    RepairPrewarm(effectCheckReportInfo);
                    break;

                case CheckParticleSystem.CheckOptions.SubEmittersError:
                    break;

                case CheckParticleSystem.CheckOptions.ZeroSurface:
                    RepairZeroSurface(effectCheckReportInfo);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 修复粒子系统零表面警告
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairZeroSurface(EffectCheckReportInfo effectCheckReportInfo) {
            var modelImporter = AssetImporter.GetAtPath(effectCheckReportInfo.assetPath) as ModelImporter;
            if (ReferenceEquals(modelImporter, null) == false) {
                if (modelImporter.isReadable == false) {
                    DebugUtil.Log("修复了一个模型的读写设置错误!", null, "green");
                    modelImporter.isReadable = true;
                    AssetDatabase.ImportAsset(effectCheckReportInfo.assetPath);
                }

                EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
            }
        }

        /// <summary>
        /// 修复粒子系统预热
        /// </summary>
        private static void RepairPrewarm(EffectCheckReportInfo effectCheckReportInfo)
        {
            var prefab = effectCheckReportInfo.asset as GameObject;
            if (prefab != null)
            {
                var particles = prefab.transform.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var particle in particles)
                {
                    var main = particle.main;
                    main.prewarm = false;
                }
                
                EditorUtility.SetDirty(prefab);
                EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
            }
        }
    }
}