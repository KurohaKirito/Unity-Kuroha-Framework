using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QRendererComponent: QBaseComponent
    {
        // PRIVATE
        private Color activeColor;
        private Color inactiveColor;
        private Color specialColor;
        private Texture2D rendererButtonTexture;
        private int targetRendererMode = -1; 

        // CONSTRUCTOR
        public QRendererComponent()
        {
            rect.width = 12;

            rendererButtonTexture = QResources.Instance().GetTexture(QTexture.QRendererButton);

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.RendererShow              , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.RendererShowDuringPlayMode, settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalActiveColor     , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor   , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalSpecialColor    , settingsChanged);
            settingsChanged();
        }

        // PRIVATE
        private void settingsChanged()
        {
            enabled                     = QSettings.Instance().Get<bool>(EM_QHierarchySettings.RendererShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.RendererShowDuringPlayMode);
            activeColor                 = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalActiveColor);
            inactiveColor               = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
            specialColor                = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalSpecialColor);
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < 12)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= 12;
                rect.x = curRect.x;
                rect.y = curRect.y;
                return EM_QLayoutStatus.Success;
            }
        }

        public override void DisabledHandler(GameObject gameObject, QObjectList objectList)
        {
            if (objectList != null && objectList.wireframeHiddenObjects.Contains(gameObject))
            {      
                objectList.wireframeHiddenObjects.Remove(gameObject);
                Renderer renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null) setSelectedRenderState(renderer, false);
            }
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                bool wireframeHiddenObjectsContains = isWireframeHidden(gameObject, objectList);
                if (wireframeHiddenObjectsContains)
                {
                    QColorUtils.SetColor(specialColor);
                    UnityEngine.GUI.DrawTexture(rect, rendererButtonTexture);
                    QColorUtils.ClearColor();
                }
                else if (renderer.enabled)
                {
                    QColorUtils.SetColor(activeColor);
                    UnityEngine.GUI.DrawTexture(rect, rendererButtonTexture);
                    QColorUtils.ClearColor();
                }
                else
                {
                    QColorUtils.SetColor(inactiveColor);
                    UnityEngine.GUI.DrawTexture(rect, rendererButtonTexture);
                    QColorUtils.ClearColor();
                }
            }
        }

        public override void EventHandler(GameObject gameObject, QObjectList objectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                Renderer renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    bool wireframeHiddenObjectsContains = isWireframeHidden(gameObject, objectList);
                    bool isEnabled = renderer.enabled;
                    
                    if (currentEvent.type == EventType.MouseDown)
                    {
                        targetRendererMode = ((!isEnabled) == true ? 1 : 0);
                    }
                    else if (currentEvent.type == EventType.MouseDrag && targetRendererMode != -1)
                    {
                        if (targetRendererMode == (isEnabled == true ? 1 : 0)) return;
                    } 
                    else
                    {
                        targetRendererMode = -1;
                        return;
                    }

                    Undo.RecordObject(renderer, "renderer visibility change");                    
                    
                    if (currentEvent.control || currentEvent.command)
                    {
                        if (!wireframeHiddenObjectsContains)
                        {
                            setSelectedRenderState(renderer, true);
                            SceneView.RepaintAll();
                            setWireframeMode(gameObject, objectList, true);
                        }
                    }
                    else
                    {
                        if (wireframeHiddenObjectsContains)
                        {
                            setSelectedRenderState(renderer, false);
                            SceneView.RepaintAll();
                            setWireframeMode(gameObject, objectList, false);
                        }
                        else
                        {
                            Undo.RecordObject(renderer, isEnabled ? "Disable Component" : "Enable Component");
                            renderer.enabled = !isEnabled;
                        }
                    }
                    
                    EditorUtility.SetDirty(gameObject);
                }
                currentEvent.Use();
            }
        }

        // PRIVATE
        public bool isWireframeHidden(GameObject gameObject, QObjectList objectList)
        {
            return objectList == null ? false : objectList.wireframeHiddenObjects.Contains(gameObject);
        }
        
        public void setWireframeMode(GameObject gameObject, QObjectList objectList, bool targetWireframe)
        {
            if (objectList == null && targetWireframe) objectList = QObjectListManager.Instance().getObjectList(gameObject, true);
            if (objectList != null)
            {
                Undo.RecordObject(objectList, "Renderer Visibility Change");
                if (targetWireframe) objectList.wireframeHiddenObjects.Add(gameObject);
                else objectList.wireframeHiddenObjects.Remove(gameObject);
                EditorUtility.SetDirty(objectList);
            }
        }

        static public void setSelectedRenderState(Renderer renderer, bool visible)
        {
            #if UNITY_5_5_OR_NEWER
            EditorUtility.SetSelectedRenderState(renderer, visible ? EditorSelectedRenderState.Wireframe : EditorSelectedRenderState.Hidden);
            #else
            EditorUtility.SetSelectedWireframeHidden(renderer, visible);
            #endif
        }
    }
}

