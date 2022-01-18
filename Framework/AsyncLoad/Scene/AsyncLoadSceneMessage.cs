using Kuroha.Framework.Message;

namespace Kuroha.Framework.AsyncLoad.Scene
{
    public class AsyncLoadSceneMessage : BaseMessage
    {
        /// <summary>
        /// 异步加载的场景名
        /// </summary>
        public readonly string path;

        /// <summary>
        /// 最小加载时长 (秒)
        /// </summary>
        public readonly float minLoadTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AsyncLoadSceneMessage(string path, float minLoadTime)
        {
            this.path = path;
            this.minLoadTime = minLoadTime;
        }
    }
}
