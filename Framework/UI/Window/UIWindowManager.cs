using System.Collections.Generic;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.UI.Window
{
    public class UIWindowManager
    {
        /// <summary>
        /// UI 预制体路径
        /// </summary>
        private const string UI_PREFAB_PATH = "Prefabs/UI/Window/";

        /// <summary>
        /// UI 池
        /// </summary>
        private readonly Dictionary<string, UIWindowController> uiPool = new Dictionary<string, UIWindowController>(5);

        /// <summary>
        /// 当前 UI
        /// </summary>
        private UIWindowController current;

        /// <summary>
        /// UI 父物体
        /// </summary>
        private readonly Transform uiParent;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="position"></param>
        public UIWindowManager(Transform position)
        {
            uiParent = position;
        }

        /// <summary>
        /// 打开
        /// </summary>
        public UIWindowController Open<T>(string uiName, string message) where T : UIWindowController, new()
        {
            // 先检查 UI 是否已经打开了
            if (current != null && current.Name == uiName)
            {
                DebugUtil.Log("UI 当前处于打开状态, 请勿重复打开!", null, "red");
            }
            else
            {
                // 如果当前正在显示 UI, 则关闭当前 UI
                current?.UI.SetActive(false);

                // 如果 UI 已经在缓存池中了
                if (uiPool.ContainsKey(uiName))
                {
                    uiPool[uiName].Display(message);
                    uiPool[uiName].Reset();
                    current = uiPool[uiName];
                    DebugUtil.Log("UI 已经在缓存池中了", null, "green");
                }
                else
                {
                    var prefabPath = $"{UI_PREFAB_PATH}{uiName}/{uiName}";
                    var uiPrefab = Resources.Load<GameObject>(prefabPath);
                    var newUI = Object.Instantiate(uiPrefab, uiParent, false);
                    var newView = newUI.GetComponent<UIWindowView>();
                    
                    var newController = new T();
                    newController.Init(newView, uiName);
                    newController.Display(message);
                    
                    current = newController;
                    uiPool[uiName] = newController;
                    DebugUtil.Log("新建了 UI, 并加入缓存池", null, "green");
                }
            }

            return current;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            // 关闭 (隐藏) 当前 UI
            current?.Hide();
            
            // 清空 current
            current = null;
        }
    }
}