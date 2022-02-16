using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentLayerIcon : QHierarchyBaseComponent
    {
        private List<QLayerTexture> layerTextureList;

        /// <summary>
        /// 构造方法
        /// </summary>
        public QHierarchyComponentLayerIcon()
        {
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LayerIconShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LayerIconShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LayerIconSize, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LayerIconList, SettingsChanged);
            
            SettingsChanged();
        }

        /// <summary>
        /// 设置更改
        /// </summary>
        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LayerIconShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LayerIconShowDuringPlayMode);
            var size = (EM_QHierarchySizeAll) QSettings.Instance().Get<int>(EM_QHierarchySettings.LayerIconSize);
            
            rect.width = rect.height = size switch
            {
                EM_QHierarchySizeAll.Small => 14,
                EM_QHierarchySizeAll.Normal => 15,
                EM_QHierarchySizeAll.Big => 16,
                _ => 14
            };
            
            layerTextureList = QLayerTexture.loadLayerTextureList();
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
            rect.y = curRect.y - (rect.height - 16) / 2;
            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var gameObjectLayerName = LayerMask.LayerToName(gameObject.layer);

            var layerTexture = layerTextureList.Find(texture => texture.layer == gameObjectLayerName);
            if (layerTexture != null && layerTexture.texture != null)
            {
                UnityEngine.GUI.DrawTexture(rect, layerTexture.texture, ScaleMode.ScaleToFit, true);
            }
        }
    }
}
