using System.Collections;
using UnityEngine;

namespace Kuroha.Util.RunTime
{
    /// <summary>
    /// 可以返回结果的协程
    /// </summary>
    public class CoroutineUtil
    {
        /// <summary>
        /// 协程结果
        /// </summary>
        public object result;
        
        /// <summary>
        /// Unity 协程
        /// </summary>
        public readonly Coroutine coroutine;
        
        /// <summary>
        /// Unity 协程方法
        /// </summary>
        private readonly IEnumerator coroutineFunc;

        /// <summary>
        /// 构造方法
        /// </summary>
        public CoroutineUtil(MonoBehaviour owner, IEnumerator func)
        {
            coroutineFunc = func;
            coroutine = owner.StartCoroutine(Run());
        }

        /// <summary>
        /// 执行协程
        /// </summary>
        private IEnumerator Run()
        {
            while (coroutineFunc.MoveNext())
            {
                result = coroutineFunc.Current;
                yield return result;
            }
        }
    }
}
