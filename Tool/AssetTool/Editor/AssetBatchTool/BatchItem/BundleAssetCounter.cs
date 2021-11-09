using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuroha.GUI.Editor;
using Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchGUI;
using Kuroha.Util.Editor;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchItem
{
    public static class BundleAssetCounter
    {
        public struct BundleAssetCounterData
        {
            /// <summary>
            /// 当前文件夹路径
            /// </summary>
            public DirectoryInfo currentFolder;

            /// <summary>
            /// 资源数量
            /// </summary>
            public List<FileInfo> assets;

            /// <summary>
            /// 文件夹数量
            /// </summary>
            public List<DirectoryInfo> folders;

            /// <summary>
            /// 是否超出最大数量
            /// </summary>
            public bool isOverMaxLimit;

            /// <summary>
            /// 是否文件夹和资源同级
            /// </summary>
            public bool isFoldersAndAssets;
        }

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
        /// 折叠框
        /// </summary>
        private static bool foldout = true;
        
        /// <summary>
        /// 每个捆绑包中资源的最大数量
        /// </summary>
        private static int maxCount = 70;
        
        /// <summary>
        /// 检测的路径
        /// </summary>
        private static string folderPath = @"Assets\ToBundle\";
        
        /// <summary>
        /// 是否在控制台输出检测结果
        /// </summary>
        private static bool logSwitch = true;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            foldout = EditorGUILayout.Foldout(foldout,
                AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.BundleAssetCounter], true);
            
            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 是否在控制台输出检测结果");
                    GUILayout.BeginVertical("Box");
                    logSwitch = EditorGUILayout.Toggle("Print Console", logSwitch, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                
                    EditorGUILayout.LabelField("2. 设定每个捆绑包中资源的最大数量, 检测到超出数量的包会输出警告");
                    GUILayout.BeginVertical("Box");
                    maxCount = EditorGUILayout.IntField("Max Count", maxCount, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                
                    EditorGUILayout.LabelField("3. 设定检测的路径");
                    GUILayout.BeginVertical("Box");
                    folderPath = EditorGUILayout.TextField("Asset Path", folderPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                
                    EditorGUILayout.LabelField("4. 点击按钮开始检测");
                    GUILayout.BeginVertical("Box");
                    UnityEngine.GUI.enabled = string.IsNullOrEmpty(folderPath) == false;
                    if (GUILayout.Button("Start", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        Process(Count(folderPath, maxCount), logSwitch, true);
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
        /// <param name="max">最大数量</param>
        public static List<BundleAssetCounterData> Count(string path, int max)
        {
            // 定义结果集
            var detectResult = new List<BundleAssetCounterData>();
            
            // 检测路径队伍
            var allDirectory = new List<DirectoryInfo>();
            var fullPath = PathUtil.GetAssetPath(path);
            allDirectory.Add(new DirectoryInfo(fullPath));
            
            // 进度计数器
            var progressBar = 0;

            // 执行检测
            while (progressBar < allDirectory.Count)
            {
                ProgressBar.DisplayProgressBar("批处理工具", $"同级问题检测中: {progressBar + 1}/{allDirectory.Count}", progressBar + 1, allDirectory.Count);

                // 定义当前检测路径
                var currentPath = allDirectory[progressBar];
                
                // 获取 当前检测路径 中所有的子文件夹
                var dirs = currentPath.GetDirectories();
                
                // 获取 当前检测路径 中所有文件
                var files = currentPath.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                files = files.Where(f => f.Name.EndsWith(".meta") == false).ToArray();

                #region 分析

                // 文件夹和资源同级
                if (dirs.Length > 0 && files.Length > 0)
                {
                    detectResult.Add(new BundleAssetCounterData
                    {
                        currentFolder = currentPath,
                        assets = files.ToList(),
                        folders = dirs.ToList(),
                        isOverMaxLimit = true,
                        isFoldersAndAssets = true
                    });
                    
                    allDirectory.AddRange(dirs);
                }

                // 中间层文件夹
                else if (dirs.Length > 0 && files.Length == 0)
                {
                    detectResult.Add(new BundleAssetCounterData
                    {
                        currentFolder = currentPath,
                        assets = null,
                        folders = dirs.ToList(),
                        isOverMaxLimit = false,
                        isFoldersAndAssets = false
                    });
                    
                    allDirectory.AddRange(dirs);
                }

                // 底层文件夹
                else if (dirs.Length == 0 && files.Length > 0)
                {
                    var isOverMaxLimit = files.Length > max;
                    
                    detectResult.Add(new BundleAssetCounterData
                    {
                        currentFolder = currentPath,
                        assets = files.ToList(),
                        folders = null,
                        isOverMaxLimit = isOverMaxLimit,
                        isFoldersAndAssets = false
                    });
                }

                #endregion
                
                progressBar++;
            }

            return detectResult;
        }

        /// <summary>
        /// 提炼整理检测结果
        /// </summary>
        /// <param name="detectResult">未经处理的检测结果</param>
        /// <param name="isConsole">是否输出到控制台</param>
        /// <param name="isExportFile">是否将结果导出文件</param>
        private static void Process(in List<BundleAssetCounterData> detectResult, bool isConsole, bool isExportFile)
        {
            // 删除掉名为 .git 的文件夹
            var results = detectResult.Where(t => t.currentFolder.Name.Equals(".git") == false).ToList();
            
            // 输出结果
            var logList = new List<string>();
            foreach (var result in results)
            {
                var dir = PathUtil.GetAssetPath(result.currentFolder.FullName);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(dir);
                
                if (result.isFoldersAndAssets)
                {
                    var log = $"路径 {dir} 下同时存在文件夹和资源, 文件夹有 {result.folders.Count} 个, 文件有 {result.assets.Count} 个!";
                    
                    if (isExportFile)
                    {
                        logList.Add(log);
                    }
                    
                    if (isConsole)
                    {
                        DebugUtil.LogError(log, obj);
                    }
                }
                else if (result.isOverMaxLimit)
                {
                    var log = $"路径 {dir} 下有 {result.assets.Count} 个资源, 超出最大限制!";
                    
                    if (isExportFile)
                    {
                        logList.Add(log);
                    }
                    
                    if (isConsole)
                    {
                        DebugUtil.Log($"<color=#DB4D6D>{log}</color>", obj);
                    }
                }
                else if (result.assets != null)
                {
                    var log = $"路径 {dir} 下有 {result.assets.Count} 个资源!";
                    
                    if (isExportFile)
                    {
                        logList.Add(log);
                    }
                    
                    if (isConsole)
                    {
                        DebugUtil.Log(log, obj);
                    }
                }
            }

            // 导出结果到文件
            if (isExportFile)
            {
                File.WriteAllLines("C:\\AssetCounter.txt", logList);
            }
        }
    }
}