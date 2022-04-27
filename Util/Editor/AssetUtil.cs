using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Kuroha.Framework.GUI.Editor;
using Kuroha.Framework.Utility.RunTime;
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
        /// 设置纹理设置
        /// </summary>
        public static bool SetTextureImportSettings(string assetPath, string platformName, int size, TextureImporterFormat rgba, TextureImporterFormat rgb)
        {
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null)
            {
                var setting = new TextureImporterPlatformSettings
                {
                    name = platformName,
                    maxTextureSize = size,
                    format = textureImporter.DoesSourceTextureHaveAlpha() ? rgba : rgb
                };
                textureImporter.SetPlatformTextureSettings(setting);
                AssetDatabase.ImportAsset(assetPath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 批量删除资源
        /// 需要一个整合了待删除资源所在路径的文件
        /// 注意路径必须是 Assets 相对路径
        /// </summary>
        /// <param name="assetFilePath">包含待删除资源路径信息的文件</param>
        public static void DeleteAsset(ref string assetFilePath)
        {
            var counter = 0;
            var index = 0;

            // 待删除资源
            var toDeleteAssets = System.IO.File.ReadAllLines(assetFilePath);
            var totalCount = toDeleteAssets.Length;

            while (true)
            {
                if (ProgressBar.DisplayProgressBarCancel("正在批量删除资源", $"{index + 1}/{totalCount}", index + 1,
                    totalCount))
                {
                    DebugUtil.Log($"共成功删除了 {counter}/{totalCount} 项资源!", null, "green");
                    break;
                }

                if (index < totalCount)
                {
                    if (UnityEditor.FileUtil.DeleteFileOrDirectory(toDeleteAssets[index]))
                    {
                        ++counter;
                    }
                    else
                    {
                        DebugUtil.LogError($"删除失败: {toDeleteAssets[index]}", null, "red");
                    }

                    ++index;
                }

                if (index >= totalCount)
                {
                    break;
                }
            }

            DebugUtil.Log($"共成功删除了 {counter}/{totalCount} 项资源!", null, "green");

            assetFilePath = "已执行删除!";
        }

        /// <summary>
        /// 获取场景中全部的 Transform
        /// </summary>
        /// <param name="type">筛选规则</param>
        /// <returns></returns>
        public static List<Transform> GetAllTransformInScene(FindType type)
        {
            // 获取当前已加载的全部资源
            var allLoadTransforms = Resources.FindObjectsOfTypeAll<UnityEngine.Transform>();

            // 后续需要使用 Selection.objects 进行筛选是否为场景物体, 因此先备份 Selection.objects
            var previousSelection = Selection.objects;

            // 遍历筛选条件
            var objects = new List<UnityEngine.Object>();
            foreach (var loadTransform in allLoadTransforms)
            {
                if (loadTransform != null)
                {
                    switch (type)
                    {
                        case FindType.All:
                            objects.Add(loadTransform.gameObject);
                            break;
                        case FindType.EnableOnly:
                            if (loadTransform.gameObject.activeInHierarchy)
                            {
                                objects.Add(loadTransform.gameObject);
                            }

                            break;
                        case FindType.DisableOnly:
                            if (loadTransform.gameObject.activeInHierarchy == false)
                            {
                                objects.Add(loadTransform.gameObject);
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }
            }

            // 筛选是否是场景物体
            Selection.objects = objects.ToArray();
            var resultTransforms = Selection.GetTransforms((int)SelectionMode.Editable + SelectionMode.ExcludePrefab);

            // 还原 Selection.objects
            Selection.objects = previousSelection;

            // 返回结果
            return resultTransforms.ToList();
        }

        /// <summary>
        /// 获取场景中全部的游戏物体
        /// </summary>
        /// <param name="type">筛选规则</param>
        /// <returns></returns>
        public static List<T> GetAllComponentsInScene<T>(FindType type) where T : UnityEngine.Component
        {
            var allTransform = GetAllTransformInScene(type);

            // 返回结果
            var result = new List<T>();
            foreach (var transform in allTransform)
            {
                if (transform.TryGetComponent<T>(out var component))
                {
                    result.Add(component);
                }
            }

            return result;
        }

        /// <summary>
        /// 拷贝源文件夹到新的文件夹
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

        /// <summary>
        /// 将一个资源移动到新的目录下
        /// </summary>
        /// <param name="assetFilePath">包含待移动资源路径信息的文件</param>
        /// <param name="newFolder">目标目录</param>
        public static void MoveFileToNewFolder(ref string assetFilePath, string newFolder)
        {
            var counter = 0;
            var index = 0;

            // 待移动资源
            var toMoveAssets = System.IO.File.ReadAllLines(assetFilePath);
            var totalCount = toMoveAssets.Length;

            while (true)
            {
                if (ProgressBar.DisplayProgressBarCancel("正在批量移动资源", $"{index + 1}/{totalCount}", index + 1,
                    totalCount))
                {
                    DebugUtil.Log($"共成功移动了 {counter}/{totalCount} 项资源!", null, "green");
                    break;
                }

                if (index < totalCount)
                {
                    if (MoveAsset(toMoveAssets[index], newFolder))
                    {
                        ++counter;
                    }
                    else
                    {
                        DebugUtil.LogError($"移动失败: {toMoveAssets[index]}", null, "red");
                    }

                    index++;
                }

                if (index >= totalCount)
                {
                    break;
                }
            }

            DebugUtil.Log($"共成功移动了 {counter}/{totalCount} 项资源!", null, "green");

            assetFilePath = "已执行移动!";
        }

        /// <summary>
        /// 将一个资源移动到新的目录下
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="newFolder">目标目录</param>
        private static bool MoveAsset(string assetPath, string newFolder)
        {
            var success = false;
            var oldPath = assetPath.Replace('\\', '/');
            newFolder = newFolder.Replace('\\', '/');

            // 待移动资源
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(oldPath);
            if (asset != null)
            {
                // 目标路径
                var path = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(newFolder);
                if (path != null)
                {
                    // 待移动资源完整名称
                    var assetName = assetPath.Substring(assetPath.LastIndexOf("/", StringComparison.Ordinal));
                    var newPath = newFolder + assetName;

                    // 移动资源
                    AssetDatabase.MoveAsset(oldPath, newPath);

                    success = true;
                }
                else
                {
                    DebugUtil.Log($"目标路径为空: {newFolder}", null, "red");
                }
            }
            else
            {
                DebugUtil.Log($"资源为空: {assetPath}", null, "red");
            }

            return success;
        }
    }
}
