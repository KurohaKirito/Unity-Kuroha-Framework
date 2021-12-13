using System;
using UnityEngine;

namespace Kuroha.Framework.BugReport
{
    [Serializable]
    public class UnityLog
    {
        /// <summary>
        /// 日志内容
        /// </summary>
        public string condition;
        
        /// <summary>
        /// 堆栈跟踪
        /// </summary>
        public string stacktrace;
        
        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType type;

        public UnityLog(string condition, string stacktrace, LogType type)
        {
            this.condition = condition;
            this.stacktrace = stacktrace;
            this.type = type;
        }
    }
}
