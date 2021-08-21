using System;
using System.Collections.Generic;
using System.IO;
using Kuroha.Tool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Release;
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
            var assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(effectCheckReportInfo.asset));
            if (ReferenceEquals(assetImporter, null) == false)
            {
                var modelImporter = assetImporter as ModelImporter;
                if (ReferenceEquals(modelImporter, null) == false)
                {
                    modelImporter.isReadable = false;
                    EditorUtility.SetDirty(effectCheckReportInfo.asset);
                    EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                }
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
            var fullPath = Path.GetFullPath(AssetDatabase.GetAssetPath(effectCheckReportInfo.asset));
            var metaPath = $"{fullPath}.meta";
            metaPath = metaPath.Replace("\\", "/");

            var streamReader = new StreamReader(metaPath);
            var newMeta = new List<string>();

            var line = streamReader.ReadLine();
            while (string.IsNullOrEmpty(line) == false)
            {
                line = line.Replace("\n", "");

                if (line.IndexOf("normalImportMode: 0", StringComparison.Ordinal) != -1)
                {
                    line = "    normalImportMode: 2";
                }

                if (line.IndexOf("tangentImportMode: 3", StringComparison.Ordinal) != -1)
                {
                    line = "    tangentImportMode: 2";
                }

                if (line.IndexOf("blendShapeNormalImportMode: 1", StringComparison.Ordinal) != -1)
                {
                    line = "    blendShapeNormalImportMode: 2";
                }

                newMeta.Add(line);
                line = streamReader.ReadLine();
            }

            streamReader.Close();
            File.Delete(metaPath);

            var metaTempPath = $"{metaPath}.tmp";
            var writer = new StreamWriter(metaTempPath);
            foreach (var each in newMeta)
            {
                writer.WriteLine(each);
            }

            writer.Close();

            File.Copy(metaTempPath, metaPath);
            File.Delete(metaTempPath);
            EditorUtility.SetDirty(effectCheckReportInfo.asset);
            EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
        }
    }
}