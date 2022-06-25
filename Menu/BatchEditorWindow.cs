using System;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.Util.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Script.Effect.Editor.AssetTool.Menu
{
    public class BatchEditorWindow : EditorWindow
    {
        private string filePath;
        private int layerIndex;
        private ModelImporterMeshCompression compress;
        private int counter;
        private GameObject sceneObject;
        private int culling = 15;
        private int roomPartIndex = 1;
        private string roomLayer;
        private GameObject rendererObject;
        private GameObject colliderObject;

        public static void Open()
        {
            GetWindow<BatchEditorWindow>();
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

            if (sceneObject != null && GUILayout.Button("场景物体: 分离 Collider 和 Renderer"))
            {
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

            if (sceneObject != null && GUILayout.Button("场景物体: 关闭全部的光照探针和反射探针"))
            {
                // 获得所有的 Mesh Renderer
                var renderers = sceneObject.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    renderer.lightProbeUsage = LightProbeUsage.Off;
                }
                
                var skinnedRenderers = sceneObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var renderer in skinnedRenderers)
                {
                    renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    renderer.lightProbeUsage = LightProbeUsage.Off;
                }
                
                EditorUtility.SetDirty(sceneObject);
            }

            if (sceneObject != null)
            {
                culling = EditorGUILayout.IntField("设置 Culling 层的占比:", culling);
            }
            
            if (sceneObject != null && GUILayout.Button("场景物体: 批量添加 Part LodGroup"))
            {
                var trans = sceneObject.transform;
                for (var index = 0; index < trans.childCount; index++)
                {
                    var child = trans.GetChild(index);
                    var lodGroup = child.gameObject.AddComponent<LODGroup>();
                    
                    var lodArray = new LOD[1];
                    var rendererArray = child.GetComponentsInChildren<Renderer>(true);
                    lodArray[0] = new LOD(culling / 100f, rendererArray);
                    lodGroup.SetLODs(lodArray);
                    lodGroup.RecalculateBounds();
                }
            }
            
            roomPartIndex = EditorGUILayout.IntField("设置 Room Part 的索引:", roomPartIndex);
            roomLayer = EditorGUILayout.TextField("设置 Room Part 的楼层:", roomLayer);

            if (Selection.transforms.Length > 1 && GUILayout.Button("场景物体: 划分 Room Part 1F"))
            {
                var selects = Selection.transforms;
                var root = selects[0].parent.parent;
                var partRoot = new GameObject($"Building_{roomLayer}Part_{roomPartIndex}").transform;
                partRoot.SetParent(root);
                
                foreach (var sel in selects)
                {
                    sel.parent.SetParent(partRoot);
                }
            }
            
            rendererObject = EditorGUILayout.ObjectField("rendererObject: ", rendererObject, typeof(GameObject), true) as GameObject;
            colliderObject = EditorGUILayout.ObjectField("colliderObject: ", colliderObject, typeof(GameObject), true) as GameObject;
            if (rendererObject != null && colliderObject != null && rendererObject.transform.childCount == colliderObject.transform.childCount && GUILayout.Button("场景物体: 还原"))
            {
                for (var index = 0; index < colliderObject.transform.childCount; index++)
                {
                    rendererObject.transform.GetChild(0).SetParent(colliderObject.transform.GetChild(index));
                }
            }

            if (GUILayout.Button("检查预制体 Missing"))
            {
                var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = filePath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:Prefab", new[]
                {
                    assetPath
                });
                Debug.Log($"个数: {guids.Length}");

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    var children = obj.GetComponentsInChildren<Transform>();
                    foreach (var child in children)
                    {
                        if (child.name.IndexOf("Missing Prefab", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            DebugUtil.LogError($"Missing Prefab: {obj}", obj, "yellow");
                        }
                    }
                }
            }
        }
    }
}
