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
    public class QTagIconComponent: QBaseComponent
    {
        private List<QTagTexture> tagTextureList;

        // CONSTRUCTOR
        public QTagIconComponent()
        {
            rect.width  = 14;
            rect.height = 14;

            QSettings.Instance().addEventListener(EM_QSetting.TagIconShow              , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.TagIconShowDuringPlayMode, settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.TagIconSize              , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.TagIconList              , settingsChanged);
            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QSetting.TagIconShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QSetting.TagIconShowDuringPlayMode);
            QHierarchySizeAll size = (QHierarchySizeAll)QSettings.Instance().Get<int>(EM_QSetting.TagIconSize);
            rect.width = rect.height = (size == QHierarchySizeAll.Normal ? 15 : (size == QHierarchySizeAll.Big ? 16 : 13));        
            this.tagTextureList = QTagTexture.loadTagTextureList();
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
            string gameObjectTag = "";
            try { gameObjectTag = gameObject.tag; }
            catch {}

            QTagTexture tagTexture = tagTextureList.Find(t => t.tag == gameObjectTag);
            if (tagTexture != null && tagTexture.texture != null)
            {
                UnityEngine.GUI.DrawTexture(rect, tagTexture.texture, ScaleMode.ScaleToFit, true);
            }
        }
    }
}

