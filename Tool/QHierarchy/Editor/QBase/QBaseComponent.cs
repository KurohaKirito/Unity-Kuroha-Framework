using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QBase
{
    public class QBaseComponent
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
        /// 控件绘制矩形
        /// </summary>
        protected Rect rect = new Rect(0, 0, 16, 16);

        /// <summary>
        /// 构造方法
        /// </summary>
        protected QBaseComponent()
        {
        }

        public virtual EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            return EM_QLayoutStatus.Success;
        }

        public virtual void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect) { }

        public virtual void EventHandler(GameObject gameObject, QObjectList objectList, Event currentEvent) { }

        public virtual void DisabledHandler(GameObject gameObject, QObjectList objectList) { }

        public bool IsEnabled()
        {
            // 如果功能没有启用, 直接返回 false
            if (enabled == false)
            {
                return enabled;
            }
            
            // 如果当前处于编辑状态 或者处于播放状态且播放状态下启用, 则返回 true
            return Application.isPlaying == false || showComponentDuringPlayMode;
        }

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
