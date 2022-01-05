using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    [CustomEditor(typeof(QObjectList))]
    public class QObjectListInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("\nThis is an auto created GameObject that managed by QHierarchyMain.\n\n" +
                                    "It stores references to some GameObjects in the current scene. This object will not be included in the application build.\n\n" +
                                    "You can safely remove it, but lock / unlock / visible / etc. states will be reset. Delete this object if you want to remove the QHierarchyMain.\n\n" +
                                    "This object can be hidden if you uncheck \"Show QHierarchyMain GameObject\" in the settings of the QHierarchyMain.\n"
                , MessageType.Info, true);

            if (QSettings.Instance().Get<bool>(EM_QHierarchySettings.AdditionalShowObjectListContent))
            {
                if (UnityEngine.GUI.Button(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(20)), "Hide content"))
                {
                    QSettings.Instance().Set(EM_QHierarchySettings.AdditionalShowObjectListContent, false);
                }

                base.OnInspectorGUI();
            }
            else
            {
                if (UnityEngine.GUI.Button(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(20)), "Show content"))
                {
                    QSettings.Instance().Set(EM_QHierarchySettings.AdditionalShowObjectListContent, true);
                }
            }
        }
    }
}
