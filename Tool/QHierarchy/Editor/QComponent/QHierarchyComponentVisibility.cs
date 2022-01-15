using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentVisibility: QHierarchyBaseComponent
    {
        // PRIVATE
        private Color activeColor;
        private Color inactiveColor;
        private Color specialColor;
        private Texture2D visibilityButtonTexture;
        private Texture2D visibilityOffButtonTexture;
        private int targetVisibilityState = -1;

        // CONSTRUCTOR
        public QHierarchyComponentVisibility()
        {
            rect.width = 18;

            visibilityButtonTexture    = QResources.Instance().GetTexture(QTexture.QVisibilityButton);
            visibilityOffButtonTexture = QResources.Instance().GetTexture(QTexture.QVisibilityOffButton);
            // visibilityButtonTexture = EditorGUIUtility.IconContent("animationvisibilitytoggleon@2x").image as Texture2D;
            // visibilityOffButtonTexture = EditorGUIUtility.IconContent("animationvisibilitytoggleoff@2x").image as Texture2D;

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VisibilityShow                , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VisibilityShowDuringPlayMode  , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalActiveColor         , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor       , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalSpecialColor        , settingsChanged);
            settingsChanged();
        }

        private void settingsChanged()
        {
            enabled                     = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VisibilityShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VisibilityShowDuringPlayMode);
            activeColor                 = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalActiveColor);
            inactiveColor               = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
            // activeColor = Color.yellow;
            // inactiveColor = Color.white;
            specialColor                = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalSpecialColor);
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < 18)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= 18;
                rect.x = curRect.x;
                rect.y = curRect.y;
                return EM_QLayoutStatus.Success;
            }
        }

        public override void DisabledHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList)
        {
            if (hierarchyObjectList != null)
            {
                if (gameObject.activeSelf && hierarchyObjectList.editModeVisibleObjects.Contains(gameObject))
                {
                    hierarchyObjectList.editModeVisibleObjects.Remove(gameObject);
                    gameObject.SetActive(false);
                    EditorUtility.SetDirty(gameObject);
                }
                else if (!gameObject.activeSelf && hierarchyObjectList.editModeInvisibleObjects.Contains(gameObject))
                {
                    hierarchyObjectList.editModeInvisibleObjects.Remove(gameObject);
                    gameObject.SetActive(true);
                    EditorUtility.SetDirty(gameObject);
                }
            }
        }

        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            int visibility = gameObject.activeSelf ? 1 : 0;
            
            bool editModeVisibleObjectsContains = isEditModeVisibile(gameObject, hierarchyObjectList);
            bool editModeInvisibleObjectsContains = isEditModeInvisibile(gameObject, hierarchyObjectList);
            
            if (!EditorApplication.isPlayingOrWillChangePlaymode && ((!gameObject.activeSelf && editModeVisibleObjectsContains) || (gameObject.activeSelf && editModeInvisibleObjectsContains)))
                gameObject.SetActive(!gameObject.activeSelf);
                        

            Transform transform = gameObject.transform;
            while (transform.parent != null)
            {
                transform = transform.parent;
                if (!transform.gameObject.activeSelf) 
                {
                    visibility = 2;
                    break;
                }
            }
                       
            if (!EditorApplication.isPlayingOrWillChangePlaymode && (editModeVisibleObjectsContains || editModeInvisibleObjectsContains))
            {
                if (visibility == 0)
                {
                    QHierarchyColorUtils.SetColor(specialColor);
                    UnityEngine.GUI.DrawTexture(rect, visibilityOffButtonTexture);
                }
                else if (visibility == 1)
                {
                    QHierarchyColorUtils.SetColor(specialColor);
                    UnityEngine.GUI.DrawTexture(rect, visibilityButtonTexture);
                }
                else
                {
                    QHierarchyColorUtils.SetColor(specialColor, 1.0f, 0.4f);
                    UnityEngine.GUI.DrawTexture(rect, editModeVisibleObjectsContains ? visibilityButtonTexture : visibilityOffButtonTexture);
                }
            }
            else
            {
                if (visibility == 0)
                {
                    QHierarchyColorUtils.SetColor(inactiveColor);
                    UnityEngine.GUI.DrawTexture(rect, visibilityOffButtonTexture);
                }
                else if (visibility == 1)
                {
                    QHierarchyColorUtils.SetColor(activeColor);
                    UnityEngine.GUI.DrawTexture(rect, visibilityButtonTexture);
                }
                else
                {
                    if (gameObject.activeSelf)
                    {
                        QHierarchyColorUtils.SetColor(activeColor, 0.65f, 0.65f);
                        UnityEngine.GUI.DrawTexture(rect, visibilityButtonTexture);                    
                    }
                    else
                    {
                        QHierarchyColorUtils.SetColor(inactiveColor, 0.85f, 0.85f);
                        UnityEngine.GUI.DrawTexture(rect, visibilityOffButtonTexture);                    
                    }
                }
            }
            QHierarchyColorUtils.ClearColor();
        }

        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.type == EventType.MouseDown)
                {
                    targetVisibilityState = ((!gameObject.activeSelf) == true ? 1 : 0);
                }
                else if (currentEvent.type == EventType.MouseDrag && targetVisibilityState != -1)
                {
                    if (targetVisibilityState == (gameObject.activeSelf == true ? 1 : 0)) return;
                } 
                else
                {
                    targetVisibilityState = -1;
                    return;
                }
                                                            
                bool showWarning = QSettings.Instance().Get<bool>(EM_QHierarchySettings.AdditionalShowModifierWarning);
                
                List<GameObject> targetGameObjects = new List<GameObject>();
                if (currentEvent.control || currentEvent.command) 
                {
                    if (currentEvent.shift)
                    {
                        if (!showWarning || EditorUtility.DisplayDialog("Change edit-time visibility", "Are you sure you want to turn " + (gameObject.activeSelf ? "off" : "on") + " the edit-time visibility of this GameObject and all its children? (You can disable this warning in the settings)", "Yes", "Cancel"))
                        {
                            GetGameObjectListRecursive(gameObject, ref targetGameObjects);
                        }
                    }
                    else if (currentEvent.alt)
                    {
                        if (gameObject.transform.parent != null)
                        {
                            if (!showWarning || EditorUtility.DisplayDialog("Change edit-time visibility", "Are you sure you want to turn " + (gameObject.activeSelf ? "off" : "on") + " the edit-time visibility this GameObject and its siblings? (You can disable this warning in the settings)", "Yes", "Cancel"))
                            {
                                GetGameObjectListRecursive(gameObject.transform.parent.gameObject, ref targetGameObjects, 1);
                                targetGameObjects.Remove(gameObject.transform.parent.gameObject);
                            }
                        }
                        else
                        {
                            Debug.Log("This action for root objects is supported for Unity3d 5.3.3 and above");
                            return;
                        }
                    }
                    else
                    {
                        GetGameObjectListRecursive(gameObject, ref targetGameObjects, 0);
                    }
                }
                else if (currentEvent.shift)
                {
                    if (!showWarning || EditorUtility.DisplayDialog("Change visibility", "Are you sure you want to turn " + (gameObject.activeSelf ? "off" : "on") + " the visibility of this GameObject and all its children? (You can disable this warning in the settings)", "Yes", "Cancel"))
                    {
                        GetGameObjectListRecursive(gameObject, ref targetGameObjects);           
                    }
                }
                else if (currentEvent.alt) 
                {
                    if (gameObject.transform.parent != null)
                    {
                        if (!showWarning || EditorUtility.DisplayDialog("Change visibility", "Are you sure you want to turn " + (gameObject.activeSelf ? "off" : "on") + " the visibility this GameObject and its siblings? (You can disable this warning in the settings)", "Yes", "Cancel"))
                        {
                            GetGameObjectListRecursive(gameObject.transform.parent.gameObject, ref targetGameObjects, 1);
                            targetGameObjects.Remove(gameObject.transform.parent.gameObject);
                        }
                    }
                    else
                    {
                        Debug.Log("This action for root objects is supported for Unity3d 5.3.3 and above");
                        return;
                    }
                }
                else 
                {
                    if (Selection.Contains(gameObject))
                    {
                        targetGameObjects.AddRange(Selection.gameObjects);
                    }
                    else
                    {
                        GetGameObjectListRecursive(gameObject, ref targetGameObjects, 0);
                    };
                }
                
                setVisibility(targetGameObjects, hierarchyObjectList, !gameObject.activeSelf, currentEvent.control || currentEvent.command);
                currentEvent.Use();  
            } 
        }

        // PRIVATE
        private bool isEditModeVisibile(GameObject gameObject, QHierarchyObjectList hierarchyObjectList)
        {
            return hierarchyObjectList == null ? false : hierarchyObjectList.editModeVisibleObjects.Contains(gameObject);
        }
        
        private bool isEditModeInvisibile(GameObject gameObject, QHierarchyObjectList hierarchyObjectList)
        {
            return hierarchyObjectList == null ? false : hierarchyObjectList.editModeInvisibleObjects.Contains(gameObject);
        }
        
        private void setVisibility(List<GameObject> gameObjects, QHierarchyObjectList hierarchyObjectList, bool targetVisibility, bool editMode)
        {
            if (gameObjects.Count == 0) return;

            if (hierarchyObjectList == null && editMode) hierarchyObjectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObjects[0], true);
            if (hierarchyObjectList != null) Undo.RecordObject(hierarchyObjectList, "visibility change");
            
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {        
                GameObject curGameObject = gameObjects[i];
                Undo.RecordObject(curGameObject, "visibility change");
                
                if (editMode)
                {
                    if (!targetVisibility)
                    {
                        hierarchyObjectList.editModeVisibleObjects.Remove(curGameObject);        
                        if (!hierarchyObjectList.editModeInvisibleObjects.Contains(curGameObject))
                            hierarchyObjectList.editModeInvisibleObjects.Add(curGameObject);
                    }
                    else
                    {
                        hierarchyObjectList.editModeInvisibleObjects.Remove(curGameObject);                            
                        if (!hierarchyObjectList.editModeVisibleObjects.Contains(curGameObject))
                            hierarchyObjectList.editModeVisibleObjects.Add(curGameObject);
                    }
                }
                else if (hierarchyObjectList != null)
                {
                    hierarchyObjectList.editModeVisibleObjects.Remove(curGameObject);
                    hierarchyObjectList.editModeInvisibleObjects.Remove(curGameObject);
                }
                
                curGameObject.SetActive(targetVisibility);
                EditorUtility.SetDirty(curGameObject);
            }
        }
    }
}