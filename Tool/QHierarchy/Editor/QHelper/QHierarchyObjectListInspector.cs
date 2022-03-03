using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    /// <summary>
    /// 重绘 QHierarchyObjectList 的 Inspector 面板
    /// </summary>
    [CustomEditor(typeof(QHierarchyObjectList))]
    public class QHierarchyObjectListInspector : UnityEditor.Editor
    {
        /// <summary>
        /// 帮助提示
        /// </summary>
        private const string HELP_MESSAGE = "这是一个由 QHierarchy 插件管理并自动创建的游戏物体.它存储了当前场景中一些游戏对象的引用.\r\n此对象不会包含在应用程序构建中. 您可以安全地删除它, 但删除后 '是否锁定' '颜色标记' 等设置将被重置.\r\n可以在 QHierarchy 设置中隐藏此列表.";
        
        /// <summary>
        /// 重新绘制 Inspector 面板
        /// </summary>
        public override void OnInspectorGUI()
        {
            // 绘制帮助提示框
            EditorGUILayout.HelpBox(HELP_MESSAGE, MessageType.Info, true);

            if (QSettings.Instance().Get<bool>(EM_QHierarchySettings.AdditionalShowObjectListContent))
            {
                var newRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(20));
                if (UnityEngine.GUI.Button(newRect, "隐藏脚本内容"))
                {
                    QSettings.Instance().Set(EM_QHierarchySettings.AdditionalShowObjectListContent, false);
                }

                base.OnInspectorGUI();
            }
            else
            {
                var newRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(20));
                if (UnityEngine.GUI.Button(newRect, "显示脚本内容"))
                {
                    QSettings.Instance().Set(EM_QHierarchySettings.AdditionalShowObjectListContent, true);
                }
            }
        }
    }
}
