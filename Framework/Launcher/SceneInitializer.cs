using UnityEngine;

namespace Kuroha.Framework.Launcher
{
    /// <summary>
    /// 场景初始化器
    /// </summary>
    public class SceneInitializer : MonoBehaviour
    {
        private void Start()
        {
            Invoke(nameof(SceneInit), 0.5f);
        }

        /// <summary>
        /// 初始化场景
        /// </summary>
        protected virtual void SceneInit()
        {
            // ...
        }
    }
}