using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool
{
    public class LayerBatchEditor : EditorWindow
    {
        private string filePath;
        private int layerIndex;
        
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
        }
    }
}