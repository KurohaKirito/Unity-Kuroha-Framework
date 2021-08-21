using System;
using System.Collections.Generic;
using System.IO;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kuroha.Tool.Editor.EffectCheckTool.Check
{
    public static class CheckModel
    {
        /// <summary>
        /// 模型资源检查类型
        /// </summary>
        public static readonly string[] checkOptions =
        {
            "读写权限设置",
            "投射阴影设置",
            "法线导入设置"
        };

        /// <summary>
        /// 模型资源检查类型
        /// </summary>
        public enum CheckOptions
        {
            ReadWriteEnable,
            RendererCastShadow,
            Normals
        }

        /// <summary>
        /// 对模型文件进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos)
        {
            var fullPath = $"{Application.dataPath}/{itemData.path}";
            var direction = new DirectoryInfo(fullPath);
            var files = direction.GetFiles("*", SearchOption.AllDirectories);

            for (var i = 0; i < files.Length; i++)
            {
                ProgressBar.DisplayProgressBar("FBX 资源排查", "排查中", i + 1, files.Length);

                if (files[i].Name.EndsWith(".meta"))
                {
                    continue;
                }

                var assetPath = files[i].FullName
                    .Substring(files[i].FullName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase));

                switch ((CheckOptions)itemData.checkType)
                {
                    case CheckOptions.ReadWriteEnable:
                        CheckReadWriteEnable(assetPath, files[i], itemData, ref reportInfos);
                        break;

                    case CheckOptions.RendererCastShadow:
                        CheckMeshRendererCastShadows(assetPath, files[i], itemData, ref reportInfos);
                        CheckSkinnedMeshRendererCastShadows(assetPath, files[i], itemData, ref reportInfos);
                        break;

                    case CheckOptions.Normals:
                        CheckNormals(assetPath, files[i], itemData, ref reportInfos);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 检测: 读写设置
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckReadWriteEnable(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item,
            ref List<EffectCheckReportInfo> report)
        {
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter == null || modelImporter.isReadable == false)
            {
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            var content = $"FBX 读写设置错误: {assetInfo.FullName} 的子物件 {modelImporter.name} 当前读写: true >>> false";
            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath,
                EffectCheckReportInfo.EffectCheckReportType.FBXReadWriteEnable, content, item));
        }

        /// <summary>
        /// 检测: MeshRenderer 组件的阴影投射
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckMeshRendererCastShadows(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item,
            ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var meshRenderers = asset.GetComponentsInChildren<MeshRenderer>(true);
            if (meshRenderers == null || meshRenderers.Length <= 0)
            {
                return;
            }

            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer.shadowCastingMode == ShadowCastingMode.Off)
                {
                    continue;
                }

                var content =
                    $"阴影投射 (Cast Shadows) 未关闭: {assetInfo.FullName} 中的子物体 {meshRenderer.name} 的 Mesh Renderer";
                report.Add(EffectCheckReport.AddReportInfo(asset, assetPath,
                    EffectCheckReportInfo.EffectCheckReportType.FBXMeshRendererCastShadows, content, item));
                break;
            }
        }

        /// <summary>
        /// 检测: SkinnedMeshRenderer 组件的阴影投射
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckSkinnedMeshRendererCastShadows(string assetPath, FileSystemInfo assetInfo,
            CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null))
            {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var skinnedMeshRenderers = asset.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length <= 0)
            {
                return;
            }

            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.shadowCastingMode == ShadowCastingMode.Off)
                {
                    continue;
                }

                var content =
                    $"阴影投射 (Cast Shadows) 未关闭: {assetInfo.FullName} 中的子物体 {skinnedMeshRenderer.name} 的 Skinned Mesh Renderer";
                report.Add(EffectCheckReport.AddReportInfo(asset, assetPath,
                    EffectCheckReportInfo.EffectCheckReportType.FBXMeshRendererCastShadows, content, item));
                break;
            }
        }

        /// <summary>
        /// 检测: 模型法线导入
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckNormals(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item,
            ref List<EffectCheckReportInfo> report)
        {
            if (assetPath.IndexOf("collider", StringComparison.OrdinalIgnoreCase) < 0 &&
                assetPath.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return;
            }

            var assetImporter = AssetImporter.GetAtPath(assetPath);
            if (!(assetImporter is ModelImporter modelImporter))
            {
                return;
            }

            if (modelImporter.importNormals == ModelImporterNormals.None)
            {
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            var content = $"未关闭 Normals: {assetInfo.FullName} 子物体 {modelImporter.name}.";
            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath,
                EffectCheckReportInfo.EffectCheckReportType.FBXNormals, content, item));
        }
    }
}