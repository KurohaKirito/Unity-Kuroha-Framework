using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentTagIcon : QHierarchyBaseComponent
    {
        private List<QTagTexture> tagTextureList;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentTagIcon()
        {
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagIconShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagIconShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagIconSize, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagIconList, SettingsChanged);
            
            SettingsChanged();
        }

        /// <summary>
        /// 更改设置
        /// </summary>
        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TagIconShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TagIconShowDuringPlayMode);
            var size = (EM_QHierarchySizeAll) QSettings.Instance().Get<int>(EM_QHierarchySettings.TagIconSize);

            rect.width = rect.height = size switch
            {
                EM_QHierarchySizeAll.Small => 14,
                EM_QHierarchySizeAll.Normal => 15,
                EM_QHierarchySizeAll.Big => 16,
                _ => 14
            };

            tagTextureList = QTagTexture.LoadTagTextureList();
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
            rect.y = curRect.y - (rect.height - GAME_OBJECT_HEIGHT) / 2;
            
            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        public override void Draw(GameObject gameObjectToDraw, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var gameObjectTag = gameObjectToDraw.tag;
            var tagTexture = tagTextureList.Find(texture => texture.tag == gameObjectTag);
            if (tagTexture != null && tagTexture.texture != null)
            {
                UnityEngine.GUI.DrawTexture(rect, tagTexture.texture, ScaleMode.ScaleToFit, true);
            }
        }
    }
}
