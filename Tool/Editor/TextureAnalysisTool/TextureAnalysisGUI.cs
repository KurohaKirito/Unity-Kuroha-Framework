using System;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.TextureAnalysisTool {
    /// <summary>
    /// GUI 绘制类
    /// </summary>
    public class TextureAnalysisGUI : UnityEditor.Editor {
        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_INPUT_AREA_WIDTH = 650;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;

        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;

        /// <summary>
        /// 待检测目录
        /// </summary>
        private static string detectPath = "Assets/Art/Effects/Textures";

        /// <summary>
        /// 待检测游戏物体
        /// </summary>
        private static GameObject detectGameObject;

        /// <summary>
        /// 检测类型
        /// </summary>
        private static TextureAnalysisData.DetectType detectType = TextureAnalysisData.DetectType.Path;

        /// <summary>
        /// 检测类型
        /// </summary>
        private static TextureAnalysisData.DetectTypeAtPath detectTypeAtPath = TextureAnalysisData.DetectTypeAtPath.Prefabs;

        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool textureAnalysisFoldout = true;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI() {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            textureAnalysisFoldout = EditorGUILayout.Foldout(textureAnalysisFoldout, "纹理统计分析工具", true);
            if (textureAnalysisFoldout) {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 当前支持 4 种检查类型");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("(1) Path:        \t检查指定目录, 目录必须是 Assets/ 的相对目录.");
                    EditorGUILayout.LabelField("    Ⅰ  Textures:\t检查指定目录下的全部纹理.");
                    EditorGUILayout.LabelField("    Ⅱ  Prefabs: \t检查指定目录下的全部预制体所引用的纹理.");
                    EditorGUILayout.LabelField("(2) Scene:       \t检查当前场景中所引用的纹理.");
                    EditorGUILayout.LabelField("(3) Game Object: \t检查指定游戏物体中所引用的纹理.");
                    EditorGUI.indentLevel--;

                    GUILayout.BeginVertical("Box");
                    {
                        detectType = (TextureAnalysisData.DetectType) EditorGUILayout.EnumPopup("选择检查类型", detectType, GUILayout.Width(240));
                    }
                    GUILayout.EndVertical();

                    switch (detectType) {
                        case TextureAnalysisData.DetectType.Scene:
                            break;
                        case TextureAnalysisData.DetectType.Path: {
                            GUILayout.BeginVertical("Box");
                            {
                                detectTypeAtPath = (TextureAnalysisData.DetectTypeAtPath) EditorGUILayout.EnumPopup("选择检查类型", detectTypeAtPath, GUILayout.Width(240));
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical("Box");
                            {
                                detectPath = EditorGUILayout.TextField("输入待检查目录: ", detectPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                            }
                            GUILayout.EndVertical();
                        }
                            break;
                        case TextureAnalysisData.DetectType.GameObject: {
                            GUILayout.BeginVertical("Box");
                            {
                                detectGameObject = EditorGUILayout.ObjectField("选择检测的游戏物体: ", detectGameObject, typeof(GameObject), true, GUILayout.Width(UI_INPUT_AREA_WIDTH)) as GameObject;
                            }
                            GUILayout.EndVertical();
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    GUILayout.Space(UI_DEFAULT_MARGIN);
                    GUILayout.Label($"2. 点击开始按钮, 开始分析.");
                    GUILayout.BeginVertical("Box");
                    {
                        if (GUILayout.Button("开始", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                            TextureAnalysisTableWindow.Open(detectType, detectTypeAtPath, detectPath, detectGameObject, false);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }
    }
}
