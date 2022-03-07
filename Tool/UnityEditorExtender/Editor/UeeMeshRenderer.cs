using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.UnityEditorExtender.Editor
{
    /// <summary>
    /// Uee: Unity Editor Extender
    /// Unity 编辑器原布局扩展器
    /// </summary>
    [CustomEditor(typeof(MeshRenderer))]
    public class UeeMeshRenderer : DecoratorEditor
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public UeeMeshRenderer() : base ("MeshRendererEditor")
        {
            // ...
        }
        
        /// <summary>
        /// 绘制 Inspector
        /// </summary>
        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();
        
            if(GUILayout.Button("Adding this button"))
            {
                Debug.Log("Adding this button");
            }
        }
    }
}
