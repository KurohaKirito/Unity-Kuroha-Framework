using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using Kuroha.Tool.QHierarchy.Editor.QData;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    /// <summary>
    /// QHierarchyComponent : 显示子物体数量
    /// </summary>
    public class QHierarchyComponentChildrenCount : QBaseComponent
    {
        private readonly GUIStyle labelStyle;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentChildrenCount()
        {
            labelStyle = new GUIStyle
            {
                fontSize = 9,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleRight
            };

            rect.width = 22;
            rect.height = 16;

            QSettings.Instance().addEventListener(EM_QSetting.ChildrenCountShow, OnSettingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.ChildrenCountShowDuringPlayMode, OnSettingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.ChildrenCountLabelSize, OnSettingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.ChildrenCountLabelColor, OnSettingsChanged);
            OnSettingsChanged();
        }

        /// <summary>
        /// 当用户更改设置时触发
        /// </summary>
        private void OnSettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QSetting.ChildrenCountShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QSetting.ChildrenCountShowDuringPlayMode);
            QHierarchySize labelSize = (QHierarchySize) QSettings.Instance().Get<int>(EM_QSetting.ChildrenCountLabelSize);
            labelStyle.normal.textColor = QSettings.Instance().getColor(EM_QSetting.ChildrenCountLabelColor);
            labelStyle.fontSize = labelSize == QHierarchySize.Normal ? 8 : 9;
            rect.width = labelSize == QHierarchySize.Normal ? 17 : 22;
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < rect.width)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= rect.width + 2;
                rect.x = curRect.x;
                rect.y = curRect.y;
                rect.y += (EditorGUIUtility.singleLineHeight - rect.height) * 0.5f;
                rect.height = EditorGUIUtility.singleLineHeight;
                return EM_QLayoutStatus.Success;
            }
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {
            int childrenCount = gameObject.transform.childCount;
            if (childrenCount > 0) UnityEngine.GUI.Label(rect, childrenCount.ToString(), labelStyle);
        }
    }
}
