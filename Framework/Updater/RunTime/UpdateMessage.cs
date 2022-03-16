using Kuroha.Framework.Message;
using Kuroha.Framework.Message.RunTime;

namespace Kuroha.Framework.Updater
{
    public class UpdateMessage : BaseMessage
    {
        /// <summary>
        /// 时间增量
        /// </summary>
        public float deltaTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="delta"></param>
        public UpdateMessage(float delta)
        {
            deltaTime = delta;
        }
    }
}
