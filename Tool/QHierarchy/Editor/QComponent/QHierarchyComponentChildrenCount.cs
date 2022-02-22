using UnityEngine;
using Kuroha.Tool.QHierarchy.RunTime;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    /// <summary>
    /// QHierarchyComponent : 显示子物体数量
    /// </summary>
    public class QHierarchyComponentChildrenCount : QHierarchyBaseComponent
    {
        /// <summary>
        /// 标签样式
        /// </summary>
        private readonly GUIStyle labelStyle;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentChildrenCount()
        {
            labelStyle = new GUIStyle
            {
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleRight
            };

            rect.height = GAME_OBJECT_HEIGHT;

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ChildrenCountShow, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ChildrenCountShowDuringPlayMode, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ChildrenCountLabelSize, OnSettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ChildrenCountLabelColor, OnSettingsChanged);

            OnSettingsChanged();
        }

        /// <summary>
        /// 当用户更改设置时触发
        /// </summary>
        private void OnSettingsChanged()
        {
            // 取出设置: 是否启用功能
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ChildrenCountShow);

            // 取出设置: 是否在播放模式下显示
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ChildrenCountShowDuringPlayMode);

            // 取出设置: 数字标签显示大小
            var labelSize = (EM_QHierarchySize) QSettings.Instance().Get<int>(EM_QHierarchySettings.ChildrenCountLabelSize);

            // 取出设置: 数字标签显示颜色
            labelStyle.normal.textColor = QSettings.Instance().GetColor(EM_QHierarchySettings.ChildrenCountLabelColor);

            labelStyle.fontSize = labelSize switch
            {
                EM_QHierarchySize.Normal => 11,
                EM_QHierarchySize.Big => 13,
                _ => 11
            };

            rect.width = labelSize switch
            {
                EM_QHierarchySize.Normal => 22,
                EM_QHierarchySize.Big => 24,
                _ => 22
            };
        }

        /// <summary>
        /// 进行布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < rect.width + COMPONENT_SPACE)
            {
                return EM_QLayoutStatus.Failed;
            }

            // 从右向左绘制
            curRect.x -= rect.width + COMPONENT_SPACE;
            rect.x = curRect.x;
            rect.y = curRect.y;

            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 进行绘制
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var childrenCount = gameObject.transform.childCount;
            if (childrenCount > 0)
            {
                UnityEngine.GUI.Label(rect, childrenCount.ToString("000"), labelStyle);
            }
        }
    }
}
