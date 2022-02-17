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
        /// <summary>
        /// 功能组件字典
        /// </summary>
        private readonly Dictionary<EM_QHierarchyComponent, QHierarchyBaseComponent> componentDictionary;

        private readonly List<QHierarchyBaseComponent> preComponents;
        private readonly List<QHierarchyBaseComponent> orderedComponents;
        private readonly Texture2D trimIcon;

        private int indentation;
        private bool hideIconsIfThereIsNoFreeSpace;
        private Color inactiveColor;
        private Color backgroundColor;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyMain()
        {
            componentDictionary = new Dictionary<EM_QHierarchyComponent, QHierarchyBaseComponent>
            {
                {
                    EM_QHierarchyComponent.LockComponent, new QHierarchyComponentLock()
                },
                {
                    EM_QHierarchyComponent.VisibilityComponent, new QHierarchyComponentVisibility()
                },
                {
                    EM_QHierarchyComponent.StaticComponent, new QStaticComponent()
                },
                {
                    EM_QHierarchyComponent.RendererComponent, new QRendererComponent()
                },
                {
                    EM_QHierarchyComponent.TagAndLayerComponent, new QTagLayerComponent()
                },
                {
                    EM_QHierarchyComponent.GameObjectIconComponent, new QHierarchyComponentGameObjectIcon()
                },
                {
                    EM_QHierarchyComponent.ErrorComponent, new QHierarchyComponentError()
                },
                {
                    EM_QHierarchyComponent.TagIconComponent, new QHierarchyComponentTagIcon()
                },
                {
                    EM_QHierarchyComponent.LayerIconComponent, new QHierarchyComponentLayerIcon()
                },
                {
                    EM_QHierarchyComponent.ColorComponent, new QHierarchyComponentColor()
                },
                {
                    EM_QHierarchyComponent.ComponentsComponent, new QHierarchyComponentComponents()
                },
                {
                    EM_QHierarchyComponent.ChildrenCountComponent, new QHierarchyComponentChildrenCount()
                },
                {
                    EM_QHierarchyComponent.PrefabComponent, new QHierarchyComponentPrefab()
                },
                {
                    EM_QHierarchyComponent.VerticesAndTrianglesCount, new QVerticesAndTrianglesCountComponent()
                }
            };

            preComponents = new List<QHierarchyBaseComponent>
            {
                new QHierarchyComponentMonoBehavior(), new QTreeMapComponent(), new QSeparatorComponent()
            };

            orderedComponents = new List<QHierarchyBaseComponent>();

            trimIcon = QResources.Instance().GetTexture(QTexture.QTrimIcon);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalIndentation, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ComponentsOrder, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalHideIconsIfNotFit, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalBackgroundColor, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor, OnSettingsChanged);
            OnSettingsChanged();
        }

        /// <summary>
        /// 修改设置事件
        /// </summary>
        private void OnSettingsChanged()
        {
            var componentOrder = QSettings.Instance().Get<string>(EM_QHierarchySettings.ComponentsOrder);
            var componentIds = componentOrder.Split(';');

            if (componentIds.Length != QSettings.DEFAULT_ORDER_COUNT)
            {
                QSettings.Instance().Set(EM_QHierarchySettings.ComponentsOrder, QSettings.DEFAULT_ORDER, false);
                componentIds = QSettings.DEFAULT_ORDER.Split(';');
            }

            orderedComponents.Clear();
            foreach (var stringID in componentIds)
            {
                orderedComponents.Add(componentDictionary[(EM_QHierarchyComponent) Enum.Parse(typeof(EM_QHierarchyComponent), stringID)]);
            }

            orderedComponents.Add(componentDictionary[EM_QHierarchyComponent.ComponentsComponent]);

            indentation = QSettings.Instance().Get<int>(EM_QHierarchySettings.AdditionalIndentation);
            hideIconsIfThereIsNoFreeSpace = QSettings.Instance().Get<bool>(EM_QHierarchySettings.AdditionalHideIconsIfNotFit);
            backgroundColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalBackgroundColor);
            inactiveColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
        }

        /// <summary>
        /// Hierarchy 单物体的 GUI 方法
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="selectionRect"></param>
        public void HierarchyWindowItemOnGUIHandler(int instanceId, Rect selectionRect)
        {
            QHierarchyColorUtils.SetDefaultColor(UnityEngine.GUI.color);
            var gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (gameObject != null)
            {
                var curRect = new Rect(selectionRect) {width = 16};
                curRect.x += selectionRect.width - indentation;

                var gameObjectNameWidth = hideIconsIfThereIsNoFreeSpace ? UnityEngine.GUI.skin.label.CalcSize(new GUIContent(gameObject.name)).x : 0;
                var objectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObject, false);
                var minX = hideIconsIfThereIsNoFreeSpace ? selectionRect.x + gameObjectNameWidth + 7 : 0;
                DrawComponents(orderedComponents, selectionRect, ref curRect, gameObject, objectList, true, minX);
            }
        }

        /// <summary>
        /// 绘制功能组件
        /// </summary>
        /// <param name="components"></param>
        /// <param name="selectionRect"></param>
        /// <param name="rect"></param>
        /// <param name="gameObject"></param>
        /// <param name="hierarchyObjectList"></param>
        /// <param name="drawBackground"></param>
        /// <param name="minX"></param>
        private void DrawComponents(in List<QHierarchyBaseComponent> components, Rect selectionRect, ref Rect rect, GameObject gameObject, QHierarchyObjectList hierarchyObjectList, bool drawBackground = false, float minX = 50)
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
                        layoutStatus = component.Layout(gameObject, hierarchyObjectList, selectionRect, ref rect, rect.x - minX);
                        if (layoutStatus != EM_QLayoutStatus.Success)
                        {
                            toComponent = layoutStatus == EM_QLayoutStatus.Failed ? i : i + 1;
                            rect.x -= 7;
                            break;
                        }
                    }
                    else
                    {
                        component.DisabledHandler(gameObject, hierarchyObjectList);
                    }
                }

                if (drawBackground)
                {
                    if (backgroundColor.a != 0)
                    {
                        rect.width = selectionRect.x + selectionRect.width - rect.x /*- indentation*/;
                        EditorGUI.DrawRect(rect, backgroundColor);
                    }

                    DrawComponents(preComponents, selectionRect, ref rect, gameObject, hierarchyObjectList);
                }

                for (var i = 0; i < toComponent; i++)
                {
                    var component = components[i];
                    if (component.IsEnabled())
                    {
                        component.Draw(gameObject, hierarchyObjectList, selectionRect);
                    }
                }

                if (layoutStatus != EM_QLayoutStatus.Success)
                {
                    rect.width = 7;
                    QHierarchyColorUtils.SetColor(inactiveColor);
                    UnityEngine.GUI.DrawTexture(rect, trimIcon);
                    QHierarchyColorUtils.ClearColor();
                }
            }
            else if (Event.current.isMouse)
            {
                for (int i = 0, n = components.Count; i < n; i++)
                {
                    var component = components[i];
                    if (component.IsEnabled())
                    {
                        if (component.Layout(gameObject, hierarchyObjectList, selectionRect, ref rect, rect.x - minX) != EM_QLayoutStatus.Failed)
                        {
                            component.EventHandler(gameObject, hierarchyObjectList, Event.current);
                        }
                    }
                }
            }
        }
    }
}
