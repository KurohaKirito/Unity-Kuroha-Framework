using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.InspectorExtender.Editor
{
    /// <summary>
    /// 扩展 Mesh Renderer
    /// </summary>
    [CustomEditor(typeof(MeshRenderer))]
    public class IeMeshRenderer : IeBase
    {
        private MeshFilter selfFilter;
        private MeshRenderer self;
        private bool showLayerValue;
        private readonly List<string> layerNames = new List<string>();
        private const string BUTTON_TO_EDIT = "Edit Layer Value";
        private const string BUTTON_TO_SAVE = "Save Layer Value";

        /// <summary>
        /// 构造方法
        /// </summary>
        public IeMeshRenderer() : base("UnityEditor.MeshRendererEditor") { }

        /// <summary>
        /// 绘制 Inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            self = target as MeshRenderer;

            #region 绘制按钮
            
            if (self != null)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button(showLayerValue ? BUTTON_TO_SAVE : BUTTON_TO_EDIT))
                {
                    showLayerValue = !showLayerValue;
                }
                
                if (selfFilter == null)
                {
                    selfFilter = self.GetComponent<MeshFilter>();
                }

                if (selfFilter != null)
                {
                    var mesh = selfFilter.sharedMesh;
                
                    if (GUILayout.Button("Save Mesh..."))
                    {
                        SaveMesh(mesh, mesh.name, false, true);
                    }
                
                    if (GUILayout.Button("Save Mesh As New Instance..."))
                    {
                        SaveMesh(mesh, mesh.name, true, true);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }

            #endregion

            ShowSortingLayer();
        }
        
        /// <summary>
        /// 另存为 Mesh
        /// </summary>
        private static void SaveMesh(Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
        {
            var path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            path = FileUtil.GetProjectRelativePath(path);

            var meshToSave = makeNewInstance ? Instantiate(mesh) : mesh;
            
            if (optimizeMesh)
            {
                MeshUtility.Optimize(meshToSave);
            }

            AssetDatabase.CreateAsset(meshToSave, path);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 显示 Sorting Layer 信息
        /// </summary>
        private void ShowSortingLayer()
        {
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
