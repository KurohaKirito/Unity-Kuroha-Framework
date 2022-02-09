using UnityEngine;
using UnityEditor;
using System.Reflection;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using Kuroha.Util.RunTime;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentGameObjectIcon : QHierarchyBaseComponent
    {
        private readonly MethodInfo getIconMethodInfo;
        private readonly object[] getIconMethodParams;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentGameObjectIcon()
        {
            rect.width = 14;
            rect.height = 14;

            getIconMethodInfo = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            getIconMethodParams = new object[1];

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.GameObjectIconShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.GameObjectIconShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.GameObjectIconSize, SettingsChanged);
            SettingsChanged();
        }

        /// <summary>
        /// 修改设置
        /// </summary>
        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.GameObjectIconShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.GameObjectIconShowDuringPlayMode);
            var size = (EM_QHierarchySizeAll) QSettings.Instance().Get<int>(EM_QHierarchySettings.GameObjectIconSize);

            rect.width = rect.height = size switch
            {
                EM_QHierarchySizeAll.Normal => 15,
                EM_QHierarchySizeAll.Big => 16,
                _ => 13
            };
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < rect.width + COMPONENT_SPACE)
            {
                return EM_QLayoutStatus.Failed;
            }

            curRect.x -= rect.width + COMPONENT_SPACE;
            rect.x = curRect.x;
            rect.y = curRect.y - (rect.height - 16) / 2;
            
            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hierarchyObjectList"></param>
        /// <param name="selectionRect"></param>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            getIconMethodParams[0] = gameObject;
            
            var icon = (Texture2D) getIconMethodInfo.Invoke(null, getIconMethodParams);
            if (icon != null)
            {
                UnityEngine.GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
            }
        }

        /// <summary>
        /// 点击事件
        /// </summary>
        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            // 左键点击图标
            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                currentEvent.Use();
                
                var dynamicAssembly = ReflectionUtil.GetAssembly(typeof(EditorWindow));
                var dynamicClass = ReflectionUtil.GetClass(dynamicAssembly, "UnityEditor.IconSelector");
                
                // 由于目标方法重载, 需要使用参数类型进行区分
                // private internal static bool ShowAtPosition(Object   targetObj, Rect activatorRect, bool showLabelIcons)
                // private internal static bool ShowAtPosition(Object[] targetObj, Rect activatorRect, bool showLabelIcons)
                var paramsTypeArray = new[] { typeof(Object), typeof(Rect), typeof(bool) };
                var dynamicMethod = ReflectionUtil.GetMethod(dynamicClass, "ShowAtPosition", BindingFlags.Static | BindingFlags.NonPublic, paramsTypeArray);
                
                
                // 调用
                var paramsArray = new object[] { gameObject, rect, true };
                ReflectionUtil.CallMethod(dynamicMethod, paramsArray);
            }
        }
    }
}
