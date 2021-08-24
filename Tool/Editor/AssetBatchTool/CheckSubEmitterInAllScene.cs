using System;
using Kuroha.GUI.Editor;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kuroha.Tool.Editor.AssetBatchTool
{
    /// <summary>
    /// 收集场景中的粒子特效, 判断是否有 sub-emitter 错误
    /// </summary>
    public static class CheckSubEmitterInAllScene
    {
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
                ProgressBar.DisplayProgressBar("检测中", $"{index + 1}/{scenePaths.Length}", index + 1, scenePaths.Length);
                
                var path = scenePaths[index];
                if (path.IndexOf("scenes/main", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                if (path.IndexOf("levelEditor", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                if (path.IndexOf("maps/editor", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                DebugUtil.Log($"当前检测的场景是: {path}");
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

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