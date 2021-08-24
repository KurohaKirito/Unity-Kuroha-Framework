using Kuroha.Util.Release;
using UnityEditor;

namespace Kuroha.GUI.Editor
{
    /// <summary>
    /// 可自动关闭的进度条
    /// </summary>
    public static class ProgressBar
    {
        /// <summary>
        /// 不可中途取消的进度条
        /// </summary>
        /// <param name="title">进度条窗口标题</param>
        /// <param name="info">进度条下方的描述信息</param>
        /// <param name="current">当前进度</param>
        /// /// <param name="total">总进度</param>
        public static void DisplayProgressBar(string title, string info, int current, int total)
        {
            if (current < 0)
            {
                DebugUtil.LogError("进度条的当前进度不允许为负!");
                EditorUtility.ClearProgressBar();
            }
            if (total <= 0)
            {
                DebugUtil.LogError("进度条的总进度必须大于零!");
                EditorUtility.ClearProgressBar();
            }
            else
            {
                if (current >= total)
                {
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    EditorUtility.DisplayProgressBar(title, info, (float) current / total);
                }
            }
        }

        /// <summary>
        /// 可中途取消的进度条
        /// </summary>
        /// <param name="title">进度条窗口标题</param>
        /// <param name="info">进度条下方的描述信息</param>
        /// <param name="current">当前进度</param>
        /// /// <param name="total">总进度</param>
        public static bool DisplayProgressBarCancel(string title, string info, int current, int total)
        {
            var isCancel = false;
            
            if (current < 0)
            {
                DebugUtil.LogError("进度条的当前进度不允许为负!");
                isCancel = true;
                EditorUtility.ClearProgressBar();
            }
            if (total <= 0)
            {
                DebugUtil.LogError("进度条的总进度必须大于零!");
                isCancel = true;
                EditorUtility.ClearProgressBar();
            }
            else
            {
                if (current >= total)
                {
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    isCancel = EditorUtility.DisplayCancelableProgressBar(title, info, (float) current / total);

                    if (isCancel)
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }
            }
            
            return isCancel;
        }
    }
}