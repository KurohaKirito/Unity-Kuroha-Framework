using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.RunTime
{
    /// <summary>
    /// 用于展示当前 QHierarchy 插件所引用的游戏物体列表
    /// </summary>
    [ExecuteInEditMode]
    public class QHierarchyObjectList : MonoBehaviour, ISerializationCallbackReceiver
    {
        public static readonly List<QHierarchyObjectList> instances = new List<QHierarchyObjectList>();
        public readonly Dictionary<GameObject, Color> gameObjectColor = new Dictionary<GameObject, Color>();

        [Header("锁定的游戏物体")]
        public List<GameObject> lockedObjects = new List<GameObject>();
        [Header("编辑器下可见的物体")]
        public List<GameObject> editModeVisibleObjects = new List<GameObject>();
        [Header("编辑器下不可见的物体")]
        public List<GameObject> editModeInvisibleObjects = new List<GameObject>();
        [Header("标记颜色的游戏物体")]
        public List<GameObject> gameObjectColorKeys = new List<GameObject>();
        [Header("游戏物体标记的颜色")]
        public List<Color> gameObjectColorValues = new List<Color>();

        public void Awake()
        {
            CheckIntegrity();

            foreach (var editModeGameObject in editModeVisibleObjects)
            {
                editModeGameObject.SetActive(!Application.isPlaying);
            }

            foreach (var editModeGameObject in editModeInvisibleObjects)
            {
                editModeGameObject.SetActive(Application.isPlaying);
            }

            if (Application.isEditor == false && Application.isPlaying)
            {
                instances.Remove(this);
                DestroyImmediate(gameObject);
                return;
            }

            instances.RemoveAll(item => item == null);
            if (instances.Contains(this) == false)
            {
                instances.Add(this);
            }
        }

        public void OnEnable()
        {
            if (instances.Contains(this) == false)
            {
                instances.Add(this);
            }
        }

        public void OnDestroy()
        {
            if (Application.isPlaying == false)
            {
                CheckIntegrity();

                foreach (var obj in editModeVisibleObjects)
                {
                    obj.SetActive(false);
                }

                foreach (var obj in editModeInvisibleObjects)
                {
                    obj.SetActive(true);
                }

                foreach (var obj in lockedObjects)
                {
                    obj.hideFlags &= ~HideFlags.NotEditable;
                }

                instances.Remove(this);
            }
        }

        public void Merge(QHierarchyObjectList anotherInstance)
        {
            for (var index = 0; index < anotherInstance.lockedObjects.Count; index++)
            {
                if (lockedObjects.Contains(anotherInstance.lockedObjects[index]) == false)
                {
                    lockedObjects.Add(anotherInstance.lockedObjects[index]);
                }
            }

            for (var index = 0; index < anotherInstance.editModeVisibleObjects.Count; index++)
            {
                if (editModeVisibleObjects.Contains(anotherInstance.editModeVisibleObjects[index]) == false)
                {
                    editModeVisibleObjects.Add(anotherInstance.editModeVisibleObjects[index]);
                }
            }

            for (var index = 0; index < anotherInstance.editModeInvisibleObjects.Count; index++)
            {
                if (editModeInvisibleObjects.Contains(anotherInstance.editModeInvisibleObjects[index]) == false)
                {
                    editModeInvisibleObjects.Add(anotherInstance.editModeInvisibleObjects[index]);
                }
            }

            for (var index = 0; index < anotherInstance.gameObjectColorKeys.Count; index++)
            {
                if (gameObjectColorKeys.Contains(anotherInstance.gameObjectColorKeys[index]) == false)
                {
                    gameObjectColorKeys.Add(anotherInstance.gameObjectColorKeys[index]);
                    gameObjectColorValues.Add(anotherInstance.gameObjectColorValues[index]);
                    gameObjectColor.Add(anotherInstance.gameObjectColorKeys[index], anotherInstance.gameObjectColorValues[index]);
                }
            }
        }

        public void CheckIntegrity()
        {
            lockedObjects.RemoveAll(item => item == null);
            editModeVisibleObjects.RemoveAll(item => item == null);
            editModeInvisibleObjects.RemoveAll(item => item == null);

            // 因为需要移除元素, 所以必须倒序
            for (var index = gameObjectColorKeys.Count - 1; index >= 0; index--)
            {
                if (gameObjectColorKeys[index] == null)
                {
                    gameObjectColorKeys.RemoveAt(index);
                    gameObjectColorValues.RemoveAt(index);
                }
            }

            OnAfterDeserialize();
        }

        public void OnBeforeSerialize()
        {
            gameObjectColorKeys.Clear();
            gameObjectColorValues.Clear();
            
            foreach (var pair in gameObjectColor)
            {
                gameObjectColorKeys.Add(pair.Key);
                gameObjectColorValues.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            gameObjectColor.Clear();
            
            for (var index = 0; index < gameObjectColorKeys.Count; index++)
            {
                gameObjectColor.Add(gameObjectColorKeys[index], gameObjectColorValues[index]);
            }
        }
    }
}
