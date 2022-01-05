using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.RunTime
{
    /// <summary>
    /// 用于展示当前 QHierarchy 插件所引用的游戏物体列表
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("")]
    public class QObjectList : MonoBehaviour, ISerializationCallbackReceiver
    {
        public static readonly List<QObjectList> instances = new List<QObjectList>();
        
        public readonly Dictionary<GameObject, Color> gameObjectColor = new Dictionary<GameObject, Color>();

        [Header("锁定的游戏物体")]
        public List<GameObject> lockedObjects = new List<GameObject>();
        [Header("编辑器下可见的物体")]
        public List<GameObject> editModeVisibleObjects = new List<GameObject>();
        [Header("编辑器下不可见的物体")]
        public List<GameObject> editModeInvisibleObjects = new List<GameObject>();
        public List<GameObject> wireframeHiddenObjects = new List<GameObject>();
        public List<GameObject> gameObjectColorKeys = new List<GameObject>();
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

            if (!Application.isEditor && Application.isPlaying)
            {
                instances.Remove(this);
                DestroyImmediate(gameObject);
                return;
            }

            instances.RemoveAll(item => item == null);
            if (!instances.Contains(this)) instances.Add(this);
        }

        public void OnEnable()
        {
            if (!instances.Contains(this)) instances.Add(this);

#if UNITY_EDITOR
            foreach (var wireframeGameObject in wireframeHiddenObjects)
            {
                var rendererComponent = wireframeGameObject.GetComponent<Renderer>();
                if (rendererComponent != null)
                {
                    EditorUtility.SetSelectedRenderState(rendererComponent, EditorSelectedRenderState.Hidden);
                }
            }
#endif
        }

        public void OnDestroy()
        {
            if (!Application.isPlaying)
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

        public void Merge(QObjectList anotherInstance)
        {
            for (var index = anotherInstance.lockedObjects.Count - 1; index >= 0; index--)
            {
                if (!lockedObjects.Contains(anotherInstance.lockedObjects[index]))
                    lockedObjects.Add(anotherInstance.lockedObjects[index]);
            }

            for (var index = anotherInstance.editModeVisibleObjects.Count - 1; index >= 0; index--)
            {
                if (!editModeVisibleObjects.Contains(anotherInstance.editModeVisibleObjects[index]))
                    editModeVisibleObjects.Add(anotherInstance.editModeVisibleObjects[index]);
            }

            for (var index = anotherInstance.editModeInvisibleObjects.Count - 1; index >= 0; index--)
            {
                if (!editModeInvisibleObjects.Contains(anotherInstance.editModeInvisibleObjects[index]))
                    editModeInvisibleObjects.Add(anotherInstance.editModeInvisibleObjects[index]);
            }

            for (var index = anotherInstance.wireframeHiddenObjects.Count - 1; index >= 0; index--)
            {
                if (!wireframeHiddenObjects.Contains(anotherInstance.wireframeHiddenObjects[index]))
                    wireframeHiddenObjects.Add(anotherInstance.wireframeHiddenObjects[index]);
            }

            for (var index = anotherInstance.gameObjectColorKeys.Count - 1; index >= 0; index--)
            {
                if (!gameObjectColorKeys.Contains(anotherInstance.gameObjectColorKeys[index]))
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
            wireframeHiddenObjects.RemoveAll(item => item == null);

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
            for (var i = 0; i < gameObjectColorKeys.Count; i++)
            {
                gameObjectColor.Add(gameObjectColorKeys[i], gameObjectColorValues[i]);
            }
        }
    }
}
