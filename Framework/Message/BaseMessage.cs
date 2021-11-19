namespace Kuroha.Framework.Message
{
    public class BaseMessage
    {
        /// <summary>
        /// 消息名称
        /// </summary>
        public readonly string messageName;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseMessage()
        {
            messageName = GetType().Name;
        }
    }
}
