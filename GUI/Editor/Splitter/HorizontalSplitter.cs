using UnityEditor;
using UnityEngine;
using System;

namespace Kuroha.GUI.Editor.Splitter
{
    [Serializable]
    public class HorizontalSplitter : Splitter
    {
        /// <summary>
        /// 分割条的无色区域
        /// 默认分割条为 16 像素, 这里设置顶部 7 像素和 底部 8 像素都不显示, 仅显示中间的 1 个像素.
        /// 但是整个厚度为 16 像素的区域都可以触发鼠标变化, 可以触发拖拽.
        /// </summary>
        private static readonly RectOffset barRectOffset = new RectOffset(0, 0, 7, 8);

        /// <summary>
        /// 分割条大小
        /// </summary>
        private const int BAR_SIZE = 16;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="window">使用当前布局的窗口</param>
        /// <param name="mainSize">主区域的默认大小, 上下切分时为默认高度, 左右切分时为默认宽度</param>
        /// <param name="minSize">主区域的最小大小, 上下切分时为最小高度, 左右切分时为最小宽度</param>
        /// <param name="isFreeze">是否冻结分割线 (不允许滑动调整范围)</param>
        public HorizontalSplitter(EditorWindow window, float mainSize, float minSize, bool isFreeze)
            : base(window, SplitMode.Horizontal, mainSize, minSize, BAR_SIZE, isFreeze) { }

        /// <summary>
        /// 分割条的无色区域
        /// </summary>
        /// <returns></returns>
        protected override RectOffset BarRectOffset()
        {
            return barRectOffset;
        }

        /// <summary>
        /// 主区域
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        protected override Rect MainRect(Rect rect)
        {
            return new Rect(rect)
            {
                x = 0,
                y = 0,
                height = mainAreaSize
            };
        }

        /// <summary>
        /// 子区域
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        protected override Rect SubRect(Rect rect)
        {
            return new Rect(rect)
            {
                x = 0,
                y = mainAreaSize + 5,
                height = rect.height - mainAreaSize - 15
            };
        }

        /// <summary>
        /// 分割条
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        protected override Rect BarRect(Rect rect)
        {
            return new Rect(rect)
            {
                x = 0,
                y = mainAreaSize - barSize / 2,
                height = barSize
            };
        }
    }
}
