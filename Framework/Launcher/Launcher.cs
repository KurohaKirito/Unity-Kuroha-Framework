using System.Collections.Generic;
using Kuroha.Framework.AsyncLoad;
using UnityEngine;

namespace Kuroha.Framework.Launcher
{
    public class Launcher : MonoBehaviour
    {
        /// <summary>
        /// 启动器队列
        /// </summary>
        private Queue<ILauncher> launcherQueue;

        /// <summary>
        /// 启动游戏
        /// </summary>
        private void Start()
        {
            InitLauncher();
            RegisterFrameworkLauncher();
            RegisterLauncher();

            // 启动所有的启动器, 延时一段时间是因为需要等待全部的 Start 方法执行完
            Invoke(nameof(LauncherComponent), 0.25f);
        }

        /// <summary>
        /// 初始化启动器
        /// </summary>
        private void InitLauncher()
        {
            launcherQueue ??= new Queue<ILauncher>();
        }

        /// <summary>
        /// 注册框架
        /// </summary>
        private void RegisterFrameworkLauncher()
        {
            launcherQueue.Enqueue(new AsyncLoadScene());
        }

        /// <summary>
        /// 注册组件
        /// </summary>
        protected virtual void RegisterLauncher() { }

        /// <summary>
        /// 启动所有的组件
        /// </summary>
        private void LauncherComponent()
        {
            while (launcherQueue.Count > 0)
            {
                launcherQueue.Dequeue().OnLaunch();
            }
        }
    }
}
