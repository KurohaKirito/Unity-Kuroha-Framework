using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.ItemListView;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.ItemSetView;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Report;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Check {
    public static class CheckTexture {
        /// <summary>
        /// 贴图资源检查类型
        /// </summary>
        public static readonly string[] checkOptions = {
            "尺寸大小", "Mip Maps Enable", "Read Write Enable", "压缩格式"
        };

        /// <summary>
        /// 贴图资源检查类型
        /// </summary>
        public enum CheckOptions {
            Size,
            MipMaps,
            ReadWriteEnable,
            CompressFormat,
        }

        /// <summary>
        /// 检查 Size 时的子检查项
        /// </summary>
        public static readonly string[] sizeOptions = {
            "32", "64", "128", "256", "512", "1024", "2048"
        };

        /// <summary>
        /// 对贴图文件进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos) {
            if (itemData.path.StartsWith("Assets")) {
                var fullPath = System.IO.Path.GetFullPath(itemData.path);
                if (Directory.Exists(fullPath)) {
                    var direction = new DirectoryInfo(fullPath);
                    var searchType = itemData.isCheckSubFile? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var files = direction.GetFiles("*", searchType);
                    for (var index = 0; index < files.Length; index++) {
                        ProgressBar.DisplayProgressBar("特效检测工具", $"Texture 排查中: {index + 1}/{files.Length}", index + 1, files.Length);
                        if (files[index].Name.EndsWith(".meta")) {
                            continue;
                        }

                        var assetPath = PathUtil.GetAssetPath(files[index].FullName);
                        var pattern = itemData.assetWhiteRegex;
                        if (string.IsNullOrEmpty(pattern) == false) {
                            var regex = new Regex(pattern);
                            if (regex.IsMatch(assetPath)) {
                                continue;
                            }
                        }

                        switch ((CheckOptions)itemData.checkType) {
                            case CheckOptions.Size:
                                if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureSize, files[index], itemData, ref reportInfos) == false) {
                                    CheckSize(assetPath, files[index], itemData, ref reportInfos);
                                }

                                break;

                            case CheckOptions.ReadWriteEnable:
                                if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureReadWriteEnable, files[index], itemData, ref reportInfos) == false) {
                                    CheckReadWriteEnable(assetPath, files[index], itemData, ref reportInfos);
                                }

                                break;

                            case CheckOptions.MipMaps:
                                if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, files[index], itemData, ref reportInfos) == false) {
                                    CheckMipMaps(assetPath, files[index], itemData, ref reportInfos);
                                }

                                break;

                            case CheckOptions.CompressFormat:
                                if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, files[index], itemData, ref reportInfos) == false) {
                                    CheckCompressFormat(assetPath, files[index], itemData, ref reportInfos);
                                }
                                break;
                            
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            } else {
                DebugUtil.LogError("检测路径必须以 Assets 开头!");
            }
        }

        /// <summary>
        /// 检测: 贴图后缀类型是否非法
        /// </summary>
        /// <param name="type">报告类型</param>
        /// <param name="assetInfo">资源文件</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        /// <returns>合法: false 非法: true</returns>
        private static bool IsInvalid(EffectCheckReportInfo.EffectCheckReportType type, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var filePath = assetInfo.FullName;

            if (filePath.EndsWith(".png") || filePath.EndsWith(".tga") || filePath.EndsWith(".psd")) {
                return false;
            }

            var assetPath = filePath.Substring(filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase));
            var content = $"非法图片类型: {assetPath}";
            var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);

            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, type, content, item));
            return true;
        }

        /// <summary>
        /// 检测: 贴图尺寸
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源文件</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckSize(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            // 参数标准
            var parameter = item.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
            var widthIndex = Convert.ToInt32(parameter[0]);
            var heightIndex = Convert.ToInt32(parameter[1]);
            var width = Convert.ToInt32(sizeOptions[widthIndex]);
            var height = Convert.ToInt32(sizeOptions[heightIndex]);

            // 读取导入设置
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            // 原始尺寸
            TextureUtil.GetTextureOriginalSize(textureImporter, out var originWidth, out var originHeight);
            if (ReferenceEquals(textureImporter, null) == false) {
                if (textureImporter.textureShape == TextureImporterShape.TextureCube) {
                    originWidth /= 2;
                }

                if (originWidth > width && originHeight > height) {
                    #region Android

                    if (TextureUtil.GetTextureSizeAndroid(textureImporter, out var maxSizeAndroid)) {
                        if (maxSizeAndroid > width && maxSizeAndroid > height) {
                            var content = $"Android: 纹理尺寸过大, 路径为: {assetInfo.FullName}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 Android 导入设置: {maxSizeAndroid} >>> 规范: ({width}X{height})";
                            var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                        }
                    } else {
                        TextureUtil.GetTextureSizeDefault(textureImporter, out var maxSizeDefault);
                        if (maxSizeDefault > width && maxSizeDefault > height) {
                            var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                            var content = $"未启用 Android 导入, 资源路径为: {assetInfo.FullName}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 Default 导入设置: {maxSizeDefault} >>> 规范: ({width}X{height})";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                        }
                    }

                    #endregion

                    #region iPhone

                    if (TextureUtil.GetTextureSizeIPhone(textureImporter, out var maxSizeIPhone)) {
                        if (maxSizeIPhone > width && maxSizeIPhone > height) {
                            var content = $"iPhone: 纹理尺寸过大, 路径为: {assetInfo.FullName}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 iPhone 导入设置: {maxSizeIPhone} >>> 规范: ({width}X{height})";
                            var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                        }
                    } else {
                        TextureUtil.GetTextureSizeDefault(textureImporter, out var maxSizeDefault);
                        if (maxSizeDefault > width && maxSizeDefault > height) {
                            var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                            var content = $"未启用 iPhone 导入, 资源路径为: {assetInfo.FullName}, 纹理原始尺寸: ({originWidth}X{originHeight}), 当前 Default 导入设置: {maxSizeDefault} >>> 规范: ({width}X{height})";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
                        }
                    }

                    #endregion
                }
            }
        }
        
        /// <summary>
        /// 检测: 贴图压缩格式
        /// </summary>
        private static void CheckCompressFormat(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            // 读取导入设置
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            // 压缩格式
            if (ReferenceEquals(textureImporter, null) == false)
            {
                #region Android
                
                if (TextureUtil.GetTextureFormatAndroid(textureImporter, out var formatAndroid))
                {
                    if (formatAndroid != TextureImporterFormat.ETC2_RGB4 && formatAndroid != TextureImporterFormat.ETC2_RGBA8)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                        var content = $"Android: 纹理压缩格式不是 ETC2, 路径为: {assetInfo.FullName}, 当前压缩格式: {formatAndroid}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, content, item));
                    }
                }
                else
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    var content = $"未启用 Android 导入, 资源路径为: {assetInfo.FullName}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, content, item));
                }

                #endregion

                #region iPhone

                if (TextureUtil.GetTextureFormatIPhone(textureImporter, out var formatIOS))
                {
                    if (formatIOS != TextureImporterFormat.PVRTC_RGB4 && formatIOS != TextureImporterFormat.PVRTC_RGBA4)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                        var content = $"iPhone: 纹理压缩格式不是 PVRTC, 路径为: {assetInfo.FullName}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, content, item));
                    }
                }
                else
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    var content = $"未启用 iPhone 导入, 资源路径为: {assetInfo.FullName}";
                    report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureCompressFormat, content, item));
                }

                #endregion
            }
        }

        /// <summary>
        /// 检测: 贴图读写设置
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源文件</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckReadWriteEnable(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null) {
                var isOpenReadWriter = Convert.ToBoolean(item.parameter);
                if (textureImporter.isReadable != isOpenReadWriter) {
                    var tips = isOpenReadWriter? "需要强制开启" : "需要强制关闭";
                    var content = $"Read/Write Enable 配置不规范, 路径为: {assetInfo.FullName} 当前设置: {textureImporter.isReadable} >>> {tips}";
                    var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureReadWriteEnable, content, item));
                }
            }
        }

        /// <summary>
        /// 检测: 贴图 MipMap 设置
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源文件</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckMipMaps(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null) {
                var isOpenMinMaps = Convert.ToBoolean(item.parameter);

                if (textureImporter.mipmapEnabled != isOpenMinMaps) {
                    var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    if (isOpenMinMaps) {
                        // 尺寸小于 64 的纹理不需要开启 MipMaps
                        if (asset.width > 64 || asset.height > 64) {
                            var content = $"Mip Maps 配置不规范, 路径为: {assetInfo.FullName} 当前 MinMaps: {textureImporter.mipmapEnabled} >>> 需要强制开启 ({asset.width}X{asset.height})";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, content, item));
                        }
                    } else {
                        var content = $"Mip Maps 配置不规范, 路径为: {assetInfo.FullName} 当前 MinMaps: {textureImporter.mipmapEnabled} >>> 需要强制关闭";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, content, item));
                    }
                }
            }
        }
    }
}