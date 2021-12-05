using System.Collections;
using UnityEngine;

namespace Kuroha.Framework.Launcher
{
    /// <summary>
    /// 场景初始化器
    /// </summary>
    public class SceneInitializer : MonoBehaviour
    {
        /// <summary>
        /// 初始化
        /// </summary>
        private void Start()
        {
            Invoke(nameof(Init), 0.5f);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            SceneInit();
            StartCoroutine(SceneInitCoroutine());
        }

        /// <summary>
        /// 初始化场景
        /// </summary>
        protected virtual void SceneInit()
        {
            // ...
        }

        /// <summary>
        /// 初始化场景 [协程]
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator SceneInitCoroutine()
        {
            yield break;
        }
    }
}
