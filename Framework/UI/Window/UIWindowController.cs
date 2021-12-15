using UnityEngine;

namespace Kuroha.Framework.UI.Window
{
    /// <summary>
    /// 抽象类, 所有 Controller 层的父类
    /// </summary>
    public abstract class UIWindowController
    {
        /// <summary>
        /// View 层
        /// </summary>
        protected UIWindowView baseView;

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
        /// 初始化窗口
        /// </summary>
        public virtual void Init(in UIWindowView view, in string prefabName)
        {
            baseView = view;
            Name = prefabName;
        }

        /// <summary>
        /// 重置
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        protected static void OnButtonClick_Close()
        {
            Kuroha.Framework.UI.Manager.UIManager.Instance.Window.Close();
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        public virtual void Display<T>(T content) { }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public virtual void Hide() { }
    }
}
