using System;
using System.Collections.Generic;
using Kuroha.Framework.Message;
using Kuroha.Framework.Singleton;
using Kuroha.Framework.UI.Panel;
using Kuroha.Framework.UI.Window;
using Kuroha.Framework.Updater;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.UI.Manager
{
    public class UIManager : Singleton<UIManager>, IUpdateable
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static UIManager Instance => InstanceBase as UIManager;

        /// <summary>
        /// 唯一相机
        /// </summary>
        public Camera mainCamera;
        
        /// <summary>
        /// Panel Manager
        /// </summary>
        public UIPanelManager Panel { get; private set; }
        
        /// <summary>
        /// Window Manager
        /// </summary>
        public UIWindowManager Window { get; private set; }

        /// <summary>
        /// Panel 类面板的父物体
        /// </summary>
        public Transform panelParent;

        /// <summary>
        /// Window 类面板的父物体
        /// </summary>
        /// <returns></returns>
        public Transform windowParent;
        
        /// <summary>
        /// UI 帧更新事件
        /// </summary>
        private event Action UpdateEvent;

        /// <summary>
        /// UI 帧更新事件列表
        /// </summary>
        [SerializeField]
        private List<string> eventNameList = new List<string>();

        /// <summary>
        /// 单例
        /// </summary>
        public override void OnLauncher()
        {
            if (ReferenceEquals(panelParent, null) == false && ReferenceEquals(windowParent, null) == false)
            {
                Panel = new UIPanelManager(panelParent);
                Window = new UIWindowManager(windowParent);
            }
            else
            {
                DebugUtil.LogError("Panel Parent 或者 Window Parent 未赋值!", this, "red");
            }
            
            if (ReferenceEquals(mainCamera, null))
            {
                DebugUtil.LogError("Main Camera 未赋值!", this, "red");
            }
        }

        /// <summary>
        /// 帧更新
        /// </summary>
        public bool OnUpdate(BaseMessage message)
        {
            UpdateEvent?.Invoke();
            return false;
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public void AddUpdateListener(Action action, string methodName)
        {
            eventNameList.Add(methodName);
            UpdateEvent += action;
            Updater.Updater.Instance.Register(this);
        }
        
        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveUpdateListener(Action action, string methodName)
        {
            eventNameList.Remove(methodName);
            UpdateEvent -= action;
            Updater.Updater.Instance.Unregister(this);
        }
    }
}
