using System;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Framework.GUI.Editor.Splitter
{
    [Serializable]
    public abstract class Splitter
    {
        internal enum SplitMode
        {
            Horizontal,
            Vertical
        }

        // 不允许子类访问的字段
        private EditorWindow editorWindow;
        private MouseCursor mouseCursor;
        private SplitMode splitMode;
        private float lockSize;
        private bool isResizing;
        private bool isFreeze;

        // 需要子类访问的字段
        protected float barSize;
        protected float mainAreaSize;

        /// <summary>
        /// 可触发鼠标变化的区域, 即分割条的全部有效区域
        /// </summary>
        private Rect mouseCursorRect;

        /// <summary>
        /// 主区域占整个窗口的比例
        /// </summary>
        private float mainAreaRatio = 0.5f;

        /// <summary>
        /// 专业版: 分割条的颜色
        /// </summary>
        private static readonly Color splitterColorPro = Color.black;

        /// <summary>
        /// 免费版: 分割条的颜色
        /// </summary>
        private static readonly Color splitterColorFree = Color.gray;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="window">使用当前布局的窗口</param>
        /// <param name="splitMode">分割方式, 分为上下切分和左右切分</param>
        /// <param name="mainAreaSize">主区域的默认大小, 上下切分时为默认高度, 左右切分时为默认宽度</param>
        /// <param name="minSize">主区域的最小大小, 上下切分时为最小高度, 左右切分时为最小宽度</param>
        /// <param name="barSize">分割条的有效大小, 即鼠标放置时会变化的区域, 上下切分时为有效高度, 左右切分时为有效宽度</param>
        /// <param name="isFreeze">是否冻结分割线 (不允许滑动调整范围)</param>
        internal Splitter(EditorWindow window, SplitMode splitMode, float mainAreaSize, float minSize, float barSize, bool isFreeze)
        {
            editorWindow = window;
            this.mainAreaSize = mainAreaSize;
            this.splitMode = splitMode;
            lockSize = minSize;
            this.barSize = barSize;
            this.isFreeze = isFreeze;
            mouseCursor = this.splitMode == SplitMode.Vertical
                ? MouseCursor.ResizeHorizontal
                : MouseCursor.ResizeVertical;
        }

        /// <summary>
        /// 主窗口
        /// </summary>
        /// <param name="rect">区域矩形</param>
        /// <returns></returns>
        protected abstract Rect MainRect(Rect rect);

        /// <summary>
        /// 子窗口
        /// </summary>
        /// <param name="rect">区域矩形</param>
        /// <returns></returns>
        protected abstract Rect SubRect(Rect rect);

        /// <summary>
        /// 分割条的全部区域
        /// </summary>
        /// <param name="rect">区域矩形</param>
        /// <returns></returns>
        protected abstract Rect BarRect(Rect rect);

        /// <summary>
        /// 分割条的无色区域
        /// 默认分割条为 16 像素, 这里设置顶部 7 像素和 底部 8 像素都不显示, 仅显示中间的 1 个像素.
        /// 但是整个厚度为 16 像素的区域都可以触发鼠标变化, 可以触发拖拽.
        /// </summary>
        protected abstract RectOffset BarRectOffset();

        /// <summary>
        /// 绘制界面
        /// </summary>
        /// <param name="windowRect"></param>
        /// <param name="mainGUI"></param>
        /// <param name="subGUI"></param>
        public void OnGUI(Rect windowRect, Action<Rect> mainGUI, Action<Rect> subGUI)
        {
            var current = Event.current;

            // 绘制主区域内容
            mainGUI(MainRect(windowRect));

            // 绘制子区域内容
            subGUI(SubRect(windowRect));

            // 分割条全部有效区域 (可触发鼠标变化的整个区域, 外观上部分不显示)
            mouseCursorRect = BarRect(windowRect);
            EditorGUIUtility.AddCursorRect(mouseCursorRect, mouseCursor);
            
            if (isFreeze == false)
            {
                // 单个区域的最大大小
                var clampMax = splitMode == SplitMode.Vertical ? windowRect.width - lockSize : windowRect.height - lockSize;
                
                // 整个区域的最大大小 (两个区域之和, 即整个显示区域的大小)
                var targetSplitterValue = splitMode == SplitMode.Vertical ? windowRect.width : windowRect.height;
                
                // 主区域占整个区域的比例
                mainAreaRatio = splitMode == SplitMode.Vertical ? mainAreaSize / windowRect.width : mainAreaSize / windowRect.height;
                
                // 鼠标点击了分割条
                if (current.type == EventType.MouseDown)
                {
                    if (mouseCursorRect.Contains(current.mousePosition))
                    {
                        isResizing = true;
                    }
                }

                // 鼠标松开
                if (current.type == EventType.MouseUp)
                {
                    isResizing = false;
                }

                // 鼠标按住分割条并滑动
                if (isResizing)
                {
                    if (current.type == EventType.MouseDrag)
                    {
                        var targetValue = splitMode == SplitMode.Vertical ? current.mousePosition.x : current.mousePosition.y;
                        var diffValue = splitMode == SplitMode.Vertical ? windowRect.width : windowRect.height;
                        mainAreaRatio = targetValue / diffValue;
                    }
                }
                else if (current.type != EventType.Layout && current.type != EventType.Used)
                {
                    mainAreaRatio = targetSplitterValue * mainAreaRatio / targetSplitterValue;
                }
                
                // 计算主区域大小
                mainAreaSize = Mathf.Clamp(targetSplitterValue * mainAreaRatio, lockSize, clampMax);
            }
            
            // 绘制分割条
            var color = EditorGUIUtility.isProSkin ? splitterColorPro : splitterColorFree;
            
            // API: RectOffset.Remove(rect) => 从指定的 rect 中移除 RectOffset 偏移
            EditorGUI.DrawRect(BarRectOffset().Remove(mouseCursorRect), color);

            // 即时刷新
            if (isResizing)
            {
                editorWindow.Repaint();
            }
        }
    }
}