using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.AssetBatchTool
{
    public static class UnusedAssetCleaner
    {
        /// <summary>
        /// 支持检测的类型
        /// </summary>
        public enum UnusedAssetType {
            Model,
            Material,
            Texture
        }
        
        /// <summary>
        /// 检测的路径
        /// </summary>
        private static string unusedDetectPath;

        /// <summary>
        /// 检测的资源类型
        /// </summary>
        private static UnusedAssetType unusedAssetType = UnusedAssetType.Texture;
        
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool unusedAssetFoldout = true;
        
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
        /// 配置文件的路径
        /// </summary>
        #if Kuroha
        private static string WhiteList => $"{Application.dataPath}/Kuroha/Config/UnusedAssetWhiteList.txt";
        #else
        private static string WhiteList => $"{Application.dataPath}/Art/Effects/UnusedAssetWhiteList.txt";
        #endif

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            SetDefaultPath();
            
            GUILayout.Space (2 * UI_DEFAULT_MARGIN);

            unusedAssetFoldout = EditorGUILayout.Foldout (unusedAssetFoldout,
                AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.UnusedAssetChecker], true);
            
            if (unusedAssetFoldout)
            {
                GUILayout.Space (UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical ("Box");
                {
                    EditorGUILayout.LabelField("1. 选择检测的资源类型.");
                    GUILayout.BeginVertical ("Box");
                    unusedAssetType = (UnusedAssetType) EditorGUILayout.EnumPopup("Select Asset Type", unusedAssetType, GUILayout.Width (UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    
                    EditorGUILayout.LabelField("2. 输入待检测资源的路径.");
                    GUILayout.BeginVertical ("Box");
                    unusedDetectPath = EditorGUILayout.TextField ("Input Asset Path", unusedDetectPath, GUILayout.Width (UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();

                    EditorGUILayout.LabelField("3. 点击按钮, 开始检测.");
                    GUILayout.BeginVertical ("Box");
                    if (GUILayout.Button ("Start", GUILayout.Height (UI_BUTTON_HEIGHT), GUILayout.Width (UI_BUTTON_WIDTH)))
                    {
                        Detect(unusedAssetType, unusedDetectPath, false);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 设置默认路径
        /// </summary>
        private static void SetDefaultPath()
        {
            switch (unusedAssetType)
            {
                case UnusedAssetType.Model:
                    if (string.IsNullOrEmpty(unusedDetectPath))
                    {
                        unusedDetectPath = "Assets/Art/Effects/Models";
                    }
                    break;
                
                case UnusedAssetType.Material:
                    if (string.IsNullOrEmpty(unusedDetectPath))
                    {
                        unusedDetectPath = "Assets/Art/Effects/Materials";
                    }
                    break;
                
                case UnusedAssetType.Texture:
                    if (string.IsNullOrEmpty(unusedDetectPath))
                    {
                        unusedDetectPath = "Assets/Art/Effects/Textures";
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 检测
        /// </summary>
        public static List<string> Detect(UnusedAssetType detectType, string detectPath, bool isAutoCheck)
        {
            // 白名单
            var whiteList = File.ReadAllLines(WhiteList);
            
            // 获取目录下全部指定类型的资源的 guid
            var enumStr = Enum.GetName(typeof(UnusedAssetType), detectType);
            var typeStr = $"t:{enumStr}";
            var guids = AssetDatabase.FindAssets(typeStr, new[] {detectPath});
            
            // 调用资源查找工具
            Kuroha.Tool.Editor.AssetSearchTool.Searcher.ReferenceSearcher.Find(guids);
            
            // 取出结果
            var references = Kuroha.Tool.Editor.AssetSearchTool.Searcher.ReferenceSearcher.references;
            var keyCount = references.Keys.Count;
            
            // 整理结果
            var counter = 0;
            var resultExport = new List<string>();
            var resultAuto = new List<string>();
            
            foreach (var key in references.Keys)
            {
                if (Kuroha.GUI.Editor.ProgressBar.DisplayProgressBarCancel("无引用资源分析工具", $"正在整理结果: {++counter}/{keyCount}", counter, keyCount))
                {
                    break;
                }
                
                // 获取被引用次数
                var referenceCount = references[key].Count;
                // 获取资源的路径
                var assetPath = AssetDatabase.GUIDToAssetPath(key);
                // 统计全部的引用情况
                resultExport.Add ($"资源 {assetPath} 被引用 {references[key].Count} 次!");

                // 统计全部没有被引用的资源
                if (referenceCount <= 0)
                {
                    var isHad = false;
                    foreach (var white in whiteList)
                    {
                        // Assets/Art/Effects/Materials/Common/Noise/Noise_11_uv.mat
                        var whiteFile = assetPath.Replace('\\', '/');
                        if (white.Equals(whiteFile))
                        {
                            isHad = true;
                            break;
                        }
                    }

                    if (isHad == false)
                    {
                        resultAuto.Add(assetPath);
                    }
                }
            }
            
            // 不是自动检测时将结果导出到文件
            if (isAutoCheck == false)
            {
                File.WriteAllLines($@"C:\Result_Unused_{enumStr}_Asset.md", resultExport);
            }
            
            return resultAuto;
        }
    }
}
