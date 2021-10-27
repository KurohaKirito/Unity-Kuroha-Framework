using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kuroha.Util.RunTime
{
    /// <summary>
    /// 协程实现异步加载场景
    /// </summary>
    public class AsyncLoadScene : MonoBehaviour
    {
        /// <summary>
        /// 加载进度
        /// </summary>
        private int curProgressValue;

        /// <summary>
        /// 总进度
        /// </summary>
        private int allProgressValue;

        /// <summary>
        /// 异步加载进程
        /// </summary>
        private AsyncOperation asyncOperation;
        
        /// <summary>
        /// 单例
        /// </summary>
        public static AsyncLoadScene Async { get; private set; }
        
        /// <summary>
        /// 单例
        /// </summary>
        private void Awake()
        {
            Async = this;
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Start()
        {
            curProgressValue = 0;
            allProgressValue = 100;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 开始加载
        /// </summary>
        public void StartLoad(string targetSceneName)
        {
            asyncOperation = SceneManager.LoadSceneAsync(targetSceneName);
            asyncOperation.allowSceneActivation = false;
        }

        /// <summary>
        /// 帧更新
        /// </summary>
        private void Update()
        {
            // ReSharper disable once MergeIntoPattern
            if (asyncOperation != null && asyncOperation.isDone == false)
            {
                if (curProgressValue < allProgressValue)
                {
                    curProgressValue++;
                }
                else
                {
                    asyncOperation.allowSceneActivation = true;
                    asyncOperation = null;
                }
            }
        }
    }
}