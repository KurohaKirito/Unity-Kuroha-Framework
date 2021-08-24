using System;
using System.Collections.Generic;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.EffectCheckTool.GUI;
using Kuroha.Tool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.Editor.EffectCheckTool.Repair;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kuroha.Tool.Editor.EffectCheckTool.Report
{
    public static class EffectCheckReport
    {
        /// <summary>
        /// 全部待显示的检查结果
        /// </summary>
        public static List<EffectCheckReportInfo> reportInfos;

        /// <summary>
        /// 添加一个 "待修复问题" 项
        /// </summary>
        /// <param name="asset">问题所指向的游戏物体, 用于自动修复</param>
        /// <param name="assetPath">问题所指向的游戏物体的资源路径</param>
        /// <param name="effectCheckReportType">问题的类型</param>
        /// <param name="content">问题的描述</param>
        /// <param name="itemData">检查项的具体信息</param>
        /// <returns>"待修复问题" 页面信息</returns>
        public static EffectCheckReportInfo AddReportInfo(Object asset, string assetPath,
            EffectCheckReportInfo.EffectCheckReportType effectCheckReportType,
            string content, CheckItemInfo itemData)
        {
            var reportInfo = new EffectCheckReportInfo
            {
                asset = asset,
                assetPath = assetPath,
                content = content,
                effectCheckReportType = effectCheckReportType,

                assetType = itemData.assetsType,
                modeType = itemData.checkType,
                parameter = itemData.parameter,
                isEnable = itemData.effectEnable,
                dangerLevel = itemData.dangerLevel
            };
            return reportInfo;
        }

        /// <summary>
        /// 修复全部问题项
        /// </summary>
        public static void AllRepair()
        {
            var count = reportInfos.Count;

            for (var i = reportInfos.Count - 1; i >= 0; i--)
            {
                if (ProgressBar.DisplayProgressBarCancel("一键修复", $"修复中 {count - i}/{count}", count - i, count))
                {
                    break;
                }

                // 没有勾选的问题项不进行修复
                if (!reportInfos[i].isEnable)
                {
                    continue;
                }

                Repair(reportInfos[i]);
            }
        }

        /// <summary>
        /// 修复问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">问题项</param>
        public static void Repair(EffectCheckReportInfo effectCheckReportInfo)
        {
            var repaired = false;

            switch (effectCheckReportInfo.assetType)
            {
                case EffectToolData.AssetsType.Animator:
                    RepairAnimator.Repair(effectCheckReportInfo);
                    repaired = true;
                    break;

                case EffectToolData.AssetsType.ParticleSystem:
                    RepairParticle.Repair(effectCheckReportInfo);
                    repaired = true;
                    break;

                case EffectToolData.AssetsType.Mesh:
                    break;

                case EffectToolData.AssetsType.Texture:
                    break;

                case EffectToolData.AssetsType.Prefab:
                    break;

                case EffectToolData.AssetsType.Model:
                    RepairModel.Repair(effectCheckReportInfo);
                    repaired = true;
                    break;

                case EffectToolData.AssetsType.Asset:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (repaired)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 选中问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">问题项</param>
        public static void Ping(EffectCheckReportInfo effectCheckReportInfo)
        {
            if (ReferenceEquals(effectCheckReportInfo.asset, null))
            {
                return;
            }
            
            EditorGUIUtility.PingObject(effectCheckReportInfo.asset);
            Selection.activeObject = effectCheckReportInfo.asset;
        }

        /// <summary>
        /// 根据问题类型, 得出是否支持自动修复
        /// </summary>
        /// <param name="type">问题类型</param>
        public static bool RepairOrSelect(EffectCheckReportInfo.EffectCheckReportType type)
        {
            /*
             * 目前支持自动修复的内容仅有:
             *
             * 动画系统-Cull Mode 剔除模式
             * FBX-读写
             * FBX-阴影投射
             * FBX-Normals 平滑处理
             */

            var isCanRepair = false;

            switch (type)
            {
                // 1
                case EffectCheckReportInfo.EffectCheckReportType.AnimatorCullMode:
                    isCanRepair = true;
                    break;
                
                // 1
                case EffectCheckReportInfo.EffectCheckReportType.MeshUV:
                    break;

                // 3
                case EffectCheckReportInfo.EffectCheckReportType.FBXReadWriteEnable:
                    isCanRepair = true;
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.FBXMeshRendererCastShadows:
                    isCanRepair = true;
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.FBXNormals:
                    isCanRepair = true;
                    break;
                
                // 9
                case EffectCheckReportInfo.EffectCheckReportType.ParticlePrewarm:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.ParticleCastShadows:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.ParticleRendererMode:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.ParticleMeshTrisLimit:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.ParticleMeshVertexData:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.ParticleReceiveShadows:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.ParticleSubEmittersError:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.ParticleCollisionAndTrigger:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.ParticleZeroSurface:
                    break;
                
                // 4
                case EffectCheckReportInfo.EffectCheckReportType.PrefabName:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.PrefabDisableObject:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.PrefabForbidCollision:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.PrefabTextureSize:
                    break;
                
                // 3
                case EffectCheckReportInfo.EffectCheckReportType.TextureSize:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps:
                    break;
                case EffectCheckReportInfo.EffectCheckReportType.TextureReadWriteEnable:
                    break;

                // 1
                case EffectCheckReportInfo.EffectCheckReportType.AssetName:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return isCanRepair;
        }
    }
}