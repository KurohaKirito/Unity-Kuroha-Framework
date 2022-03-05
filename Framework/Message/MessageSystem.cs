using System.Collections.Generic;
using Kuroha.Framework.Singleton;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.Message
{
    /// <summary>
    /// 全局消息系统, 继承自单例组件
    /// </summary>
    public class MessageSystem : Singleton<MessageSystem>
    {
        #region 编辑器 API

        #if KUROHA_DEBUG_MODE

        [Header("当前的消息及监听者列表")] [SerializeField]
        private List<Kuroha.Framework.Message.MessageListener> messageListenerList;

        private void OnGUI()
        {
            messageListenerList ??= new List<Kuroha.Framework.Message.MessageListener>();

            messageListenerList.Clear();
            foreach (var key in listenerDic.Keys)
            {
                var val = new Kuroha.Framework.Message.MessageListener
                {
                    messageTypeName = key
                };

                var valList = new List<string>();
                foreach (var handlers in listenerDic[key])
                {
                    valList.Add($"{handlers.Method.DeclaringType}.{handlers.Method.Name}()");
                }

                val.listenerList = valList;
                messageListenerList.Add(val);
            }
        }

        #endif

        #endregion

        /// <summary>
        /// 消息处理器
        /// 返回终止处理标志: 禁止后续处理返回 true, 允许后续处理返回 false.
        /// </summary>
        public delegate bool MessageHandler(BaseMessage message);

        /// <summary>
        /// 单例
        /// </summary>
        public static MessageSystem Instance => InstanceBase as MessageSystem;

        /// <summary>
        /// 监听字典
        /// </summary>
        private readonly Dictionary<string, List<MessageHandler>> listenerDic = new Dictionary<string, List<MessageHandler>>();

        /// <summary>
        /// 消息队列的最大处理时长
        /// </summary>
        private const float MAX_QUEUE_PROCESS_TIME = 0.16667f;

        /// <summary>
        /// 消息队列
        /// </summary>
        private readonly Queue<BaseMessage> messageQueue = new Queue<BaseMessage>();
        
        /// <summary>
        /// 帧更新
        /// </summary>
        private void Update()
        {
            var timer = 0f;
            while (messageQueue.Count > 0)
            {
                if (timer > MAX_QUEUE_PROCESS_TIME)
                {
                    return;
                }

                // 处理消息
                var message = messageQueue.Dequeue();
                if (TriggerMessage(message))
                {
                    timer += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 消息入队
        /// </summary>
        private bool EnqueueMessage(BaseMessage message)
        {
            var flag = false;
            if (messageQueue.Contains(message) == false)
            {
                messageQueue.Enqueue(message);
                flag = true;
            }

            return flag;
        }

        /// <summary>
        /// 触发消息
        /// 正常情况下是在 Update 中触发事件 (下一帧)
        /// </summary>
        private bool TriggerMessage(BaseMessage msg)
        {
            var msgName = msg.messageName;
            if (listenerDic.ContainsKey(msgName) == false)
            {
                DebugUtil.LogError($"没有监听器在监听该消息 {msgName}, 因此忽略该消息!", null, "red");
                return false;
            }

            var listenerList = listenerDic[msgName];
            var listenerCount = listenerList.Count;
            for (var i = 0; i < listenerCount; ++i)
            {
                // 如果有消息禁止了后续的消息处理, 则中止消息处理
                var isOverHandle = listenerList[i](msg);

                if (listenerList.Count != listenerCount)
                {
                    DebugUtil.Log($"消息 {msgName} 的监听者被动态修改了, {listenerCount} => {listenerList.Count}, 请检查是否在该消息的处理方法中注册或注销了该消息的监听!", this, "yellow");
                }

                if (isOverHandle)
                {
                    return true;
                }
            }

            return true;
        }

        #region 对外 API

        /// <summary>
        /// 注册监听
        /// </summary>
        /// <returns>成功标志</returns>
        public bool AddListener<T>(MessageHandler handler) where T : BaseMessage
        {
            var flag = false;

            var msgName = typeof(T).Name;
            if (listenerDic.ContainsKey(msgName) == false)
            {
                listenerDic.Add(msgName, new List<MessageHandler>());
            }

            var listenerList = listenerDic[msgName];
            if (listenerList.Contains(handler) == false)
            {
                listenerList.Add(handler);
                flag = true;
            }

            return flag;
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        /// <returns>成功标志</returns>
        public bool RemoveListener<T>(MessageHandler handler) where T : BaseMessage
        {
            var flag = false;

            var msgName = typeof(T).Name;
            if (listenerDic.ContainsKey(msgName) == false)
            {
                DebugUtil.LogError($"全局消息系统: 监听移除失败, 因为此消息 {msgName} 当前没有任何监听者, 请排查错误!", null, "red");
            }

            var listenerList = listenerDic[msgName];
            if (listenerList.Contains(handler) == false)
            {
                DebugUtil.LogError($"全局消息系统: 监听移除失败, 因为待移除的监听器并没有监听该类型的消息 {msgName}!", null, "red");
            }
            else
            {
                listenerList.Remove(handler);
                flag = true;
            }

            return flag;
        }

        /// <summary>
        /// 向消息系统发送一条消息 (将当前消息排队到消息队列, 等待处理)
        /// </summary>
        public bool Send(BaseMessage msg)
        {
            return EnqueueMessage(msg);
        }

        /// <summary>
        /// 向消息系统请求一条消息 (插队, 立即处理当前消息)
        /// </summary>
        public bool Request(BaseMessage msg)
        {
            return TriggerMessage(msg);
        }

        #endregion
    }
}
