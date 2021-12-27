using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QBase
{
    public class QBaseComponent
    {
        protected bool enabled;
        protected bool showComponentDuringPlayMode = false;
        protected Rect rect = new Rect(0, 0, 16, 16);

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
            if (enabled == false)
            {
                return enabled;
            }

            return !Application.isPlaying || showComponentDuringPlayMode;
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
