using System;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.Util.Unity;
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
        private GameObject sceneObject;

        [MenuItem("Funny/层级批量设置工具")]
        public static void Open()
        {
            GetWindow<LayerBatchEditor>();
        }

        private async void OnGUI()
        {
            Debug.Log($"设置个数: {counter}");

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
                    var guids = AssetDatabase.FindAssets("t:Prefab", new[]
                    {
                        assetPath
                    });
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
                var guids = AssetDatabase.FindAssets("t:Prefab", new[]
                {
                    assetPath
                });
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
                var guids = AssetDatabase.FindAssets("t:mesh", new[]
                {
                    assetPath
                });
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
            }

            if (GUILayout.Button("Select File"))
            {
                filePath = EditorUtility.OpenFilePanel("Select File", filePath, "");
            }

            if (GUILayout.Button("Material"))
            {
                var toSets = System.IO.File.ReadAllLines(filePath);

                foreach (var set in toSets)
                {
                    var modelImporter = AssetImporter.GetAtPath(set) as ModelImporter;

                    if (modelImporter != null)
                    {
                        #region 删除模型的内嵌材质

                        // 开启材质导入, 提取出模型的内嵌材质到 Materials 文件夹
                        modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                        modelImporter.materialLocation = ModelImporterMaterialLocation.External;
                        modelImporter.SaveAndReimport();

                        // 删除提取出来的材质球
                        var subPath = set.Substring(0, set.LastIndexOf("/", StringComparison.Ordinal)) + "/Materials";
                        AssetDatabase.DeleteAsset(subPath);

                        // 修改模型材质引用类型为内嵌材质
                        modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;
                        modelImporter.SaveAndReimport();

                        #endregion
                    }
                }
            }

            if (GUILayout.Button("设置 Mesh"))
            {
                counter = 0;
                var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = filePath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:mesh", new[]
                {
                    assetPath
                });
                Debug.Log($"个数: {guids.Length}");

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);

                    await mesh.SetReadable(false);
                    AssetDatabase.Refresh();
                    counter++;
                }
            }

            if (GUILayout.Button("动画表情"))
            {
                var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = filePath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:Prefab", new[]
                {
                    assetPath
                });
                Debug.Log($"Prefab 个数: {guids.Length}");

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    var animators = obj.GetComponentsInChildren<Animator>();
                    if (animators != null && animators.Length > 0)
                    {
                        counter++;
                        Debug.Log($"动画表情 {counter}: {obj.name}", obj);
                    }
                }
            }

            sceneObject = EditorGUILayout.ObjectField("场景物体: ", sceneObject, typeof(GameObject), true) as GameObject;

            if (GUILayout.Button("分离 Collider 和 Renderer"))
            {
                if (sceneObject == null)
                {
                    return;
                }
                
                // ----------------------------------- 创建所有渲染器的父物体 ----------------------------------------------
                var parent = new GameObject("Mesh Renderer Parent").transform;
                parent.SetParent(sceneObject.transform, true);

                // 获得所有的 Mesh Renderer
                var renderers = sceneObject.GetComponentsInChildren<MeshRenderer>(true);

                // 更改层级
                foreach (var renderer in renderers)
                {
                    renderer.transform.SetParent(parent, true);
                }

                // ----------------------------------- 创建所有碰撞器的父物体 ----------------------------------------------
                parent = new GameObject("Mesh Collider Parent").transform;
                parent.SetParent(sceneObject.transform, true);

                // 获得所有的 Mesh Collider
                var colliders = sceneObject.GetComponentsInChildren<MeshCollider>(true);

                // 更改层级
                foreach (var collider in colliders)
                {
                    collider.transform.SetParent(parent, true);
                }
            }
        }
    }
}
