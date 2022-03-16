using Kuroha.Framework.Message;
using Kuroha.Framework.Message.RunTime;
using Kuroha.Framework.Singleton;
using Kuroha.Framework.Singleton.RunTime;
using UnityEngine;

namespace Kuroha.Framework.Updater
{
    /// <summary>
    /// 帧更新器
    ///
    /// 这里的 Update() 方法仅用来给消息系统发送更新消息, 真正的更新逻辑的触发是由消息系统触发的
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

        #if KUROHA_DEBUG_MODE
        [Header("帧更新列表")] [SerializeField] private System.Collections.Generic.List<string> updaterList;
        #endif

        #endregion

        /// <summary>
        /// 单例初始化
        /// </summary>
        protected override void AutoInit()
        {
            base.AutoInit();

            if (updateMessage == null)
            {
                updateMessage = new UpdateMessage(Time.deltaTime);
            }
        }

        /// <summary>
        /// 帧更新
        /// </summary>
        private void Update()
        {
            updateMessage.deltaTime = Time.deltaTime;
            MessageSystem.Instance.Send(updateMessage);
        }

        /// <summary>
        /// 注册帧更新
        /// </summary>
        /// <param name="updater"></param>
        public void Register(IUpdater updater)
        {
            if (MessageSystem.Instance.AddListener<UpdateMessage>(updater.UpdateEvent))
            {
                #if KUROHA_DEBUG_MODE

                if (updaterList == null)
                {
                    updaterList = new System.Collections.Generic.List<string>(5);
                }
                
                updaterList.Add(updater.GetType().FullName);
                
                #endif
            }
        }

        /// <summary>
        /// 注销帧更新
        /// </summary>
        /// <param name="updater"></param>
        public void Unregister(IUpdater updater)
        {
            if (MessageSystem.Instance.RemoveListener<UpdateMessage>(updater.UpdateEvent))
            {
                #if KUROHA_DEBUG_MODE
                updaterList.Remove(updater.GetType().FullName);
                #endif
            }
        }
    }
}
