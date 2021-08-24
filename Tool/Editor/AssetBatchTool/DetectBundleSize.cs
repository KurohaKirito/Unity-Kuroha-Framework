using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuroha.Util.Release;
using UnityEditor;

namespace Kuroha.Tool.Editor.AssetBatchTool
{
    public class DetectBundleSize
    {
        private static void Detect()
        {
            const string PATH = @"C:\Workspace\Sausage\Assets\Art\Effects\Textures\";
            var detectResult = new Dictionary<DirectoryInfo, int>();
            var allDirectory = new List<DirectoryInfo>
            {
                new DirectoryInfo(PATH)
            };

            for (var index = 0; index < allDirectory.Count; index++)
            {
                // 获取路径中第一层中的子文件夹
                var subDir = allDirectory[index].GetDirectories();

                // 获取路径中第一层中的文件
                var files = allDirectory[index].GetFiles("*.*", SearchOption.TopDirectoryOnly).ToList();
                for (var i = files.Count - 1; i >= 0; i--)
                {
                    if (files[i].FullName.IndexOf(".meta", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        files.RemoveAt(i);
                    }
                }

                if (subDir.Length > 0 && files.Count > 0)
                {
                    // 资源与文件夹同级
                    DebugUtil.LogError($"资源与文件夹同级, {allDirectory[index].FullName}");
                    allDirectory.AddRange(subDir);
                }
                else if (subDir.Length == 0 && files.Count == 0)
                {
                    // 空文件夹
                    DebugUtil.LogError($"空文件夹, {allDirectory[index].FullName}");
                }
                else if (subDir.Length > 0 && files.Count == 0)
                {
                    // 中间层文件夹
                    allDirectory.AddRange(subDir);
                }
                else if (subDir.Length == 0 && files.Count > 0)
                {
                    // 底层文件夹
                    detectResult.Add(allDirectory[index], files.Count);
                }
            }

            //输出结果
            var result = new List<string>();
            foreach (var key in detectResult.Keys.Where(key => !key.Name.Equals(".git")))
            {
                var dir = key.FullName.Substring(key.FullName.IndexOf("Assets", StringComparison.Ordinal));
                var log = $"路径 {dir} 下有 {detectResult[key]} 个预制体资源";
                result.Add(log);

                var go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dir);
                DebugUtil.Log(log, go);
            }

            // 导出结果
            File.WriteAllLines("C:\\AssetCounter.txt", result);
        }
    }
}