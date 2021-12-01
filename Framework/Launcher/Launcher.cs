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
            RegisterLauncher();
            LauncherComponent();
        }

        /// <summary>
        /// 初始化启动器
        /// </summary>
        private void InitLauncher()
        {
            launcherQueue ??= new Queue<ILauncher>();
        }

        /// <summary>
        /// 注册需要启动的组件
        /// </summary>
        private void RegisterLauncher()
        {
            launcherQueue.Enqueue(new AsyncLoadScene());
        }

        /// <summary>
        /// 启动所有的组件
        /// </summary>
        private void LauncherComponent()
        {
            while (launcherQueue.Count > 0)
            {
                launcherQueue.Dequeue().OnLauncher();
            }
        }
    }
}