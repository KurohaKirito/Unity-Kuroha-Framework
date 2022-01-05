using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentComponents : QBaseComponent
    {
        // PRIVATE
        private readonly GUIStyle hintLabelStyle;
        private readonly Color backgroundDarkColor;
        private readonly Texture2D componentIcon;
        private readonly List<Component> components = new List<Component>();
        private Rect eventRect = new Rect(0, 0, 16, 16);
        private int componentsToDraw;
        private List<string> ignoreScripts;

        // CONSTRUCTOR
        public QHierarchyComponentComponents()
        {
            backgroundDarkColor = QResources.Instance().GetColor(QColor.BackgroundDark);
            componentIcon = QResources.Instance().GetTexture(QTexture.QComponentUnknownIcon);

            hintLabelStyle = new GUIStyle
            {
                normal = {
                    textColor = QResources.Instance().GetColor(QColor.Gray)
                },
                fontSize = 11,
                clipping = TextClipping.Clip
            };

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ComponentsShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ComponentsShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ComponentsIconSize, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ComponentsIgnore, SettingsChanged);

            SettingsChanged();
        }

        // PRIVATE
        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ComponentsShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ComponentsShowDuringPlayMode);
            var size = (EM_QHierarchySizeAll)QSettings.Instance().Get<int>(EM_QHierarchySettings.ComponentsIconSize);

            rect.width = rect.height = size switch {
                EM_QHierarchySizeAll.Normal => 15,
                EM_QHierarchySizeAll.Big => 16,
                _ => 13
            };

            var ignoreString = QSettings.Instance().Get<string>(EM_QHierarchySettings.ComponentsIgnore);
            if (ignoreString != "")
            {
                ignoreScripts = new List<string>(ignoreString.Split(',', ';', '.', ' '));
                ignoreScripts.RemoveAll(item => item == "");
            }
            else
            {
                ignoreScripts = null;
            }
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            var currentComponents = new List<Component>();
            gameObject.GetComponents(currentComponents);

            components.Clear();
            if (ignoreScripts != null)
            {
                foreach (var component in currentComponents)
                {
                    var componentName = component.GetType().FullName;
                    if (componentName != null)
                    {
                        var ignore = false;
                        for (var index = ignoreScripts.Count - 1; index >= 0; index--)
                        {
                            if (componentName.Contains(ignoreScripts[index]))
                            {
                                ignore = true;
                                break;
                            }
                        }

                        if (ignore == false)
                        {
                            components.Add(component);
                        }
                    }
                }
            }
            else
            {
                components.AddRange(currentComponents);
            }

            var maxComponentsCount = Mathf.FloorToInt((maxWidth - 2) / rect.width);
            componentsToDraw = Math.Min(maxComponentsCount, components.Count - 1);

            var totalWidth = 2 + rect.width * componentsToDraw;

            curRect.x -= totalWidth;

            rect.x = curRect.x;
            rect.y = curRect.y - (rect.height - 16) / 2;

            eventRect.width = totalWidth;
            eventRect.x = rect.x;
            eventRect.y = rect.y;

            if (maxComponentsCount >= components.Count - 1)
            {
                return EM_QLayoutStatus.Success;
            }
            
            return maxComponentsCount == 0 ? EM_QLayoutStatus.Failed : EM_QLayoutStatus.Partly;
        }

        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            for (int i = components.Count - componentsToDraw, n = components.Count; i < n; i++)
            {
                var component = components[i];
                if (component is Transform)
                {
                    continue;
                }

                var content = EditorGUIUtility.ObjectContent(component, null);

                var objectEnabled = true;
                
                try
                {
                    var propertyInfo = component.GetType().GetProperty("enabled");
                    if (propertyInfo != null)
                    {
                        objectEnabled = (bool)propertyInfo.GetGetMethod().Invoke(component, null);
                    }
                }
                catch
                {
                    // ignore
                }

                var color = UnityEngine.GUI.color;
                color.a = objectEnabled? 1f : 0.3f;
                UnityEngine.GUI.color = color;
                UnityEngine.GUI.DrawTexture(rect, content.image == null? componentIcon : content.image, ScaleMode.ScaleToFit);
                color.a = 1;
                UnityEngine.GUI.color = color;

                if (rect.Contains(Event.current.mousePosition))
                {
                    var componentName = "Missing script";
                    if (component != null)
                    {
                        componentName = component.GetType().Name;
                    }

                    var labelWidth = Mathf.CeilToInt(hintLabelStyle.CalcSize(new GUIContent(componentName)).x);
                    selectionRect.x = rect.x - labelWidth / 2f - 4;
                    selectionRect.width = labelWidth + 8;
                    selectionRect.height -= 1;

                    if (selectionRect.y > 16)
                    {
                        selectionRect.y -= 16;
                    }
                    else
                    {
                        selectionRect.x += labelWidth / 2 + 18;
                    }

                    EditorGUI.DrawRect(selectionRect, backgroundDarkColor);
                    selectionRect.x += 4;
                    selectionRect.y += (EditorGUIUtility.singleLineHeight - rect.height) * 0.5f;
                    selectionRect.height = EditorGUIUtility.singleLineHeight;

                    EditorGUI.LabelField(selectionRect, componentName, hintLabelStyle);
                }

                rect.x += rect.width;
            }
        }

        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent) {
            if (currentEvent.isMouse && currentEvent.button == 0 && eventRect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.type == EventType.MouseDown)
                {
                    var id = Mathf.FloorToInt((currentEvent.mousePosition.x - eventRect.x) / rect.width) + components.Count - 1 - componentsToDraw + 1;

                    try
                    {
                        var propertyInfo = components[id].GetType().GetProperty("enabled");
                        
                        var componentEnabled = propertyInfo != null && (bool)propertyInfo.GetGetMethod().Invoke(components[id], null);
                        
                        Undo.RecordObject(components[id], componentEnabled? "Disable Component" : "Enable Component");
                        
                        if (propertyInfo != null)
                        {
                            propertyInfo.GetSetMethod().Invoke(components[id], new object[]
                            {
                                !componentEnabled
                            });
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    EditorUtility.SetDirty(gameObject);
                }

                currentEvent.Use();
            }
        }
    }
}
