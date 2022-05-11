using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kuroha.Framework.Utility.RunTime;
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
        private bool meshFoldout = true;
        private Mesh selfMesh;
        private MeshFilter selfFilter;
        private MeshRenderer selfMeshRenderer;
        private MeshRenderer selfMeshRendererOld;
        
        private bool rwEnable;
        private string meshPath;
        private ModelImporterMeshCompression compression;
        private bool revertButton;
        private bool applyButton;
        private readonly Regex regexReadWrite = new Regex(@"m_IsReadable: [\d]");
        private readonly Regex regexCompression = new Regex(@"m_MeshCompression: [\d]");

        private bool showLayerValue;
        private readonly List<string> layerNames = new List<string>();
        private const string BUTTON_TO_EDIT = "Edit Layer Value";
        private const string BUTTON_TO_SAVE = "Save Layer Value";

        /// <summary>
        /// 构造方法
        /// </summary>
        public IeMeshRenderer() : base("UnityEditor.MeshRendererEditor") { }

        private void ReloadTarget()
        {
            selfMeshRendererOld = selfMeshRenderer;
            
            selfFilter = selfMeshRenderer.GetComponent<MeshFilter>();
            
            if (selfFilter != null)
            {
                selfMesh = selfFilter.sharedMesh;
            }

            if (selfMesh != null)
            {
                meshPath = AssetDatabase.GetAssetPath(selfMesh);
                rwEnable = selfMesh.isReadable;
                compression = MeshUtility.GetMeshCompression(selfMesh);
                DebugUtil.Log("重新加载了 Mesh", this, "yellow");
            }
        }
        
        private void UpdateTarget()
        {
            selfMeshRenderer = target as MeshRenderer;
            
            if (selfFilter == null || selfMesh == null || selfMeshRenderer != selfMeshRendererOld)
            {
                ReloadTarget();
            }
        }

        private async void OnMeshGUI()
        {
            if (selfMesh == null)
            {
                return;
            }
            
            meshFoldout = EditorGUILayout.Foldout(meshFoldout, "Mesh", true);
            if (meshFoldout)
            {
                OnApplyButtonGUI();
                
                if (revertButton)
                {
                    ReloadTarget();
                }
                
                if (applyButton)
                {
                    await SetSettings();
                }
            }
        }

        private void OnApplyButtonGUI()
        {
            // Unity 内置资源不可修改
            if (meshPath.IndexOf("Assets", StringComparison.Ordinal) < 0)
            {
                GUI.enabled = false;
            }
            
            rwEnable = EditorGUILayout.Toggle("Read / Write Enabled", rwEnable);
            compression = (ModelImporterMeshCompression) EditorGUILayout.EnumPopup("Mesh Compression", compression);
            
            if (rwEnable == selfMesh.isReadable && compression == MeshUtility.GetMeshCompression(selfMesh))
            {
                GUI.enabled = false;
            }
                
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            revertButton = GUILayout.Button("Revert");
            applyButton = GUILayout.Button("Apply");
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }
        
        private async Task SetSettings()
        {
            string meshData;
            var meshFullPath = Path.GetFullPath(meshPath);
            DebugUtil.Log($"meshFullPath: {meshFullPath}");
            using (var reader = new StreamReader(meshFullPath))
            {
                meshData = await reader.ReadToEndAsync();
                var readWriteString = "m_IsReadable: " + (rwEnable ? 1 : 0);
                var compressionString = "m_MeshCompression: " + Convert.ToInt32(compression);
                meshData = regexReadWrite.Replace(meshData, readWriteString);
                meshData = regexCompression.Replace(meshData, compressionString);
            }
            
            using (var writer = new StreamWriter(meshFullPath))
            {
                await writer.WriteAsync(meshData);
            }
            
            AssetDatabase.Refresh();
            ReloadTarget();
        }

        /// <summary>
        /// 绘制 Inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UpdateTarget();
            OnMeshGUI();

            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            #region 绘制按钮

            if (selfMeshRenderer != null)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button(showLayerValue ? BUTTON_TO_SAVE : BUTTON_TO_EDIT))
                {
                    showLayerValue = !showLayerValue;
                }
                
                if (selfFilter == null)
                {
                    selfFilter = selfMeshRenderer.GetComponent<MeshFilter>();
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
                if (selfMeshRenderer != null)
                {
                    if (layerNames.Count <= 0 || layerNames.Count != SortingLayer.layers.Length)
                    {
                        layerNames.Clear();
                        layerNames.AddRange(SortingLayer.layers.Select(layer => layer.name));
                    }
                    
                    // 显示 Sorting Layer
                    var layerValue = SortingLayer.GetLayerValueFromID(selfMeshRenderer.sortingLayerID);
                    layerValue = EditorGUILayout.Popup("Sorting Layer", layerValue, layerNames.ToArray());
                    selfMeshRenderer.sortingLayerName = SortingLayer.layers[layerValue].name;
                    selfMeshRenderer.sortingLayerID = SortingLayer.layers[layerValue].id;
                    
                    // 显示 Order in Layer
                    selfMeshRenderer.sortingOrder = EditorGUILayout.IntField("Order in Layer", selfMeshRenderer.sortingOrder);
                }
            }
        }
    }
}
