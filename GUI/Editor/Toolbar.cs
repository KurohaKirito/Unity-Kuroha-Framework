using System;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.GUI.Editor {
    public static class Toolbar {
        private const int WIDTH_SPACE = 4;
        private static int lastClick;
        private static float animeStepWidth;
        private static float boxRectY;

        /// <summary>
        /// 结构体: 标签页数据
        /// </summary>
        public struct ToolbarData {
            public bool playAnime;
            public float curPositionX;
            public float boxRectHeight;

            public readonly float animeTime;
            public readonly string[] toolbarTitles;

            public ToolbarData(int height, float animeLength, string[] titles) {
                // 初始化高度
                boxRectHeight = height;
                // 初始不播放动画
                playAnime = false;
                // 初始化标签页标题
                toolbarTitles = titles;
                // 接收动画时间长度
                animeTime = animeLength;

                // 初始化当前 X 坐标
                curPositionX = WIDTH_SPACE;
            }
        }

        /// <summary>
        /// 动画版标签页
        /// </summary>
        /// <param name="data">标签页数据</param>
        /// <param name="window">窗口</param>
        /// <param name="toolbarIndex">当前选中的标签页的序号</param>
        /// <param name="actions">行为</param>
        /// <returns></returns>
        public static int ToolbarAnime(ref ToolbarData data, EditorWindow window, ref int toolbarIndex, params Action[] actions) {
            // 错误检测
            if (actions.Length != data.toolbarTitles.Length) {
                DebugUtil.Log($"标签页数据不匹配! 标签页有 {data.toolbarTitles.Length} 个, 方法有 {actions.Length} 个!");
            } else {
                // 获取窗口矩形
                var windowRect = window.position;

                // 绘制标签页
                toolbarIndex = GUILayout.Toolbar(toolbarIndex, data.toolbarTitles);

                // 判断是否播放动画
                if (lastClick != toolbarIndex) {
                    lastClick = toolbarIndex;
                    data.playAnime = true;
                    data.curPositionX = -windowRect.width;
                    animeStepWidth = windowRect.width / (Application.isPlaying ? data.animeTime / 5 : data.animeTime);
                } else if (data.curPositionX > WIDTH_SPACE) {
                    data.playAnime = false;
                    data.curPositionX = WIDTH_SPACE;
                }

                // 刷新 Box Rect
                var boxRect = GUILayoutUtility.GetRect(windowRect.width - 10, data.boxRectHeight);

                // 仅固定布局下有效
                if (Event.current.type != EventType.Layout && boxRect.y > 0) {
                    boxRectY = boxRect.y;
                }

                // 绘制 Box Rect
                GUILayout.BeginArea(new Rect(data.curPositionX, boxRectY, windowRect.width - WIDTH_SPACE * 2, data.boxRectHeight));
                {
                    actions[toolbarIndex]();
                }
                GUILayout.EndArea();

                if (data.playAnime) {
                    data.curPositionX += animeStepWidth;
                    window.Repaint();
                }
            }

            return toolbarIndex;
        }
    }
}
