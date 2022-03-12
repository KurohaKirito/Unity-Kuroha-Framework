using System.Collections.Generic;
using System.Threading.Tasks;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.Launcher
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
            var launcher = GameObject.Find($"Singleton_{nameof(Launcher)}");
            if (launcher == null)
            {
                await Launcher.Instance.InitAsync();
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
            if (startEventQueue == null)
            {
                startEventQueue = new Queue<IOnStart>();
            }

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
            if (destroyEventQueue == null)
            {
                destroyEventQueue = new Queue<IOnDestroy>();
            }
            
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
            if (applicationQuitEventQueue == null)
            {
                applicationQuitEventQueue = new Queue<IOnApplicationQuit>();
            }
            
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
            if (startEventQueue == null)
            {
                startEventQueue = new Queue<IOnStart>();
            }
            
            startEventQueue.Enqueue(func);
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        protected void RegisterDestroyEvent(IOnDestroy func)
        {
            if (destroyEventQueue == null)
            {
                destroyEventQueue = new Queue<IOnDestroy>();
            }
            
            destroyEventQueue.Enqueue(func);
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        protected void RegisterOnApplicationQuit(IOnApplicationQuit func)
        {
            if (applicationQuitEventQueue == null)
            {
                applicationQuitEventQueue = new Queue<IOnApplicationQuit>();
            }
            
            applicationQuitEventQueue.Enqueue(func);
        }
    }
}
