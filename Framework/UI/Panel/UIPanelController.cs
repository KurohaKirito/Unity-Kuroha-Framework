using UnityEngine;

namespace Kuroha.Framework.UI.Panel
{
    /// <summary>
    /// 抽象类, 所有 Controller 层的父类
    /// </summary>
    public class UIPanelController
    {
        /// <summary>
        /// View 层
        /// </summary>
        protected UIPanelView baseView;
        
        /// <summary>
        /// UI 名 (同时作为 UI 的唯一标识)
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// 得到 UI 在 Hierarchy 面板中的游戏物体
        /// </summary>
        /// <returns></returns>
        public GameObject UI => baseView.gameObject;

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init(in UIPanelView view, in string prefabName)
        {
            baseView = view;
            Name = prefabName;
        }

        /// <summary>
        /// 重置
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// 关闭
        /// </summary>
        protected static void OnButtonClick_Close()
        {
            Kuroha.Framework.UI.Manager.UIManager.UI.Panel.Close();
        }
    }
}
