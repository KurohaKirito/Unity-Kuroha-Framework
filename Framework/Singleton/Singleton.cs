using System.Diagnostics;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.Singleton
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
        private static T instance;

        /// <summary>
        /// 单例
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected static Singleton<T> Instance
        {
            get
            {
                if (ReferenceEquals(instance, null))
                {
                    if (Instance.active)
                    {
                        FirstGet();
                    
                        var gameObject = new GameObject($"Singleton_{typeof(T).Name}", typeof(T));
                        instance = gameObject.GetComponent<T>();
                        DontDestroyOnLoad(instance);
                    }
                    else
                    {
                        DebugUtil.LogError("错误! OnDestroy() 中访问单例时, 必须先判断其活动标志!", null, "red");
                    }
                }
                
                return instance;
            }
            
            set => instance = value as T;
        }

        /// <summary>
        /// 首次访问
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        private static void FirstGet()
        {
            var components = FindObjectsOfType<T>();
            if (components != null && components.Length > 0)
            {
                DebugUtil.LogError("预先存在单例组件在场景中! 单例组件禁止预先创建! 请检查并修改!", null, "red");
                foreach (var component in components)
                {
                    Destroy(component);
                }
            }
        }

        /// <summary>
        /// 单例活动标志
        /// </summary>
        public static bool IsActive => ReferenceEquals(Instance, null) == false && Instance.active;

        /// <summary>
        /// 销毁
        /// </summary>
        private void OnDestroy()
        {
            active = false;
        }

        /// <summary>
        /// 隐藏
        /// </summary>
        private void OnEnable()
        {
            active = false;
        }
    }
}
