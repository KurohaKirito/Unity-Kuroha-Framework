using UnityEngine;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentTreeMap : QHierarchyBaseComponent
    {
        private const float TREE_STEP_WIDTH = 14.0f;

        private Color treeMapColor;
        
        private readonly Texture2D treeMapLevelTexture;
        private readonly Texture2D treeMapCurrentTexture;
        private readonly Texture2D treeMapLastTexture;
        private readonly Texture2D treeMapObjectTexture;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentTreeMap()
        {
            rect.width = TREE_STEP_WIDTH;
            rect.height = GAME_OBJECT_HEIGHT;
            showComponentDuringPlayMode = true;
            
            treeMapLevelTexture = QResources.Instance().GetTexture(EM_QHierarchyTexture.QTreeMapLevel);
            treeMapCurrentTexture = QResources.Instance().GetTexture(EM_QHierarchyTexture.QTreeMapCurrent);
            treeMapObjectTexture = QResources.Instance().GetTexture(EM_QHierarchyTexture.QTreeMapLine);
            treeMapLastTexture = QResources.Instance().GetTexture(EM_QHierarchyTexture.QTreeMapLast);
            
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TreeMapShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TreeMapColor, SettingsChanged);

            SettingsChanged();
        }

        /// <summary>
        /// 设置更改
        /// </summary>
        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TreeMapShow);
            treeMapColor = QSettings.Instance().GetColor(EM_QHierarchySettings.TreeMapColor);
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            rect.y = selectionRect.y;
            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var childCount = gameObject.transform.childCount;
            
            var level = Mathf.RoundToInt(selectionRect.x / 14.0f);
            
            var gameObjectTransform = gameObject.transform;
            Transform parentTransform = null;

            for (int i = 0, j = level - 1; j >= 0; i++, j--)
            {
                rect.x = 14 * j;
                
                switch (i)
                {
                    case 0:
                    {
                        if (childCount == 0)
                        {
                            UnityEngine.GUI.color = treeMapColor;
                            UnityEngine.GUI.DrawTexture(rect, treeMapObjectTexture);
                        }

                        gameObjectTransform = gameObject.transform;
                        break;
                    }
                    case 1:
                    {
                        UnityEngine.GUI.color = treeMapColor;
                        if (parentTransform == null)
                        {
                            UnityEngine.GUI.DrawTexture(rect, gameObjectTransform.GetSiblingIndex() == gameObject.scene.rootCount - 1 ? treeMapLastTexture : treeMapCurrentTexture);
                        }
                        else if (gameObjectTransform.GetSiblingIndex() == parentTransform.childCount - 1)
                        {
                            UnityEngine.GUI.DrawTexture(rect, treeMapLastTexture);
                        }
                        else
                        {
                            UnityEngine.GUI.DrawTexture(rect, treeMapCurrentTexture);
                        }

                        gameObjectTransform = parentTransform;
                        break;
                    }
                    default:
                    {
                        if (parentTransform == null)
                        {
                            if (gameObjectTransform.GetSiblingIndex() != gameObject.scene.rootCount - 1)
                            {
                                UnityEngine.GUI.DrawTexture(rect, treeMapLevelTexture);
                            }
                        }
                        else if (gameObjectTransform.GetSiblingIndex() != parentTransform.childCount - 1)
                        {
                            UnityEngine.GUI.DrawTexture(rect, treeMapLevelTexture);
                        }

                        gameObjectTransform = parentTransform;
                        break;
                    }
                }

                if (gameObjectTransform != null)
                {
                    parentTransform = gameObjectTransform.parent;
                }
                else
                {
                    break;
                }
            }

            UnityEngine.GUI.color = QHierarchyColorUtils.DefaultColor;
        }
    }
}
