using System;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool
{
    public class LayerBatchEditor : EditorWindow
    {
        private string filePath;
        private int layerIndex;
        private ModelImporterMeshCompression compress;
        private int counter;
        
        [MenuItem("Funny/层级批量设置工具")]
        public static void Open()
        {
            GetWindow<LayerBatchEditor>();
        }
        
        private void OnGUI()
        {
            if (GUILayout.Button("选择路径"))
            {
                filePath = EditorUtility.OpenFolderPanel("Select Folder", filePath, "");
            }
            
            layerIndex = EditorGUILayout.Popup("选择 Layer", layerIndex, UnityEditorInternal.InternalEditorUtility.layers);

            if (GUILayout.Button("设置层级"))
            {
                if (string.IsNullOrEmpty(filePath) == false)
                {
                    var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                    var assetPath = filePath.Substring(assetsIndex);
                    var guids = AssetDatabase.FindAssets("t:Prefab", new[] {assetPath});
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        var root = prefab.transform;
                        root.gameObject.layer = layerIndex;
                                
                        var children = root.GetComponentsInChildren<Transform>();
                        foreach (var child in children)
                        {
                            child.gameObject.layer = layerIndex;
                        }
                                
                        EditorUtility.SetDirty(prefab);
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("生成全部预制体"))
            {
                var assetPath = PathUtil.GetAssetPath(filePath);
                var guids = AssetDatabase.FindAssets("t:Prefab", new[] { assetPath });
                DebugUtil.Log($"一共找到了 {guids.Length} 个预制体", null, "yellow");

                var counterX = 20;
                var counterY = 20;
                
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    var prefab = Instantiate(asset);
                    var transform = prefab.transform;
                    transform.SetPositionAndRotation(new Vector3(counterX, counterY, 0), Quaternion.Euler(0, 90, 0));

                    counterY++;
                    if (counterY % 19 == 0)
                    {
                        counterY = 20;
                        counterX += 2;
                    }
                }
            }
            
            compress = (ModelImporterMeshCompression) EditorGUILayout.EnumPopup("选择压缩格式", compress);

            if (GUILayout.Button("设置压缩等级"))
            {
                counter = 0;
                var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = filePath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:mesh", new [] { assetPath });
                Debug.Log($"个数: {guids.Length}");

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    MeshUtility.SetMeshCompression(mesh, compress);
                    EditorUtility.SetDirty(mesh);
                    counter++;
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Debug.Log($"设置个数: {counter}");
            }
        }
    }
}