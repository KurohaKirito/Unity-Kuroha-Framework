using Kuroha.Tool.AssetTool.Editor.ProfilerTool.ProfilerTool;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.ProfilerTool.LoadTimeRecordTool
{
    public static class LoadTimeRecordGUI
    {
        /// <summary>
        /// 整合了需要批量删除的资源所在路径的文件
        /// </summary>
        private static string filePath = string.Empty;
        
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool foldout = true;
        
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
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            foldout = EditorGUILayout.Foldout(foldout, ProfilerToolGUI.tools[(int) ProfilerToolGUI.ToolType.AsyncLoadTool], true);
            
            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 请选择从真机导出的异步加载时长统计文件.");
                    
                    GUILayout.Space(UI_DEFAULT_MARGIN);
                
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical(GUILayout.Width(UI_BUTTON_WIDTH + 60));
                        {
                            GUILayout.BeginHorizontal("Box");
                            {
                                if (GUILayout.Button("Select File", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                                {
                                    filePath = EditorUtility.OpenFilePanel("Select File", filePath, "");
                                }
                            }
                            GUILayout.EndHorizontal();
                        
                            GUILayout.Space(UI_DEFAULT_MARGIN);
                        
                            GUILayout.Label("2. 点击按钮, 展示统计结果.");
                            GUILayout.BeginHorizontal("Box");
                            {
                                if (GUILayout.Button("Show", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                                {
                                    LoadTimeRecordTableWindow.Open(ref filePath);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        if (string.IsNullOrEmpty(filePath))
                        {
                            filePath = "请选择文件...";
                        }
                        GUILayout.Label(filePath, "WordWrapLabel", GUILayout.Width(200));
                        GUILayout.EndVertical();
                    }
                
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.EndVertical();
            }
        }
    }
}