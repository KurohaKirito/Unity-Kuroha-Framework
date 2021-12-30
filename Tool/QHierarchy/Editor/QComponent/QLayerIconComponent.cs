using System;
using System.Collections.Generic;
using System.Reflection;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using qtools.qhierarchy.phierarchy;
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

            QSettings.Instance().addEventListener(EM_QSetting.LayerIconShow              , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.LayerIconShowDuringPlayMode, settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.LayerIconSize              , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.LayerIconList              , settingsChanged);
            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged()
        {
            enabled                     = QSettings.Instance().Get<bool>(EM_QSetting.LayerIconShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QSetting.LayerIconShowDuringPlayMode);
            QHierarchySizeAll size      = (QHierarchySizeAll)QSettings.Instance().Get<int>(EM_QSetting.LayerIconSize);
            rect.width = rect.height    = (size == QHierarchySizeAll.Normal ? 15 : (size == QHierarchySizeAll.Big ? 16 : 13));        
            this.layerTextureList = QLayerTexture.loadLayerTextureList();
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
                rect.y = curRect.y - (rect.height - 16) / 2;
                return EM_QLayoutStatus.Success;
            }
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
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

