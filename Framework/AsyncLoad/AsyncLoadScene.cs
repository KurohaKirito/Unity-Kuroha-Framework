using Kuroha.Framework.Message;
using Kuroha.Framework.Updater;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kuroha.Framework.AsyncLoad
{
    /// <summary>
    /// 协程实现异步加载场景
    /// </summary>
    public class AsyncLoadScene : UpdateableMonoBehaviour
    {
        /// <summary>
        /// 计时器
        /// </summary>
        [SerializeField]
        private float timer;

        /// <summary>
        /// 最短加载时长
        /// </summary>
        [SerializeField]
        private float minLoadTime;

        /// <summary>
        /// 正在加载的场景的路径
        /// </summary>
        [SerializeField]
        private string scenePath;
        
        /// <summary>
        /// 异步加载进程
        /// </summary>
        private AsyncOperation asyncOperation;

        /// <summary>
        /// 初始化
        /// </summary>
        private void Start()
        {
            #if UNITY_EDITOR
            DontDestroyOnLoad(this);
            #endif
            
            MessageSystem.Instance.AddListener<AsyncLoadSceneMessage>(Load);
        }

        /// <summary>
        /// 帧更新
        /// </summary>
        public override bool OnUpdate(BaseMessage message)
        {
            if (message is UpdateMessage msg)
            {
                if (asyncOperation is { isDone: false })
                {
                    // 计时中
                    if (timer < minLoadTime)
                    {
                        timer += msg.deltaTime;
                    }
                    // 计时完成
                    else
                    {
                        asyncOperation.allowSceneActivation = true;
                        Clear();
                    }
                }
            }

            return base.OnUpdate(message);
        }
        
        /// <summary>
        /// 开始加载
        /// </summary>
        private bool Load(BaseMessage message)
        {
            // 转换消息类型
            if (message is AsyncLoadSceneMessage async)
            {
                ResetAsyncLoad(async.minLoadTime);
                scenePath = async.path;
                
                asyncOperation = SceneManager.LoadSceneAsync(scenePath);
                asyncOperation.allowSceneActivation = false;
                Updater.Updater.Instance.Register(this);
            }

            return true;
        }

        /// <summary>
        /// 重置异步加载器
        /// </summary>
        private void ResetAsyncLoad(float min)
        {
            timer = 0;
            minLoadTime = min;
            scenePath = default;
            asyncOperation = null;
        }

        /// <summary>
        /// 清理
        /// </summary>
        private void Clear()
        {
            ResetAsyncLoad(0);
            Updater.Updater.Instance.Unregister(this);
        }
    }
}
