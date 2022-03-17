using System;
using System.Collections.Generic;
using System.Linq;
using Kuroha.Framework.Utility.RunTime;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.ItemListView;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.ItemSetView;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.Report;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.EffectCheckTool.Editor.Check
{
    public class CheckModelImporter
    {
        /// <summary>
        /// 检查哪里的纹理
        /// </summary>
        public enum EM_GetAssetOption
        {
            /// <summary>
            /// 直接检查资源管理器中的纹理
            /// </summary>
            InExplorer,
            
            /// <summary>
            /// 检查预制体中引用的全部纹理
            /// </summary>
            InPrefab,
            
            /// <summary>
            /// 检查材质球中引用的全部纹理
            /// </summary>
            InMaterial
        }
        
        /// <summary>
        /// 模型资源检查类型
        /// </summary>
        public enum EM_CheckOption
        {
            ReadWriteEnable,
            Normals,
            MeshOptimize,
            MeshCompression,
            WeldVertices
        }

        private readonly CheckItemInfo checkItemInfo;
        private readonly EM_GetAssetOption getOption;
        private readonly EM_CheckOption checkOption;
        private readonly string checkOptionParameter;
        private readonly string[] checkOptionParameterArray;
        private readonly List<ModelImporter> assetsToCheck;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CheckModelImporter(CheckItemInfo checkItemInfo)
        {
            this.checkItemInfo = checkItemInfo;
            assetsToCheck = new List<ModelImporter>();
            
            getOption = (EM_GetAssetOption) this.checkItemInfo.getAssetType;
            checkOption = (EM_CheckOption) this.checkItemInfo.checkOption;
            checkOptionParameter = this.checkItemInfo.parameter;
            checkOptionParameterArray = this.checkItemInfo.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
        }

        /// <summary>
        /// 资源管理器中的资源
        /// </summary>
        private void GetAssetInExplorer()
        {
            assetsToCheck.Clear();
            
            var guids = AssetDatabase.FindAssets("t:Model", new[] { checkItemInfo.checkPath });
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath);
            var assetImporters = paths.Select(AssetImporter.GetAtPath);
            AddAssetToCheck(assetImporters);
        }
        
        private void GetAssetInPrefab()
        {
            // assetsToCheck.Clear();
            //
            // var guids = AssetDatabase.FindAssets("t:Prefab", new[] { checkItemInfo.checkPath });
            // var paths = guids.Select(AssetDatabase.GUIDToAssetPath);
            // var prefabs = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>);

            // foreach (var prefab in prefabs)
            // {
            //     TextureUtil.GetMeshesInGameObject(prefab, out _, out var assetPaths);
            //     var assetImporters = assetPaths.Select(AssetImporter.GetAtPath);
            //     AddAssetToCheck(assetImporters);
            // }
        }
        
        private void GetAssetInMaterial()
        {
            
        }
        
        /// <summary>
        /// 添加资源到待检查列表
        /// </summary>
        private void AddAssetToCheck(IEnumerable<AssetImporter> assetImporters)
        {
            foreach (var assetImporter in assetImporters)
            {
                if (assetImporter is ModelImporter importer)
                {
                    assetsToCheck.Add(importer);
                }
                else
                {
                    DebugUtil.LogError("此资源并不是模型类型!", assetImporter, "red");
                }
            }
        }
        
        /// <summary>
        /// 对模型文件进行检测
        /// </summary>
        public void Check(ref List<EffectCheckReportInfo> reportInfos)
        {
            if (checkItemInfo.checkPath.IndexOf("Assets", StringComparison.Ordinal) != 0)
            {
                DebugUtil.LogError("检测路径必须以 Assets 开头!");
                return;
            }

            // 获取待检测资源
            switch (getOption)
            {
                case EM_GetAssetOption.InExplorer:
                    GetAssetInExplorer();
                    break;
                
                case EM_GetAssetOption.InPrefab:
                    GetAssetInPrefab();
                    break;
                
                case EM_GetAssetOption.InMaterial:
                    GetAssetInMaterial();
                    break;
                
                default:
                    DebugUtil.LogError("枚举值 EM_GetAssetOption 错误!");
                    break;
            }
            
            // 遍历检测资源
            foreach (var asset in assetsToCheck)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                
                switch (checkOption)
                {
                    case EM_CheckOption.ReadWriteEnable:
                        CheckReadWriteEnable(asset, path, checkItemInfo, ref reportInfos);
                        break;

                    case EM_CheckOption.Normals:
                        CheckNormals(asset, path, checkItemInfo, ref reportInfos);
                        break;

                    case EM_CheckOption.MeshOptimize:
                        CheckOptimizeMesh(asset, path, checkItemInfo, ref reportInfos);
                        break;
                            
                    case EM_CheckOption.MeshCompression:
                        CheckMeshCompression(asset, path, checkItemInfo, ref reportInfos);
                        break;
                            
                    case EM_CheckOption.WeldVertices:
                        CheckWeldVertices(asset, path, checkItemInfo, ref reportInfos);
                        break;
                        
                    default:
                        DebugUtil.LogError("枚举值 EM_CheckOption 错误!");
                        break;
                }
            }
        }

        /// <summary>
        /// 检测: 读写设置
        /// </summary>
        private void CheckReadWriteEnable(ModelImporter importer, string path, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var set = bool.Parse(checkOptionParameter);
            
            if (importer.isReadable != set)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var content = $"模型的读写权限错误!\t路径: {path}";
                report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.FBXReadWriteEnable, content, item));
            }
        }
        
        /// <summary>
        /// 检测: 模型法线导入
        /// </summary>
        private void CheckNormals(ModelImporter importer, string path, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var set = (ModelImporterNormals) int.Parse(checkOptionParameter);
            
            if (importer.importNormals != set)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var content = $"模型的法线设置错误!\t路径: {path}";
                report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.FBXNormals, content, item));
            }
        }
        
        /// <summary>
        /// 检测: MeshOptimize
        /// </summary>
        private void CheckOptimizeMesh(ModelImporter importer, string path, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var set = bool.Parse(checkOptionParameter);
            
            if (importer.optimizeMeshVertices != set)
            {
                var content = $"模型的网格优化错误!\t路径: {path} ({importer.optimizeMeshVertices}) >>> ({set})";
                report.Add(EffectCheckReport.AddReportInfo(importer, path, EffectCheckReportInfo.EffectCheckReportType.FBXOptimizeMesh, content, item));
            }
        }
        
        /// <summary>
        /// 检测: MeshCompression
        /// </summary>
        private void CheckMeshCompression(ModelImporter importer, string path, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var parameter = Convert.ToInt32(checkOptionParameter) switch
            {
                0 => ModelImporterMeshCompression.Off,
                1 => ModelImporterMeshCompression.Low,
                2 => ModelImporterMeshCompression.Medium,
                3 => ModelImporterMeshCompression.High,
                _ => ModelImporterMeshCompression.Off
            };
            
            if (importer.meshCompression != parameter)
            {
                var content = $"模型的网格压缩错误!\t路径: : {path} ({importer.meshCompression}) >>> ({parameter})";
                report.Add(EffectCheckReport.AddReportInfo(importer, path, EffectCheckReportInfo.EffectCheckReportType.FBXMeshCompression, content, item));
            }
        }
        
        /// <summary>
        /// 检测: WeldVertices
        /// </summary>
        private void CheckWeldVertices(ModelImporter importer, string path, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var set = bool.Parse(checkOptionParameter);
            
            if (importer.weldVertices != set)
            {
                var content = $"模型的顶点焊接错误: {path} ({importer.weldVertices}) >>> ({set})";
                report.Add(EffectCheckReport.AddReportInfo(importer, path, EffectCheckReportInfo.EffectCheckReportType.FBXWeldVertices, content, item));
            }
        }
    }
}