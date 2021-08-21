using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.TextureAnalysisTool
{
    /// <summary>
    /// GUI 绘制类
    /// </summary>
    public class TextureAnalysisGUI : UnityEditor.Editor
    {
        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_INPUT_AREA_WIDTH = 400;

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
        private static string detectPath = "Art/Effects/Textures";

        /// <summary>
        /// 检测类型
        /// </summary>
        private static TextureAnalysisData.DetectType detectType = TextureAnalysisData.DetectType.Path;

        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool textureAnalysisFoldout = true;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            textureAnalysisFoldout = EditorGUILayout.Foldout(textureAnalysisFoldout, "纹理统计分析工具", true);
            if (textureAnalysisFoldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 当前支持两种检查类型");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("(1) Path:  \t 检查指定目录下的纹理, 目录必须是 Assets/ 的相对目录.");
                    EditorGUILayout.LabelField("(2) Scene: \t 检查当前场景中的纹理.");
                    EditorGUI.indentLevel--;
                    
                    GUILayout.BeginVertical("Box");
                    {
                        detectType = (TextureAnalysisData.DetectType)EditorGUILayout.EnumPopup("选择检查类型", detectType, GUILayout.Width(240));
                    }
                    GUILayout.EndVertical();
                    
                    GUILayout.BeginVertical("Box");
                    {
                        if (detectType == TextureAnalysisData.DetectType.Path)
                        {
                            detectPath = EditorGUILayout.TextField("输入待检查目录: ", detectPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.Space(UI_DEFAULT_MARGIN);
                    GUILayout.Label($"2. 点击开始按钮, 开始分析.");
                    GUILayout.BeginVertical("Box");
                    {
                        if (GUILayout.Button("开始", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            TextureAnalysisTableWindow.Open(detectType, $"Assets/{detectPath}");
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }
    }
}