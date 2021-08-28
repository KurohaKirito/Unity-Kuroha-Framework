using System;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.AssetBatchTool;
using Kuroha.Tool.Editor.EffectCheckTool.GUI;
using Kuroha.Tool.Editor.FashionAnalysisTool;
using Kuroha.Tool.Editor.MeshAnalysisTool;
using Kuroha.Tool.Editor.ModelAnalysisTool;
using Kuroha.Tool.Editor.TextureAnalysisTool;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.AssetCheckTool
{
    public class AssetCheckToolWindow : EditorWindow
    {
        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;
        
        /// <summary>
        /// 标签页数据
        /// </summary>
        private static Toolbar.ToolbarData toolbarData;

        /// <summary>
        /// 标签页名称
        /// </summary>
        private static string[] toolBarNames;
        
        /// <summary>
        /// 标签页序号
        /// </summary>
        private static int toolBarIndex;

        /// <summary>
        /// 标题风格
        /// </summary>
        private static GUIStyle titleStyle;

        /// <summary>
        /// 版本风格
        /// </summary>
        private static GUIStyle versionStyle;
        
        /// <summary>
        /// OnGUI 集合
        /// </summary>
        private static Action[] actions;

        /// <summary>
        /// 资源检测工具
        /// </summary>
        #if UNITY_2019_2_OR_NEWER == false
        [MenuItem("Funny/资源检测工具/Asset Check Tool")]
        #else
        [MenuItem("Kuroha/AssetTool")]
        #endif
        public static void Open()
        {
            var window = GetWindow<AssetCheckToolWindow>("资源检测工具");
            window.minSize = new Vector2(800, 800);
            window.maxSize = window.minSize;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            toolBarNames = new[] {"特效检测", "时装检测工具", "模型统计分析", "贴图统计分析", "网格统计分析", "批处理"};
            
            #if UNITY_2019_2_OR_NEWER == false
                toolbarData = new Toolbar.ToolbarData(800, 16, toolBarNames);
            #else
                toolbarData = new Toolbar.ToolbarData(800, 320, toolBarNames);
            #endif
            
            titleStyle = new GUIStyle
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };
            
            versionStyle = new GUIStyle
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };

            actions = new Action[] {
                EffectCheckToolGUI.OnGUI,
                FashionAnalysisGUI.OnGUI,
                ModelAnalysisGUI.OnGUI,
                TextureAnalysisGUI.OnGUI,
                MeshAnalysisToolGUI.OnGUI,
                () => {
                    AssetBatchToolGUI.OnGUI(this);
                }
            };
        }

        /// <summary>
        /// 界面绘制
        /// </summary>
        private void OnGUI()
        {
            CheckLabelAlignment();
            GUILayout.BeginVertical();
            
            // draw the title
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
            GUILayout.Label("Asset Check Tools", titleStyle);

            // draw the version
            GUILayout.Space(UI_DEFAULT_MARGIN);
            GUILayout.Label("Current Version: 2.0.0", versionStyle);
            GUILayout.EndVertical();

            // draw the toolbar
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
            toolbarData.boxRectHeight = position.height;
            toolBarIndex = Toolbar.ToolbarAnime(ref toolbarData, this, ref toolBarIndex, actions);
        }

        /// <summary>
        /// 检查 Label 对齐方式
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_2018_4_1")]
        private static void CheckLabelAlignment()
        {
            if (UnityEngine.GUI.skin.label.alignment != TextAnchor.MiddleLeft)
            {
                UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
        }
    }
}