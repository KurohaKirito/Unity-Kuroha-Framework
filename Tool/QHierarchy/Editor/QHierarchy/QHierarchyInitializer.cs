using Kuroha.Tool.QHierarchy.Editor.QHelper;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QHierarchy
{
    /// <summary>
    /// 只能修饰具有静态构造函数的类
    /// </summary>
    [InitializeOnLoad]
    public class QHierarchyInitializer
    {
        /// <summary>
        /// 工具实例
        /// </summary>
        private static QHierarchyMain hierarchyMain;

        /// <summary>
        /// 静态构造函数始终保证在使用类的任何静态函数或实例之前调用
        /// </summary>
        static QHierarchyInitializer()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUIHandler;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUIHandler;
            
            EditorApplication.hierarchyChanged -= HierarchyWindowChanged;
            EditorApplication.hierarchyChanged += HierarchyWindowChanged;

            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        /// <summary>
        /// Unity 执行 "撤销" 时触发的回调
        /// </summary>
        private static void UndoRedoPerformed()
        {
            EditorApplication.RepaintHierarchyWindow();          
        }

        /// <summary>
        /// 初始化 QHierarchyMain 工具
        /// </summary>
        private static void InitQHierarchy()
        {       
            hierarchyMain = new QHierarchyMain();
        }

        /// <summary>
        /// 编辑器帧更新
        /// </summary>
        private static void EditorUpdate()
        {
            if (hierarchyMain == null)
            {
                InitQHierarchy();
            }
            
            QObjectListManager.Instance().OnEditorUpdate();
        }

        /// <summary>
        /// Unity 绘制 Hierarchy 面板时触发的回调
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="selectionRect"></param>
        private static void HierarchyWindowItemOnGUIHandler(int instanceId, Rect selectionRect) {
            if (hierarchyMain == null)
            {
                InitQHierarchy();
            }

            hierarchyMain?.HierarchyWindowItemOnGUIHandler(instanceId, selectionRect);
        }

        /// <summary>
        /// Unity Hierarchy 面板发生更改时会调用的回调
        /// </summary>
        private static void HierarchyWindowChanged()
        {
            if (hierarchyMain == null)
            {
                InitQHierarchy();
            }
            
            QObjectListManager.Instance().Validate();
        }
    }
}
