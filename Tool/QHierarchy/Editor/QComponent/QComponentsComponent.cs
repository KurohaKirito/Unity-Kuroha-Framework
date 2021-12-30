using System;
using System.Reflection;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QComponentsComponent: QBaseComponent
    {
        // PRIVATE
        private GUIStyle hintLabelStyle;
        private Color grayColor;
        private Color backgroundDarkColor;
        private Texture2D componentIcon;
        private List<Component> components = new List<Component>();   
        private Rect eventRect = new Rect(0, 0, 16, 16);
        private int componentsToDraw;
        private List<string> ignoreScripts;

        // CONSTRUCTOR
        public QComponentsComponent ()
        {
            this.backgroundDarkColor = QResources.Instance().GetColor(QColor.BackgroundDark);
            this.grayColor           = QResources.Instance().GetColor(QColor.Gray);
            this.componentIcon       = QResources.Instance().GetTexture(QTexture.QComponentUnknownIcon);

            hintLabelStyle = new GUIStyle();
            hintLabelStyle.normal.textColor = grayColor;
            hintLabelStyle.fontSize = 11;
            hintLabelStyle.clipping = TextClipping.Clip;  

            QSettings.Instance().addEventListener(EM_QSetting.ComponentsShow              , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.ComponentsShowDuringPlayMode, settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.ComponentsIconSize          , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.ComponentsIgnore            , settingsChanged);
            settingsChanged();
        }

        // PRIVATE
        private void settingsChanged()
        {
            enabled                     = QSettings.Instance().Get<bool>(EM_QSetting.ComponentsShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QSetting.ComponentsShowDuringPlayMode);
            QHierarchySizeAll size = (QHierarchySizeAll)QSettings.Instance().Get<int>(EM_QSetting.ComponentsIconSize);
            rect.width = rect.height = (size == QHierarchySizeAll.Normal ? 15 : (size == QHierarchySizeAll.Big ? 16 : 13));       

            string ignoreString = QSettings.Instance().Get<string>(EM_QSetting.ComponentsIgnore);
            if (ignoreString != "") 
            {
                ignoreScripts = new List<string>(ignoreString.Split(new char[] { ',', ';', '.', ' ' }));
                ignoreScripts.RemoveAll(item => item == "");
            }
            else ignoreScripts = null;
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            Component[] currentComponents = gameObject.GetComponents<Component>();

            components.Clear();
            if (ignoreScripts != null)
            {
                for (int i = 0; i < currentComponents.Length; i++)
                {
                    string componentName = currentComponents[i].GetType().FullName;
                    bool ignore = false;
                    for (int j = ignoreScripts.Count - 1; j >= 0; j--)
                    {
                        if (componentName.Contains(ignoreScripts[j]))
                        {
                            ignore = true;
                            break;
                        } 
                    }
                    if (!ignore) components.Add(currentComponents[i]);
                }
            }
            else
            {
                components.AddRange(currentComponents);
            }

            int maxComponentsCount = Mathf.FloorToInt((maxWidth - 2) / rect.width);
            componentsToDraw = Math.Min(maxComponentsCount, components.Count - 1);

            float totalWidth = 2 + rect.width * componentsToDraw;
    
            curRect.x -= totalWidth;

            rect.x = curRect.x;
            rect.y = curRect.y - (rect.height - 16) / 2;

            eventRect.width = totalWidth;
            eventRect.x = rect.x;
            eventRect.y = rect.y;

            if (maxComponentsCount >= components.Count - 1) return EM_QLayoutStatus.Success;
            else if (maxComponentsCount == 0) return EM_QLayoutStatus.Failed;
            else return EM_QLayoutStatus.Partly;
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {
            for (int i = components.Count - componentsToDraw, n = components.Count; i < n; i++)
            {
                Component component = components[i];
                if (component is Transform) continue;
                                
                GUIContent content = EditorGUIUtility.ObjectContent(component, null);

                bool enabled = true;
                try
                {
                    PropertyInfo propertyInfo = component.GetType().GetProperty("enabled");
                    enabled = (bool)propertyInfo.GetGetMethod().Invoke(component, null);
                }
                catch {}

                Color color = UnityEngine.GUI.color;
                color.a = enabled ? 1f : 0.3f;
                UnityEngine.GUI.color = color;
                UnityEngine.GUI.DrawTexture(rect, content.image == null ? componentIcon : content.image, ScaleMode.ScaleToFit);
                color.a = 1;
                UnityEngine.GUI.color = color;

                if (rect.Contains(Event.current.mousePosition))
                {        
                    string componentName = "Missing script";
                    if (component != null) componentName = component.GetType().Name;

                    int labelWidth = Mathf.CeilToInt(hintLabelStyle.CalcSize(new GUIContent(componentName)).x);                    
                    selectionRect.x = rect.x - labelWidth / 2 - 4;
                    selectionRect.width = labelWidth + 8;
                    selectionRect.height -= 1;

                    if (selectionRect.y > 16) selectionRect.y -= 16;
                    else selectionRect.x += labelWidth / 2 + 18;

                    EditorGUI.DrawRect(selectionRect, backgroundDarkColor);
                    selectionRect.x += 4;
                    selectionRect.y += (EditorGUIUtility.singleLineHeight - rect.height) * 0.5f;
                    selectionRect.height = EditorGUIUtility.singleLineHeight;

                    EditorGUI.LabelField(selectionRect, componentName, hintLabelStyle);
                }

                rect.x += rect.width;
            }
        }

        public override void EventHandler(GameObject gameObject, QObjectList objectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.button == 0 && eventRect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.type == EventType.MouseDown)
                {
                    int id = Mathf.FloorToInt((currentEvent.mousePosition.x - eventRect.x) / rect.width) + components.Count - 1 - componentsToDraw + 1;

                    try
                    {
                        PropertyInfo propertyInfo = components[id].GetType().GetProperty("enabled");
                        bool enabled = (bool)propertyInfo.GetGetMethod().Invoke(components[id], null);
                        Undo.RecordObject(components[id], enabled ? "Disable Component" : "Enable Component");
                        propertyInfo.GetSetMethod().Invoke(components[id], new object[] { !enabled });
                    }
                    catch {}

                    EditorUtility.SetDirty(gameObject);
                }
                currentEvent.Use();
            }
        }
    }
}
