using System.Collections.Generic;

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
        /// 帧更新器类列表
        /// </summary>
        [SerializeField]
        private List<string> updateClassList;

        /// <summary>
        /// 帧更新
        /// </summary>
        private void Update()
        {
            updateMessage ??= new UpdateMessage(Time.deltaTime);
            updateMessage.deltaTime = Time.deltaTime;
            MessageSystem.Instance.Send(updateMessage);
        }

        /// <summary>
        /// 注册帧更新
        /// </summary>
        /// <param name="updateable"></param>
        public void Register(UpdateableMonoBehaviour updateable)
        {
            if (MessageSystem.Instance.Register<UpdateMessage>(updateable.OnUpdate))
            {
                updateClassList ??= new List<string>(5);
                updateClassList.Add(updateable.GetType().FullName);
                DebugUtil.Log($"{updateMessage?.deltaTime} 成功注册帧更新事件!");
            }
        }

        /// <summary>
        /// 注销帧更新
        /// </summary>
        /// <param name="updateable"></param>
        public void Unregister(UpdateableMonoBehaviour updateable)
        {
            if (MessageSystem.Instance.Unregister<UpdateMessage>(updateable.OnUpdate))
            {
                updateClassList.Remove(updateable.GetType().FullName);
                DebugUtil.Log($"{updateMessage?.deltaTime} 成功注销帧更新事件!");
            }
        }
    }
}
