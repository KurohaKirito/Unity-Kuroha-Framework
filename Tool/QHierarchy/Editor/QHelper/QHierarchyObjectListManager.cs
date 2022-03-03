using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    public class QHierarchyObjectListManager
    {
        private static QHierarchyObjectListManager instance;
        public static QHierarchyObjectListManager Instance()
        {
            return instance ??= new QHierarchyObjectListManager();
        }
        
        /// <summary>
        /// 上次选择的数量
        /// </summary>
        private int lastSelectionCount;
        
        /// <summary>
        /// 上次选择的物体
        /// </summary>
        private GameObject lastSelectionGameObject;
        
        /// <summary>
        /// 是否显示物品列表
        /// </summary>
        private bool showObjectList;
        
        /// <summary>
        /// 显示特殊物体列表
        /// </summary>
        private bool showList;
        
        /// <summary>
        /// 播放模式下也显示特殊物体列表
        /// </summary>
        private bool showListPlayMode;
        
        private const string Q_OBJECT_LIST_NAME = "QHierarchyObjectList";
        private readonly Dictionary<Scene, QHierarchyObjectList> objectListDictionary = new Dictionary<Scene, QHierarchyObjectList>();
        
        /// <summary>
        /// 上次打开的场景
        /// </summary>
        private Scene lastActiveScene;
        
        /// <summary>
        /// 上次选择的场景数量
        /// </summary>
        private int lastSceneCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        private QHierarchyObjectListManager()
        {
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalShowHiddenQHierarchyObjectList, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockPreventSelectionOfLockedObjects, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockShow, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockShowDuringPlayMode, OnSettingsChanged);
            
            OnSettingsChanged();
        }

        /// <summary>
        /// 更改设置
        /// </summary>
        private void OnSettingsChanged()
        {
            showObjectList = QSettings.Instance().Get<bool>(EM_QHierarchySettings.AdditionalShowHiddenQHierarchyObjectList);
            showList = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockShow) && QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockPreventSelectionOfLockedObjects);
            showListPlayMode = showList && QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockShowDuringPlayMode);
        }

        /// <summary>
        /// 检测是否选择发生了更改
        /// </summary>
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
        
        /// <summary>
        /// 编辑器更新
        /// </summary>
        public void OnEditorUpdate()
        {
            var objectListInstance = QHierarchyObjectList.instances;
            var objectListCount = objectListInstance.Count;
            if (objectListCount > 0)
            {
                for (var index = 0; index < objectListCount; ++index)
                {
                    var objectList = objectListInstance[index];
                    var objectListScene = objectList.gameObject.scene;
                    
                    if (objectListDictionary.ContainsKey(objectListScene))
                    {
                        // 移除字典中的空值
                        if (objectListDictionary[objectListScene] == null)
                        {
                            objectListDictionary.Remove(objectListScene);
                        }
                        
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

                    var objectListGameObject = objectList.gameObject;
                    var flag1 = showObjectList && (objectListGameObject.hideFlags & HideFlags.HideInHierarchy) > 0;
                    var flag2 = showObjectList == false && (objectListGameObject.hideFlags & HideFlags.HideInHierarchy) == 0;
                    
                    if (flag1 || flag2)
                    {
                        objectList.gameObject.hideFlags ^= HideFlags.HideInHierarchy;
                        EditorApplication.DirtyHierarchyWindowSorting();
                    }
                }
                
                if (showList && Application.isPlaying == false || Application.isPlaying && showListPlayMode && IsSelectionChanged())
                {
                    var selectionGameObjects = Selection.gameObjects;
                    var actual = new List<GameObject>(selectionGameObjects.Length);
                    var found = false;
                    
                    foreach (var obj in selectionGameObjects)
                    {
                        if (objectListDictionary.ContainsKey(obj.scene))
                        {
                            if (objectListDictionary[obj.scene].lockedObjects.Contains(obj))
                            {
                                found = true;
                            }
                            else
                            {
                                actual.Add(obj);
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

        public bool IsSceneChanged()
        {
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
            if (hierarchyObjectList.CompareTag("EditorOnly"))
            {
                hierarchyObjectList.tag = "Untagged";
            }
            
            var monoScript = MonoScript.FromMonoBehaviour(hierarchyObjectList);
            
            if (MonoImporter.GetExecutionOrder(monoScript) != -10000)
            {
                MonoImporter.SetExecutionOrder(monoScript, -10000);
            }
        }
    }
}
