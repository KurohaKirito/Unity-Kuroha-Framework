using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Kuroha.Util.RunTime
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public class LogUtil : MonoBehaviour
    {
        /// <summary>
        /// 日志文件路径
        /// </summary>
        private string logFilePath;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Start()
        {
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            logFilePath = $"{Application.dataPath}/Config/Log/{date}.txt";
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// 追加日志信息
        /// </summary>
        /// <param name="content">信息内容</param>
        public void AppendLog(string content)
        {
            content = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} : {content}\n";
            File.AppendAllText(logFilePath, content, Encoding.UTF8);
        }
    }
}