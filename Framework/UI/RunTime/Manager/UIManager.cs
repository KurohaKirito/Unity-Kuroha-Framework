using System;
using System.Collections.Generic;
using Kuroha.Framework.Message.RunTime;
using Kuroha.Framework.Singleton.RunTime;
using Kuroha.Framework.UI.RunTime.Panel;
using Kuroha.Framework.UI.RunTime.Window;
using Kuroha.Framework.Updater;
using UnityEngine;

namespace Kuroha.Framework.UI.RunTime.Manager
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
        public Camera MainCamera { get; set; }

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
        private event Action UIUpdateEvent;

        /// <summary>
        /// UI 帧更新事件列表
        /// </summary>
        [SerializeField] private List<string> eventNameList = new List<string>();

        /// <summary>
        /// 单例
        /// </summary>
        protected sealed override void AutoInit()
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
        public bool UpdateEvent(BaseMessage message)
        {
            UIUpdateEvent?.Invoke();
            return false;
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public void AddUpdateListener(Action action)
        {
            eventNameList.Add($"{action.Method.DeclaringType}.{action.Method.Name}()");
            UIUpdateEvent += action;
            Updater.Updater.Instance.Register(this);
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveUpdateListener(Action action)
        {
            eventNameList.Remove($"{action.Method.DeclaringType}.{action.Method.Name}()");
            UIUpdateEvent -= action;
            Updater.Updater.Instance.Unregister(this);
        }
    }
}
