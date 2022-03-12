using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Util.Editor;
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
        private static QHierarchyMain qHierarchyMain;

        /// <summary>
        /// 静态构造函数不使用访问修饰符
        /// 静态构造函数不能直接调用, 仅由公共语言运行时 (CLR) 自动调用, 此处则是使用 [InitializeOnLoad] 特性来调用
        /// </summary>
        static QHierarchyInitializer()
        {
            var timer = new TimerUtil(100, EditorApplication.RepaintHierarchyWindow)
            {
                AutoReStart = true
            };
            
            timer.Start();
            
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
        /// 初始化 QHierarchy 工具
        /// </summary>
        private static void InitQHierarchy()
        {
            if (qHierarchyMain == null)
            {
                qHierarchyMain = new QHierarchyMain();
            }
        }

        /// <summary>
        /// 编辑器帧更新
        /// </summary>
        private static void EditorUpdate()
        {
            InitQHierarchy();
            QHierarchyObjectListManager.Instance().OnEditorUpdate();
        }

        /// <summary>
        /// Unity 绘制 Hierarchy 面板时触发的回调
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="selectionRect"></param>
        private static void HierarchyWindowItemOnGUIHandler(int instanceId, Rect selectionRect)
        {
            InitQHierarchy();
            qHierarchyMain.HierarchyWindowItemOnGUIHandler(instanceId, selectionRect);
        }

        /// <summary>
        /// Unity Hierarchy 面板发生更改时会调用的回调
        /// </summary>
        private static void HierarchyWindowChanged()
        {
            InitQHierarchy();
            QHierarchyObjectListManager.Instance().Validate();
        }
        
        /// <summary>
        /// Unity 执行 "撤销" 时触发的回调
        /// </summary>
        private static void UndoRedoPerformed()
        {
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}
