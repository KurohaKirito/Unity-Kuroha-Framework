using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentSeparator : QHierarchyBaseComponent
    {
        private Color separatorColor;
        private Color evenShadingColor;
        private Color oddShadingColor;
        private bool showRowShading;

        /// <summary>
        /// 构造方法
        /// </summary>
        public QHierarchyComponentSeparator()
        {
            showComponentDuringPlayMode = true;

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorShowRowShading, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorEvenRowShadingColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.SeparatorOddRowShadingColor, SettingsChanged);

            SettingsChanged();
        }

        /// <summary>
        /// 更改设置
        /// </summary>
        private void SettingsChanged()
        {
            showRowShading = QSettings.Instance().Get<bool>(EM_QHierarchySettings.SeparatorShowRowShading);
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.SeparatorShow);
            evenShadingColor = QSettings.Instance().GetColor(EM_QHierarchySettings.SeparatorEvenRowShadingColor);
            oddShadingColor = QSettings.Instance().GetColor(EM_QHierarchySettings.SeparatorOddRowShadingColor);
            separatorColor = QSettings.Instance().GetColor(EM_QHierarchySettings.SeparatorColor);
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
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
                selectionRect.height -= 1;
                selectionRect.y += 1;
                EditorGUI.DrawRect(selectionRect, Mathf.FloorToInt((selectionRect.y - 4) / 16 % 2) == 0 ? evenShadingColor : oddShadingColor);
            }
        }
    }
}