using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.Editor.QComponent;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QHierarchy
{
    /// <summary>
    /// QHierarchyMain 工具主类
    /// </summary>
    public class QHierarchyMain
    {
        // PRIVATE
        private readonly HashSet<int> errorHandled = new HashSet<int>();
        
        /// <summary>
        /// 功能组件字典
        /// </summary>
        private readonly Dictionary<QHierarchyComponentEnum, QBaseComponent> componentDictionary;
        private readonly List<QBaseComponent> preComponents;
        private readonly List<QBaseComponent> orderedComponents;
        private readonly Texture2D trimIcon;

        private int indentation;
        private bool hideIconsIfThereIsNoFreeSpace;
        private Color inactiveColor;
        private Color backgroundColor;

        // CONSTRUCTOR
        public QHierarchyMain() {
            componentDictionary = new Dictionary<QHierarchyComponentEnum, QBaseComponent>
            {
                {
                    QHierarchyComponentEnum.LockComponent, new QLockComponent()
                },
                {
                    QHierarchyComponentEnum.VisibilityComponent, new QVisibilityComponent()
                }, {
                    QHierarchyComponentEnum.StaticComponent, new QStaticComponent()
                }, {
                    QHierarchyComponentEnum.RendererComponent, new QRendererComponent()
                }, {
                    QHierarchyComponentEnum.TagAndLayerComponent, new QTagLayerComponent()
                }, {
                    QHierarchyComponentEnum.GameObjectIconComponent, new QGameObjectIconComponent()
                }, {
                    QHierarchyComponentEnum.ErrorComponent, new QErrorComponent()
                }, {
                    QHierarchyComponentEnum.TagIconComponent, new QTagIconComponent()
                }, {
                    QHierarchyComponentEnum.LayerIconComponent, new QLayerIconComponent()
                }, {
                    QHierarchyComponentEnum.ColorComponent, new QColorComponent()
                }, {
                    QHierarchyComponentEnum.ComponentsComponent, new QComponentsComponent()
                }, {
                    QHierarchyComponentEnum.ChildrenCountComponent, new QHierarchyComponentChildrenCount()
                }, {
                    QHierarchyComponentEnum.PrefabComponent, new QPrefabComponent()
                }, {
                    QHierarchyComponentEnum.VerticesAndTrianglesCount, new QVerticesAndTrianglesCountComponent()
                }
            };

            preComponents = new List<QBaseComponent> {
                new QMonoBehaviorIconComponent(), new QTreeMapComponent(), new QSeparatorComponent()
            };

            orderedComponents = new List<QBaseComponent>();

            trimIcon = QResources.Instance().GetTexture(QTexture.QTrimIcon);

            QSettings.Instance().addEventListener(EM_QSetting.AdditionalIndentation, OnSettingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.ComponentsOrder, OnSettingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.AdditionalHideIconsIfNotFit, OnSettingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.AdditionalBackgroundColor, OnSettingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.AdditionalInactiveColor, OnSettingsChanged);

            OnSettingsChanged();
        }

        
        /// <summary>
        /// 修改设置事件
        /// </summary>
        private void OnSettingsChanged()
        {
            var componentOrder = QSettings.Instance().Get<string>(EM_QSetting.ComponentsOrder);
            
            var componentIds = componentOrder.Split(';');
            
            if (componentIds.Length != QSettings.DEFAULT_ORDER_COUNT)
            {
                QSettings.Instance().Set(EM_QSetting.ComponentsOrder, QSettings.DEFAULT_ORDER, false);
                componentIds = QSettings.DEFAULT_ORDER.Split(';');
            }

            orderedComponents.Clear();

            foreach (var stringID in componentIds)
            {
                orderedComponents.Add(componentDictionary[(QHierarchyComponentEnum)Enum.Parse(typeof(QHierarchyComponentEnum), stringID)]);
            }
            
            orderedComponents.Add(componentDictionary[QHierarchyComponentEnum.ComponentsComponent]);

            indentation = QSettings.Instance().Get<int>(EM_QSetting.AdditionalIndentation);
            hideIconsIfThereIsNoFreeSpace = QSettings.Instance().Get<bool>(EM_QSetting.AdditionalHideIconsIfNotFit);
            backgroundColor = QSettings.Instance().getColor(EM_QSetting.AdditionalBackgroundColor);
            inactiveColor = QSettings.Instance().getColor(EM_QSetting.AdditionalInactiveColor);
        }

        public void HierarchyWindowItemOnGUIHandler(int instanceId, Rect selectionRect) {
            try {
                QColorUtils.setDefaultColor(UnityEngine.GUI.color);

                GameObject gameObject = (GameObject)EditorUtility.InstanceIDToObject(instanceId);
                if (gameObject == null)
                    return;

                Rect curRect = new Rect(selectionRect);
                curRect.width = 16;
                curRect.x += selectionRect.width - indentation;

                float gameObjectNameWidth = hideIconsIfThereIsNoFreeSpace? UnityEngine.GUI.skin.label.CalcSize(new GUIContent(gameObject.name)).x : 0;

                QObjectList objectList = QObjectListManager.Instance().getObjectList(gameObject, false);

                DrawComponents(orderedComponents, selectionRect, ref curRect, gameObject, objectList, true, hideIconsIfThereIsNoFreeSpace? selectionRect.x + gameObjectNameWidth + 7 : 0);

                errorHandled.Remove(instanceId);
            } catch (Exception exception) {
                if (errorHandled.Add(instanceId)) {
                    Debug.LogError(exception.ToString());
                }
            }
        }

        private void DrawComponents(in List<QBaseComponent> components, Rect selectionRect, ref Rect rect,
            GameObject gameObject, QObjectList objectList, bool drawBackground = false, float minX = 50)
        {
            if (Event.current.type == EventType.Repaint)
            {
                var toComponent = components.Count;
                
                var layoutStatus = EM_QLayoutStatus.Success;

                var componentCount = toComponent;
                for (var i = 0; i < componentCount; i++)
                {
                    var component = components[i];
                    if (component.IsEnabled())
                    {
                        layoutStatus = component.Layout(gameObject, objectList, selectionRect, ref rect, rect.x - minX);
                        if (layoutStatus != EM_QLayoutStatus.Success)
                        {
                            toComponent = layoutStatus == EM_QLayoutStatus.Failed? i : i + 1;
                            rect.x -= 7;

                            break;
                        }
                    }
                    else
                    {
                        component.DisabledHandler(gameObject, objectList);
                    }
                }

                if (drawBackground)
                {
                    if (backgroundColor.a != 0) {
                        rect.width = selectionRect.x + selectionRect.width - rect.x /*- indentation*/;
                        EditorGUI.DrawRect(rect, backgroundColor);
                    }

                    DrawComponents(preComponents, selectionRect, ref rect, gameObject, objectList);
                }

                for (var i = 0; i < toComponent; i++)
                {
                    var component = components[i];
                    if (component.IsEnabled())
                    {
                        component.Draw(gameObject, objectList, selectionRect);
                    }
                }

                if (layoutStatus != EM_QLayoutStatus.Success)
                {
                    rect.width = 7;
                    QColorUtils.setColor(inactiveColor);
                    UnityEngine.GUI.DrawTexture(rect, trimIcon);
                    QColorUtils.clearColor();
                }
            } else if (Event.current.isMouse)
            {
                for (int i = 0, n = components.Count; i < n; i++) 
                {
                    var component = components[i];
                    if (component.IsEnabled()) {
                        if (component.Layout(gameObject, objectList, selectionRect, ref rect, rect.x - minX) != EM_QLayoutStatus.Failed) {
                            component.EventHandler(gameObject, objectList, Event.current);
                        }
                    }
                }
            }
        }
    }
}