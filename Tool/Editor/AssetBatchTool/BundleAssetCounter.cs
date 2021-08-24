using System;
using System.Collections.Generic;
using System.Linq;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.AssetBatchTool
{
    public static class BundleAssetCounter
    {
        /// <summary>
        /// 折叠狂
        /// </summary>
        private static bool bundleAssetCounterFoldout = true;
        
        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;
        
        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;
        
        /// <summary>
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_INPUT_AREA_WIDTH = 400;
        
        /// <summary>
        /// 每个捆绑包中资源的最大数量
        /// </summary>
        private static int bundleAssetCounterLogMaxCount = 70;
        
        /// <summary>
        /// 检测的路径
        /// </summary>
        private static string bundleAssetCounterPath = @"Assets\ToBundle\";
        
        /// <summary>
        /// 检测的资源的类型
        /// </summary>
        private static string bundleAssetCounterType = "*.prefab";
        
        /// <summary>
        /// 是否在控制台输出检测结果
        /// </summary>
        private static bool bundleAssetCounterLogSwitch;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            bundleAssetCounterFoldout = EditorGUILayout.Foldout(bundleAssetCounterFoldout, "捆绑包资源数量分析", true);
            if (bundleAssetCounterFoldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 是否在控制台输出检测结果.");
                    GUILayout.BeginVertical("Box");
                    bundleAssetCounterLogSwitch = EditorGUILayout.Toggle("Print Console", bundleAssetCounterLogSwitch, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                
                    EditorGUILayout.LabelField("2. 设定每个捆绑包中资源的最大数量, 检测到超出数量的包会输出警告.");
                    GUILayout.BeginVertical("Box");
                    bundleAssetCounterLogMaxCount = EditorGUILayout.IntField("Max Count", bundleAssetCounterLogMaxCount, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                
                    EditorGUILayout.LabelField("3. 设定检测的资源的类型.");
                    GUILayout.BeginVertical("Box");
                    bundleAssetCounterType = EditorGUILayout.TextField("Asset Type", bundleAssetCounterType, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                
                    EditorGUILayout.LabelField("4. 设定检测的路径.");
                    GUILayout.BeginVertical("Box");
                    bundleAssetCounterPath = EditorGUILayout.TextField("Asset Path", bundleAssetCounterPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                
                    EditorGUILayout.LabelField("5. 点击按钮, 开始检测.");
                    GUILayout.BeginVertical("Box");
                    UnityEngine.GUI.enabled = string.IsNullOrEmpty(bundleAssetCounterPath) == false && string.IsNullOrEmpty(bundleAssetCounterType) == false;
                    if (GUILayout.Button("Start", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        Count(bundleAssetCounterPath);
                    }
                    UnityEngine.GUI.enabled = true;
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 计算指定路径下所有最底层文件夹中资源的数量
        /// </summary>
        /// <param name="path">指定路径</param>
        private static void Count(string path)
        {
            var detectResult = new Dictionary<System.IO.DirectoryInfo, int>();
            var allDirectory = new List<System.IO.DirectoryInfo>
            {
                new System.IO.DirectoryInfo(path)
            };

            for (var index = 0; index < allDirectory.Count; index++)
            {
                // 获取路径中所有的子文件夹
                var subDir = allDirectory[index].GetDirectories();

                // 获取路径中所有的特定类型的资源
                var files = allDirectory[index].GetFiles("*.prefab", System.IO.SearchOption.TopDirectoryOnly);
                
                // Kuroha.Util.Release.DebugUtil.Log($"共检测到了 {files.Length} 个指定类型的资源.");

                // 异常
                if (subDir.Length > 0 && files.Length > 0)
                {
                    DebugUtil.ClearConsole();
                    DebugUtil.LogError($"文件夹和资源同级: {allDirectory[index].FullName}");
                    
                    // 打印路径名
                    foreach (var dir in subDir)
                    {
                        if (dir.Name.Equals(".git") == false)
                        {
                            Kuroha.Util.Release.DebugUtil.Log($"其中子目录有: {dir.FullName}");
                        }
                    }

                    // 打印文件名
                    foreach (var file in files)
                    {
                        Kuroha.Util.Release.DebugUtil.Log($"其中子文件有: {file.FullName}");
                    }

                    return;
                }

                // 中间层
                if (subDir.Length > 0 && files.Length == 0)
                {
                    allDirectory.AddRange(subDir);
                }

                // 底层
                else if (subDir.Length == 0 && files.Length > 0)
                {
                    detectResult.Add(allDirectory[index], files.Length);
                }
            }

            //输出结果
            var result = new List<string>();
            var resultList = detectResult.Keys.Where(key => key.Name.Equals(".git") == false).ToList();
            
            // Kuroha.Util.Release.DebugUtil.Log($"共有 {resultList.Count} 个存在指定资源的文件夹.");
            foreach (var key in resultList)
            {
                var dir = key.FullName.Substring(key.FullName.IndexOf("Assets", StringComparison.Ordinal));
                var log = $"路径 {dir} 下有 {detectResult[key]} 个预制体资源";
                var go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dir);
                
                if (detectResult[key] > bundleAssetCounterLogMaxCount)
                {
                    result.Add(log);
                    Kuroha.Util.Release.DebugUtil.Log($"<color=#DB4D6D>{log}</color>", go);
                }
                else if (bundleAssetCounterLogSwitch)
                {
                    Kuroha.Util.Release.DebugUtil.Log(log, go);
                }
            }

            // 导出结果
            System.IO.File.WriteAllLines("C:\\AssetCounter.txt", result);
        }
    }
}