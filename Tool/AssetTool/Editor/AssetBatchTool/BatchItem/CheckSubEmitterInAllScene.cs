using System;
using Kuroha.GUI.Editor;
using Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchGUI;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchItem
{
    /// <summary>
    /// 收集场景中的粒子特效, 判断是否有 sub-emitter 错误
    /// </summary>
    public static class CheckSubEmitterInAllScene
    {
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool foldout = true;
        
        /// <summary>
        /// 枪械路径
        /// </summary>
        private static string path = "ToBundle/Skin/Items";
        
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
            
            foldout = EditorGUILayout.Foldout(foldout,
                AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.CheckSubEmitterInAllScene], true);
            
            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 输入待检测场景的路径. 默认为 Assets 根目录.");
                    GUILayout.BeginVertical("Box");
                    path = EditorGUILayout.TextField("Input Path To Detect", path, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();

                    EditorGUILayout.LabelField("2. 点击按钮, 开始检测.");
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
            // 获得所有场景的资源路径
            var scenes = AssetDatabase.FindAssets("t:Scene");
            var scenePaths = new string[scenes.Length];
            for (var index = 0; index < scenes.Length; index++)
            {
                scenePaths[index] = AssetDatabase.GUIDToAssetPath(scenes[index]);
            }

            // 遍历场景
            for (var index = 0; index < scenePaths.Length; index++)
            {
                ProgressBar.DisplayProgressBar("批处理工具", $"Sub-Emitter 检测中: {index + 1}/{scenePaths.Length}", index + 1, scenePaths.Length);
                
                var scenePath = scenePaths[index];
                if (scenePath.IndexOf("scenes/main", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                if (scenePath.IndexOf("levelEditor", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                if (scenePath.IndexOf("maps/editor", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                DebugUtil.Log($"当前检测的场景是: {scenePath}");
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                var rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    var particles = root.GetComponentsInChildren<ParticleSystem>();
                    foreach (var particle in particles)
                    {
                        if (particle.subEmitters.enabled)
                        {
                            var subEmittersCount = particle.subEmitters.subEmittersCount;
                            DebugUtil.LogError($"场景: {scene.name}, 根物体: {root.name}, 粒子系统: {particle.name} 启用了 {subEmittersCount} 个 Sub-Emitter");

                            if (subEmittersCount <= 0)
                            {
                                continue;
                            }

                            for (var i = 0; i < subEmittersCount; i++)
                            {
                                // 获取所有的子粒子系统
                                var allSubParticleSystems = particle.GetComponentsInChildren<ParticleSystem>(true);
                                // 获取 SubEmitterSystem 设置
                                var setting = particle.subEmitters.GetSubEmitterSystem(i);

                                var isError = true;
                                if (ReferenceEquals(setting, null) == false)
                                {
                                    foreach (var subParticleSystem in allSubParticleSystems)
                                    {
                                        if (setting == subParticleSystem)
                                        {
                                            isError = false;
                                        }
                                    }
                                }

                                if (isError == false)
                                {
                                    continue;
                                }

                                DebugUtil.LogError($"Sub-EmittersError: 场景: {scene.name}, 根物体: {root.name}, 子物体: {particle.gameObject.name}");
                            }
                        }
                    }
                }

                EditorSceneManager.CloseScene(scene, false);
            }
        }
    }
}