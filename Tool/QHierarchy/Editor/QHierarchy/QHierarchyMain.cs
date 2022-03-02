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
        /// 在面板右侧的功能
        /// </summary>
        private readonly Dictionary<EM_QHierarchyComponent, QHierarchyBaseComponent> componentsAtRight;

        /// <summary>
        /// 在面板左侧的功能
        /// </summary>
        private readonly List<QHierarchyBaseComponent> componentsAtLeft;
        
        private readonly List<QHierarchyBaseComponent> orderedComponents;
        
        /// <summary>
        /// 一个左三角
        /// 用于表示当前 Hierarchy 面板宽度太小, 图标显示未显示完全, 请扩大 Hierarchy 面板宽度
        /// </summary>
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
            componentsAtRight = new Dictionary<EM_QHierarchyComponent, QHierarchyBaseComponent>
            {
                {
                    EM_QHierarchyComponent.LockComponent, new QHierarchyComponentLock()
                },
                {
                    EM_QHierarchyComponent.VisibilityComponent, new QHierarchyComponentVisibility()
                },
                {
                    EM_QHierarchyComponent.StaticComponent, new QHierarchyComponentStatic()
                },
                {
                    EM_QHierarchyComponent.ColorComponent, new QHierarchyComponentColor()
                },
                {
                    EM_QHierarchyComponent.ErrorComponent, new QHierarchyComponentError()
                },
                {
                    EM_QHierarchyComponent.RendererComponent, new QHierarchyComponentRenderer()
                },
                {
                    EM_QHierarchyComponent.PrefabComponent, new QHierarchyComponentPrefab()
                },
                {
                    EM_QHierarchyComponent.TagAndLayerComponent, new QHierarchyComponentTagLayerName()
                },
                {
                    EM_QHierarchyComponent.GameObjectIconComponent, new QHierarchyComponentGameObjectIcon()
                },
                {
                    EM_QHierarchyComponent.TagIconComponent, new QHierarchyComponentTagIcon()
                },
                {
                    EM_QHierarchyComponent.LayerIconComponent, new QHierarchyComponentLayerIcon()
                },
                {
                    EM_QHierarchyComponent.ChildrenCountComponent, new QHierarchyComponentChildrenCount()
                },
                {
                    EM_QHierarchyComponent.VerticesAndTrianglesCount, new QHierarchyComponentVerticesAndTrianglesCount()
                },
                {
                    EM_QHierarchyComponent.ComponentsComponent, new QHierarchyComponentComponents()
                }
            };

            componentsAtLeft = new List<QHierarchyBaseComponent>
            {
                new QHierarchyComponentMonoBehavior(),
                new QHierarchyComponentTreeMap(),
                new QHierarchyComponentSeparator()
            };

            orderedComponents = new List<QHierarchyBaseComponent>();

            trimIcon = QResources.Instance().GetTexture(EM_QHierarchyTexture.QTrimIcon);
            
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
            // 读取功能排序设置
            var componentOrder = QSettings.Instance().Get<string>(EM_QHierarchySettings.ComponentsOrder);
            var componentIds = componentOrder.Split(';');
            if (componentIds.Length != QSettings.DEFAULT_ORDER_COUNT)
            {
                QSettings.Instance().Set(EM_QHierarchySettings.ComponentsOrder, QSettings.DEFAULT_ORDER, false);
                componentIds = QSettings.DEFAULT_ORDER.Split(';');
            }

            // 根据设置进行排序
            orderedComponents.Clear();
            foreach (var stringID in componentIds)
            {
                orderedComponents.Add(componentsAtRight[(EM_QHierarchyComponent) Enum.Parse(typeof(EM_QHierarchyComponent), stringID)]);
            }
            orderedComponents.Add(componentsAtRight[EM_QHierarchyComponent.ComponentsComponent]);

            // 读取其他设置
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
            QHierarchyColorUtils.DefaultColor = UnityEngine.GUI.color;
            
            var gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (gameObject == null)
            {
                return;
            }
            
            var curRect = new Rect(selectionRect)
            {
                width = 16
            };
            curRect.x += selectionRect.width - indentation;

            var gameObjectNameWidth = hideIconsIfThereIsNoFreeSpace ? UnityEngine.GUI.skin.label.CalcSize(new GUIContent(gameObject.name)).x : 0;
                
            var objectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObject, false);
            var minX = hideIconsIfThereIsNoFreeSpace ? selectionRect.x + gameObjectNameWidth + 7 : 0;
            
            DrawComponents(orderedComponents, selectionRect, ref curRect, gameObject, objectList, true, minX);
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
                        rect.width = selectionRect.x + selectionRect.width - rect.x;
                        EditorGUI.DrawRect(rect, backgroundColor);
                    }

                    DrawComponents(componentsAtLeft, selectionRect, ref rect, gameObject, hierarchyObjectList);
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
                    UnityEngine.GUI.color = inactiveColor;
                    UnityEngine.GUI.DrawTexture(rect, trimIcon);
                    UnityEngine.GUI.color = QHierarchyColorUtils.DefaultColor;
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
