using System;
using System.Collections.Generic;
using Kuroha.Framework.Utility.RunTime;
using Kuroha.Util.Editor;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.TextureAnalysisTool.Editor
{
    public static class TextureRepeatChecker
    {
        public struct RepeatTextureInfo
        {
            public string id;
            public List<string> assetPaths;
        }

        private static readonly List<RepeatTextureInfo> repeatTextureList = new List<RepeatTextureInfo>();
        private static readonly Dictionary<string, string> base64Dictionary = new Dictionary<string, string>();

        /// <summary>
        /// 重复纹理检测
        /// </summary>
        /// <param name="assetPathA">纹理路径</param>
        /// <param name="isBegin">是否是第一次检测</param>
        public static void CheckOneTexture(string assetPathA, bool isBegin)
        {
            if (isBegin)
            {
                repeatTextureList.Clear();
                base64Dictionary.Clear();
            }

            TextureCompareBase64(assetPathA);
        }

        /// <summary>
        /// 将图片转换为 Base64 后, 匹配 Base64, 判断图片是否相同
        /// </summary>
        /// <param name="assetPathA">文件 A 的资源路径</param>
        private static void TextureCompareBase64(string assetPathA)
        {
            var textureA = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPathA);
            textureA = TextureUtil.CopyTexture(textureA) as Texture2D;
            if (textureA == null)
            {
                DebugUtil.LogWarning("图片导入格式不是 2D 纹理, 请检查!", AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPathA));
                return;
            }

            // 转换为 PNG 或 TGA (EncodeToPNG 和 EncodeToTGA 两者选其一, 均可实现效果)
            var bytesA = textureA.EncodeToPNG();
            var baserA = Convert.ToBase64String(bytesA);
            AddResultBase64(baserA, assetPathA);
        }

        /// <summary>
        /// 添加检测结果
        /// </summary>
        /// <param name="baserA">文件的 Base64 字符串</param>
        /// <param name="assetPathA">文件的资源路径</param>
        private static void AddResultBase64(string baserA, string assetPathA)
        {
            if (base64Dictionary.TryGetValue(baserA, out var assetPathB))
            {
                if (assetPathB != assetPathA)
                {
                    // 需要添加一个重复, 先判断这个重复是否已经存在
                    var infoIsHad = false;
                    var infoHadIndex = 0;

                    for (var i = 0; i < repeatTextureList.Count; i++)
                    {
                        if (repeatTextureList[i].assetPaths == null)
                        {
                            continue;
                        }

                        foreach (var assetPath in repeatTextureList[i].assetPaths)
                        {
                            if (assetPath == assetPathB)
                            {
                                infoIsHad = true;
                                infoHadIndex = i;
                            }
                        }
                    }

                    // 如果这一组重复已经存在, 则直接添加此纹理
                    if (infoIsHad)
                    {
                        repeatTextureList[infoHadIndex].assetPaths.Add(assetPathA);
                    }
                    // 如果这一组重复不存在, 则新建一组重复
                    else
                    {
                        var id = repeatTextureList.Count + 1;
                        repeatTextureList.Add(new RepeatTextureInfo
                        {
                            id = id.ToString(),
                            assetPaths = new List<string> { assetPathA, assetPathB }
                        });
                    }
                }
            }
            else
            {
                base64Dictionary[baserA] = assetPathA;
            }
        }

        /// <summary>
        /// 获取检测结果
        /// </summary>
        /// <returns></returns>
        public static List<RepeatTextureInfo> GetResult()
        {
            return repeatTextureList;
        }
    }
}
