using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QTreeMapComponent: QHierarchyBaseComponent
    {
        // CONST
        private const float TREE_STEP_WIDTH  = 14.0f;
        
        // PRIVATE
        private bool enhanced;
        private bool transparentBackground;
        private Color treeMapColor;
        private Color backgroundColor;
        private readonly Texture2D treeMapLevelTexture;       
        private readonly Texture2D treeMapLevel4Texture;       
        private readonly Texture2D treeMapCurrentTexture;   
        private readonly Texture2D treeMapLastTexture;
        private readonly Texture2D treeMapObjectTexture;    
        
        // CONSTRUCTOR
        public QTreeMapComponent()
        {
            treeMapLevelTexture   = QResources.Instance().GetTexture(QTexture.QTreeMapLevel);
            treeMapLevel4Texture  = QResources.Instance().GetTexture(QTexture.QTreeMapLevel4);
            treeMapCurrentTexture = QResources.Instance().GetTexture(QTexture.QTreeMapCurrent);
            treeMapObjectTexture  = QResources.Instance().GetTexture(QTexture.QTreeMapLine);
            treeMapLastTexture    = QResources.Instance().GetTexture(QTexture.QTreeMapLast);
            
            rect.width  = 14;
            rect.height = 16;
            
            showComponentDuringPlayMode = true;

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalBackgroundColor   , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TreeMapShow                 , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TreeMapColor                , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TreeMapEnhanced             , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TreeMapTransparentBackground, SettingsChanged);
            
            SettingsChanged();
        }
        
        /// <summary>
        /// 设置更改
        /// </summary>
        private void SettingsChanged()
        {
            backgroundColor       = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalBackgroundColor);
            enabled               = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TreeMapShow);
            treeMapColor          = QSettings.Instance().GetColor(EM_QHierarchySettings.TreeMapColor);
            enhanced              = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TreeMapEnhanced);
            transparentBackground = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TreeMapTransparentBackground);
        }
        
        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            rect.y = selectionRect.y;
            
            if (transparentBackground == false) 
            {
                rect.x = 0;
                rect.width = selectionRect.x - 14;
                EditorGUI.DrawRect(rect, backgroundColor);
                rect.width = 14;
            }

            return EM_QLayoutStatus.Success;
        }

        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var childCount = gameObject.transform.childCount;
            var level = Mathf.RoundToInt(selectionRect.x / 14.0f);

            if (enhanced)
            {
                var gameObjectTransform = gameObject.transform;
                Transform parentTransform = null;

                for (int i = 0, j = level - 1; j >= 0; i++, j--)
                {
                    rect.x = 14 * j;
                    if (i == 0)
                    {
                        if (childCount == 0) {
                            #if UNITY_2018_3_OR_NEWER
                                QHierarchyColorUtils.SetColor(treeMapColor);
                            #endif
                            UnityEngine.GUI.DrawTexture(rect, treeMapObjectTexture);
                        }
                        gameObjectTransform = gameObject.transform;
                    }
                    else if (i == 1)
                    {
                        QHierarchyColorUtils.SetColor(treeMapColor);
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
                QHierarchyColorUtils.ClearColor();
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
                                QHierarchyColorUtils.SetColor(treeMapColor);
                            #endif
                            UnityEngine.GUI.DrawTexture(rect, treeMapObjectTexture);
                        }
                    }
                    else if (i == 1)
                    {
                        QHierarchyColorUtils.SetColor(treeMapColor);
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
                QHierarchyColorUtils.ClearColor();
            }
        }
    }
}
