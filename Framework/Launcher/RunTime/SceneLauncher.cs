using System.Collections.Generic;
using System.Threading.Tasks;
using Kuroha.Framework.Utility.RunTime;
using UnityEngine;

namespace Kuroha.Framework.Launcher.RunTime
{
    public class SceneLauncher : MonoBehaviour
    {
        private Queue<IOnStart> startEventQueue;
        private Queue<IOnDestroy> destroyEventQueue;
        private Queue<IOnApplicationQuit> applicationQuitEventQueue;

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void SceneStart() { }

        /// <summary>
        /// 注册事件
        /// </summary>
        protected virtual void RegisterEvent() { }
        
        /// <summary>
        /// 启动游戏框架
        /// </summary>
        private static async Task LaunchFramework()
        {
            var launcher = GameObject.Find($"Singleton_{nameof(RunTime.Launcher)}");
            if (launcher == null)
            {
                await RunTime.Launcher.Instance.InitAsync();
                DebugUtil.Log("框架启动完成!", null, "green");
            }
            else
            {
                DebugUtil.Log("框架启动完成! 无需重复启动!", null, "yellow");
            }
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        private async void Start()
        {
            await LaunchFramework();
            RegisterEvent();
            ExecuteStartEvent();
            SceneStart();
        }
        
        /// <summary>
        /// Unity Event
        /// </summary>
        private void OnDestroy()
        {
            ExecuteDestroyEvent();
        }
        
        /// <summary>
        /// Unity Event
        /// </summary>
        private void OnApplicationQuit()
        {
            ExecuteApplicationQuitEvent();
        }
        
        /// <summary>
        /// 自定义事件
        /// </summary>
        private void ExecuteStartEvent()
        {
            startEventQueue ??= new Queue<IOnStart>();

            while (startEventQueue.Count > 0)
            {
                startEventQueue.Dequeue().StartEvent();
            }
        }
        
        /// <summary>
        /// 自定义事件
        /// </summary>
        private void ExecuteDestroyEvent()
        {
            destroyEventQueue ??= new Queue<IOnDestroy>();

            while (destroyEventQueue.Count > 0)
            {
                destroyEventQueue.Dequeue().DestroyEvent();
            }
        }
        
        /// <summary>
        /// 自定义事件
        /// </summary>
        private void ExecuteApplicationQuitEvent()
        {
            applicationQuitEventQueue ??= new Queue<IOnApplicationQuit>();

            while (applicationQuitEventQueue.Count > 0)
            {
                applicationQuitEventQueue.Dequeue().ApplicationQuitEvent();
            }
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        protected void RegisterStartEvent(IOnStart func)
        {
            startEventQueue ??= new Queue<IOnStart>();

            startEventQueue.Enqueue(func);
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        protected void RegisterDestroyEvent(IOnDestroy func)
        {
            destroyEventQueue ??= new Queue<IOnDestroy>();

            destroyEventQueue.Enqueue(func);
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        protected void RegisterOnApplicationQuit(IOnApplicationQuit func)
        {
            applicationQuitEventQueue ??= new Queue<IOnApplicationQuit>();

            applicationQuitEventQueue.Enqueue(func);
        }
    }
}
