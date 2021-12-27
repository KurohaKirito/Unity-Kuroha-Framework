using System;
using UnityEngine;
using UnityEditor;
using qtools.qhierarchy.phierarchy;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.Editor.QData;
using System.Reflection;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QGameObjectIconComponent: QBaseComponent
    {
        // PRIVATE
        private MethodInfo getIconMethodInfo;
        private object[] getIconMethodParams;

        // CONSTRUCTOR
        public QGameObjectIconComponent ()
        {
            rect.width = 14;
            rect.height = 14;

            getIconMethodInfo   = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static );
            getIconMethodParams = new object[1];

            QSettings.getInstance().addEventListener(EM_QSetting.GameObjectIconShow                 , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.GameObjectIconShowDuringPlayMode   , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.GameObjectIconSize                          , settingsChanged);
            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged()
        {
            enabled = QSettings.getInstance().get<bool>(EM_QSetting.GameObjectIconShow);
            showComponentDuringPlayMode = QSettings.getInstance().get<bool>(EM_QSetting.GameObjectIconShowDuringPlayMode);
            QHierarchySizeAll size = (QHierarchySizeAll)QSettings.getInstance().get<int>(EM_QSetting.GameObjectIconSize);
            rect.width = rect.height = (size == QHierarchySizeAll.Normal ? 15 : (size == QHierarchySizeAll.Big ? 16 : 13));     
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < rect.width + 2)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= rect.width + 2;
                rect.x = curRect.x;
                rect.y = curRect.y - (rect.height - 16) / 2;
                return EM_QLayoutStatus.Success;
            }
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {                      
            getIconMethodParams[0] = gameObject;
            Texture2D icon = (Texture2D)getIconMethodInfo.Invoke(null, getIconMethodParams );    
            if (icon != null) 
                UnityEngine.GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
        }
                
        public override void EventHandler(GameObject gameObject, QObjectList objectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                currentEvent.Use();

                Type iconSelectorType = Assembly.Load("UnityEditor").GetType("UnityEditor.IconSelector");
                MethodInfo showIconSelectorMethodInfo = iconSelectorType.GetMethod("ShowAtPosition", BindingFlags.Static | BindingFlags.NonPublic);
                showIconSelectorMethodInfo.Invoke(null, new object[] { gameObject, rect, true });
            }
        }
    }
}

