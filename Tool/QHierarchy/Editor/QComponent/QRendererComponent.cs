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
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
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

        public override void DisabledHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList)
        {
            if (hierarchyObjectList != null && hierarchyObjectList.wireframeHiddenObjects.Contains(gameObject))
            {      
                hierarchyObjectList.wireframeHiddenObjects.Remove(gameObject);
                Renderer renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null) setSelectedRenderState(renderer, false);
            }
        }

        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                bool wireframeHiddenObjectsContains = isWireframeHidden(gameObject, hierarchyObjectList);
                if (wireframeHiddenObjectsContains)
                {
                    QHierarchyColorUtils.SetColor(specialColor);
                    UnityEngine.GUI.DrawTexture(rect, rendererButtonTexture);
                    QHierarchyColorUtils.ClearColor();
                }
                else if (renderer.enabled)
                {
                    QHierarchyColorUtils.SetColor(activeColor);
                    UnityEngine.GUI.DrawTexture(rect, rendererButtonTexture);
                    QHierarchyColorUtils.ClearColor();
                }
                else
                {
                    QHierarchyColorUtils.SetColor(inactiveColor);
                    UnityEngine.GUI.DrawTexture(rect, rendererButtonTexture);
                    QHierarchyColorUtils.ClearColor();
                }
            }
        }

        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                Renderer renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    bool wireframeHiddenObjectsContains = isWireframeHidden(gameObject, hierarchyObjectList);
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
                            setWireframeMode(gameObject, hierarchyObjectList, true);
                        }
                    }
                    else
                    {
                        if (wireframeHiddenObjectsContains)
                        {
                            setSelectedRenderState(renderer, false);
                            SceneView.RepaintAll();
                            setWireframeMode(gameObject, hierarchyObjectList, false);
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
        public bool isWireframeHidden(GameObject gameObject, QHierarchyObjectList hierarchyObjectList)
        {
            return hierarchyObjectList == null ? false : hierarchyObjectList.wireframeHiddenObjects.Contains(gameObject);
        }
        
        public void setWireframeMode(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, bool targetWireframe)
        {
            if (hierarchyObjectList == null && targetWireframe) hierarchyObjectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObject, true);
            if (hierarchyObjectList != null)
            {
                Undo.RecordObject(hierarchyObjectList, "Renderer Visibility Change");
                if (targetWireframe) hierarchyObjectList.wireframeHiddenObjects.Add(gameObject);
                else hierarchyObjectList.wireframeHiddenObjects.Remove(gameObject);
                EditorUtility.SetDirty(hierarchyObjectList);
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

