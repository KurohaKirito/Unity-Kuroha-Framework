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
    public class QSeparatorComponent: QHierarchyBaseComponent
    {
        // PRIVATE
        private Color separatorColor;
        private Color evenShadingColor;
        private Color oddShadingColor;
        private bool showRowShading;

        // CONSTRUCTOR
        public QSeparatorComponent ()
        {
            showComponentDuringPlayMode = true;

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorShowRowShading   , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorShow             , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorColor                , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorEvenRowShadingColor  , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorOddRowShadingColor , settingsChanged);

            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged()
        {
            showRowShading   = QSettings.Instance().Get<bool>(EM_QHierarchySettings.SeparatorShowRowShading);
            enabled          = QSettings.Instance().Get<bool>(EM_QHierarchySettings.SeparatorShow);
            evenShadingColor = QSettings.Instance().GetColor(EM_QHierarchySettings.SeparatorEvenRowShadingColor);
            oddShadingColor  = QSettings.Instance().GetColor(EM_QHierarchySettings.SeparatorOddRowShadingColor);
            separatorColor   = QSettings.Instance().GetColor(EM_QHierarchySettings.SeparatorColor);
        }

        // DRAW
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            rect.y = selectionRect.y;
            rect.width = selectionRect.width + selectionRect.x;
            rect.height = 1;
            rect.x = 0;

            EditorGUI.DrawRect(rect, separatorColor);

            if (showRowShading)
            {
                selectionRect.width += selectionRect.x;
                selectionRect.x = 0;
                selectionRect.height -=1;
                selectionRect.y += 1;
                EditorGUI.DrawRect(selectionRect, ((Mathf.FloorToInt(((selectionRect.y - 4) / 16) % 2) == 0)) ? evenShadingColor : oddShadingColor);
            }
        }
    }
}
