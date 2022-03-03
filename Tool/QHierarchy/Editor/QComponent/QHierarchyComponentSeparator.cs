using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentSeparator : QHierarchyBaseComponent
    {
        /// <summary>
        /// 分隔线颜色
        /// </summary>
        private Color separatorColor;
        
        /// <summary>
        /// 偶数行颜色
        /// </summary>
        private Color evenShadingColor;
        
        /// <summary>
        /// 奇数行颜色
        /// </summary>
        private Color oddShadingColor;
        
        /// <summary>
        /// 是否显示行颜色
        /// </summary>
        private bool isShowRowColor;

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
            isShowRowColor = QSettings.Instance().Get<bool>(EM_QHierarchySettings.SeparatorShowRowShading);
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.SeparatorShow);
            evenShadingColor = QSettings.Instance().GetColor(EM_QHierarchySettings.SeparatorEvenRowShadingColor);
            oddShadingColor = QSettings.Instance().GetColor(EM_QHierarchySettings.SeparatorOddRowShadingColor);
            separatorColor = QSettings.Instance().GetColor(EM_QHierarchySettings.SeparatorColor);
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
        public override void Draw(GameObject gameObjectToDraw, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            // Inspector Width
            var inspectorWidth = selectionRect.width + selectionRect.x;
            
            // 绘制 1 像素的分隔线
            rect.x = 0;
            rect.y = selectionRect.y;
            rect.width = inspectorWidth;
            rect.height = 1;
            EditorGUI.DrawRect(rect, separatorColor);

            // 显示行着色
            if (isShowRowColor)
            {
                // 露出 1 像素的分隔线
                var rowRect = new Rect
                {
                    x = 0,
                    width = inspectorWidth,
                    y = selectionRect.y + 1,
                    height = selectionRect.height - 1
                };
                
                // 计算是否是奇数
                // 第 1 行的 Y 值为 16
                // 第 2 行的 Y 值为 32
                var y = (int) selectionRect.y;
                var isOdd = y / GAME_OBJECT_HEIGHT % 2 == 1;
                EditorGUI.DrawRect(rowRect, isOdd ? evenShadingColor : oddShadingColor);
            }
        }
    }
}
