using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    public class QObjectListManager
    {
        // CONST
        private const string QObjectListName = "QHierarchyObjectList";

        // SINGLETON
        private static QObjectListManager instance;
        public static QObjectListManager Instance()
        {
            return instance ??= new QObjectListManager();
        }

        // PRIVATE
        private bool showObjectList;
        private bool preventSelectionOfLockedObjects;
        private bool preventSelectionOfLockedObjectsDuringPlayMode;
        private GameObject lastSelectionGameObject = null;
        private int lastSelectionCount = 0;

        // CONSTRUCTOR
        private QObjectListManager()
        {
            QSettings.Instance().addEventListener(EM_QSetting.AdditionalShowHiddenQHierarchyObjectList , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.LockPreventSelectionOfLockedObjects, settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.LockShow              , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.LockShowDuringPlayMode, settingsChanged);
            settingsChanged();
        }

        private void settingsChanged()
        {
            showObjectList = QSettings.Instance().Get<bool>(EM_QSetting.AdditionalShowHiddenQHierarchyObjectList);
            preventSelectionOfLockedObjects = QSettings.Instance().Get<bool>(EM_QSetting.LockShow) && QSettings.Instance().Get<bool>(EM_QSetting.LockPreventSelectionOfLockedObjects);
            preventSelectionOfLockedObjectsDuringPlayMode = preventSelectionOfLockedObjects && QSettings.Instance().Get<bool>(EM_QSetting.LockShowDuringPlayMode);
        }

        private bool isSelectionChanged()
        {
            if (lastSelectionGameObject != Selection.activeGameObject || lastSelectionCount  != Selection.gameObjects.Length)
            {
                lastSelectionGameObject = Selection.activeGameObject;
                lastSelectionCount = Selection.gameObjects.Length;
                return true;
            }
            return false;
        }

        public void validate()
        {
            QObjectList.instances.RemoveAll(item => item == null);
            foreach (QObjectList objectList in QObjectList.instances)
                objectList.CheckIntegrity();
            #if UNITY_5_3_OR_NEWER
            objectListDictionary.Clear();
            foreach (QObjectList objectList in QObjectList.instances)            
                objectListDictionary.Add(objectList.gameObject.scene, objectList);
            #endif
        }
        
        private Dictionary<Scene, QObjectList> objectListDictionary = new Dictionary<Scene, QObjectList>();
        private Scene lastActiveScene;
        private int lastSceneCount = 0;

        public void update()
        {
            try
            {     
                List<QObjectList> objectListList = QObjectList.instances;
                int objectListCount = objectListList.Count;
                if (objectListCount > 0) 
                {
                    for (int i = objectListCount - 1; i >= 0; i--)
                    {
                        QObjectList objectList = objectListList[i];
                        Scene objectListScene = objectList.gameObject.scene;
						
						if (objectListDictionary.ContainsKey(objectListScene) && objectListDictionary[objectListScene] == null)
                            objectListDictionary.Remove(objectListScene);
							
                        if (objectListDictionary.ContainsKey(objectListScene))
                        {
                            if (objectListDictionary[objectListScene] != objectList)
                            {
                                objectListDictionary[objectListScene].Merge(objectList);
                                GameObject.DestroyImmediate(objectList.gameObject);
                            }
                        }
                        else
                        {
                            objectListDictionary.Add(objectListScene, objectList);
                        }
                    }

                    foreach (KeyValuePair<Scene, QObjectList> objectListKeyValue in objectListDictionary)
                    {
                        QObjectList objectList = objectListKeyValue.Value;
                        setupObjectList(objectList);
                        if (( showObjectList && ((objectList.gameObject.hideFlags & HideFlags.HideInHierarchy)  > 0)) ||
                            (!showObjectList && ((objectList.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0)))
                        {
                            objectList.gameObject.hideFlags ^= HideFlags.HideInHierarchy;      
                            EditorApplication.DirtyHierarchyWindowSorting();
                        }
                    }
                    
                    if ((!Application.isPlaying && preventSelectionOfLockedObjects) || 
                        ((Application.isPlaying && preventSelectionOfLockedObjectsDuringPlayMode)) && 
                        isSelectionChanged())
                    {
                        GameObject[] selections = Selection.gameObjects;
                        List<GameObject> actual = new List<GameObject>(selections.Length);
                        bool found = false;
                        for (int i = selections.Length - 1; i >= 0; i--)
                        {
                            GameObject gameObject = selections[i];
                            
                            if (objectListDictionary.ContainsKey(gameObject.scene))
                            {
                                bool isLock = objectListDictionary[gameObject.scene].lockedObjects.Contains(selections[i]);
                                if (!isLock) actual.Add(selections[i]);
                                else found = true;
                            }
                        }
                        if (found) Selection.objects = actual.ToArray();
                    }   

                    lastActiveScene = EditorSceneManager.GetActiveScene();
                    lastSceneCount = EditorSceneManager.loadedSceneCount;
                }
            }
            catch 
            {
            }
        }

        public QObjectList getObjectList(GameObject gameObject, bool createIfNotExist = true)
        { 
            QObjectList objectList = null;
            objectListDictionary.TryGetValue(gameObject.scene, out objectList);
            
            if (objectList == null && createIfNotExist)
            {         
                objectList = createObjectList(gameObject);
                if (gameObject.scene != objectList.gameObject.scene) EditorSceneManager.MoveGameObjectToScene(objectList.gameObject, gameObject.scene);
                objectListDictionary.Add(gameObject.scene, objectList);
            }

            return objectList;
        }

        public bool isSceneChanged()
        {
            if (lastActiveScene != EditorSceneManager.GetActiveScene() || lastSceneCount != EditorSceneManager.loadedSceneCount)
                return true;
            else 
                return false;
        }

        private QObjectList createObjectList(GameObject gameObject)
        {
            GameObject gameObjectList = new GameObject();
            gameObjectList.name = QObjectListName;
            QObjectList objectList = gameObjectList.AddComponent<QObjectList>();
            setupObjectList(objectList);
            return objectList;
        }

        private void setupObjectList(QObjectList objectList)
        {
            if (objectList.tag == "EditorOnly") objectList.tag = "Untagged";
            MonoScript monoScript = MonoScript.FromMonoBehaviour(objectList);
            if (MonoImporter.GetExecutionOrder(monoScript) != -10000)                    
                MonoImporter.SetExecutionOrder(monoScript, -10000);
        }
    }
}

