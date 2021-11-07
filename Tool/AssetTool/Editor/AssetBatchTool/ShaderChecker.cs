using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuroha.GUI.Editor;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.AssetBatchTool
{
    public static class ShaderChecker
    {
        public struct ShaderCheckerData
        {
            /// <summary>
            /// 预制体路径
            /// </summary>
            public string prefabPath;

            /// <summary>
            /// 子物体名称
            /// </summary>
            public string subObjectName;

            /// <summary>
            /// 材质球
            /// </summary>
            public Material material;
        }
        
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool foldout = true;
        
        /// <summary>
        /// 是否检查 Lever Editor 文件夹
        /// </summary>
        private static bool detectLevelEditor;
        
        /// <summary>
        /// 待检查路径
        /// </summary>
        private static string shaderPath = "Assets/ToBundle";
        
        /// <summary>
        /// 检查关键字
        /// </summary>
        private static string shaderKeyWord = "Lightweight Render Pipeline";
        
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
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            foldout = EditorGUILayout.Foldout(foldout,
                AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.MaterialShaderChecker], true);
            
            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 是否检测 LevelEditor 文件夹下的粒子系统.");
                    EditorGUI.indentLevel++;
                    GUILayout.BeginVertical("Box");
                    detectLevelEditor = EditorGUILayout.ToggleLeft("Detect LevelEditor Folder", detectLevelEditor, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("2. 输入检测的路径, 默认为: Assets/ToBundle.");
                    EditorGUI.indentLevel++;
                    GUILayout.BeginVertical("Box");
                    shaderPath = EditorGUILayout.TextField("Input Path To Detect", shaderPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("3. 请输入想要检测的 Shader 的名称关键字. 如: Lightweight");
                    EditorGUI.indentLevel++;
                    GUILayout.BeginVertical("Box");
                    shaderKeyWord = EditorGUILayout.TextField("Input Key Word", shaderKeyWord, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("4. 点击按钮, 开始检测.");
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal("Box");
                    if (GUILayout.Button("Start", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        Detect(GetMaterials(shaderPath), shaderKeyWord, false);
                    }
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 获取全部预制体中的材质球
        /// </summary>
        private static List<ShaderCheckerData> GetMaterials(string path)
        {
            var shaderCheckerDataList = new List<ShaderCheckerData>();
            
            // 获取相对目录下所有的预制体路径
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] {path});
            var assetPaths = new List<string>(guids.Select(AssetDatabase.GUIDToAssetPath));

            // 加载全部的预制体
            var prefabs = new List<GameObject>();
            if (assetPaths.Count > 0)
            {
                for (var index = 0; index < assetPaths.Count; index++)
                {
                    ProgressBar.DisplayProgressBar("特定 Shader 引用检测工具", $"加载预制体中: {index + 1}/{assetPaths.Count}", index + 1, assetPaths.Count);

                    if (detectLevelEditor)
                    {
                        prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(assetPaths[index]));
                    }
                    else if (assetPaths[index].IndexOf("LevelEditor", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(assetPaths[index]));
                    }
                }
            }
            
            // 遍历预制体, 取出其中引用的全部的材质球
            var counter = 0;
            foreach (var prefab in prefabs)
            {
                ProgressBar.DisplayProgressBar("特定 Shader 引用检测工具", $"加载材质球中: {++counter}/{prefabs.Count}", counter, prefabs.Count);
                
                var renderers = new List<Renderer>();
                renderers.AddRange(prefab.GetComponentsInChildren<Renderer>(true));
                
                foreach (var renderer in renderers)
                {
                    var material = renderer.sharedMaterial;
                    
                    if (material != null && shaderCheckerDataList.Exists(d => d.material == material) == false)
                    {
                        shaderCheckerDataList.Add(new ShaderCheckerData
                        {
                            prefabPath = AssetDatabase.GetAssetPath(prefab),
                            subObjectName = renderer.gameObject.name,
                            material = material
                        });
                    }
                }
            }
            
            return shaderCheckerDataList;
        }

        /// <summary>
        /// 检测材质球引用的 Shader
        /// </summary>
        public static List<string> Detect(in List<ShaderCheckerData> shaderCheckerDataList, string keyWord, bool isAutoCheck)
        {
            var result = new List<string>();

            foreach (var shaderCheckerData in shaderCheckerDataList)
            {
                var shader = shaderCheckerData.material.shader;
                if (shader != null)
                {
                    if (shader.name.Contains(keyWord))
                    {
                        var path = AssetDatabase.GetAssetPath(shaderCheckerData.material);
                        var log = isAutoCheck
                            ? path
                            : $"预制体 {shaderCheckerData.prefabPath} 上的子物体 {shaderCheckerData.subObjectName} 所使用的材质球 {path} 引用的 Shader 名为: {shader.name}";
                        
                        result.Add(log);
                    }
                }
            }

            if (isAutoCheck == false)
            {
                File.WriteAllLines("C:\\MaterialShaderChecker.txt", result);
            }

            return result;
        }
    }
}