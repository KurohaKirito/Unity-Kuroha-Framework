using System;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.GUI;
using Script.Effect.Editor.AssetTool.Tool.Editor.FashionAnalysisTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.MeshAnalysisTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.ProfilerTool.ProfilerTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.SceneAnalysisTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.TextureAnalysisTool;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetCheckTool {
    public class AssetCheckToolWindow : EditorWindow {
        /// <summary>
        /// 全局默认 margin
        /// </summary>
        public const float UI_DEFAULT_MARGIN = 5;

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
        public static void Open() {
            var window = GetWindow<AssetCheckToolWindow>("资源检测工具");
            window.minSize = new Vector2(800, 800);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable() {
            toolBarNames = new[] {
                "特效资源检测", "时装检测工具", "场景统计分析", "贴图统计分析", "网格统计分析", "批处理", "性能分析辅助"
            };
            
            toolbarData = new Toolbar.ToolbarData(800, 160, toolBarNames);

            titleStyle = new GUIStyle {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState {
                    textColor = EditorGUIUtility.isProSkin? Color.white : Color.black
                }
            };

            versionStyle = new GUIStyle {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState {
                    textColor = EditorGUIUtility.isProSkin? Color.white : Color.black
                }
            };

            actions = new Action[] {
                EffectCheckToolGUI.OnGUI, () => {
                    FashionAnalysisGUI.OnGUI(this);
                },
                SceneAnalysisGUI.OnGUI, TextureAnalysisGUI.OnGUI, MeshAnalysisToolGUI.OnGUI,
                //ParticleSystemProfiler.OnGUI,
                () => {
                    AssetBatchToolGUI.OnGUI(this);
                },
                () => {
                    ProfilerToolGUI.OnGUI(this);
                }
            };
        }

        /// <summary>
        /// 重置 ToolBar
        /// </summary>
        public void ResetToolBarIndex() {
            toolBarIndex = 0;
            Repaint();
        }

        /// <summary>
        /// 界面绘制
        /// </summary>
        private void OnGUI() {
            CheckLabel();
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
        /// 检查 Label
        /// </summary>
        private static void CheckLabel() {
            UnityEngine.GUI.skin.label.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : Color.black;
            
            if (UnityEngine.GUI.skin.label.alignment != TextAnchor.MiddleLeft) {
                UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
        }
    }
}
