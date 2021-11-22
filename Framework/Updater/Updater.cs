using Kuroha.Framework.Message;
using Kuroha.Framework.Singleton;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.Updater
{
    /// <summary>
    /// 帧更新器
    /// </summary>
    public class Updater : Singleton<Updater>
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static Updater Instance => InstanceBase as Updater;

        /// <summary>
        /// 帧更新消息
        /// </summary>
        private UpdateMessage updateMessage;

        /// <summary>
        /// 帧更新
        /// </summary>
        private void Update()
        {
            updateMessage ??= new UpdateMessage(Time.deltaTime);
            updateMessage.deltaTime = Time.deltaTime;
            MessageSystem.Instance.EnqueueMessage(updateMessage);
        }

        /// <summary>
        /// 注册帧更新
        /// </summary>
        /// <param name="updateable"></param>
        public void RegisterUpdateableObject(UpdateableMonoBehaviour updateable)
        {
            DebugUtil.Log($"{Time.deltaTime} 注册帧更新消息成功!", this, "green");
            MessageSystem.Instance.AddListener(typeof(UpdateMessage), updateable.OnUpdate);
        }

        /// <summary>
        /// 注销帧更新
        /// </summary>
        /// <param name="updateable"></param>
        public void UnregisterUpdateableObject(UpdateableMonoBehaviour updateable)
        {
            DebugUtil.Log($"{Time.deltaTime} 注销帧更新消息成功!", this, "green");
            MessageSystem.Instance.RemoveListener(typeof(UpdateMessage), updateable.OnUpdate);
        }
    }
}
