using UnityEngine;
using UnityEditor;
using System;
using Kuroha.Tool.QHierarchy.Editor.QData;
using qtools.qhierarchy.phierarchy;
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

            treeMapLevelTexture   = QResources.getInstance().getTexture(QTexture.QTreeMapLevel);
            treeMapLevel4Texture  = QResources.getInstance().getTexture(QTexture.QTreeMapLevel4);
            treeMapCurrentTexture = QResources.getInstance().getTexture(QTexture.QTreeMapCurrent);
            #if UNITY_2018_3_OR_NEWER
                treeMapObjectTexture = QResources.getInstance().getTexture(QTexture.QTreeMapLine);
            #else
                treeMapObjectTexture  = QResources.getInstance().getTexture(QTexture.QTreeMapObject);
            #endif
            treeMapLastTexture    = QResources.getInstance().getTexture(QTexture.QTreeMapLast);
            
            rect.width  = 14;
            rect.height = 16;
            
            showComponentDuringPlayMode = true;

            QSettings.getInstance().addEventListener(EM_QSetting.AdditionalBackgroundColor, settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.TreeMapShow           , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.TreeMapColor          , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.TreeMapEnhanced       , settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.TreeMapTransparentBackground, settingsChanged);
            settingsChanged();
        }
        
        // PRIVATE
        private void settingsChanged() {
            backgroundColor     = QSettings.getInstance().getColor(EM_QSetting.AdditionalBackgroundColor);
            enabled             = QSettings.getInstance().get<bool>(EM_QSetting.TreeMapShow);
            treeMapColor        = QSettings.getInstance().getColor(EM_QSetting.TreeMapColor);
            enhanced            = QSettings.getInstance().get<bool>(EM_QSetting.TreeMapEnhanced);
            transparentBackground = QSettings.getInstance().get<bool>(EM_QSetting.TreeMapTransparentBackground);
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

