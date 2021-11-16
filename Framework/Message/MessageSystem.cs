using System.Collections.Generic;
using Kuroha.Framework.Singleton;

namespace Kuroha.Framework.Message
{
    /// <summary>
    /// 全局消息系统, 继承自单例组件
    /// </summary>
    public class MessageSystem : Singleton<MessageSystem>
    {
        /// <summary>
        /// 消息委托
        /// </summary>
        public delegate bool MessageHandlerDelegate(BaseMessage message);
        
        /// <summary>
        /// 单例
        /// </summary>
        public static MessageSystem Instance
        {
            get => (MessageSystem)InstanceBase;
            set => InstanceBase = value;
        }



        private Dictionary<string, List<MessageHandlerDelegate>> listenerDic = new Dictionary<string, List<MessageHandlerDelegate>>();
        
        
    }
}
