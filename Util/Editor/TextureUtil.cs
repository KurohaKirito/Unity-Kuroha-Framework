using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Kuroha.GUI.Editor;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Util.Editor
{
    /// <summary>
    /// 纹理工具
    /// </summary>
    public static class TextureUtil
    {
        /// <summary>
        /// 纹理数据
        /// </summary>
        public struct TextureData
        {
            public Texture asset;
            public string path;
            public string guid;
        }
        
        /// <summary>
        /// 产生一份可读写的源纹理的拷贝并返回
        /// </summary>
        /// <param name="source">源纹理</param>
        public static Texture CopyTexture(Texture source)
        {
            if (ReferenceEquals(source, null) == false)
            {
                var renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                Graphics.Blit(source, renderTexture);

                var previous = RenderTexture.active;
                RenderTexture.active = renderTexture;

                var readableText = new Texture2D(source.width, source.height);
                readableText.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                readableText.Apply();

                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTexture);

                return readableText;
            }

            return null;
        }

        /// <summary>
        /// 泊松采样, 判断是否是纯色纹理
        /// </summary>
        /// <param name="texture">待检测纹理</param>
        /// <returns>true: 纯色纹理, false: 不是纯色纹理</returns>
        public static bool IsSolidColor(Texture texture)
        {
            var currentTextureWidth = texture.width;
            var currentTextureHeight = texture.height;
            var renderTextureWidth = currentTextureWidth;
            var renderTextureHeight = currentTextureHeight;

            if (currentTextureWidth >= 128)
            {
                renderTextureWidth = currentTextureWidth / 8;
            }

            if (currentTextureHeight >= 128)
            {
                renderTextureHeight = currentTextureHeight / 8;
            }

            var renderTexture = RenderTexture.GetTemporary(renderTextureWidth, renderTextureHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            Graphics.Blit(texture, renderTexture);

            // 总采样数
            var samplePointCount = renderTextureWidth * renderTextureHeight;
            // 最大可采样数
            var sampleCountMax = samplePointCount;
            // 迭代上限, 迭代次数越多结果越准确
            var iterateMax = 1000;
            // 计数器
            var counter = 0;

            // RenderTexture 像素画到一张小图上, 之后读取像素
            RenderTexture.active = renderTexture;
            var texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            // 把像素点读取存储到数组中
            var samplePoints = new List<Vector2>();
            for (var i = 0; i < renderTextureWidth; i++)
            {
                for (var j = 0; j < renderTextureHeight; j++)
                {
                    samplePoints.Add(new Vector2(i, j));
                }
            }

            var initColor = new Color(0, 0, 0);
            while (samplePoints.Count > 0 && sampleCountMax > 0 && iterateMax-- > 0)
            {
                // 在这些点中随便选一个采样点
                var next = (int) Mathf.Lerp(0, samplePoints.Count - 1, UnityEngine.Random.value);
                var sample = samplePoints[next];
                
                // 定义 "是否找到临近分布点" 标志
                var found = false;
                
                // 迭代 30 次, 找到泊松分布点
                const int LOOP = 30;
                
                // 采样半径为 1 像素
                const float RADIUS = 1;

                for (var i = 0; i < LOOP; i++)
                {
                    // 随机周长
                    var angle = 2 * Mathf.PI * UnityEngine.Random.value;
                    var r = Mathf.Sqrt(UnityEngine.Random.value * 3 * RADIUS + RADIUS);

                    // 得到临近分布点
                    var candidate = sample + r * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                    if (candidate.x >= 0 && candidate.x <= renderTextureWidth &&
                        candidate.y >= 0 && candidate.y <= renderTextureHeight)
                    {
                        found = true;
                        samplePoints.Add(candidate);
                        counter++;

                        // 找到采样点 进行颜色采样
                        var candidateColor = texture2D.GetPixelBilinear(candidate.x, candidate.y);
                        if (sampleCountMax == samplePointCount)
                        {
                            initColor = candidateColor;
                        }
                        else
                        {
                            initColor += candidateColor;
                            if (candidateColor != initColor / counter)
                            {
                                // 如果颜色有差异, 说明不是纯色纹理, 直接返回 false
                                return false;
                            }
                        }

                        samplePointCount--;
                        break;
                    }
                }

                if (found == false)
                {
                    // 如果这个点找不到周围可用点则移出采样点列表
                    var lastIndex = samplePoints.Count - 1;
                    samplePoints[next] = samplePoints[lastIndex];
                    samplePoints.RemoveAt(lastIndex);
                }
            }

            RenderTexture.ReleaseTemporary(renderTexture);
            return true;
        }

        /// <summary>
        /// 获取场景中 MeshRenderer 和 SkinnedMeshRenderer 所引用到的全部纹理  (不包含冗余纹理)
        /// </summary>
        /// <param name="assets">返回查询到的纹理资源本身</param>
        /// <param name="assetPaths">返回查询到的纹理资源所在的 Asset 相对路径</param>
        public static void GetTexturesInScene(out List<Texture> assets, out List<string> assetPaths)
        {
            // 定义结果
            assets = new List<Texture>();
            assetPaths = new List<string>();

            // 获取全部的 Renderer
            #if UNITY_2019_2_OR_NEWER == false
                var renderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
            #else
                var renderers = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
            #endif
            
            if (renderers.IsNotNullAndEmpty())
            {
                // 遍历 Renderer
                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                    {
                        // 获取全部的材质
                        var materials = renderer.sharedMaterials;
                        if (materials.IsNotNullAndEmpty())
                        {
                            // 遍历材质
                            foreach (var material in materials)
                            {
                                if (ReferenceEquals(material, null) == false)
                                {
                                    // 获取材质引用的全部纹理
                                    GetAllTexturesInMaterial(material, out List<TextureData> textures);
                                    if (textures.Count > 0)
                                    {
                                        // 遍历纹理
                                        foreach (var data in textures)
                                        {
                                            if (ReferenceEquals(data.asset, null) == false)
                                            {
                                                assets.Add(data.asset);
                                                assetPaths.Add(data.path);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定路径中的全部纹理资源  (不包含冗余纹理)
        /// </summary>
        /// <param name="paths">指定 Assets 相对路径, 路径规则和 FindAssets 规则相同</param>
        /// <param name="assets">返回查询到的纹理资源本身</param>
        /// <param name="assetPaths">返回查询到的纹理资源所在的 Asset 相对路径</param>
        public static void GetTexturesInPath(string[] paths, out List<Texture> assets, out List<string> assetPaths)
        {
            // 定义结果
            assets = new List<Texture>();
            assetPaths = new List<string>();

            // 获取路径下全部纹理的 guids
            var guids = AssetDatabase.FindAssets("t:Texture", paths);
            if (guids.IsNotNullAndEmpty())
            {
                for (var i = 0; i < guids.Length; i++)
                {
                    ProgressBar.DisplayProgressBar("Texture", $"资源获取中: {i + 1}/{guids.Length}", i + 1, guids.Length);

                    var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var asset = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    assets.Add(asset);
                    assetPaths.Add(assetPath);
                }
            }
        }

        /// <summary>
        /// 获取一个材质球中引用的全部纹理 (不包含冗余纹理)
        /// </summary>
        /// <param name="material">指定的纹理</param>
        /// <param name="textureDataList">返回的纹理数据</param>
        public static void GetTexturesInMaterial(Material material, out List<TextureData> textureDataList)
        {
            textureDataList = new List<TextureData>();
            var depends = EditorUtility.CollectDependencies(new Object[] { material });

            foreach (var depend in depends)
            {
                if (depend is Texture asset)
                {
                    var path = AssetDatabase.GetAssetPath(asset);
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    
                    textureDataList.Add(new TextureData
                    {
                        asset = asset,
                        path = path,
                        guid = guid
                    });
                }
            }
        }
        
        /// <summary>
        /// 获取一个材质球中引用的全部纹理 (包含冗余纹理)
        /// </summary>
        /// <param name="material">指定的纹理</param>
        /// <param name="textureDataList">返回的纹理数据</param>
        public static void GetAllTexturesInMaterial(Material material, out List<TextureData> textureDataList)
        {
            textureDataList = new List<TextureData>();
            
            // 直接以文本形式逐行读取 Material 文件 (这样才能读取到的冗余的纹理引用)
            var materialPathName = Path.GetFullPath(AssetDatabase.GetAssetPath(material));
            using (var reader = new StreamReader(materialPathName))
            {
                var regex = new Regex(@"\s+guid:\s+(\w+),");
                var line = reader.ReadLine();
                while (line != null)
                {
                    // 包含纹理贴图引用的行
                    if (line.Contains("m_Texture:"))
                    {
                        // 使用正则表达式获取纹理贴图的 guid
                        var match = regex.Match(line);
                        if (match.Success)
                        {
                            var guid = match.Groups[1].Value;
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                    
                            textureDataList.Add(new TextureData
                            {
                                asset = asset,
                                path = path,
                                guid = guid
                            });
                        }
                    }

                    line = reader.ReadLine();
                }
            }
        }
    }
}