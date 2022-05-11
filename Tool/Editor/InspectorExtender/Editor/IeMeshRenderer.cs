using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private Mesh selfMesh;
        private MeshFilter selfFilter;
        private MeshRenderer selfMeshRenderer;
        private MeshRenderer selfMeshRendererOld;

        private bool meshFoldout = true;
        private bool rwEnable;
        private string meshPath;
        private ModelImporterMeshCompression compression;
        private bool revertButton;
        private bool applyButton;
        private readonly Regex regexReadWrite = new Regex(@"m_IsReadable: [\d]");

        private bool layerSortFoldout = true;
        private readonly List<string> layerNames = new List<string>();

        /// <summary>
        /// 构造方法
        /// </summary>
        public IeMeshRenderer() : base("UnityEditor.MeshRendererEditor")
        {
        }

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

            meshFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(meshFoldout, "Mesh");
            if (meshFoldout)
            {
                OnApplyButtonGUI();

                if (revertButton)
                {
                    ReloadTarget();
                }

                if (applyButton)
                {
                    // 先修改压缩等级, 后修改读写, 顺序不能颠倒
                    if (compression != MeshUtility.GetMeshCompression(selfMesh))
                    {
                        SetMeshCompression();
                    }

                    if (rwEnable != selfMesh.isReadable)
                    {
                        await SetMeshReadable();
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void OnApplyButtonGUI()
        {
            // Unity 内置资源不可修改
            if (meshPath.IndexOf("Assets", StringComparison.Ordinal) < 0)
            {
                GUI.enabled = false;
            }

            rwEnable = EditorGUILayout.Toggle("Read Write Enabled", rwEnable);
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

        private async Task SetMeshReadable()
        {
            string meshData;
            var meshFullPath = Path.GetFullPath(meshPath);

            using (var reader = new StreamReader(meshFullPath))
            {
                meshData = await reader.ReadToEndAsync();
                var readWriteString = "m_IsReadable: " + (rwEnable ? 1 : 0);
                meshData = regexReadWrite.Replace(meshData, readWriteString);
            }

            using (var writer = new StreamWriter(meshFullPath))
            {
                await writer.WriteAsync(meshData);
            }

            AssetDatabase.Refresh();
            ReloadTarget();
        }

        private void SetMeshCompression()
        {
            MeshUtility.SetMeshCompression(selfMesh, compression);
            EditorUtility.SetDirty(selfMesh);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UpdateTarget();

            OnLayerSortGUI();

            OnMeshGUI();
        }

        private void OnLayerSortGUI()
        {
            layerSortFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(layerSortFoldout, "Sorting Layer");
            if (layerSortFoldout)
            {
                ShowSortingLayer();
                // if (selfFilter == null)
                // {
                //     selfFilter = selfMeshRenderer.GetComponent<MeshFilter>();
                // }
                //
                // if (selfFilter != null)
                // {
                //     var mesh = selfFilter.sharedMesh;
                //
                //     if (GUILayout.Button("Save Mesh..."))
                //     {
                //         SaveMesh(mesh, mesh.name, false, true);
                //     }
                //
                //     if (GUILayout.Button("Save Mesh As New Instance..."))
                //     {
                //         SaveMesh(mesh, mesh.name, true, true);
                //     }
                // }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void ShowSortingLayer()
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

        [MenuItem("CONTEXT/MeshFilter/Save Mesh...")]
        public static void SaveMeshInPlace(MenuCommand menuCommand)
        {
            var meshFilter = menuCommand.context as MeshFilter;
            if (meshFilter!=null)
            {
                var mesh = meshFilter.sharedMesh;
                SaveMesh(mesh, mesh.name, false, true);
            }
        }

        [MenuItem("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
        public static void SaveMeshNewInstanceItem(MenuCommand menuCommand)
        {
            var meshFilter = menuCommand.context as MeshFilter;
            if (meshFilter!=null)
            {
                var mesh = meshFilter.sharedMesh;
                SaveMesh(mesh, mesh.name, true, true);
            }
        }
        
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
    }
}