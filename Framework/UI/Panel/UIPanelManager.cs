﻿using System.Collections.Generic;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.UI.Panel
{
    public class UIPanelManager
    {
        /// <summary>
        /// UI 预制体路径
        /// </summary>
        private const string UI_PREFAB_PATH = "Prefabs/UI/Panel/";
        
        /// <summary>
        /// UI 栈
        /// </summary>
        private readonly Stack<UIPanelController> uiStack = new Stack<UIPanelController>(10);

        /// <summary>
        /// UI 缓存池 (缓存所有已经打开过的 UI)
        /// </summary>
        private readonly Dictionary<string, UIPanelController> uiPool = new Dictionary<string, UIPanelController>(10);

        /// <summary>
        /// 当前 UI
        /// </summary>
        private UIPanelController Current
        {
            get
            {
                if (uiStack == null || uiStack.Count <= 0)
                {
                    return null;
                }
                
                return uiStack.Peek();
            }
        }

        /// <summary>
        /// UI 父物体
        /// </summary>
        private readonly Transform uiParent;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="position"></param>
        public UIPanelManager(Transform position)
        {
            uiParent = position;
        }

        /// <summary>
        /// 打开
        /// </summary>
        public void Open<T>(string uiName) where T : UIPanelController, new()
        {
            // 先检查 UI 是否已经打开了
            if (Current != null && Current.Name == uiName)
            {
                DebugUtil.Log("UI 当前处于打开状态, 请勿重复打开!", null, "red");
            }
            else
            {
                DebugUtil.Log("UI 没有打开", null, "green");
                
                // 如果当前正在显示 UI, 则关闭当前 UI
                Current?.UI.SetActive(false);

                // 先检查 UI 是否已经在缓存池中了
                if (uiPool.ContainsKey(uiName))
                {
                    uiPool[uiName].UI.SetActive(true);
                    uiStack.Push(uiPool[uiName]);
                    DebugUtil.Log("UI 已经在缓存池中了", null, "green");
                }
                else
                {
                    var prefabPath = $"{UI_PREFAB_PATH}{uiName}/{uiName}";
                    var uiPrefab = Resources.Load<GameObject>(prefabPath);
                    var newUI = Object.Instantiate(uiPrefab, uiParent, false);
                    var newView = newUI.GetComponent<UIPanelView>();
                    var newController = new T();
                    newController.Init(newView, uiName);
                    
                    uiStack.Push(newController);
                    uiPool[uiName] = newController;
                    DebugUtil.Log("新建了 UI, 并加入缓存池", null, "green");
                }
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if (Current != null && uiStack.Count > 0)
            {
                // 关闭 (隐藏) 当前 UI
                Current.UI.SetActive(false);
                
                // 当前 UI 出栈
                uiStack.Pop();
            }
            
            // 显示上一级 UI
            if (Current != null && uiStack.Count > 0)
            {
                Current.UI.SetActive(true);
            }
        }
    }
}