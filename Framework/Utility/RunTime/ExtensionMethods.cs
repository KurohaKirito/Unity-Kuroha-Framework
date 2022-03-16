using System.Runtime.CompilerServices;

namespace Kuroha.Framework.Utility.RunTime
{
    public static class ExtensionMethods
    {
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
