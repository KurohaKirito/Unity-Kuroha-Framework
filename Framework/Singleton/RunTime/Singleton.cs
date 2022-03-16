using System.Threading.Tasks;
using Kuroha.Framework.Utility.RunTime;
using UnityEngine;

namespace Kuroha.Framework.Singleton.RunTime
{
    /// <summary>
    /// 单例基类
    /// 每个单例首次访问时都会进行合法性验证, 仅在编辑器内验证, 发布后不进行验证
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        /// <summary>
        /// 单例活动标志
        /// </summary>
        private bool active = true;

        /// <summary>
        /// 单例
        /// </summary>
        private static T instanceBase;

        /// <summary>
        /// 单例
        /// </summary>
        protected static Singleton<T> InstanceBase
        {
            get
            {
                // 只有第 1 次调用的时候, instanceBase = null, 后续这个 if 全部为 false
                if (instanceBase == null)
                {
                    // 判断是否需要新建单例物体
                    if (IsNeedCreateSingleton(out var script))
                    {
                        // 在场景中创建单例物体
                        script = CreateSingleton();
                    }

                    script.AutoInit();
                }

                return instanceBase;
            }
        }
        
        /// <summary>
        /// 单例活动标志
        /// </summary>
        public static bool IsActive => instanceBase != null && InstanceBase.active;

        /// <summary>
        /// 检测场景中的单例
        /// </summary>
        private static bool IsNeedCreateSingleton(out T script)
        {
            script = null;
            var toCreate = false;

            var components = FindObjectsOfType<T>();

            // 场景中没有预先创建此单例
            if (components.Length == 0)
            {
                toCreate = true;
            }

            // 场景中预先创建了此单例
            else if (components.Length == 1)
            {
                instanceBase = components[0];
                DontDestroyOnLoad(instanceBase);
                script = instanceBase;
            }

            // 错误, 场景中预先创建了多个此单例
            else if (components.Length > 1)
            {
                DebugUtil.LogError("错误: 预先创建了多个单例组件在场景中! 请检查并修改!", null, "red");
                foreach (var component in components)
                {
                    Destroy(component);
                }

                toCreate = true;
            }

            return toCreate;
        }

        /// <summary>
        /// 创建一个单例
        /// </summary>
        private static T CreateSingleton()
        {
            var gameObject = new GameObject($"Singleton_{typeof(T).Name}", typeof(T));
            instanceBase = gameObject.GetComponent<T>();
            DontDestroyOnLoad(instanceBase);

            return instanceBase;
        }

        /// <summary>
        /// 自动初始化
        /// </summary>
        protected virtual void AutoInit() { }

        /// <summary>
        /// [Async] 初始化
        /// </summary>
        public virtual Task InitAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        protected virtual void OnDestroy()
        {
            active = false;
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            active = false;
        }
    }
}
