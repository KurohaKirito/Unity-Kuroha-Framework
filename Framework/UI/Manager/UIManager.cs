using System;
using System.Collections.Generic;
using Kuroha.Framework.UI.Panel;
using Kuroha.Framework.UI.Window;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.UI.Manager
{
    public class UIManager : MonoBehaviour
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static UIManager UI { get; private set; }
        
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
        private void Awake()
        {
            if (ReferenceEquals(panelParent, null) == false && ReferenceEquals(windowParent, null) == false)
            {
                UI = this;
                Panel = new UIPanelManager(panelParent);
                Window = new UIWindowManager(windowParent);
            }
            else
            {
                DebugUtil.LogError("UIManager 的变量未赋值!", this, "red");
            }
        }
        
        /// <summary>
        /// 帧更新
        /// </summary>
        private void Update()
        {
            UpdateEvent?.Invoke();
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public void AddListener(Action action, string methodName)
        {
            eventNameList.Add(methodName);
            UpdateEvent += action;
        }
        
        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener(Action action, string methodName)
        {
            eventNameList.Remove(methodName);
            UpdateEvent -= action;
        }
    }
}
