using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Util.Editor
{
    public static class AssetUtil
    {
        public enum FindType
        {
            All,
            EnableOnly,
            DisableOnly
        }

        /// <summary>
        /// 批量删除资源
        /// 需要一个整合了待删除资源所在路径的文件
        /// 注意路径必须是 Assets 相对路径
        /// </summary>
        /// <param name="assetFilePath">包含待删除资源路径信息的文件</param>
        public static void DeleteAsset(ref string assetFilePath)
        {
            var path = assetFilePath;
            var counter = 0;

            using (var file = new StreamReader(path))
            {
                var line = file.ReadLine();

                while (line.IsNotNullAndEmpty())
                {
                    if (UnityEditor.FileUtil.DeleteFileOrDirectory(line))
                    {
                        counter++;
                    }
                    else
                    {
                        DebugUtil.LogError($"删除失败: {line}");
                    }

                    line = file.ReadLine();
                }
            }

            DebugUtil.Log($"共成功删除了 {counter} 项资源!");

            if (counter > 0)
            {
                DebugUtil.Log("请保存并刷新, 让 Unity 自动删除资源对应的 meta 文件!");
            }
            
            assetFilePath = "已执行删除!";
        }

        /// <summary>
        /// 获取场景中全部的 Transform
        /// </summary>
        /// <param name="type">筛选规则</param>
        /// <returns></returns>
        public static List<Transform> GetAllTransformInScene(FindType type)
        {
            var result = new List<Transform>();
            var allTransforms = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Transform>();
            
            foreach (var transform in allTransforms)
            {
                switch (type)
                {
                    case FindType.All:
                        if (transform != null)
                        {
                            result.Add(transform);
                        }
                        break;
                    
                    case FindType.EnableOnly:
                        if (transform != null && transform.gameObject.activeSelf)
                        {
                            result.Add(transform);
                        }
                        break;
                    
                    case FindType.DisableOnly:
                        if (transform != null && transform.gameObject.activeSelf == false)
                        {
                            result.Add(transform);
                        }
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            return result;
        }
        
        /// <summary>
        /// 获取场景中全部的游戏物体
        /// </summary>
        /// <param name="type">筛选规则</param>
        /// <returns></returns>
        public static List<T> GetAllComponentsInScene<T>(FindType type) where T : UnityEngine.Component
        {
            var result = new List<T>();
            var allTransforms = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Transform>();
            
            foreach (var transform in allTransforms)
            {
                if (transform.TryGetComponent<T>(out var component))
                {
                    switch (type)
                    {
                        case FindType.All:
                            result.Add(component);
                            break;
                    
                        case FindType.EnableOnly:
                            if (component.gameObject.activeSelf)
                            {
                                result.Add(component);
                            }
                            break;
                    
                        case FindType.DisableOnly:
                            if (component.gameObject.activeSelf == false)
                            {
                                result.Add(component);
                            }
                            break;
                    
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }
            }

            return result;
        }
        
        /// <summary>
        /// 拷贝文件夹到新的文件夹
        /// </summary>
        /// <param name="sourcePath">源文件所在目录</param>
        /// <param name="savePath">保存的目标目录</param>
        /// <returns>true:拷贝成功; false:拷贝失败</returns>
        public static void CopyFolderToAssetsFolder(string sourcePath, string savePath)
        {
            var sourceDirs = new List<string>();
            var sourceFiles = new List<string>();

            sourceDirs.Add(sourcePath);
            for (var index = 0; index < sourceDirs.Count; index++)
            {
                sourceDirs.AddRange(Directory.GetDirectories(sourceDirs[index]));
                sourceFiles.AddRange(Directory.GetFiles(sourceDirs[index]));
            }

            DebugUtil.Log($"共需要 {sourceDirs.Count} 个目录, {sourceFiles.Count} 个文件");

            foreach (var sourceDir in sourceDirs)
            {
                var targetFolder = sourceDir.Replace(sourcePath, savePath);
                var newFolderName = targetFolder.Replace('\\', '/').Split('/').Last();
                var parentFolder =
                    targetFolder.Substring(targetFolder.IndexOf("Assets", StringComparison.OrdinalIgnoreCase));
                parentFolder = parentFolder.Replace('\\', '/').Replace($"/{newFolderName}", "");

                DebugUtil.Log($"创建目录: {parentFolder}, {newFolderName}");
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }

            foreach (var sourceFile in sourceFiles)
            {
                var targetFile = sourceFile.Replace(sourcePath, savePath);
                DebugUtil.Log($"创建文件: {targetFile}");

                File.Copy(sourceFile, targetFile, true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}