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
        private static T instanceBase;

        /// <summary>
        /// 单例
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected static Singleton<T> InstanceBase
        {
            get
            {
                if (ReferenceEquals(instanceBase, null))
                {
                    FirstGet();
                    
                    var gameObject = new GameObject($"Singleton_{typeof(T).Name}", typeof(T));
                    instanceBase = gameObject.GetComponent<T>();
                    DontDestroyOnLoad(gameObject);
                }
                
                return instanceBase;
            }
            
            set => instanceBase = value as T;
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
        public static bool IsActive => ReferenceEquals(InstanceBase, null) == false && InstanceBase.active;

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
