using System;
using System.IO;
using System.Text.RegularExpressions;
using Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchGUI;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchItem
{
    /// <summary>
    /// 压缩动画片段
    /// </summary>
    public static class AnimationClipCompress
    {
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool foldout = true;
        
        /// <summary>
        /// 动画片段资源路径
        /// </summary>
        private static string path = "Assets";
        
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
        
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
            
            foldout = EditorGUILayout.Foldout(foldout, AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.CheckSubEmitterInAllScene], true);
            
            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 输入待处理动画片段的路径. 默认为 Assets 根目录.");
                    GUILayout.BeginVertical("Box");
                    path = EditorGUILayout.TextField("Input Path To Detect", path, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();

                    EditorGUILayout.LabelField("2. 点击按钮, 开始压缩.");
                    GUILayout.BeginVertical("Box");
                    UnityEngine.GUI.enabled = string.IsNullOrEmpty(path) == false;
                    if (GUILayout.Button("Start", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        Check();
                    }

                    UnityEngine.GUI.enabled = true;
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }
        
        private static void Check()
        {
            var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { path });

            var total = guids.Length;
            for (var index = 0; index < total; index++)
            {
                Kuroha.GUI.Editor.ProgressBar.DisplayProgressBar("动画片段压缩中, 可能遇到动画片段较大的情况, 请耐心等候", $"{index + 1}/{total}", index + 1, total);
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[index]);
                var extension = Path.GetExtension(assetPath);
                if (extension == ".anim")
                {
                    var filePath = Path.GetFullPath(assetPath);
                    var content = File.ReadAllText(filePath);
                    content = Regex.Replace(content, "-?[0-9]{1,10}\\.[0-9]{1,15}", Compress);
                    File.WriteAllText(filePath, content);
                }
            }
        }

        private static string Compress(Match match)
        {
            // 小数点后位数最大限制
            const int DELTA_LIMIT = 4;
            
            // 小数点的位置
            var dotIndex = match.Value.IndexOf(".", StringComparison.Ordinal);
            
            // 小数位的个数
            var delta = match.Value.Length - dotIndex;
            
            // 缩小小数位数
            var result = match.Value;
            if (delta > DELTA_LIMIT + 1)
            {
                result = result.Remove(dotIndex + DELTA_LIMIT + 1);
            }
            if (result == "0.0000" || result == "-0.0000")
            {
                result = "0";
            }
            return result;
        }
    }
}
