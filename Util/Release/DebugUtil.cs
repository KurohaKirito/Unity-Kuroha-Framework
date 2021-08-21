using UnityEditor;

namespace Kuroha.Util.Release
{
    public static class DebugUtil
    {
        /// <summary>
        /// 日志开关
        /// </summary>
        public static bool LogEnable { get; set; } = true;

        /// <summary>
        /// 清空控制台
        /// </summary>
        public static void ClearConsole()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
            var logEntries = assembly?.GetType("UnityEditor.LogEntries");
            var clearConsoleMethod = logEntries?.GetMethod("Clear");
            clearConsoleMethod?.Invoke(new object(), null);
        }

        /// <summary>
        /// 向控制台输出日志
        /// </summary>
        /// <param name="log">日志信息</param>
        /// <param name="go">游戏物体</param>
        public static void Log(object log, UnityEngine.Object go = null)
        {
            if (LogEnable)
            {
                UnityEngine.Debug.Log(log, go);
            }
        }

        /// <summary>
        /// 向控制台输出警告
        /// </summary>
        /// <param name="log">警告信息</param>
        /// <param name="go">游戏物体</param>
        public static void LogWarning(object log, UnityEngine.Object go = null)
        {
            if (LogEnable)
            {
                UnityEngine.Debug.LogWarning(log, go);
            }
        }

        /// <summary>
        /// 向控制台输出错误
        /// </summary>
        /// <param name="log">错误信息</param>
        /// <param name="go">游戏物体</param>
        public static void LogError(object log, UnityEngine.Object go = null)
        {
            if (LogEnable)
            {
                UnityEngine.Debug.LogError(log, go);
            }
        }
    }
}