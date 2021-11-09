using System;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor.Splitter;
using Script.Effect.Editor.AssetTool.Tool.Editor.ProfilerTool.AsyncLoadTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.ProfilerTool.MemoryTool;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.ProfilerTool.ProfilerTool {
    public static class ProfilerToolGUI {
        /// <summary>
        /// 工具类型
        /// </summary>
        public enum ToolType {
            MemoryTool = 0,
            AsyncLoadTool = 1,
        }

        /// <summary>
        /// 工具名称
        /// </summary>
        public static readonly string[] tools = {
            "Memory 工具", "异步加载时长统计"
        };

        /// <summary>
        /// 当前的 Batch
        /// </summary>
        private static ToolType currentTool;

        /// <summary>
        /// 滑动条
        /// </summary>
        private static Vector2 scrollView;

        /// <summary>
        /// 垂直分割布局
        /// </summary>
        private static VerticalSplitter splitter;

        /// <summary>
        /// 按钮风格
        /// </summary>
        private static GUIStyle buttonStyle;

        /// <summary>
        /// 绘制界面
        /// </summary>
        /// <param name="window"></param>
        public static void OnGUI(in EditorWindow window) {
            if (splitter == null) {
                splitter = new VerticalSplitter(window, 210, 210, false);
            }

            splitter.OnGUI(window.position, MainRect, SubRect);

            if (buttonStyle == null) {
                buttonStyle = new GUIStyle("Button");
            }

            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.normal.textColor = EditorGUIUtility.isProSkin? Color.white : Color.black;
        }

        /// <summary>
        /// 主区域
        /// </summary>
        /// <param name="rect"></param>
        private static void MainRect(Rect rect) {
            GUILayout.BeginArea(rect);
            {
                scrollView = GUILayout.BeginScrollView(scrollView);
                {
                    GUILayout.Space(5);

                    for (var index = 0; index < tools.Length; index++) {
                        var oldColor = UnityEngine.GUI.backgroundColor;
                        if (currentTool == (ToolType)index) {
                            UnityEngine.GUI.backgroundColor = new Color(92 / 255f, 223 / 255f, 240 / 255f);
                        }

                        if (GUILayout.Button(tools[index], buttonStyle, GUILayout.Width(196), GUILayout.Height(30))) {
                            currentTool = (ToolType)index;
                        }

                        UnityEngine.GUI.backgroundColor = oldColor;

                        if (index + 1 < tools.Length) {
                            GUILayout.Space(10);
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        /// <summary>
        /// 子区域
        /// </summary>
        /// <param name="rect"></param>
        private static void SubRect(Rect rect) {
            GUILayout.BeginArea(rect);
            {
                switch (currentTool) {
                    case ToolType.MemoryTool:
                        ProfilerMemoryToolGUI.OnGUI();
                        break;

                    case ToolType.AsyncLoadTool:
                        AsyncLoadGUI.OnGUI();
                        break;

                    default:
                        DebugUtil.LogError("忘记注册 OnGUI 事件了!");
                        throw new ArgumentOutOfRangeException();
                }
            }
            GUILayout.EndArea();
        }
    }
}