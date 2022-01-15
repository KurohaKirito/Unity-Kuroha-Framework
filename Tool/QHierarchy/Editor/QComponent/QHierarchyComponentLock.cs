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
    public class QHierarchyComponentLock: QHierarchyBaseComponent
    {
        // PRIVATE
        private Color activeColor;
        private Color inactiveColor;
        private Texture2D lockButtonTexture;
        private bool showModifierWarning;
        private int targetLockState = -1;

        // CONSTRUCTOR
        public QHierarchyComponentLock()
        {
            rect.width = 13;

            lockButtonTexture = QResources.Instance().GetTexture(QTexture.QLockButton);

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalShowModifierWarning , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockShow                      , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockShowDuringPlayMode        , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalActiveColor         , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor       , settingsChanged);
            settingsChanged();
        }

        // PRIVATE
        private void settingsChanged()
        {
            showModifierWarning         = QSettings.Instance().Get<bool>(EM_QHierarchySettings.AdditionalShowModifierWarning);
            enabled                     = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockShowDuringPlayMode);
            activeColor                 = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalActiveColor);
            inactiveColor               = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
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

        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {  
            bool isLock = isGameObjectLock(gameObject, hierarchyObjectList);

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

            QHierarchyColorUtils.SetColor(isLock ? activeColor : inactiveColor);
            UnityEngine.GUI.DrawTexture(rect, lockButtonTexture);
            QHierarchyColorUtils.ClearColor();
        }

        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                bool isLock = isGameObjectLock(gameObject, hierarchyObjectList);

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
                        GetGameObjectListRecursive(gameObject, ref targetGameObjects);           
                    }
                }
                else if (currentEvent.alt)
                {
                    if (gameObject.transform.parent != null)
                    {
                        if (!showModifierWarning || EditorUtility.DisplayDialog("Change locking", "Are you sure you want to " + (isLock ? "unlock" : "lock") + " this GameObject and its siblings? (You can disable this warning in the settings)", "Yes", "Cancel"))
                        {
                            GetGameObjectListRecursive(gameObject.transform.parent.gameObject, ref targetGameObjects, 1);
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
                        GetGameObjectListRecursive(gameObject, ref targetGameObjects, 0);
                    };
                }
                
                setLock(targetGameObjects, hierarchyObjectList, !isLock);
                currentEvent.Use();
            }
        } 

        public override void DisabledHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList)
        {	
            if (hierarchyObjectList != null && hierarchyObjectList.lockedObjects.Contains(gameObject))
            {
                hierarchyObjectList.lockedObjects.Remove(gameObject);
                gameObject.hideFlags &= ~HideFlags.NotEditable;
                EditorUtility.SetDirty(gameObject);
            }
        }

        // PRIVATE
        private bool isGameObjectLock(GameObject gameObject, QHierarchyObjectList hierarchyObjectList)
        {
            return hierarchyObjectList == null ? false : hierarchyObjectList.lockedObjects.Contains(gameObject);
        }
        
        private void setLock(List<GameObject> gameObjects, QHierarchyObjectList hierarchyObjectList, bool targetLock)
        {
            if (gameObjects.Count == 0) return;

            if (hierarchyObjectList == null) hierarchyObjectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObjects[0], true);
            Undo.RecordObject(hierarchyObjectList, targetLock ? "Lock" : "Unlock");   
            
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {     
                GameObject curGameObject = gameObjects[i];
                Undo.RecordObject(curGameObject, targetLock ? "Lock" : "Unlock");
                
                if (targetLock)
                {
                    curGameObject.hideFlags |= HideFlags.NotEditable;
                    if (!hierarchyObjectList.lockedObjects.Contains(curGameObject))
                        hierarchyObjectList.lockedObjects.Add(curGameObject);
                }
                else
                {
                    curGameObject.hideFlags &= ~HideFlags.NotEditable;
                    hierarchyObjectList.lockedObjects.Remove(curGameObject);
                }
                
                EditorUtility.SetDirty(curGameObject);
            }
        }
    }
}
