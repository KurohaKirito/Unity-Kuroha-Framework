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
            if (current >= total)
            {
                EditorUtility.ClearProgressBar();
            }
            else
            {
                EditorUtility.DisplayProgressBar(title, info, (float)current / total);
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

            if (current >= total)
            {
                EditorUtility.ClearProgressBar();
            }
            else
            {
                isCancel = EditorUtility.DisplayCancelableProgressBar(title, info, (float)current / total);

                if (isCancel)
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            return isCancel;
        }
    }
}