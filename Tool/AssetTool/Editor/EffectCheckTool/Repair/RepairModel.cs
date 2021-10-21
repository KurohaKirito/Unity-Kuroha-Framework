using System;
using Kuroha.Tool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kuroha.Tool.Editor.EffectCheckTool.Repair
{
    public static class RepairModel
    {
        /// <summary>
        /// 自动修复问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">问题项</param>
        public static void Repair(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modeType = (CheckModel.CheckOptions)effectCheckReportInfo.modeType;

            switch (modeType)
            {
                case CheckModel.CheckOptions.ReadWriteEnable:
                    RepairReadWriteEnable(effectCheckReportInfo);
                    break;

                case CheckModel.CheckOptions.RendererCastShadow:
                    RepairRendererCastShadow(effectCheckReportInfo);
                    break;

                case CheckModel.CheckOptions.Normals:
                    RepairNormals(effectCheckReportInfo);
                    break;

                case CheckModel.CheckOptions.OptimizeMesh:
                    RepairOptimizeMesh(effectCheckReportInfo);
                    break;
                
                case CheckModel.CheckOptions.MeshCompression:
                    RepairMeshCompression(effectCheckReportInfo);
                    break;
                
                case CheckModel.CheckOptions.WeldVertices:
                    RepairWeldVertices(effectCheckReportInfo);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 修复模型的读写权限设置
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairReadWriteEnable(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modelImporter = AssetImporter.GetAtPath(effectCheckReportInfo.assetPath) as ModelImporter;
            if (ReferenceEquals(modelImporter, null) == false)
            {
                modelImporter.isReadable = false;
                AssetDatabase.ImportAsset(effectCheckReportInfo.assetPath);
                EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
            }
        }

        /// <summary>
        /// 修复模型的阴影投射设置 (是否产生阴影)
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairRendererCastShadow(EffectCheckReportInfo effectCheckReportInfo)
        {
            if (ReferenceEquals(effectCheckReportInfo.asset, null) == false)
            {
                // MeshRenderer && SkinnedMeshRenderer
                if (effectCheckReportInfo.asset is GameObject obj)
                {
                    var renderers = obj.GetComponentsInChildren<Renderer>();
                    if (renderers.IsNotNullAndEmpty())
                    {
                        foreach (var renderer in renderers)
                        {
                            if (renderer.shadowCastingMode != ShadowCastingMode.Off)
                            {
                                renderer.shadowCastingMode = ShadowCastingMode.Off;
                            }
                        }

                        EditorUtility.SetDirty(effectCheckReportInfo.asset);
                        EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                    }
                }
            }
        }

        /// <summary>
        /// 修复模型的法线导入设置
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairNormals(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modelImporter = AssetImporter.GetAtPath(effectCheckReportInfo.assetPath) as ModelImporter;
            if (ReferenceEquals(modelImporter, null) == false)
            {
                modelImporter.importNormals = ModelImporterNormals.None;
                AssetDatabase.ImportAsset(effectCheckReportInfo.assetPath);
                EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
            }
        }
        
        /// <summary>
        /// 修复模型的 "网格优化" 导入设置
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairOptimizeMesh(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modelImporter = AssetImporter.GetAtPath(effectCheckReportInfo.assetPath) as ModelImporter;
            if (ReferenceEquals(modelImporter, null) == false)
            {
                modelImporter.optimizeMesh = true;
                AssetDatabase.ImportAsset(effectCheckReportInfo.assetPath);
                EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
            }
        }
        
        /// <summary>
        /// 修复模型的 "网格压缩" 导入设置
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairMeshCompression(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modelImporter = AssetImporter.GetAtPath(effectCheckReportInfo.assetPath) as ModelImporter;
            if (ReferenceEquals(modelImporter, null) == false)
            {
                modelImporter.meshCompression = ModelImporterMeshCompression.Low;
                AssetDatabase.ImportAsset(effectCheckReportInfo.assetPath);
                EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
            }
        }
        
        /// <summary>
        /// 修复模型的 "顶点焊接" 导入设置
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairWeldVertices(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modelImporter = AssetImporter.GetAtPath(effectCheckReportInfo.assetPath) as ModelImporter;
            if (ReferenceEquals(modelImporter, null) == false)
            {
                modelImporter.weldVertices = true;
                AssetDatabase.ImportAsset(effectCheckReportInfo.assetPath);
                EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
            }
        }
    }
}