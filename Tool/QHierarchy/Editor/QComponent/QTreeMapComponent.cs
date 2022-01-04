using UnityEngine;
using UnityEditor;
using System;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using System.Collections.Generic;
using System.Collections;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QTreeMapComponent: QBaseComponent
    {
        // CONST
        private const float TREE_STEP_WIDTH  = 14.0f;
        
        // PRIVATE
        private Texture2D treeMapLevelTexture;       
        private Texture2D treeMapLevel4Texture;       
        private Texture2D treeMapCurrentTexture;   
        private Texture2D treeMapLastTexture;
        private Texture2D treeMapObjectTexture;    
        private bool enhanced;
        private bool transparentBackground;
        private Color backgroundColor;
        private Color treeMapColor;
        
        // CONSTRUCTOR
        public QTreeMapComponent()
        { 

            treeMapLevelTexture   = QResources.Instance().GetTexture(QTexture.QTreeMapLevel);
            treeMapLevel4Texture  = QResources.Instance().GetTexture(QTexture.QTreeMapLevel4);
            treeMapCurrentTexture = QResources.Instance().GetTexture(QTexture.QTreeMapCurrent);
            #if UNITY_2018_3_OR_NEWER
                treeMapObjectTexture = QResources.Instance().GetTexture(QTexture.QTreeMapLine);
            #else
                treeMapObjectTexture  = QResources.Instance().getTexture(QTexture.QTreeMapObject);
            #endif
            treeMapLastTexture    = QResources.Instance().GetTexture(QTexture.QTreeMapLast);
            
            rect.width  = 14;
            rect.height = 16;
            
            showComponentDuringPlayMode = true;

            QSettings.Instance().addEventListener(EM_QSetting.AdditionalBackgroundColor, settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.TreeMapShow           , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.TreeMapColor          , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.TreeMapEnhanced       , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.TreeMapTransparentBackground, settingsChanged);
            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged() {
            backgroundColor     = QSettings.Instance().GetColor(EM_QSetting.AdditionalBackgroundColor);
            enabled             = QSettings.Instance().Get<bool>(EM_QSetting.TreeMapShow);
            treeMapColor        = QSettings.Instance().GetColor(EM_QSetting.TreeMapColor);
            enhanced            = QSettings.Instance().Get<bool>(EM_QSetting.TreeMapEnhanced);
            transparentBackground = QSettings.Instance().Get<bool>(EM_QSetting.TreeMapTransparentBackground);
        }
        
        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            rect.y = selectionRect.y;
            
            if (!transparentBackground) 
            {
                rect.x = 0;
                
                rect.width = selectionRect.x - 14;
                EditorGUI.DrawRect(rect, backgroundColor);
                rect.width = 14;
            }

            return EM_QLayoutStatus.Success;
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {
            int childCount = gameObject.transform.childCount;
            int level = Mathf.RoundToInt(selectionRect.x / 14.0f);

            if (enhanced)
            {
                Transform gameObjectTransform = gameObject.transform;
                Transform parentTransform = null;

                for (int i = 0, j = level - 1; j >= 0; i++, j--)
                {
                    rect.x = 14 * j;
                    if (i == 0)
                    {
                        if (childCount == 0) {
                            #if UNITY_2018_3_OR_NEWER
                                QColorUtils.setColor(treeMapColor);
                            #endif
                            UnityEngine.GUI.DrawTexture(rect, treeMapObjectTexture);
                        }
                        gameObjectTransform = gameObject.transform;
                    }
                    else if (i == 1)
                    {
                        QColorUtils.setColor(treeMapColor);
                        if (parentTransform == null) {
                            if (gameObjectTransform.GetSiblingIndex() == gameObject.scene.rootCount - 1) {
                                UnityEngine.GUI.DrawTexture(rect, treeMapLastTexture);
                            } else {
                                UnityEngine.GUI.DrawTexture(rect, treeMapCurrentTexture);
                            }
                        } else if (gameObjectTransform.GetSiblingIndex() == parentTransform.childCount - 1) {
                            UnityEngine.GUI.DrawTexture(rect, treeMapLastTexture);
                        } else {
                            UnityEngine.GUI.DrawTexture(rect, treeMapCurrentTexture);
                        }
                        gameObjectTransform = parentTransform;
                    }
                    else
                    {
                        if (parentTransform == null) {
                            if (gameObjectTransform.GetSiblingIndex() != gameObject.scene.rootCount - 1)
                                UnityEngine.GUI.DrawTexture(rect, treeMapLevelTexture);
                        } else if (gameObjectTransform.GetSiblingIndex() != parentTransform.childCount - 1)
                            UnityEngine.GUI.DrawTexture(rect, treeMapLevelTexture);

                        gameObjectTransform = parentTransform;                       
                    }
                    if (gameObjectTransform != null) 
						parentTransform = gameObjectTransform.parent;
					else
                        break;
                }
                QColorUtils.clearColor();
            }
            else
            {
                for (int i = 0, j = level - 1; j >= 0; i++, j--)
                {
                    rect.x = 14 * j;
                    if (i == 0)
                    {
                        if (childCount > 0)
                            continue;
                        else {
                            #if UNITY_2018_3_OR_NEWER
                                QColorUtils.setColor(treeMapColor);
                            #endif
                            UnityEngine.GUI.DrawTexture(rect, treeMapObjectTexture);
                        }
                    }
                    else if (i == 1)
                    {
                        QColorUtils.setColor(treeMapColor);
                        UnityEngine.GUI.DrawTexture(rect, treeMapCurrentTexture);
                    }
                    else
                    {
                        rect.width = 14 * 4;
                        rect.x -= 14 * 3;
                        j -= 3;
                        UnityEngine.GUI.DrawTexture(rect, treeMapLevel4Texture);
                        rect.width = 14;
                    }
                }
                QColorUtils.clearColor();
            }
        }
    }
}

