using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentPrefab : QHierarchyBaseComponent
    {
        private Color activeColor;
        private Color inactiveColor;
        private bool onlyShowBroken;
        private readonly Texture2D prefabTexture;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentPrefab()
        {
            rect.width = 9;

            prefabTexture = QResources.Instance().GetTexture(QTexture.QPrefabIcon);

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.PrefabShowBrakedPrefabsOnly, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.PrefabShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalActiveColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor, SettingsChanged);
            SettingsChanged();
        }

        /// <summary>
        /// 更改设置
        /// </summary>
        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.PrefabShow);
            activeColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalActiveColor);
            inactiveColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
            onlyShowBroken = QSettings.Instance().Get<bool>(EM_QHierarchySettings.PrefabShowBrakedPrefabsOnly);
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < 9)
            {
                return EM_QLayoutStatus.Failed;
            }

            curRect.x -= 9;
            rect.x = curRect.x;
            rect.y = curRect.y;
            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
            
            // Missing 和 Disconnected
            if (prefabStatus == PrefabInstanceStatus.MissingAsset || prefabStatus == PrefabInstanceStatus.Disconnected)
            {
                QHierarchyColorUtils.SetColor(inactiveColor);
                UnityEngine.GUI.DrawTexture(rect, prefabTexture);
                QHierarchyColorUtils.ClearColor();
            }
            
            // 正常 Prefab
            else if (onlyShowBroken == false && prefabStatus != PrefabInstanceStatus.NotAPrefab)
            {
                QHierarchyColorUtils.SetColor(activeColor);
                UnityEngine.GUI.DrawTexture(rect, prefabTexture);
                QHierarchyColorUtils.ClearColor();
            }
        }
    }
}