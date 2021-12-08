using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Kuroha.Framework.Launcher
{
    public class SceneLauncher : MonoBehaviour
    {
        /// <summary>
        /// Start 事件队列
        /// </summary>
        private Queue<IOnStart> onStartQueue;
        
        /// <summary>
        /// OnDestroy 事件队列
        /// </summary>
        private Queue<IOnDestroy> onDestroyQueue;
        
        /// <summary>
        /// OnApplicationQuit 事件队列
        /// </summary>
        private Queue<IOnApplicationQuit> onApplicationQuitQueue;

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

        /// <summary>
        /// Unity 事件: Start
        /// </summary>
        private async void Start()
        {
            // 启动游戏框架
            await LaunchFramework();
            
            // 注册用户事件
            RegisterEvent();
            
            // 执行用户注册的 Start 事件
            ExecuteStart();
            
            // 执行场景的初始化
            SceneStart();
        }

        /// <summary>
        /// Unity 事件: OnDestroy
        /// </summary>
        private void OnDestroy()
        {
            ExecuteOnDestroy();
        }

        /// <summary>
        /// Unity 事件: OnApplicationQuit
        /// </summary>
        private void OnApplicationQuit()
        {
            ExecuteOnApplicationQuit();
        }
        
        /// <summary>
        /// 执行 Start 事件队列
        /// </summary>
        private void ExecuteStart()
        {
            onStartQueue ??= new Queue<IOnStart>();
            while (onStartQueue.Count > 0)
            {
                onStartQueue.Dequeue().OnStart();
            }
        }
        
        /// <summary>
        /// 执行 OnDestroy 事件队列
        /// </summary>
        private void ExecuteOnDestroy()
        {
            onDestroyQueue ??= new Queue<IOnDestroy>();
            while (onDestroyQueue.Count > 0)
            {
                onDestroyQueue.Dequeue().OnDestroy();
            }
        }
        
        /// <summary>
        /// 执行 OnApplicationQuit 事件队列
        /// </summary>
        private void ExecuteOnApplicationQuit()
        {
            onApplicationQuitQueue ??= new Queue<IOnApplicationQuit>();
            while (onApplicationQuitQueue.Count > 0)
            {
                onApplicationQuitQueue.Dequeue().OnApplicationQuit();
            }
        }
        
        /// <summary>
        /// 注册 Start 事件
        /// </summary>
        protected void RegisterStart(IOnStart func)
        {
            onStartQueue ??= new Queue<IOnStart>();
            onStartQueue.Enqueue(func);
        }
        
        /// <summary>
        /// 注册 OnDestroy 事件
        /// </summary>
        protected void RegisterOnDestroy(IOnDestroy func)
        {
            onDestroyQueue ??= new Queue<IOnDestroy>();
            onDestroyQueue.Enqueue(func);
        }
        
        /// <summary>
        /// 注册 OnApplicationQuit 事件
        /// </summary>
        protected void RegisterOnApplicationQuit(IOnApplicationQuit func)
        {
            onApplicationQuitQueue ??= new Queue<IOnApplicationQuit>();
            onApplicationQuitQueue.Enqueue(func);
        }
    }
}
