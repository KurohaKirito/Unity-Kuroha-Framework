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
    public class QSeparatorComponent: QBaseComponent
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

            QSettings.getInstance().addEventListener(EM_QSetting.SeparatorShowRowShading   , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.SeparatorShow             , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.SeparatorColor                , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.SeparatorEvenRowShadingColor  , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.SeparatorOddRowShadingColor , settingsChanged);

            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged()
        {
            showRowShading   = QSettings.getInstance().get<bool>(EM_QSetting.SeparatorShowRowShading);
            enabled          = QSettings.getInstance().get<bool>(EM_QSetting.SeparatorShow);
            evenShadingColor = QSettings.getInstance().getColor(EM_QSetting.SeparatorEvenRowShadingColor);
            oddShadingColor  = QSettings.getInstance().getColor(EM_QSetting.SeparatorOddRowShadingColor);
            separatorColor   = QSettings.getInstance().getColor(EM_QSetting.SeparatorColor);
        }

        // DRAW
        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
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

