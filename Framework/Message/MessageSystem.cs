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
        /// <summary>
        /// 消息委托
        /// 返回值为消息处理终止标志
        /// 当消息由当前监听者处理完之后禁止后续处理的时候, 返回 true, 禁止后续监听者处理, 返回 false, 则允许后续监听者处理消息.
        /// </summary>
        public delegate bool MessageHandlerDelegate(BaseMessage message);

        /// <summary>
        /// 单例
        /// </summary>
        public static MessageSystem Instance => InstanceBase as MessageSystem;

        /// <summary>
        /// 监听字典
        /// </summary>
        private readonly Dictionary<string, List<MessageHandlerDelegate>> listenerDic = new Dictionary<string, List<MessageHandlerDelegate>>();

        /// <summary>
        /// 注册监听
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns>成功标志</returns>
        public bool AddListener(System.Type type, MessageHandlerDelegate handler)
        {
            var flag = false;

            if (ReferenceEquals(type, null))
            {
                DebugUtil.LogError("全局消息系统: 监听注册失败, 因为没有指定类型!", null, "red");
            }
            else
            {
                var msgName = type.Name;
                if (listenerDic.ContainsKey(msgName) == false)
                {
                    listenerDic.Add(msgName, new List<MessageHandlerDelegate>());
                }

                var listenerList = listenerDic[msgName];
                if (listenerList.Contains(handler) == false)
                {
                    listenerList.Add(handler);
                    flag = true;
                }
            }

            return flag;
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns>成功标志</returns>
        public bool RemoveListener(System.Type type, MessageHandlerDelegate handler)
        {
            var flag = false;

            if (ReferenceEquals(type, null))
            {
                DebugUtil.LogError("全局消息系统: 监听移除失败, 因为没有指定类型!", null, "red");
            }
            else
            {
                var msgName = type.Name;
                if (listenerDic.ContainsKey(msgName) == false)
                {
                    DebugUtil.LogError($"全局消息系统: 监听移除失败, 因为此消息 {nameof(type)} 当前没有任何监听者, 请排查错误!", null, "red");
                }

                var listenerList = listenerDic[msgName];
                if (listenerList.Contains(handler) == false)
                {
                    DebugUtil.LogError($"全局消息系统: 监听移除失败, 因为待移除的监听器并没有监听该类型的消息 {nameof(type)}!", null, "red");
                }
                else
                {
                    listenerList.Remove(handler);
                    flag = true;
                }
            }

            return flag;
        }

        /// <summary>
        /// 消息队列
        /// </summary>
        private readonly Queue<BaseMessage> messageQueue = new Queue<BaseMessage>();

        /// <summary>
        /// 消息入队
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool EnqueueMessage(BaseMessage message)
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
        /// 消息队列的最大处理时长
        /// </summary>
        public float maxQueueProcessTime = 0.16667f;

        /// <summary>
        /// 帧更新
        /// </summary>
        private void Update()
        {
            var timer = 0f;
            while (messageQueue.Count > 0)
            {
                if (maxQueueProcessTime > 0)
                {
                    if (timer > maxQueueProcessTime)
                    {
                        return;
                    }
                }

                // 出队, 处理消息
                var message = messageQueue.Dequeue();
                if (TriggerMessage(message))
                {
                    timer += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 触发消息
        /// 正常情况下是在 Update 中触发事件 (下一帧)
        /// 设置为 Public 外部也可以通过调用该方法立即触发事件
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool TriggerMessage(BaseMessage msg)
        {
            var msgName = msg.messageName;
            if (listenerDic.ContainsKey(msgName) == false)
            {
                DebugUtil.LogError($"没有监听器在监听该消息 {msgName}, 因此忽略该消息!", null, "red");
                return false;
            }
            
            var listenerList = listenerDic[msgName];
            foreach (var listener in listenerList)
            {
                // 如果有消息禁止了后续的消息处理, 则中止消息处理
                if (listener(msg))
                {
                    return true;
                }
            }

            return true;
        }
    }
}