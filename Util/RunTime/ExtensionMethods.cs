using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kuroha.Util.RunTime
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// 数组
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNotNullAndEmpty<T>(this IEnumerable<T> self)
        {
            return self != null && self.Any();
        }

        /// <summary>
        /// 数组
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> self)
        {
            return self == null || self.Any() == false;
        }

        /// <summary>
        /// 字典
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNotNullAndEmpty<T1, T2>(this IDictionary<T1, T2> self)
        {
            return self != null && self.Count > 0;
        }

        /// <summary>
        /// 字典
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T1, T2>(this IDictionary<T1, T2> self)
        {
            return self == null || self.Count <= 0;
        }
        
        /// <summary>
        /// Async Await
        /// </summary>
        /// <param name="asyncOperation"></param>
        /// <returns></returns>
        public static TaskAwaiter GetAwaiter(this UnityEngine.AsyncOperation asyncOperation)
        {
            var taskCompletionSource = new System.Threading.Tasks.TaskCompletionSource<object>();
            asyncOperation.completed += t =>
            {
                taskCompletionSource.SetResult(null);
            };
            return (taskCompletionSource.Task as System.Threading.Tasks.Task).GetAwaiter();
        }
    }
}
