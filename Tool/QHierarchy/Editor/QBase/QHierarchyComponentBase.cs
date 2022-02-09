using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QBase
{
    public class QHierarchyBaseComponent
    {
        /// <summary>
        /// 使能标志
        /// </summary>
        protected bool enabled;
        
        /// <summary>
        /// 播放模式下启用标志
        /// </summary>
        protected bool showComponentDuringPlayMode = false;
        
        /// <summary>
        /// 图标间隔
        /// </summary>
        protected const float COMPONENT_SPACE = 2;
        
        /// <summary>
        /// 控件绘制矩形
        /// </summary>
        protected Rect rect = new Rect(0, 0, 16, 16);

        /// <summary>
        /// 构造方法
        /// </summary>
        protected QHierarchyBaseComponent()
        {
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hierarchyObjectList"></param>
        /// <param name="selectionRect"></param>
        /// <param name="curRect"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        public virtual EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hierarchyObjectList"></param>
        /// <param name="selectionRect"></param>
        public virtual void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect) { }

        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hierarchyObjectList"></param>
        /// <param name="currentEvent"></param>
        public virtual void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent) { }

        /// <summary>
        /// 隐藏事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hierarchyObjectList"></param>
        public virtual void DisabledHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList) { }

        /// <summary>
        /// 判断当前功能是否启用
        /// </summary>
        /// <returns></returns>
        public bool IsEnabled()
        {
            // 如果功能没有启用, 直接返回 false
            if (enabled == false)
            {
                return enabled;
            }
            
            // 编辑器处于编辑状态, 直接返回 true
            // 编辑器处于编辑状态, 且设置了播放状态下启用, 则返回 true
            return Application.isPlaying == false || showComponentDuringPlayMode;
        }

        /// <summary>
        /// 递归获取全部游戏物体
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="result"></param>
        /// <param name="maxDepth"></param>
        protected static void GetGameObjectListRecursive(GameObject gameObject, ref List<GameObject> result, int maxDepth = int.MaxValue)
        {
            result.Add(gameObject);
            if (maxDepth > 0)
            {
                var transform = gameObject.transform;
                for (var index = transform.childCount - 1; index >= 0; index--)
                {
                    GetGameObjectListRecursive(transform.GetChild(index).gameObject, ref result, maxDepth - 1);
                }
            }
        }
    }
}
