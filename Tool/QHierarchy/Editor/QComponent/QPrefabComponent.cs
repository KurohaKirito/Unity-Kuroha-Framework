using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QPrefabComponent: QBaseComponent
    {
        // PRIVATE
        private Color activeColor;
        private Color inactiveColor;
        private Texture2D prefabTexture;
        private bool showPrefabConnectedIcon;

        // CONSTRUCTOR
        public QPrefabComponent()
        {
            rect.width = 9;

            prefabTexture = QResources.Instance().GetTexture(QTexture.QPrefabIcon);

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.PrefabShowBrakedPrefabsOnly  , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.PrefabShow                    , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalActiveColor         , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor       , settingsChanged);
            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged()
        {
            showPrefabConnectedIcon = QSettings.Instance().Get<bool>(EM_QHierarchySettings.PrefabShowBrakedPrefabsOnly);
            enabled                 = QSettings.Instance().Get<bool>(EM_QHierarchySettings.PrefabShow);
            activeColor             = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalActiveColor);
            inactiveColor           = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < 9)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= 9;
                rect.x = curRect.x;
                rect.y = curRect.y;
                return EM_QLayoutStatus.Success;
            }
        }
        
        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {
            #if UNITY_2018_3_OR_NEWER
                PrefabInstanceStatus prefabStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
                if (prefabStatus == PrefabInstanceStatus.MissingAsset ||
                    prefabStatus == PrefabInstanceStatus.Disconnected) {
                    QColorUtils.SetColor(inactiveColor);
                    UnityEngine.GUI.DrawTexture(rect, prefabTexture);
                    QColorUtils.ClearColor();
                } else if (!showPrefabConnectedIcon && prefabStatus != PrefabInstanceStatus.NotAPrefab) {
                    QColorUtils.SetColor(activeColor);
                    UnityEngine.GUI.DrawTexture(rect, prefabTexture);
                    QColorUtils.ClearColor();
                }
            #else
                PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);
                if (prefabType == PrefabType.MissingPrefabInstance || 
                    prefabType == PrefabType.DisconnectedPrefabInstance ||
                    prefabType == PrefabType.DisconnectedModelPrefabInstance)
                {
                    QColorUtils.SetColor(inactiveColor);
                    GUI.DrawTexture(rect, prefabTexture);
                    QColorUtils.ClearColor();
                }
                else if (!showPrefabConnectedIcon && prefabType != PrefabType.None)
                {
                    QColorUtils.SetColor(activeColor);
                    GUI.DrawTexture(rect, prefabTexture);
                    QColorUtils.ClearColor();
                }
            #endif
        }
    }
}
