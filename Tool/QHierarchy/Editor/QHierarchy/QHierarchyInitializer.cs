using Kuroha.Tool.QHierarchy.Editor.QHelper;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor
{
    [InitializeOnLoad]
    public class QHierarchyInitializer
    {
        private static qtools.qhierarchy.phierarchy.QHierarchy hierarchy;

        static QHierarchyInitializer()
        {
            EditorApplication.update -= update;
            EditorApplication.update += update;

            EditorApplication.hierarchyWindowItemOnGUI -= hierarchyWindowItemOnGUIHandler;
            EditorApplication.hierarchyWindowItemOnGUI += hierarchyWindowItemOnGUIHandler;
            
            EditorApplication.hierarchyChanged -= hierarchyWindowChanged;
            EditorApplication.hierarchyChanged += hierarchyWindowChanged;

            Undo.undoRedoPerformed -= undoRedoPerformed;
            Undo.undoRedoPerformed += undoRedoPerformed;
        }

        static void undoRedoPerformed()
        {
            EditorApplication.RepaintHierarchyWindow();          
        }

        static void init()
        {       
            hierarchy = new qtools.qhierarchy.phierarchy.QHierarchy();
        } 

        static void update()
        {
            if (hierarchy == null) init();
            QObjectListManager.getInstance().update();
        }

        static void hierarchyWindowItemOnGUIHandler(int instanceId, Rect selectionRect)
        {
            if (hierarchy == null) init();
             hierarchy.hierarchyWindowItemOnGUIHandler(instanceId, selectionRect);
        }

        static void hierarchyWindowChanged()
        {
            if (hierarchy == null) init();
            QObjectListManager.getInstance().validate();
        }
    }
}
