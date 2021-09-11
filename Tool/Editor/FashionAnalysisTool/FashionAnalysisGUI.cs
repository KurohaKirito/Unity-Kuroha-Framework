using System;
using UnityEditor;
using UnityEngine;

using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.AssetCheckTool;
using Kuroha.Tool.Editor.ModelAnalysisTool;
using Kuroha.Tool.Editor.TextureAnalysisTool;
using Kuroha.Util.Editor;
using Kuroha.Util.Release;

namespace Kuroha.Tool.Editor.FashionAnalysisTool
{
    /// <summary>
    /// GUI 绘制类
    /// </summary>
    public class FashionAnalysisGUI : UnityEditor.Editor
    {
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
        public static void OnGUI(AssetCheckToolWindow window)
        {
            if (EditorApplication.isPlaying == false)
            {
                Kuroha.GUI.Editor.Dialog.SetListener(window.ResetToolBarIndex);
                Kuroha.GUI.Editor.Dialog.Display("请先运行游戏", Dialog.DialogType.Message, "OK");
            }
            else
            {
                if (players == null) {
                    var transforms = AssetUtil.GetAllTransformInScene(AssetUtil.FindType.All);
                    foreach (var transform in transforms)
                    {
                        if (transform.name == "Players")
                        {
                            if (transform.parent.name.IndexOf("LobbyScreen", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                players = transform;
                                break;
                            }
                        }
                    }
                }

                if (players == null)
                {
                    Kuroha.GUI.Editor.Dialog.SetListener(window.ResetToolBarIndex);
                    Kuroha.GUI.Editor.Dialog.Display("请先登录进入大厅", Dialog.DialogType.Message, "OK");
                }
                else
                {
                    GUILayout.Space(2 * UI_SPACE_PIXELS);

                    fashionAnalysisFoldout = EditorGUILayout.Foldout(fashionAnalysisFoldout, "时装分析工具", true);
                    if (fashionAnalysisFoldout)
                    {
                        GUILayout.Space(UI_SPACE_PIXELS);
                        GUILayout.BeginVertical("Box");
                        {
                            GUILayout.Space(UI_SPACE_PIXELS);
                            if (player == null) {
                                player = players.Find("Player1");
                            }
                            else if (role == null) {
                                role = player.transform.Find("RoleBox/Model/Role");
                            }
                            EditorGUILayout.ObjectField("玩家游戏物体: Player1", player, typeof(Transform), true);
                            
                            DrawButton("1. 模型检测: 统计整套时装所用到的模型的面数和顶点数", "Start", CollectMesh);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("2. 贴图检测: 统计整套时装所用到的全部贴图的尺寸", "Start", CollectTextures);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("3. 动画检测: 检测时装中全部动画状态机的剔除模式, 在 Console 窗口查看检测结果", "Start", CollectAnimator);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("4. 特效检测: ", "Start", null);
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
        private static void DrawButton(string label, string button, Action action)
        {
            GUILayout.Label(label);
            GUILayout.BeginVertical("Box");
            {
                if (GUILayout.Button(button, GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                {
                    action?.Invoke();
                }
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 统计分析 Mesh
        /// </summary>
        private static void CollectMesh()
        {
            ModelAnalysisTableWindow.Open(false, role.gameObject, false);
        }

        /// <summary>
        /// 统计分析 Textures
        /// </summary>
        private static void CollectTextures()
        {
            TextureAnalysisTableWindow.Open(TextureAnalysisData.DetectType.GameObject, null, role.gameObject);
        }

        /// <summary>
        /// 统计分析动画状态机
        /// </summary>
        private static void CollectAnimator()
        {
            var hadError = false;
            var animators = role.gameObject.GetComponentsInChildren<Animator>(true);

            foreach (var animator in animators)
            {
                if (animator.cullingMode != AnimatorCullingMode.CullCompletely)
                {
                    if (animator.transform.name != "Role")
                    {
                        hadError = true;
                        var content1 = $"游戏物体 {animator.transform.name} 的动画剔除方式不正确!";
                        var content2 = $"<color='red'>{animator.cullingMode}</color> => <color='green'>{AnimatorCullingMode.CullCompletely}</color>";
                        DebugUtil.LogError($"{content1}\n{content2}", animator.gameObject);
                    }
                }
            }

            if (hadError == false)
            {
                DebugUtil.Log("<color='green'>动画状态机检测完毕, 未检测到问题.</color>");
            }
        }
    }
}
