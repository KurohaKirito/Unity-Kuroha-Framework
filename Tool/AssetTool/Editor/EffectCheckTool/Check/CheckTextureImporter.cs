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
        private enum EM_GetAssetOption
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
            InMaterial,
        }
        
        /// <summary>
        /// 检查纹理的什么选项
        /// </summary>
        public enum EM_CheckOption
        {
            Size,
            MipMaps,
            ReadWriteEnable,
            CompressFormat,
        }
        
        /// <summary>
        /// 检查纹理的什么设置
        /// </summary>
        public static readonly string[] checkOptionArray =
        {
            "纹理尺寸",
            "Mip Maps 设置",
            "读写设置",
            "压缩格式"
        };
        
        /// <summary>
        /// 检查尺寸时的子检查项
        /// </summary>
        public static readonly string[] sizeOptionArray =
        {
            "32",
            "64",
            "128",
            "256",
            "512",
            "1024",
            "2048"
        };

        private readonly CheckItemInfo checkItemInfo;
        private readonly EM_GetAssetOption getOption;
        private readonly EM_CheckOption checkOption;
        private readonly string checkOptionParameter;
        private readonly string[] checkOptionParameterArray;
        private readonly List<TextureImporter> textureImportersToCheck;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CheckTextureImporter(CheckItemInfo checkItemInfo)
        {
            this.checkItemInfo = checkItemInfo;
            textureImportersToCheck = new List<TextureImporter>();
            
            getOption = (EM_GetAssetOption) this.checkItemInfo.getAssetType;
            checkOption = (EM_CheckOption) this.checkItemInfo.checkOption;
            checkOptionParameter = this.checkItemInfo.parameter;
            checkOptionParameterArray = this.checkItemInfo.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
        }
        
        private void GetTextureImporterInExplorer()
        {
            textureImportersToCheck.Clear();
            
            var guids = AssetDatabase.FindAssets("t:Texture", new[] { checkItemInfo.checkPath });
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath);
            var assetImporters = paths.Select(AssetImporter.GetAtPath);

            foreach (var assetImporter in assetImporters) {
                if (assetImporter is TextureImporter textureImporter) {
                    textureImportersToCheck.Add(textureImporter);
                } else {
                    DebugUtil.LogError("此资源并不是纹理类型!", assetImporter, "red");
                }
            }
        }
        
        private void GetTextureImporterInPrefab()
        {
            
        }
        
        private void GetTextureImporterInMaterial()
        {
            
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
            if (checkItemInfo.checkPath.IndexOf("Assets", StringComparison.Ordinal) == 0)
            {
                DebugUtil.LogError("检测路径必须以 Assets 开头!");
                return;
            }

            switch (getOption)
            {
                case EM_GetAssetOption.InExplorer:
                    GetTextureImporterInExplorer();
                    break;
                
                case EM_GetAssetOption.InPrefab:
                    GetTextureImporterInPrefab();
                    break;
                
                case EM_GetAssetOption.InMaterial:
                    GetTextureImporterInMaterial();
                    break;
                
                default:
                    DebugUtil.LogError("枚举值 EM_GetAssetOption 错误!");
                    break;
            }

            foreach (var textureImporter in textureImportersToCheck)
            {
                var textureImporterPath = AssetDatabase.GetAssetPath(textureImporter);
                
                if (IsFormatRight(textureImporterPath))
                {
                    switch (checkOption)
                    {
                        case EM_CheckOption.Size:
                            CheckSize(textureImporter, textureImporterPath, checkItemInfo, ref reportInfos);
                            break;

                        case EM_CheckOption.ReadWriteEnable:
                            CheckReadWriteEnable(textureImporter, textureImporterPath, checkItemInfo, ref reportInfos);
                            break;

                        case EM_CheckOption.MipMaps:
                            CheckMipMaps(textureImporter, textureImporterPath, checkItemInfo, ref reportInfos);
                            break;

                        case EM_CheckOption.CompressFormat:
                            CheckCompressFormat(textureImporter, textureImporterPath, checkItemInfo, ref reportInfos);
                            break;

                        default:
                            DebugUtil.LogError("枚举值 EM_CheckOption 错误!");
                            break;
                    }
                }
            }
            
            /*
            
            
            
            
            
            if (itemData.checkPath.StartsWith("Assets"))
            {
                var fullPath = System.IO.Path.GetFullPath(itemData.checkPath);
                if (Directory.Exists(fullPath))
                {
                    var direction = new DirectoryInfo(fullPath);
                    var searchType = itemData.isCheckSubFile ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    
                    var files = direction.GetFiles("*", searchType);
                    for (var index = 0; index < files.Length; index++)
                    {
                        ProgressBar.DisplayProgressBar("特效检测工具", $"Texture 排查中: {index + 1}/{files.Length}", index + 1, files.Length);
                        if (files[index].Name.EndsWith(".meta"))
                        {
                            continue;
                        }

                        var assetPath = PathUtil.GetAssetPath(files[index].FullName);
                        var pattern = itemData.assetWhiteRegex;
                        if (string.IsNullOrEmpty(pattern) == false)
                        {
                            var regex = new Regex(pattern);
                            if (regex.IsMatch(assetPath))
                            {
                                continue;
                            }
                        }

                        switch ((EM_CheckOption) itemData.checkOption)
                        {
                            case EM_CheckOption.Size:
                                if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureSize, files[index], itemData, ref reportInfos) == false)
                                {
                                    CheckSize(assetPath, files[index], itemData, ref reportInfos);
                                }

                                break;

                            case EM_CheckOption.ReadWriteEnable:
                                if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureReadWriteEnable, files[index], itemData, ref reportInfos) == false)
                                {
                                    CheckReadWriteEnable(assetPath, files[index], itemData, ref reportInfos);
                                }

                                break;

                            case EM_CheckOption.MipMaps:
                                if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, files[index], itemData, ref reportInfos) == false)
                                {
                                    CheckMipMaps(assetPath, files[index], itemData, ref reportInfos);
                                }

                                break;

                            case EM_CheckOption.CompressFormat:
                                if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, files[index], itemData, ref reportInfos) == false)
                                {
                                    CheckCompressFormat(assetPath, files[index], itemData, ref reportInfos);
                                }

                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            else
            {
                DebugUtil.LogError("检测路径必须以 Assets 开头!");
            }
            
            */
        }
        
        /// <summary>
        /// 检测: 贴图尺寸
        /// </summary>
        private void CheckSize(TextureImporter textureImporter, string textureImporterPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            // 参数标准
            var width = Convert.ToInt32(sizeOptionArray[Convert.ToInt32(checkOptionParameterArray[0])]);
            var height = Convert.ToInt32(sizeOptionArray[Convert.ToInt32(checkOptionParameterArray[1])]);

            // 计算原始尺寸
            TextureUtil.GetTextureOriginalSize(textureImporter, out var originWidth, out var originHeight);
            if (textureImporter.textureShape == TextureImporterShape.TextureCube)
            {
                originWidth /= 2;
            }

            if (originWidth > width && originHeight > height)
            {
                #region Android

                if (TextureUtil.GetTextureSizeAndroid(textureImporter, out var maxSizeAndroid))
                {
                    if (maxSizeAndroid > width && maxSizeAndroid > height)
                    {
                        var content = $"Android: 纹理尺寸过大, 路径为: {textureImporterPath}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 Android 导入设置: {maxSizeAndroid} >>> 规范: ({width}X{height})";
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                        report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                    }
                }
                else
                {
                    TextureUtil.GetTextureSizeDefault(textureImporter, out var maxSizeDefault);
                    if (maxSizeDefault > width && maxSizeDefault > height)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                        var content = $"未启用 Android 导入, 资源路径为: {textureImporterPath}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 Default 导入设置: {maxSizeDefault} >>> 规范: ({width}X{height})";
                        report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                    }
                }

                #endregion

                #region iPhone

                if (TextureUtil.GetTextureSizeIPhone(textureImporter, out var maxSizeIPhone))
                {
                    if (maxSizeIPhone > width && maxSizeIPhone > height)
                    {
                        var content = $"iPhone: 纹理尺寸过大, 路径为: {textureImporterPath}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 iPhone 导入设置: {maxSizeIPhone} >>> 规范: ({width}X{height})";
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                        report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                    }
                }
                else
                {
                    TextureUtil.GetTextureSizeDefault(textureImporter, out var maxSizeDefault);
                    if (maxSizeDefault > width && maxSizeDefault > height)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                        var content = $"未启用 iPhone 导入, 资源路径为: {textureImporterPath}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 Default 导入设置: {maxSizeDefault} >>> 规范: ({width}X{height})";
                        report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                    }
                }

                #endregion
            }
        }
        
        /// <summary>
        /// 检测: 贴图读写设置
        /// </summary>
        private void CheckReadWriteEnable(TextureImporter textureImporter, string textureImporterPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var isOpenReadWriter = Convert.ToBoolean(checkOptionParameter);
            if (textureImporter.isReadable != isOpenReadWriter)
            {
                var tips = isOpenReadWriter ? "需要强制开启" : "需要强制关闭";
                var content = $"Read/Write Enable 配置不规范, 路径为: {textureImporterPath} 当前设置: {textureImporter.isReadable} >>> {tips}";
                var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureReadWriteEnable, content, item));
            }
        }

        /// <summary>
        /// 检测: 贴图 MipMap 设置
        /// </summary>
        private static void CheckMipMaps(TextureImporter textureImporter, string textureImporterPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var isOpenMinMaps = Convert.ToBoolean(item.parameter);

            if (textureImporter.mipmapEnabled != isOpenMinMaps)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                if (isOpenMinMaps)
                {
                    // 尺寸小于 64 的纹理不需要开启 MipMaps
                    if (asset.width > 64 || asset.height > 64)
                    {
                        var content = $"Mip Maps 配置不规范, 路径为: {textureImporterPath} 当前 MinMaps: {textureImporter.mipmapEnabled} >>> 需要强制开启 ({asset.width}X{asset.height})";
                        report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, content, item));
                    }
                }
                else
                {
                    var content = $"Mip Maps 配置不规范, 路径为: {textureImporterPath} 当前 MinMaps: {textureImporter.mipmapEnabled} >>> 需要强制关闭";
                    report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, content, item));
                }
            }
        }

        /// <summary>
        /// 检测: 贴图压缩格式
        /// </summary>
        private static void CheckCompressFormat(TextureImporter textureImporter, string textureImporterPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
             #region Android

            if (TextureUtil.GetTextureFormatAndroid(textureImporter, out var formatAndroid))
            {
                if (formatAndroid != TextureImporterFormat.ETC2_RGB4 && formatAndroid != TextureImporterFormat.ETC2_RGBA8)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                    var content = $"Android: 纹理压缩格式不是 ETC2, 路径为: {textureImporterPath}, 当前压缩格式: {formatAndroid}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, content, item));
                }
            }
            else
            {
                var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                var content = $"未启用 Android 导入, 资源路径为: {textureImporterPath}";
                report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, content, item));
            }

            #endregion

            #region iPhone

            if (TextureUtil.GetTextureFormatIPhone(textureImporter, out var formatIOS))
            {
                if (formatIOS != TextureImporterFormat.PVRTC_RGB4 && formatIOS != TextureImporterFormat.PVRTC_RGBA4)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                    var content = $"iPhone: 纹理压缩格式不是 PVRTC, 路径为: {textureImporterPath}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, content, item));
                }
            }
            else
            {
                var asset = AssetDatabase.LoadAssetAtPath<Texture>(textureImporterPath);
                var content = $"未启用 iPhone 导入, 资源路径为: {textureImporterPath}";
                report.Add(EffectCheckReport.AddReportInfo(asset, textureImporterPath, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, content, item));
            }

            #endregion
        }
    }
}
