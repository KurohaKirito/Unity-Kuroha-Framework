using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentRenderer : QHierarchyBaseComponent
    {
        private Color activeColor;
        private Color inactiveColor;
        
        private const int RECT_WIDTH = 12;
        private readonly Texture2D rendererButtonTexture;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentRenderer()
        {
            rect.width = RECT_WIDTH;

            rendererButtonTexture = QResources.Instance().GetTexture(QTexture.QRendererButton);
            
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.RendererShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.RendererShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalActiveColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor, SettingsChanged);
            
            SettingsChanged();
        }

        /// <summary>
        /// 修改设置
        /// </summary>
        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.RendererShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.RendererShowDuringPlayMode);
            activeColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalActiveColor);
            inactiveColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < rect.width + COMPONENT_SPACE)
            {
                return EM_QLayoutStatus.Failed;
            }
            
            curRect.x -= rect.width + COMPONENT_SPACE;
            rect.x = curRect.x;
            rect.y = curRect.y;
            return EM_QLayoutStatus.Success;
        }
        
        /// <summary>
        /// 绘制
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                UnityEngine.GUI.color = renderer.enabled ? activeColor : inactiveColor;
                UnityEngine.GUI.DrawTexture(rect, rendererButtonTexture);
                QHierarchyColorUtils.ResetDefaultColor();
            }
        }

        /// <summary>
        /// 左键单击事件
        /// </summary>
        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.button == 0 && currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition))
            {
                var renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    currentEvent.Use();
                    
                    var isEnabled = renderer.enabled;
                    
                    Undo.RecordObject(renderer, isEnabled ? "Disable Component" : "Enable Component");
                    renderer.enabled = !isEnabled;
                    SceneView.RepaintAll();

                    EditorUtility.SetDirty(gameObject);
                }
            }
        }
    }
}
