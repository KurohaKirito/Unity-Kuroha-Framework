using System;
using System.Collections.Generic;
using System.Linq;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemSetView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Editor;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Check
{
    public class CheckTextureImporter
    {
        /// <summary>
        /// 检查哪里的纹理
        /// </summary>
        public enum EM_GetAssetOption
        {
            /// <summary>
            /// 直接检查资源管理器中的资源
            /// </summary>
            InExplorer,
            
            /// <summary>
            /// 检查预制体中引用的全部资源
            /// </summary>
            InPrefab,
            
            /// <summary>
            /// 检查材质球中引用的全部资源
            /// </summary>
            InMaterial
        }
        
        /// <summary>
        /// 检查纹理的什么选项
        /// </summary>
        public enum EM_CheckOption
        {
            /// <summary>
            /// 导入尺寸设置
            /// </summary>
            ImporterSize,
            
            /// <summary>
            /// Mip Maps 设置
            /// </summary>
            MipMaps,
            
            /// <summary>
            /// 读写设置
            /// </summary>
            ReadWriteEnable,
            
            /// <summary>
            /// 压缩格式设置
            /// </summary>
            CompressFormat
        }

        /// <summary>
        /// 检查尺寸时的子检查项
        /// </summary>
        public static readonly string[] sizeOptionArray =
        {
            "32", "64", "128", "256", "512", "1024", "2048"
        };

        /// <summary>
        /// 检查项信息
        /// </summary>
        private readonly CheckItemInfo checkItemInfo;
        
        private readonly EM_GetAssetOption getOption;
        
        private readonly EM_CheckOption checkOption;
        
        private readonly string checkOptionParameter;
        
        private readonly string[] checkOptionParameterArray;
        
        private readonly List<TextureImporter> assetsToCheck;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CheckTextureImporter(CheckItemInfo checkItemInfo)
        {
            this.checkItemInfo = checkItemInfo;
            assetsToCheck = new List<TextureImporter>();
            
            getOption = (EM_GetAssetOption) this.checkItemInfo.getAssetType;
            checkOption = (EM_CheckOption) this.checkItemInfo.checkOption;
            checkOptionParameter = this.checkItemInfo.parameter;
            checkOptionParameterArray = this.checkItemInfo.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
        }
        
        /// <summary>
        /// 得到资源管理器中的全部纹理
        /// </summary>
        private void GetAssetInExplorer()
        {
            assetsToCheck.Clear();
            
            var guids = AssetDatabase.FindAssets("t:Texture", new[] { checkItemInfo.checkPath });
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath);
            var assetImporters = paths.Select(AssetImporter.GetAtPath);
            AddAssetToCheck(assetImporters);
        }
        
        /// <summary>
        /// 得到预制体中的全部纹理
        /// </summary>
        private void GetAssetInPrefab()
        {
            assetsToCheck.Clear();
            
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { checkItemInfo.checkPath });
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath);
            var prefabs = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>);

            foreach (var prefab in prefabs)
            {
                TextureUtil.GetTexturesInGameObject(prefab, out _, out var assetPaths);
                var assetImporters = assetPaths.Select(AssetImporter.GetAtPath);
                AddAssetToCheck(assetImporters);
            }
        }
        
        /// <summary>
        /// 得到材质中的全部纹理
        /// </summary>
        private void GetAssetInMaterial()
        {
            assetsToCheck.Clear();
            
            var guids = AssetDatabase.FindAssets("t:Material", new[] { checkItemInfo.checkPath });
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath);
            var materials = paths.Select(AssetDatabase.LoadAssetAtPath<Material>);
            
            foreach (var material in materials)
            {
                TextureUtil.GetTexturesInMaterial(material, out var textureDataList);
                var assetImporters = textureDataList.Select(t => AssetImporter.GetAtPath(t.path));
                AddAssetToCheck(assetImporters);
            }
            
        }

        private void AddAssetToCheck(IEnumerable<AssetImporter> assetImporters)
        {
            foreach (var assetImporter in assetImporters)
            {
                if (assetImporter is TextureImporter importer)
                {
                    assetsToCheck.Add(importer);
                }
                else
                {
                    DebugUtil.LogError("此资源并不是纹理类型!", assetImporter, "red");
                }
            }
        }
        
        /// <summary>
        /// 检测纹理类型是否正确
        /// </summary>
        private static bool IsFormatRight(string filePath)
        {
            if (filePath.EndsWith(".png") || filePath.EndsWith(".tga") || filePath.EndsWith(".psd") || filePath.EndsWith(".tif") || filePath.EndsWith(".jpg"))
            {
                return true;
            }
            
            DebugUtil.LogError($"非法图片类型: {filePath}");
            return false;
        }

        /// <summary>
        /// 执行检测
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
                
                if (IsFormatRight(path))
                {
                    switch (checkOption)
                    {
                        case EM_CheckOption.ImporterSize:
                            CheckSize(asset, path, checkItemInfo, ref reportInfos);
                            break;

                        case EM_CheckOption.ReadWriteEnable:
                            CheckReadWriteEnable(asset, path, checkItemInfo, ref reportInfos);
                            break;

                        case EM_CheckOption.MipMaps:
                            CheckMipMaps(asset, path, checkItemInfo, ref reportInfos);
                            break;

                        case EM_CheckOption.CompressFormat:
                            CheckCompressFormat(asset, path, checkItemInfo, ref reportInfos);
                            break;

                        default:
                            DebugUtil.LogError("枚举值 EM_CheckOption 错误!");
                            break;
                    }
                }
            }
        }
        
        /// <summary>
        /// 检测: 贴图尺寸
        /// </summary>
        private void CheckSize(TextureImporter importer, string path, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            // 参数标准
            var width = Convert.ToInt32(sizeOptionArray[Convert.ToInt32(checkOptionParameterArray[0])]);
            var height = Convert.ToInt32(sizeOptionArray[Convert.ToInt32(checkOptionParameterArray[1])]);

            // 计算原始尺寸
            TextureUtil.GetTextureOriginalSize(importer, out var originWidth, out var originHeight);
            if (importer.textureShape == TextureImporterShape.TextureCube)
            {
                originWidth /= 2;
            }

            if (originWidth > width && originHeight > height)
            {
                #region Android

                if (TextureUtil.GetTextureSizeAndroid(importer, out var maxSizeAndroid))
                {
                    if (maxSizeAndroid > width && maxSizeAndroid > height)
                    {
                        var content = $"Android: 纹理尺寸过大, 路径为: {path}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 Android 导入设置: {maxSizeAndroid} >>> 规范: ({width}X{height})";
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                        report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                    }
                }
                else
                {
                    TextureUtil.GetTextureSizeDefault(importer, out var maxSizeDefault);
                    if (maxSizeDefault > width && maxSizeDefault > height)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                        var content = $"未启用 Android 导入, 资源路径为: {path}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 Default 导入设置: {maxSizeDefault} >>> 规范: ({width}X{height})";
                        report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                    }
                }

                #endregion

                #region iPhone

                if (TextureUtil.GetTextureSizeIPhone(importer, out var maxSizeIPhone))
                {
                    if (maxSizeIPhone > width && maxSizeIPhone > height)
                    {
                        var content = $"iPhone: 纹理尺寸过大, 路径为: {path}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 iPhone 导入设置: {maxSizeIPhone} >>> 规范: ({width}X{height})";
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                        report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                    }
                }
                else
                {
                    TextureUtil.GetTextureSizeDefault(importer, out var maxSizeDefault);
                    if (maxSizeDefault > width && maxSizeDefault > height)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                        var content = $"未启用 iPhone 导入, 资源路径为: {path}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 Default 导入设置: {maxSizeDefault} >>> 规范: ({width}X{height})";
                        report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                    }
                }

                #endregion
            }
        }
        
        /// <summary>
        /// 检测: 贴图读写设置
        /// </summary>
        private void CheckReadWriteEnable(TextureImporter importer, string path, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var isOpenReadWriter = Convert.ToBoolean(checkOptionParameter);
            if (importer.isReadable != isOpenReadWriter)
            {
                var tips = isOpenReadWriter ? "需要强制开启" : "需要强制关闭";
                var content = $"Read/Write Enable 配置不规范, 路径为: {path} 当前设置: {importer.isReadable} >>> {tips}";
                var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.TextureReadWriteEnable, content, item));
            }
        }

        /// <summary>
        /// 检测: 贴图 MipMap 设置
        /// </summary>
        private void CheckMipMaps(TextureImporter importer, string path, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var isOpenMinMaps = Convert.ToBoolean(checkOptionParameter);

            if (importer.mipmapEnabled != isOpenMinMaps)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (isOpenMinMaps)
                {
                    // 尺寸小于 64 的纹理不需要开启 MipMaps
                    if (asset.width > 64 || asset.height > 64)
                    {
                        var content = $"Mip Maps 配置不规范, 路径为: {path} 当前 MinMaps: {importer.mipmapEnabled} >>> 需要强制开启 ({asset.width}X{asset.height})";
                        report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, content, item));
                    }
                }
                else
                {
                    var content = $"Mip Maps 配置不规范, 路径为: {path} 当前 MinMaps: {importer.mipmapEnabled} >>> 需要强制关闭";
                    report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, content, item));
                }
            }
        }

        /// <summary>
        /// 检测: 贴图压缩格式
        /// </summary>
        private void CheckCompressFormat(TextureImporter importer, string path, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            #region Android

            var androidError = false;
            var androidContent = string.Empty;
            var android = (TextureImporterFormat) Convert.ToInt32(checkOptionParameterArray[0]);

            if (TextureUtil.GetTextureFormatAndroid(importer, out var formatAndroid))
            {
                if (android == TextureImporterFormat.ETC2_RGB4 || android == TextureImporterFormat.ETC2_RGBA8)
                {
                    if (formatAndroid != TextureImporterFormat.ETC2_RGB4 && formatAndroid != TextureImporterFormat.ETC2_RGBA8)
                    {
                        androidError = true;
                        androidContent = $"Android: 纹理压缩格式不是 ETC2, 路径为: {path}, 当前压缩格式: {formatAndroid}";
                    }
                }
            }
            else
            {
                androidError = true;
                androidContent = $"无 Android 压缩格式设置, 资源路径为: {path}";
            }

            if (androidError)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, androidContent, item));
            }

            #endregion

            #region iPhone
            
            var iphoneError = false;
            var iphoneContent = string.Empty;
            var iphone = (TextureImporterFormat) Convert.ToInt32(checkOptionParameterArray[1]);

            if (TextureUtil.GetTextureFormatIPhone(importer, out var formatIOS))
            {
                if (iphone == TextureImporterFormat.PVRTC_RGB4 || android == TextureImporterFormat.PVRTC_RGBA4)
                {
                    if (formatIOS != TextureImporterFormat.PVRTC_RGB4 && formatIOS != TextureImporterFormat.PVRTC_RGBA4)
                    {
                        iphoneError = true;
                        iphoneContent = $"iPhone: 纹理压缩格式不是 PVRTC, 路径为: {path}, 当前压缩格式: {formatIOS}";
                    }
                }
                else if (iphone == TextureImporterFormat.ASTC_6x6 || android == TextureImporterFormat.ASTC_HDR_6x6)
                {
                    if (formatIOS != TextureImporterFormat.ASTC_6x6 && formatIOS != TextureImporterFormat.ASTC_HDR_6x6)
                    {
                        iphoneError = true;
                        iphoneContent = $"iPhone: 纹理压缩格式不是 ASTC_6x6, 路径为: {path}, 当前压缩格式: {formatIOS}";
                    }
                }
            }
            else
            {
                iphoneError = true;
                iphoneContent = $"无 iPhone 压缩格式设置, 资源路径为: {path}";
            }

            if (iphoneError) {
                var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                report.Add(EffectCheckReport.AddReportInfo(asset, path, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, iphoneContent, item));
            }

            #endregion
        }
    }
}
