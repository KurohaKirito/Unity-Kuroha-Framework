using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.AssetCheckTool;
using Kuroha.Tool.Editor.ModelAnalysisTool;
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
        private static GameObject lobby;
        
        /// <summary>
        /// 玩家游戏物体
        /// </summary>
        private static Transform player;

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
                lobby = GameObject.Find("LobbyScreen");
                if (lobby == null)
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
                                player = lobby.transform.Find("Players/Player1");
                            }
                            EditorGUILayout.ObjectField("玩家游戏物体: Player1", player, typeof(Transform), true);
                            
                            DrawButton("1. 模型检测: 统计整套时装所用到的模型的面数和顶点数", "Start", CollectMeshVertsAndTris);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("2. 贴图检测: 统计整套时装所用到的全部贴图的尺寸", "Start", CollectMeshVertsAndTris);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("3. 动画检测: ", "Start", CollectMeshVertsAndTris);
                            GUILayout.Space(UI_SPACE_PIXELS);
                            DrawButton("4. 特效检测: ", "Start", CollectMeshVertsAndTris);
                        }
                        GUILayout.EndVertical();
                    }
                }
            }
        }

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

        private static void CollectMeshVertsAndTris()
        {
            var role = player.transform.Find("RoleBox/Model/Role");
            ModelAnalysisTableWindow.Open(false, role.gameObject, false);
        }
    }
}