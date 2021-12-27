using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using qtools.qhierarchy.phierarchy;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QLockComponent: QBaseComponent
    {
        // PRIVATE
        private Color activeColor;
        private Color inactiveColor;
        private Texture2D lockButtonTexture;
        private bool showModifierWarning;
        private int targetLockState = -1;

        // CONSTRUCTOR
        public QLockComponent()
        {
            rect.width = 13;

            lockButtonTexture = QResources.getInstance().getTexture(QTexture.QLockButton);

            QSettings.getInstance().addEventListener(EM_QSetting.AdditionalShowModifierWarning , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.LockShow                      , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.LockShowDuringPlayMode        , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.AdditionalActiveColor         , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.AdditionalInactiveColor       , settingsChanged);
            settingsChanged();
        }

        // PRIVATE
        private void settingsChanged()
        {
            showModifierWarning         = QSettings.getInstance().get<bool>(EM_QSetting.AdditionalShowModifierWarning);
            enabled                     = QSettings.getInstance().get<bool>(EM_QSetting.LockShow);
            showComponentDuringPlayMode = QSettings.getInstance().get<bool>(EM_QSetting.LockShowDuringPlayMode);
            activeColor                 = QSettings.getInstance().getColor(EM_QSetting.AdditionalActiveColor);
            inactiveColor               = QSettings.getInstance().getColor(EM_QSetting.AdditionalInactiveColor);
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < 13)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= 13;
                rect.x = curRect.x;
                rect.y = curRect.y;
                return EM_QLayoutStatus.Success;
            }
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {  
            bool isLock = isGameObjectLock(gameObject, objectList);

            if (isLock == true && (gameObject.hideFlags & HideFlags.NotEditable) != HideFlags.NotEditable)
            {
                gameObject.hideFlags |= HideFlags.NotEditable;
                EditorUtility.SetDirty(gameObject);
            }
            else if (isLock == false && (gameObject.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable)
            {
                gameObject.hideFlags ^= HideFlags.NotEditable;
                EditorUtility.SetDirty(gameObject);
            }

            QColorUtils.setColor(isLock ? activeColor : inactiveColor);
            UnityEngine.GUI.DrawTexture(rect, lockButtonTexture);
            QColorUtils.clearColor();
        }

        public override void EventHandler(GameObject gameObject, QObjectList objectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                bool isLock = isGameObjectLock(gameObject, objectList);

                if (currentEvent.type == EventType.MouseDown)
                {
                    targetLockState = ((!isLock) == true ? 1 : 0);
                }
                else if (currentEvent.type == EventType.MouseDrag && targetLockState != -1)
                {
                    if (targetLockState == (isLock == true ? 1 : 0)) return;
                } 
                else
                {
                    targetLockState = -1;
                    return;
                }

                List<GameObject> targetGameObjects = new List<GameObject>();
                if (currentEvent.shift) 
                {
                    if (!showModifierWarning || EditorUtility.DisplayDialog("Change locking", "Are you sure you want to " + (isLock ? "unlock" : "lock") + " this GameObject and all its children? (You can disable this warning in the settings)", "Yes", "Cancel"))
                    {
                        GETGameObjectListRecursive(gameObject, ref targetGameObjects);           
                    }
                }
                else if (currentEvent.alt)
                {
                    if (gameObject.transform.parent != null)
                    {
                        if (!showModifierWarning || EditorUtility.DisplayDialog("Change locking", "Are you sure you want to " + (isLock ? "unlock" : "lock") + " this GameObject and its siblings? (You can disable this warning in the settings)", "Yes", "Cancel"))
                        {
                            GETGameObjectListRecursive(gameObject.transform.parent.gameObject, ref targetGameObjects, 1);
                            targetGameObjects.Remove(gameObject.transform.parent.gameObject);
                        }
                    }
                    else
                    {
                        Debug.Log("This action for root objects is supported only for Unity3d 5.3.3 and above");
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
                        GETGameObjectListRecursive(gameObject, ref targetGameObjects, 0);
                    };
                }
                
                setLock(targetGameObjects, objectList, !isLock);
                currentEvent.Use();
            }
        } 

        public override void DisabledHandler(GameObject gameObject, QObjectList objectList)
        {	
            if (objectList != null && objectList.lockedObjects.Contains(gameObject))
            {
                objectList.lockedObjects.Remove(gameObject);
                gameObject.hideFlags &= ~HideFlags.NotEditable;
                EditorUtility.SetDirty(gameObject);
            }
        }

        // PRIVATE
        private bool isGameObjectLock(GameObject gameObject, QObjectList objectList)
        {
            return objectList == null ? false : objectList.lockedObjects.Contains(gameObject);
        }
        
        private void setLock(List<GameObject> gameObjects, QObjectList objectList, bool targetLock)
        {
            if (gameObjects.Count == 0) return;

            if (objectList == null) objectList = QObjectListManager.getInstance().getObjectList(gameObjects[0], true);
            Undo.RecordObject(objectList, targetLock ? "Lock" : "Unlock");   
            
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {     
                GameObject curGameObject = gameObjects[i];
                Undo.RecordObject(curGameObject, targetLock ? "Lock" : "Unlock");
                
                if (targetLock)
                {
                    curGameObject.hideFlags |= HideFlags.NotEditable;
                    if (!objectList.lockedObjects.Contains(curGameObject))
                        objectList.lockedObjects.Add(curGameObject);
                }
                else
                {
                    curGameObject.hideFlags &= ~HideFlags.NotEditable;
                    objectList.lockedObjects.Remove(curGameObject);
                }
                
                EditorUtility.SetDirty(curGameObject);
            }
        }
    }
}

