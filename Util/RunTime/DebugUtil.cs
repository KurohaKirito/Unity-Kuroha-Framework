using UnityEditor;

namespace Kuroha.Util.RunTime
{
    public static class DebugUtil
    {
        /// <summary>
        /// 日志开关
        /// </summary>
        public static bool LogEnable { get; set; } = true;

        /// <summary>
        /// 设置颜色
        /// </summary>
        private static object SetColor(string color, object log)
        {
            return string.IsNullOrEmpty(color) == false ? $"<color={color}>{log}</color>" : log;
        }

        /// <summary>
        /// 向控制台输出日志
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="log">日志信息</param>
        /// <param name="go">游戏物体</param>
        public static void Log(string log, UnityEngine.Object go = null, string color = null)
        {
            if (LogEnable)
            {
                UnityEngine.Debug.Log(SetColor(color, log), go);
            }
        }

        /// <summary>
        /// 向控制台输出警告
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="log">警告信息</param>
        /// <param name="go">游戏物体</param>
        public static void LogWarning(string log, UnityEngine.Object go = null, string color = null)
        {
            if (LogEnable)
            {
                UnityEngine.Debug.LogWarning(SetColor(color, log), go);
            }
        }

        /// <summary>
        /// 向控制台输出错误
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="log">错误信息</param>
        /// <param name="go">游戏物体</param>
        public static void LogError(string log, UnityEngine.Object go = null, string color = null)
        {
            if (LogEnable)
            {
                UnityEngine.Debug.LogError(SetColor(color, log), go);
            }
        }
    }
}
