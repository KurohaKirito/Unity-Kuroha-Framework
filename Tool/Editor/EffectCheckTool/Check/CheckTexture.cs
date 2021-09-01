using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.Editor.EffectCheckTool.ItemSetView;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Editor;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.EffectCheckTool.Check
{
    public static class CheckTexture
    {
        /// <summary>
        /// 贴图资源检查类型
        /// </summary>
        public static readonly string[] checkOptions =
        {
            "尺寸大小",
            "Mip Maps Enable",
            "Read Write Enable"
        };

        /// <summary>
        /// 贴图资源检查类型
        /// </summary>
        public enum CheckOptions
        {
            Size,
            MipMaps,
            ReadWriteEnable
        }

        /// <summary>
        /// 检查 Size 时的子检查项
        /// </summary>
        public static readonly string[] sizeOptions =
        {
            "32",
            "64",
            "128",
            "256",
            "512",
            "1024",
            "2048"
        };

        /// <summary>
        /// 对贴图文件进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos)
        {
            if (itemData.path.StartsWith("Assets"))
            {
                var fullPath = System.IO.Path.GetFullPath(itemData.path);
                var direction = new DirectoryInfo(fullPath);
                var files = direction.GetFiles("*", SearchOption.AllDirectories);
                for (var index = 0; index < files.Length; index++)
                {
                    ProgressBar.DisplayProgressBar("特效检测工具", $"Texture 排查中: {index + 1}/{files.Length}", index + 1, files.Length);
                    if (files[index].Name.EndsWith(".meta"))
                    {
                        continue;
                    }

                    var assetPath = PathUtil.GetAssetPath(files[index].FullName);
                    var pattern = itemData.writePathRegex;
                    if (string.IsNullOrEmpty(pattern) == false)
                    {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(assetPath))
                        {
                            continue;
                        }
                    }
                    
                    switch ((CheckOptions)itemData.checkType)
                    {
                        case CheckOptions.Size:
                            if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureSize, files[index], itemData, ref reportInfos) == false)
                            {
                                CheckSize(assetPath, files[index], itemData, ref reportInfos);
                            }
                            break;

                        case CheckOptions.ReadWriteEnable:
                            if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureReadWriteEnable, files[index], itemData, ref reportInfos) == false)
                            {
                                CheckReadWriteEnable(assetPath, files[index], itemData, ref reportInfos);
                            }
                            break;

                        case CheckOptions.MipMaps:
                            if (IsInvalid(EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, files[index], itemData, ref reportInfos) == false)
                            {
                                CheckMipMaps(assetPath, files[index], itemData, ref reportInfos);
                            }
                            break;
                    
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
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
        private static bool IsInvalid(EffectCheckReportInfo.EffectCheckReportType type, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var filePath = assetInfo.FullName;

            if (filePath.EndsWith(".png") || filePath.EndsWith(".tga"))
            {
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
        private static void CheckSize(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
            var parameter = item.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
            var widthIndex = Convert.ToInt32(parameter[0]);
            var heightIndex = Convert.ToInt32(parameter[1]);
            var width = Convert.ToInt32(sizeOptions[widthIndex]);
            var height = Convert.ToInt32(sizeOptions[heightIndex]);

            if (asset.width > width || asset.height > height)
            {
                var content = $"文件尺寸不规范, 路径为: {assetInfo.FullName} 当前大小: ({asset.width}X{asset.height}) >>> 规范大小: ({width}X{height})";
                report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureSize, content, item));
            }
        }

        /// <summary>
        /// 检测: 贴图读写设置
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetInfo">资源文件</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckReadWriteEnable(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null)
            {
                var isOpenReadWriter = Convert.ToBoolean(item.parameter);
                if (textureImporter.isReadable != isOpenReadWriter)
                {
                    var tips = isOpenReadWriter ? "需要强制开启" : "需要强制关闭";
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
        private static void CheckMipMaps(string assetPath, FileSystemInfo assetInfo, CheckItemInfo item, ref List<EffectCheckReportInfo> report)
        {
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null)
            {
                var isOpenMinMaps = Convert.ToBoolean(item.parameter);

                if (textureImporter.mipmapEnabled != isOpenMinMaps)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    if (isOpenMinMaps)
                    {
                        // 尺寸小于 64 的纹理不需要开启 MipMaps
                        if (asset.width > 64 || asset.height > 64)
                        {
                            var content = $"Mip Maps 配置不规范, 路径为: {assetInfo.FullName} 当前 MinMaps: {textureImporter.mipmapEnabled} >>> 需要强制开启 ({asset.width}X{asset.height})";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, content, item));
                        }
                    }
                    else
                    {
                        var content = $"Mip Maps 配置不规范, 路径为: {assetInfo.FullName} 当前 MinMaps: {textureImporter.mipmapEnabled} >>> 需要强制关闭";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.TextureMipMaps, content, item));
                    }
                }
            }
        }
    }
}