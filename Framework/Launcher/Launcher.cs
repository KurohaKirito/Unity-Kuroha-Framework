using System.Collections.Generic;
using Kuroha.Framework.AsyncLoad;
using UnityEngine;

namespace Kuroha.Framework.Launcher
{
    public class Launcher : MonoBehaviour
    {
        /// <summary>
        /// 启动器列表
        /// </summary>
        private List<ILauncher> launcherList;

        /// <summary>
        /// 启动游戏
        /// </summary>
        private void Start()
        {
            launcherList ??= new List<ILauncher>();
            
            launcherList.Add(new AsyncLoadScene());
            
            Invoke(nameof(LauncherComponent), 0.5f);
        }

        /// <summary>
        /// 启动所有的组件
        /// </summary>
        private void LauncherComponent()
        {
            foreach (var launcher in launcherList)
            {
                launcher.OnLaunch();
            }
        }

        /// <summary>
        /// 清空内存
        /// </summary>
        private void OnDestroy()
        {
            launcherList.Clear();
            launcherList = null;
        }
    }
}
