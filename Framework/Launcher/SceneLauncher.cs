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
            }
        }

        private async void Start()
        {
            await LaunchFramework();
            DebugUtil.Log("框架启动完成!", this, "green");
            RegisterEvent();
            ExecuteStartEvent();
            SceneStart();
        }
        private void OnDestroy()
        {
            ExecuteDestroyEvent();
        }
        private void OnApplicationQuit()
        {
            ExecuteApplicationQuitEvent();
        }
        
        private void ExecuteStartEvent()
        {
            startEventQueue ??= new Queue<IOnStart>();
            while (startEventQueue.Count > 0)
            {
                startEventQueue.Dequeue().StartEvent();
            }
        }
        private void ExecuteDestroyEvent()
        {
            destroyEventQueue ??= new Queue<IOnDestroy>();
            while (destroyEventQueue.Count > 0)
            {
                destroyEventQueue.Dequeue().DestroyEvent();
            }
        }
        private void ExecuteApplicationQuitEvent()
        {
            applicationQuitEventQueue ??= new Queue<IOnApplicationQuit>();
            while (applicationQuitEventQueue.Count > 0)
            {
                applicationQuitEventQueue.Dequeue().ApplicationQuitEvent();
            }
        }
        
        protected void RegisterStartEvent(IOnStart func)
        {
            startEventQueue ??= new Queue<IOnStart>();
            startEventQueue.Enqueue(func);
        }
        protected void RegisterDestroyEvent(IOnDestroy func)
        {
            destroyEventQueue ??= new Queue<IOnDestroy>();
            destroyEventQueue.Enqueue(func);
        }
        protected void RegisterOnApplicationQuit(IOnApplicationQuit func)
        {
            applicationQuitEventQueue ??= new Queue<IOnApplicationQuit>();
            applicationQuitEventQueue.Enqueue(func);
        }
    }
}
