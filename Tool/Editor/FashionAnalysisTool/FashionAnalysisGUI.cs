using System;
using UnityEditor;
using UnityEngine;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Tool.Editor.AssetCheckTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.SceneAnalysisTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.TextureAnalysisTool;
using Script.Effect.Editor.AssetTool.Util.Editor;
using UnityEngine.Rendering;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.FashionAnalysisTool {
    /// <summary>
    /// GUI 绘制类
    /// </summary>
    public class FashionAnalysisGUI : UnityEditor.Editor {
        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_SPACE_PIXELS = 5;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;

        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;

        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool fashionAnalysisFoldout = true;

        /// <summary>
        /// 大厅
        /// </summary>
        private static Transform players;

        /// <summary>
        /// 玩家游戏物体
        /// </summary>
        private static Transform player;

        /// <summary>
        /// 角色游戏物体
        /// </summary>
        private static Transform role;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI(AssetCheckToolWindow window) {
            if (EditorApplication.isPlaying == false) {
                Dialog.SetListener(window.ResetToolBarIndex);
                Dialog.Display("请先运行游戏", Dialog.DialogType.Message, "OK");
            } else {
                if (players == null) {
                    var transforms = AssetUtil.GetAllTransformInScene(AssetUtil.FindType.All);
                    foreach (var transform in transforms) {
                        if (transform.name == "Players") {
                            if (transform.parent.name.IndexOf("LobbyScreen", StringComparison.Ordinal) >= 0) {
                                players = transform;
                                break;
                            }
                        }
                    }
                }

                if (players == null) {
                    Dialog.SetListener(window.ResetToolBarIndex);
                    Dialog.Display("请先登录进入大厅", Dialog.DialogType.Message, "OK");
                } else {
                    GUILayout.Space(2 * UI_SPACE_PIXELS);

                    fashionAnalysisFoldout = EditorGUILayout.Foldout(fashionAnalysisFoldout, "时装分析工具", true);
                    if (fashionAnalysisFoldout) {
                        GUILayout.Space(UI_SPACE_PIXELS);
                        GUILayout.BeginVertical("Box");
                        {
                            GUILayout.Space(UI_SPACE_PIXELS);
                            if (player == null) {
                                player = players.Find("Player1");
                            } else if (role == null) {
                                role = player.transform.Find("UIRolePoint1/Role");
                            }

                            EditorGUILayout.ObjectField("玩家游戏物体: Player1", player, typeof(Transform), true);

                            DrawButton("1. 模型检测: 统计整套时装所用到的模型的面数和顶点数", "Start", CollectMesh);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("2. 贴图检测: 统计整套时装所用到的全部贴图的尺寸", "Start", CollectTextures);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("3. 动画检测: 检测时装中全部动画状态机的剔除模式, 在 Console 窗口查看检测结果", "Start", CheckAnimator);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("4. 隐藏物体检测: ", "Start", CheckDisableObject);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("5. 粒子系统检测: ", "Start", CheckParticleSystem);
                            GUILayout.Space(UI_SPACE_PIXELS);
                        }
                        GUILayout.EndVertical();
                    }
                }
            }
        }

        /// <summary>
        /// 绘制按钮
        /// </summary>
        /// <param name="label"></param>
        /// <param name="button"></param>
        /// <param name="action"></param>
        private static void DrawButton(string label, string button, Action action) {
            GUILayout.Label(label);
            GUILayout.BeginVertical("Box");
            {
                if (GUILayout.Button(button, GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                    action?.Invoke();
                }
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 统计分析 Mesh
        /// </summary>
        private static void CollectMesh() {
            SceneAnalysisTableWindow.Open(false, role.gameObject, false);
        }

        /// <summary>
        /// 统计分析 Textures
        /// </summary>
        private static void CollectTextures() {
            TextureAnalysisTableWindow.Open(TextureAnalysisData.DetectType.GameObject, TextureAnalysisData.DetectTypeAtPath.Prefabs, null, role.gameObject);
        }

        /// <summary>
        /// 统计分析动画状态机
        /// </summary>
        private static void CheckAnimator() {
            var hadError = false;
            var animators = role.gameObject.GetComponentsInChildren<Animator>(true);

            foreach (var animator in animators) {
                if (animator.cullingMode != AnimatorCullingMode.CullCompletely) {
                    if (animator.transform.name != "Role") {
                        hadError = true;
                        var content1 = $"游戏物体 {animator.transform.name} 的动画剔除方式不正确!";
                        var content2 = $"<color='red'>{animator.cullingMode}</color> => <color='green'>{AnimatorCullingMode.CullCompletely}</color>";
                        DebugUtil.LogError($"{content1}\n{content2}", animator.gameObject);
                    }
                }
            }

            if (hadError == false) {
                DebugUtil.Log($"动画状态机检测完毕, 共检测了 {animators.Length} 个动画状态机, 未检测到问题", null, "green");
            }
        }

        /// <summary>
        /// 检测是否有隐藏游戏物体
        /// </summary>
        private static void CheckDisableObject() {
            var hadError = false;
            var transforms = role.gameObject.GetComponentsInChildren<Transform>(true);
            foreach (var transform in transforms) {
                if (transform.gameObject.activeSelf == false) {
                    hadError = true;
                    DebugUtil.LogError($"游戏物体 {transform.name} 为隐藏状态!", transform.gameObject, "red");
                }
            }

            if (hadError == false) {
                DebugUtil.Log($"隐藏游戏物体检测完毕, 共检测了 {transforms.Length} 个游戏物体, 未检测到问题.", null, "green");
            }
        }

        /// <summary>
        /// 统计分析粒子系统
        /// </summary>
        private static void CheckParticleSystem() {
            var hadError = false;
            var particleSystems = role.gameObject.GetComponentsInChildren<ParticleSystem>(true);

            foreach (var particleSystem in particleSystems) {
                var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();

                // 是否是 Mesh 粒子
                if (renderer.renderMode == ParticleSystemRenderMode.Mesh) {
                    var mesh = renderer.mesh;
                    if (mesh == null) {
                        DebugUtil.LogError($"特效 {particleSystem.transform.name} 使用了 Mesh 粒子但是没有指定 Mesh!", particleSystem.gameObject, "red");
                    } else if (mesh.triangles.Length >= 900) {
                        DebugUtil.LogError($"特效 {particleSystem.transform.name} 使用的 Mesh 粒子面数大于 300!", particleSystem.gameObject, "red");
                    }
                }

                // 预热是否关闭
                if (particleSystem.main.prewarm) {
                    hadError = true;
                    DebugUtil.LogError($"特效 {particleSystem.transform.name} 未关闭预热!", particleSystem.gameObject, "red");
                }

                // 阴影是否关闭
                if (renderer.shadowCastingMode != ShadowCastingMode.Off) {
                    hadError = true;
                    DebugUtil.LogError($"特效 {particleSystem.transform.name} 未关闭阴影投射!", particleSystem.gameObject, "red");
                }

                // 是否开启了碰撞器
                if (particleSystem.collision.enabled) {
                    hadError = true;
                    DebugUtil.LogError($"特效 {particleSystem.transform.name} 未关闭碰撞!", particleSystem.gameObject, "red");
                }

                // 是否开启了触发器
                if (particleSystem.trigger.enabled) {
                    hadError = true;
                    DebugUtil.LogError($"特效 {particleSystem.transform.name} 未关闭触发器!", particleSystem.gameObject, "red");
                }
            }

            if (hadError == false) {
                DebugUtil.Log($"粒子系统检测完毕, 共检测了 {particleSystems.Length} 个粒子系统, 未检测到问题.", null, "green");
            }
        }
    }
}