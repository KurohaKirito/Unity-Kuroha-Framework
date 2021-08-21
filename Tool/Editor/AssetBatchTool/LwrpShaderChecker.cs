using System;
using System.Collections.Generic;
using System.Linq;
using Kuroha.GUI.Editor;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.AssetBatchTool
{
    public static class LwrpShaderChecker
    {
        private static bool lwrpShaderFoldout = true;
        private static bool lwrpShaderToggleAutoRepair;
        private static bool lwrpShaderToggleDetectEnable;
        private static bool lwrpShaderToggleDetectLevelEditor;
        private static string lwrpShaderPath = "Assets/ToBundle";
        private static string lwrpShaderKeyWord = "Alpha Blended Pre"; // Lightweight
        
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

            lwrpShaderFoldout = EditorGUILayout.Foldout(lwrpShaderFoldout, "粒子系统 LWRP 材质球引用检测工具", true);
            if (lwrpShaderFoldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 是否自动清除粒子系统 Renderer 组件引用的 LWRP 材质.");
                    UnityEditor.EditorGUI.indentLevel++;
                    GUILayout.BeginVertical("Box");
                    lwrpShaderToggleAutoRepair = EditorGUILayout.ToggleLeft("Auto Clear LWRP Shader", lwrpShaderToggleAutoRepair, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    UnityEditor.EditorGUI.indentLevel--;
                    
                    EditorGUILayout.LabelField("2. 是否检测启用了 Renderer 的粒子系统.");
                    UnityEditor.EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("注: 此部分粒子系统在清空材质后出现的材质丢失问题, 请谨慎选择.");
                    UnityEditor.EditorGUI.indentLevel--;
                    UnityEditor.EditorGUI.indentLevel++;
                    GUILayout.BeginVertical("Box");
                    lwrpShaderToggleDetectEnable = EditorGUILayout.ToggleLeft("Detect Enabled Renderer", lwrpShaderToggleDetectEnable, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    UnityEditor.EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("3. 是否检测 LevelEditor 文件夹下的粒子系统.");
                    UnityEditor.EditorGUI.indentLevel++;
                    GUILayout.BeginVertical("Box");
                    lwrpShaderToggleDetectLevelEditor = EditorGUILayout.ToggleLeft("Detect LevelEditor Folder", lwrpShaderToggleDetectLevelEditor, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    UnityEditor.EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("4. 请输入检测的路径, 默认为: Assets/ToBundle.");
                    UnityEditor.EditorGUI.indentLevel++;
                    GUILayout.BeginVertical("Box");
                    lwrpShaderPath = EditorGUILayout.TextField("Input Path To Detect", lwrpShaderPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    UnityEditor.EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("5. 请输入想要检测的 LWRP Shader 的名称关键字. 如: Lightweight");
                    UnityEditor.EditorGUI.indentLevel++;
                    GUILayout.BeginVertical("Box");
                    lwrpShaderKeyWord = EditorGUILayout.TextField("Input Key Word", lwrpShaderKeyWord, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    UnityEditor.EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("6. 点击按钮, 开始检测.");
                    UnityEditor.EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal("Box");
                    if (GUILayout.Button("Start", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        DetectParticleSystemShader();
                    }
                    GUILayout.EndHorizontal();
                    UnityEditor.EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 检测粒子特效的 Shader
        /// </summary>
        private static void DetectParticleSystemShader()
        {
            // 获取相对目录下所有的预制体
            var guids = AssetDatabase.FindAssets("t:Prefab", new []{lwrpShaderPath});
            var assetPaths = new List<string>(guids.Select(AssetDatabase.GUIDToAssetPath));

            if (assetPaths.Count > 0)
            {
                Kuroha.Util.Release.DebugUtil.Log($"共找到了 {assetPaths.Count} 个资源!");

                var counter = 0;
                var repair = new List<string>();
                var result = new List<string>();
                var particleSystemObjList = new List<string>();
                var meshRendererObjList = new List<string>();
                var prefabs = new List<GameObject>();

                #region 读取预制体

                for (var index = 0; index < assetPaths.Count; index++)
                {
                    ProgressBar.DisplayProgressBar("读取资源中", $"{index + 1}/{assetPaths.Count}", (index + 1), assetPaths.Count);
                    if (lwrpShaderToggleDetectLevelEditor)
                    {
                        prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(assetPaths[index]));
                    }
                    else if (assetPaths[index].IndexOf("LevelEditor", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(assetPaths[index]));
                    }
                }

                Kuroha.Util.Release.DebugUtil.Log($"Prefabs: 共读取了 {prefabs.Count} 个预制体");

                #endregion

                // 遍历预制体
                foreach (var prefab in prefabs)
                {
                    if (ProgressBar.DisplayProgressBarCancel("检测中", $"{++counter}/{prefabs.Count}", counter, prefabs.Count))
                    {
                        return;
                    }

                    Material material;
                    Shader shader;

                    #region Particle System

                    foreach (var particleSystem in prefab.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        var renderer = particleSystem.GetComponent<Renderer>();
                        if (renderer == null)
                        {
                            continue;
                        }

                        material = renderer.sharedMaterial;
                        if (material == null)
                        {
                            continue;
                        }

                        shader = material.shader;
                        if (shader == null)
                        {
                            continue;
                        }

                        if (!shader.name.Contains(lwrpShaderKeyWord))
                        {
                            continue;
                        }

                        // 新增问题
                        if (!particleSystemObjList.Exists(str => str == prefab.name))
                        {
                            particleSystemObjList.Add(prefab.name);
                        }

                        result.Add($"ParticleSystem: 预制体 {AssetDatabase.GetAssetPath(prefab)} 中的子物体 {particleSystem.name} 引用的 Shader 名为: {shader.name}");

                        // 自动修复
                        if (!lwrpShaderToggleAutoRepair)
                        {
                            continue;
                        }

                        if (!repair.Exists(str => str == prefab.name))
                        {
                            repair.Add(prefab.name);
                        }

                        renderer.sharedMaterial = null;
                        EditorUtility.SetDirty(prefab);
                        AssetDatabase.SaveAssets();
                    }

                    #endregion

                    #region Mesh Renderer

                    foreach (var meshRenderer in prefab.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        if (!lwrpShaderToggleDetectEnable && meshRenderer.enabled)
                        {
                            continue;
                        }

                        material = meshRenderer.sharedMaterial;
                        if (material == null)
                        {
                            continue;
                        }

                        shader = material.shader;
                        if (shader == null)
                        {
                            continue;
                        }

                        if (!shader.name.Contains(lwrpShaderKeyWord))
                        {
                            continue;
                        }

                        // 新增问题
                        if (!meshRendererObjList.Exists(str => str == prefab.name))
                        {
                            meshRendererObjList.Add(prefab.name);
                        }

                        result.Add($"MeshRenderer: 预制体 {AssetDatabase.GetAssetPath(prefab)} 中的子物体 {meshRenderer.name} 引用的 Shader 名为: {shader.name}");

                        // 自动修复
                        if (!lwrpShaderToggleAutoRepair)
                        {
                            continue;
                        }

                        if (!repair.Exists(str => str == prefab.name))
                        {
                            repair.Add(prefab.name);
                        }

                        meshRenderer.sharedMaterial = null;
                        EditorUtility.SetDirty(prefab);
                        AssetDatabase.SaveAssets();
                    }

                    #endregion
                }

                Kuroha.Util.Release.DebugUtil.Log($"Result: 共检测出了 {result.Count} 个问题");
                Kuroha.Util.Release.DebugUtil.Log($"ParticleSystem: 共检测出了 {particleSystemObjList.Count} 个存在问题的游戏物体");
                Kuroha.Util.Release.DebugUtil.Log($"MeshRenderer: 共检测出了 {meshRendererObjList.Count} 个存在问题的游戏物体");
                Kuroha.Util.Release.DebugUtil.Log($"RepairCounter: 共修复了 {repair.Count} 个问题");

                AssetDatabase.Refresh();

                //依次输出所有结果
                System.IO.File.WriteAllLines("C:\\LWRPShader.txt", result);
                EditorUtility.DisplayDialog("检测结束", $"共找到了 {result.Count} 个问题!\n请在 C 盘根目录下的\nResult_LWRPShader.md\n文件中查看检测结果!", "OK");
            }
        }
    }
}