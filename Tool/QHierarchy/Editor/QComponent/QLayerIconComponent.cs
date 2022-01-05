using System;
using System.Collections.Generic;
using System.Reflection;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QLayerIconComponent: QBaseComponent
    {
        private List<QLayerTexture> layerTextureList;

        // CONSTRUCTOR
        public QLayerIconComponent()
        {
            rect.width  = 14;
            rect.height = 14;

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LayerIconShow              , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LayerIconShowDuringPlayMode, settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LayerIconSize              , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LayerIconList              , settingsChanged);
            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged()
        {
            enabled                     = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LayerIconShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LayerIconShowDuringPlayMode);
            EM_QHierarchySizeAll size      = (EM_QHierarchySizeAll)QSettings.Instance().Get<int>(EM_QHierarchySettings.LayerIconSize);
            rect.width = rect.height    = (size == EM_QHierarchySizeAll.Normal ? 15 : (size == EM_QHierarchySizeAll.Big ? 16 : 13));        
            this.layerTextureList = QLayerTexture.loadLayerTextureList();
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < rect.width)
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

        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {                       
            string gameObjectLayerName = LayerMask.LayerToName(gameObject.layer);

            QLayerTexture layerTexture = layerTextureList.Find(t => t.layer == gameObjectLayerName);
            if (layerTexture != null && layerTexture.texture != null)
            {
                UnityEngine.GUI.DrawTexture(rect, layerTexture.texture, ScaleMode.ScaleToFit, true);
            }
        }
    }
}

