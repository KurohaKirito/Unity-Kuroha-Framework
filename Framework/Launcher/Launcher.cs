using System.Collections.Generic;
using System.Threading.Tasks;
using Kuroha.Framework.AsyncLoad;
using Kuroha.Framework.Audio;
using Kuroha.Framework.Singleton;

namespace Kuroha.Framework.Launcher
{
    public class Launcher : Singleton<Launcher>
    {
        /// <summary>
        /// 单例
        /// </summary>
        private static Launcher Instance => InstanceBase as Launcher;
        
        /// <summary>
        /// 场景异步加载器
        /// </summary>
        private AsyncLoadScene asyncLoadScene;
        
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
        /// 初始化框架
        /// </summary>
        private async void Start()
        {
            await Instance.InitAsync();
        }

        /// <summary>
        /// [Async] 初始化
        /// </summary>
        public sealed override async Task InitAsync()
        {
            LaunchAsyncLoadScene();
            await AudioPlayManager.Instance.InitAsync();
            await BugReport.BugReport.Instance.InitAsync();

            RegisterUnityEvent();
            ExecuteOnStart();
        }

        /// <summary>
        /// 注册 Unity 事件
        /// </summary>
        protected virtual void RegisterUnityEvent() { }

        /// <summary>
        /// 初始化场景异步加载器
        /// </summary>
        private void LaunchAsyncLoadScene()
        {
            if (asyncLoadScene == null)
            {
                asyncLoadScene = new AsyncLoadScene();
                asyncLoadScene.OnLaunch();
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            asyncLoadScene = null;
            ExecuteOnDestroy();
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            asyncLoadScene = null;
            ExecuteOnApplicationQuit();
        }

        /// <summary>
        /// 执行 Start 事件队列
        /// </summary>
        private void ExecuteOnStart()
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
        protected void RegisterOnStart(IOnStart func)
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
