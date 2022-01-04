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
    public class QVisibilityComponent: QBaseComponent
    {
        // PRIVATE
        private Color activeColor;
        private Color inactiveColor;
        private Color specialColor;
        private Texture2D visibilityButtonTexture;
        private Texture2D visibilityOffButtonTexture;
        private int targetVisibilityState = -1;

        // CONSTRUCTOR
        public QVisibilityComponent()
        {
            rect.width = 18;

            visibilityButtonTexture    = QResources.Instance().GetTexture(QTexture.QVisibilityButton);
            visibilityOffButtonTexture = QResources.Instance().GetTexture(QTexture.QVisibilityOffButton);

            QSettings.Instance().addEventListener(EM_QSetting.VisibilityShow                , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.VisibilityShowDuringPlayMode  , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.AdditionalActiveColor         , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.AdditionalInactiveColor       , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.AdditionalSpecialColor        , settingsChanged);
            settingsChanged();
        }

        private void settingsChanged()
        {
            enabled                     = QSettings.Instance().Get<bool>(EM_QSetting.VisibilityShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QSetting.VisibilityShowDuringPlayMode);
            activeColor                 = QSettings.Instance().GetColor(EM_QSetting.AdditionalActiveColor);
            inactiveColor               = QSettings.Instance().GetColor(EM_QSetting.AdditionalInactiveColor);
            specialColor                = QSettings.Instance().GetColor(EM_QSetting.AdditionalSpecialColor);
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
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

        public override void DisabledHandler(GameObject gameObject, QObjectList objectList)
        {
            if (objectList != null)
            {
                if (gameObject.activeSelf && objectList.editModeVisibleObjects.Contains(gameObject))
                {
                    objectList.editModeVisibleObjects.Remove(gameObject);
                    gameObject.SetActive(false);
                    EditorUtility.SetDirty(gameObject);
                }
                else if (!gameObject.activeSelf && objectList.editModeInvisibleObjects.Contains(gameObject))
                {
                    objectList.editModeInvisibleObjects.Remove(gameObject);
                    gameObject.SetActive(true);
                    EditorUtility.SetDirty(gameObject);
                }
            }
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {
            int visibility = gameObject.activeSelf ? 1 : 0;
            
            bool editModeVisibleObjectsContains = isEditModeVisibile(gameObject, objectList);
            bool editModeInvisibleObjectsContains = isEditModeInvisibile(gameObject, objectList);
            
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
                    QColorUtils.setColor(specialColor);
                    UnityEngine.GUI.DrawTexture(rect, visibilityOffButtonTexture);
                }
                else if (visibility == 1)
                {
                    QColorUtils.setColor(specialColor);
                    UnityEngine.GUI.DrawTexture(rect, visibilityButtonTexture);
                }
                else
                {
                    QColorUtils.setColor(specialColor, 1.0f, 0.4f);
                    UnityEngine.GUI.DrawTexture(rect, editModeVisibleObjectsContains ? visibilityButtonTexture : visibilityOffButtonTexture);
                }
            }
            else
            {
                if (visibility == 0)
                {
                    QColorUtils.setColor(inactiveColor);
                    UnityEngine.GUI.DrawTexture(rect, visibilityOffButtonTexture);
                }
                else if (visibility == 1)
                {
                    QColorUtils.setColor(activeColor);
                    UnityEngine.GUI.DrawTexture(rect, visibilityButtonTexture);
                }
                else
                {
                    if (gameObject.activeSelf)
                    {
                        QColorUtils.setColor(activeColor, 0.65f, 0.65f);
                        UnityEngine.GUI.DrawTexture(rect, visibilityButtonTexture);                    
                    }
                    else
                    {
                        QColorUtils.setColor(inactiveColor, 0.85f, 0.85f);
                        UnityEngine.GUI.DrawTexture(rect, visibilityOffButtonTexture);                    
                    }
                }
            }
            QColorUtils.clearColor();
        }

        public override void EventHandler(GameObject gameObject, QObjectList objectList, Event currentEvent)
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
                                                            
                bool showWarning = QSettings.Instance().Get<bool>(EM_QSetting.AdditionalShowModifierWarning);
                
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
                
                setVisibility(targetGameObjects, objectList, !gameObject.activeSelf, currentEvent.control || currentEvent.command);
                currentEvent.Use();  
            } 
        }

        // PRIVATE
        private bool isEditModeVisibile(GameObject gameObject, QObjectList objectList)
        {
            return objectList == null ? false : objectList.editModeVisibleObjects.Contains(gameObject);
        }
        
        private bool isEditModeInvisibile(GameObject gameObject, QObjectList objectList)
        {
            return objectList == null ? false : objectList.editModeInvisibleObjects.Contains(gameObject);
        }
        
        private void setVisibility(List<GameObject> gameObjects, QObjectList objectList, bool targetVisibility, bool editMode)
        {
            if (gameObjects.Count == 0) return;

            if (objectList == null && editMode) objectList = QObjectListManager.Instance().getObjectList(gameObjects[0], true);
            if (objectList != null) Undo.RecordObject(objectList, "visibility change");
            
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {        
                GameObject curGameObject = gameObjects[i];
                Undo.RecordObject(curGameObject, "visibility change");
                
                if (editMode)
                {
                    if (!targetVisibility)
                    {
                        objectList.editModeVisibleObjects.Remove(curGameObject);        
                        if (!objectList.editModeInvisibleObjects.Contains(curGameObject))
                            objectList.editModeInvisibleObjects.Add(curGameObject);
                    }
                    else
                    {
                        objectList.editModeInvisibleObjects.Remove(curGameObject);                            
                        if (!objectList.editModeVisibleObjects.Contains(curGameObject))
                            objectList.editModeVisibleObjects.Add(curGameObject);
                    }
                }
                else if (objectList != null)
                {
                    objectList.editModeVisibleObjects.Remove(curGameObject);
                    objectList.editModeInvisibleObjects.Remove(curGameObject);
                }
                
                curGameObject.SetActive(targetVisibility);
                EditorUtility.SetDirty(curGameObject);
            }
        }
    }
}
