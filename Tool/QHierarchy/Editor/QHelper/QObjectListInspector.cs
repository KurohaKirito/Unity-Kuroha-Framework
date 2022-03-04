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
    		EditorGUILayout.HelpBox("\nThis is an auto created GameObject that managed by QHierarchy.\n\n" + 
                                    "It stores references to some GameObjects in the current scene. This object will not be included in the application build.\n\n" + 
                                    "You can safely remove it, but lock / unlock / visible / etc. states will be reset. Delete this object if you want to remove the QHierarchy.\n\n" +
                                    "This object can be hidden if you uncheck \"Show QHierarchy GameObject\" in the settings of the QHierarchy.\n"
                                    , MessageType.Info, true);

            if (QSettings.Instance().Get<bool>(EM_QSetting.AdditionalShowObjectListContent))
            {
                if (UnityEngine.GUI.Button(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(20)), "Hide content"))
                {
                    QSettings.Instance().Set(EM_QSetting.AdditionalShowObjectListContent, false);
                }
                base.OnInspectorGUI();
            }
            else
            {
                if (UnityEngine.GUI.Button(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(20)), "Show content"))
                {
                    QSettings.Instance().Set(EM_QSetting.AdditionalShowObjectListContent, true);
                }
            }
    	}
    }
}