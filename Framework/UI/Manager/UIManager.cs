using System;
using System.Collections.Generic;
using Kuroha.Framework.Message;
using Kuroha.Framework.Singleton;
using Kuroha.Framework.UI.Panel;
using Kuroha.Framework.UI.Window;
using Kuroha.Framework.Updater;
using UnityEngine;

namespace Kuroha.Framework.UI.Manager
{
    public class UIManager : Singleton<UIManager>, IUpdater
    {
        /// <summary>
        /// Panel 类面板的父物体
        /// </summary>
        private Transform panelParent;

        /// <summary>
        /// Window 类面板的父物体
        /// </summary>
        /// <returns></returns>
        private Transform windowParent;
        
        /// <summary>
        /// 单例
        /// </summary>
        public static UIManager Instance => InstanceBase as UIManager;
        
        /// <summary>
        /// 主相机
        /// </summary>
        public Camera MainCamera { get; private set; }

        /// <summary>
        /// Panel Manager
        /// </summary>
        public UIPanelManager Panel { get; private set; }

        /// <summary>
        /// Window Manager
        /// </summary>
        public UIWindowManager Window { get; private set; }
        
        /// <summary>
        /// UI 帧更新事件
        /// </summary>
        private event Action UpdateEvent;

        /// <summary>
        /// UI 帧更新事件列表
        /// </summary>
        [SerializeField] private List<string> eventNameList = new List<string>();

        /// <summary>
        /// 单例
        /// </summary>
        protected sealed override void Init()
        {
            if (MainCamera == null || Panel == null || Window == null)
            {
                MainCamera = transform.Find("Camera").GetComponent<Camera>();
            
                panelParent = transform.Find("UGUI/Panel");
                Panel = new UIPanelManager(panelParent);
            
                windowParent = transform.Find("UGUI/Window");
                Window = new UIWindowManager(windowParent);
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
