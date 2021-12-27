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

            prefabTexture = QResources.getInstance().getTexture(QTexture.QPrefabIcon);

            QSettings.getInstance().addEventListener(EM_QSetting.PrefabShowBrakedPrefabsOnly  , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.PrefabShow                    , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.AdditionalActiveColor         , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.AdditionalInactiveColor       , settingsChanged);
            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged()
        {
            showPrefabConnectedIcon = QSettings.getInstance().get<bool>(EM_QSetting.PrefabShowBrakedPrefabsOnly);
            enabled                 = QSettings.getInstance().get<bool>(EM_QSetting.PrefabShow);
            activeColor             = QSettings.getInstance().getColor(EM_QSetting.AdditionalActiveColor);
            inactiveColor           = QSettings.getInstance().getColor(EM_QSetting.AdditionalInactiveColor);
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
                    QColorUtils.setColor(inactiveColor);
                    UnityEngine.GUI.DrawTexture(rect, prefabTexture);
                    QColorUtils.clearColor();
                } else if (!showPrefabConnectedIcon && prefabStatus != PrefabInstanceStatus.NotAPrefab) {
                    QColorUtils.setColor(activeColor);
                    UnityEngine.GUI.DrawTexture(rect, prefabTexture);
                    QColorUtils.clearColor();
                }
            #else
                PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);
                if (prefabType == PrefabType.MissingPrefabInstance || 
                    prefabType == PrefabType.DisconnectedPrefabInstance ||
                    prefabType == PrefabType.DisconnectedModelPrefabInstance)
                {
                    QColorUtils.setColor(inactiveColor);
                    GUI.DrawTexture(rect, prefabTexture);
                    QColorUtils.clearColor();
                }
                else if (!showPrefabConnectedIcon && prefabType != PrefabType.None)
                {
                    QColorUtils.setColor(activeColor);
                    GUI.DrawTexture(rect, prefabTexture);
                    QColorUtils.clearColor();
                }
            #endif
        }
    }
}
