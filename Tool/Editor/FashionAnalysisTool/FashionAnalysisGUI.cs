using UnityEditor;
using UnityEngine;

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
        /// 折叠框
        /// </summary>
        private static bool fashionAnalysisFoldout = true;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            fashionAnalysisFoldout = EditorGUILayout.Foldout(fashionAnalysisFoldout, "时装分析工具", true);
            if (fashionAnalysisFoldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    GUILayout.Space(UI_DEFAULT_MARGIN);
                    GUILayout.Label("1. 点击开始按钮, 开始分析.");
                    GUILayout.BeginVertical("Box");
                    {
                        if (GUILayout.Button("Start", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            FashionAnalysisTableWindow.Open();
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }
    }
}