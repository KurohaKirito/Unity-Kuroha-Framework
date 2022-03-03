using UnityEngine;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentTreeMap : QHierarchyBaseComponent
    {
        private Color treeMapColor;
        private const int TREE_MAP_WIDTH = 14;
        private readonly Texture2D treeIconLevel;
        private readonly Texture2D treeIconCurrent;
        private readonly Texture2D treeIconLast;
        private readonly Texture2D treeIconLine;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentTreeMap()
        {
            rect.width = TREE_MAP_WIDTH;
            rect.height = GAME_OBJECT_HEIGHT;
            showComponentDuringPlayMode = true;
            
            treeIconLevel = QResources.Instance().GetTexture(EM_QHierarchyTexture.QTreeMapLevel);
            treeIconCurrent = QResources.Instance().GetTexture(EM_QHierarchyTexture.QTreeMapCurrent);
            treeIconLine = QResources.Instance().GetTexture(EM_QHierarchyTexture.QTreeMapLine);
            treeIconLast = QResources.Instance().GetTexture(EM_QHierarchyTexture.QTreeMapLast);
            
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
        public override void Draw(GameObject gameObjectToDraw, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            // 设置颜色
            UnityEngine.GUI.color = treeMapColor;
            
            // 得到当前需要绘制的物体的孩子数量
            var childCount = gameObjectToDraw.transform.childCount;
            
            // 计算出缩进级别, 每个缩进为 14 像素, 另外左侧有 4 像素留白
            var totalPixelCount = (int) selectionRect.x;
            var totalLevel = (totalPixelCount - 4) / TREE_MAP_WIDTH;
            
            // 临时变量
            var currentTransform = gameObjectToDraw.transform;
            Transform parentTransform = null;
            
            // 每一个物体都是从右向左绘制每一个图案
            for (int counter = 0, curLevel = totalLevel - 1; curLevel >= 0; counter++, curLevel--)
            {
                rect.x = TREE_MAP_WIDTH * curLevel;
                
                switch (counter)
                {
                    // 第 1 次循环
                    case 0:
                    {
                        if (childCount == 0)
                        {
                            UnityEngine.GUI.DrawTexture(rect, treeIconLine);
                        }
            
                        currentTransform = gameObjectToDraw.transform;
                        break;
                    }
                    // 第 2 次循环
                    case 1:
                    {
                        var hierarchyIndex = currentTransform.GetSiblingIndex();
                        
                        if (parentTransform == null)
                        {
                            var isHadSpecialObjectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObjectToDraw, false);
                            var sceneRootLastIndex = gameObjectToDraw.scene.rootCount - (isHadSpecialObjectList ? 2 : 1);
                            UnityEngine.GUI.DrawTexture(rect, hierarchyIndex == sceneRootLastIndex ? treeIconLast : treeIconCurrent);
                        }
                        else
                        {
                            var parentLastChildIndex = parentTransform.childCount - 1;
                            UnityEngine.GUI.DrawTexture(rect, hierarchyIndex == parentLastChildIndex ? treeIconLast : treeIconCurrent);
                        }
                    
                        currentTransform = parentTransform;
                        break;
                    }
                    // 后续循环
                    default:
                    {
                        var hierarchyIndex = currentTransform.GetSiblingIndex();
                        
                        if (parentTransform == null)
                        {
                            var isHadSpecialObjectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObjectToDraw, false);
                            var sceneRootLastIndex = gameObjectToDraw.scene.rootCount - (isHadSpecialObjectList ? 2 : 1);;
                            if (hierarchyIndex != sceneRootLastIndex)
                            {
                                UnityEngine.GUI.DrawTexture(rect, treeIconLevel);
                            }
                        }
                        else
                        {
                            var parentLastChildIndex = parentTransform.childCount - 1;
                            if (hierarchyIndex != parentLastChildIndex)
                            {
                                UnityEngine.GUI.DrawTexture(rect, treeIconLevel);
                            }
                        }
                    
                        currentTransform = parentTransform;
                        break;
                    }
                }

                if (currentTransform != null) {
                    parentTransform = currentTransform.parent;
                } else {
                    break;
                }
            }

            // 恢复默认颜色
            UnityEngine.GUI.color = QHierarchyColorUtils.DefaultColor;
        }
    }
}
