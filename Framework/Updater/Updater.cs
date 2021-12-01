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

        #region 编辑器 API

        #if UNITY_EDITOR
        [Header("帧更新列表")]
        [SerializeField]
        private List<string> updaterList;
        #endif
        
        #endregion
        
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
        public void Register(IUpdateable updateable)
        {
            if (MessageSystem.Instance.AddListener<UpdateMessage>(updateable.OnUpdate))
            {
                #if UNITY_EDITOR
                updaterList ??= new List<string>(5);
                updaterList.Add(updateable.GetType().FullName);
                #endif
                
                DebugUtil.Log($"{updateMessage?.deltaTime} 成功注册帧更新事件!");
            }
        }

        /// <summary>
        /// 注销帧更新
        /// </summary>
        /// <param name="updateable"></param>
        public void Unregister(IUpdateable updateable)
        {
            if (MessageSystem.Instance.RemoveListener<UpdateMessage>(updateable.OnUpdate))
            {
                #if UNITY_EDITOR
                updaterList.Remove(updateable.GetType().FullName);
                #endif
                
                DebugUtil.Log($"{updateMessage?.deltaTime} 成功注销帧更新事件!");
            }
        }
    }
}
