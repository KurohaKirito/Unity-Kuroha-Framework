using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Kuroha.Tool.UnityEditorExtender.Editor
{
    /// <summary>
    /// 扩展 Mesh Renderer
    /// </summary>
    [CustomEditor(typeof(MeshRenderer))]
    public class UeeMeshRenderer : UeeBase
    {
        private MeshRenderer self;
        private bool showLayerValue;
        private readonly List<string> layerNames = new List<string>();
        private const string BUTTON_TO_EDIT = "Edit Layer Value";
        private const string BUTTON_TO_SAVE = "Save Layer Value";

        /// <summary>
        /// 构造方法
        /// </summary>
        public UeeMeshRenderer() : base("UnityEditor.MeshRendererEditor") { }

        /// <summary>
        /// 绘制 Inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            self = target as MeshRenderer;

            var defaultColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = Color.green;
            if (GUILayout.Button(showLayerValue ? BUTTON_TO_SAVE : BUTTON_TO_EDIT))
            {
                showLayerValue = !showLayerValue;
            }

            UnityEngine.GUI.color = defaultColor;

            if (showLayerValue)
            {
                if (self != null)
                {
                    if (layerNames.Count <= 0 || layerNames.Count != SortingLayer.layers.Length)
                    {
                        layerNames.Clear();
                        layerNames.AddRange(SortingLayer.layers.Select(layer => layer.name));
                    }
                    
                    // 显示 Sorting Layer
                    var layerValue = SortingLayer.GetLayerValueFromID(self.sortingLayerID);
                    layerValue = EditorGUILayout.Popup("Sorting Layer", layerValue, layerNames.ToArray());
                    self.sortingLayerName = SortingLayer.layers[layerValue].name;
                    self.sortingLayerID = SortingLayer.layers[layerValue].id;
                    
                    // 显示 Order in Layer
                    self.sortingOrder = EditorGUILayout.IntField("Order in Layer", self.sortingOrder);
                }
            }
        }
    }
}
