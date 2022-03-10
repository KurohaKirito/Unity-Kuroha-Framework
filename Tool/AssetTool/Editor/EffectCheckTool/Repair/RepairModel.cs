using System;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report;
using UnityEditor;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Repair
{
    public static class RepairModel
    {
        /// <summary>
        /// 自动修复问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">问题项</param>
        public static void Repair(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modeType = (Check.CheckModelImporter.EM_CheckOption)effectCheckReportInfo.modeType;

            switch (modeType)
            {
                case Check.CheckModelImporter.EM_CheckOption.ReadWriteEnable:
                    RepairReadWriteEnable(effectCheckReportInfo);
                    break;

                case Check.CheckModelImporter.EM_CheckOption.Normals:
                    RepairNormals(effectCheckReportInfo);
                    break;

                case Check.CheckModelImporter.EM_CheckOption.MeshOptimize:
                    RepairOptimizeMesh(effectCheckReportInfo);
                    break;
                
                case Check.CheckModelImporter.EM_CheckOption.MeshCompression:
                    RepairMeshCompression(effectCheckReportInfo);
                    break;
                
                case Check.CheckModelImporter.EM_CheckOption.WeldVertices:
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
                if (modelImporter.isReadable)
                {
                    modelImporter.isReadable = false;
                    AssetDatabase.ImportAsset(effectCheckReportInfo.assetPath);
                }
                EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
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
#pragma warning disable 618
                modelImporter.optimizeMesh = true;
#pragma warning restore 618
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