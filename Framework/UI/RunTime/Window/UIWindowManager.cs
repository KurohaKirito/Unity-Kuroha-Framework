using System.Collections.Generic;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.UI.RunTime.Window
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
        public UIWindowController Open<T>(string message) where T : UIWindowController, new()
        {
            // UI 的 Controller 类的命名规则就是: uiPrefabName_Controller
            var uiPrefabName = typeof(T).Name.Replace("_Controller", "");

            // 先检查 UI 是否已经打开了
            if (current != null && current.Name == uiPrefabName)
            {
                DebugUtil.Log("UI 当前处于打开状态, 请勿重复打开!", null, "red");
            }
            else
            {
                // 如果当前正在显示 UI, 则关闭当前 UI
                current?.UI.SetActive(false);

                // 如果 UI 已经在缓存池中了
                if (uiPool.ContainsKey(uiPrefabName))
                {
                    uiPool[uiPrefabName].Display(message);
                    uiPool[uiPrefabName].Reset();
                    current = uiPool[uiPrefabName];
                    DebugUtil.Log("UI 已经在缓存池中了", null, "green");
                }
                else
                {
                    var prefabPath = $"{UI_PREFAB_PATH}{uiPrefabName}/{uiPrefabName}";
                    var uiPrefab = Resources.Load<GameObject>(prefabPath);
                    if (uiPrefab == null)
                    {
                        DebugUtil.LogError($"未获取到 {prefabPath} 预制体, 请检查命名是否符合规则: uiPrefabName_Controller", null, "red");
                    }

                    var newUI = Object.Instantiate(uiPrefab, uiParent, false);
                    var newView = newUI.GetComponent<UIWindowView>();
                    var newController = new T();
                    newController.Init(newView, uiPrefabName);
                    newController.Display(message);

                    current = newController;
                    uiPool[uiPrefabName] = newController;
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
