using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Object = UnityEngine.Object;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    public class QHierarchyObjectListManager
    {
        private const string Q_OBJECT_LIST_NAME = "QHierarchyObjectList";

        private static QHierarchyObjectListManager instance;

        public static QHierarchyObjectListManager Instance()
        {
            return instance ??= new QHierarchyObjectListManager();
        }

        // PRIVATE
        private int lastSelectionCount;
        private bool showObjectList;
        private bool preventSelectionOfLockedObjects;
        private bool preventSelectionOfLockedObjectsDuringPlayMode;
        private GameObject lastSelectionGameObject;

        // CONSTRUCTOR
        private QHierarchyObjectListManager()
        {
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalShowHiddenQHierarchyObjectList, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockPreventSelectionOfLockedObjects, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockShow, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockShowDuringPlayMode, OnSettingsChanged);
            OnSettingsChanged();
        }

        private void OnSettingsChanged()
        {
            showObjectList = QSettings.Instance().Get<bool>(EM_QHierarchySettings.AdditionalShowHiddenQHierarchyObjectList);
            preventSelectionOfLockedObjects = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockShow) && QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockPreventSelectionOfLockedObjects);
            preventSelectionOfLockedObjectsDuringPlayMode = preventSelectionOfLockedObjects && QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockShowDuringPlayMode);
        }

        private bool IsSelectionChanged()
        {
            if (lastSelectionGameObject != Selection.activeGameObject || lastSelectionCount != Selection.gameObjects.Length)
            {
                lastSelectionGameObject = Selection.activeGameObject;
                lastSelectionCount = Selection.gameObjects.Length;
                return true;
            }

            return false;
        }

        public void Validate()
        {
            QHierarchyObjectList.instances.RemoveAll(item => item == null);

            foreach (var objectList in QHierarchyObjectList.instances)
            {
                objectList.CheckIntegrity();
            }

            objectListDictionary.Clear();

            foreach (var objectList in QHierarchyObjectList.instances)
            {
                objectListDictionary.Add(objectList.gameObject.scene, objectList);
            }
        }

        private readonly Dictionary<Scene, QHierarchyObjectList> objectListDictionary = new Dictionary<Scene, QHierarchyObjectList>();
        private Scene lastActiveScene;
        private int lastSceneCount;

        public void OnEditorUpdate()
        {
            try
            {
                var objectListInstance = QHierarchyObjectList.instances;
                var objectListCount = objectListInstance.Count;
                if (objectListCount > 0)
                {
                    for (var i = objectListCount - 1; i >= 0; --i)
                    {
                        var objectList = objectListInstance[i];
                        var objectListScene = objectList.gameObject.scene;

                        if (objectListDictionary.ContainsKey(objectListScene) && objectListDictionary[objectListScene] == null)
                        {
                            objectListDictionary.Remove(objectListScene);
                        }

                        if (objectListDictionary.ContainsKey(objectListScene))
                        {
                            if (objectListDictionary[objectListScene] != objectList)
                            {
                                objectListDictionary[objectListScene].Merge(objectList);
                                Object.DestroyImmediate(objectList.gameObject);
                            }
                        }
                        else
                        {
                            objectListDictionary.Add(objectListScene, objectList);
                        }
                    }

                    foreach (var objectListKeyValue in objectListDictionary)
                    {
                        var objectList = objectListKeyValue.Value;

                        SetupObjectList(objectList);

                        if (showObjectList && (objectList.gameObject.hideFlags & HideFlags.HideInHierarchy) > 0 ||
                            !showObjectList && (objectList.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0)
                        {
                            objectList.gameObject.hideFlags ^= HideFlags.HideInHierarchy;
                            EditorApplication.DirtyHierarchyWindowSorting();
                        }
                    }

                    if (Application.isPlaying == false && preventSelectionOfLockedObjects ||
                        Application.isPlaying && preventSelectionOfLockedObjectsDuringPlayMode && IsSelectionChanged())
                    {
                        var selections = Selection.gameObjects;
                        var actual = new List<GameObject>(selections.Length);
                        var found = false;
                        for (var i = selections.Length - 1; i >= 0; i--)
                        {
                            var gameObject = selections[i];

                            if (objectListDictionary.ContainsKey(gameObject.scene))
                            {
                                var isLock = objectListDictionary[gameObject.scene].lockedObjects.Contains(selections[i]);
                                if (!isLock)
                                {
                                    actual.Add(selections[i]);
                                }
                                else
                                {
                                    found = true;
                                }
                            }
                        }

                        if (found)
                        {
                            var array = actual.ToArray();
                            if (array is Object[] objArray)
                            {
                                Selection.objects = objArray;
                            }
                        }
                    }

                    lastActiveScene = SceneManager.GetActiveScene();
                    lastSceneCount = EditorSceneManager.loadedSceneCount;
                }
            }
            catch
            {
                // ignored
            }
        }

        public QHierarchyObjectList GetObjectList(GameObject gameObject, bool createIfNotExist = true)
        {
            objectListDictionary.TryGetValue(gameObject.scene, out var hierarchyObjectList);

            if (hierarchyObjectList == null && createIfNotExist)
            {
                hierarchyObjectList = CreateObjectList(gameObject);
                if (gameObject.scene != hierarchyObjectList.gameObject.scene) SceneManager.MoveGameObjectToScene(hierarchyObjectList.gameObject, gameObject.scene);
                objectListDictionary.Add(gameObject.scene, hierarchyObjectList);
            }

            return hierarchyObjectList;
        }

        public bool IsSceneChanged() {
            return lastActiveScene != SceneManager.GetActiveScene() || lastSceneCount != EditorSceneManager.loadedSceneCount;
        }

        private static QHierarchyObjectList CreateObjectList(GameObject gameObject)
        {
            var gameObjectList = new GameObject(Q_OBJECT_LIST_NAME);
            var hierarchyObjectList = gameObjectList.AddComponent<QHierarchyObjectList>();
            SetupObjectList(hierarchyObjectList);
            return hierarchyObjectList;
        }

        private static void SetupObjectList(MonoBehaviour hierarchyObjectList)
        {
            if (hierarchyObjectList.CompareTag("EditorOnly")) hierarchyObjectList.tag = "Untagged";
            var monoScript = MonoScript.FromMonoBehaviour(hierarchyObjectList);
            if (MonoImporter.GetExecutionOrder(monoScript) != -10000) {
                MonoImporter.SetExecutionOrder(monoScript, -10000);
            }
        }
    }
}
