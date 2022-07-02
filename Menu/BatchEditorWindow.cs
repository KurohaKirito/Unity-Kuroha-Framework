using System;
using System.Collections.Generic;
using System.Linq;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.Util.Unity;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace Script.Effect.Editor.AssetTool.Menu {
    public class BatchEditorWindow : EditorWindow {
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
        private ReorderableList reorderableList;
        private readonly List<UnityEngine.Object> objPaths = new List<UnityEngine.Object>();
        private bool filterUV2;

        public static void Open() {
            GetWindow<BatchEditorWindow>();
        }

        private void OnEnable() {
            reorderableList = new ReorderableList(objPaths, typeof(UnityEngine.Object)) {
                draggable = true,
                displayAdd = true,
                displayRemove = true,
                drawElementCallback = DrawElement,
                drawHeaderCallback = DrawHeader
            };

            reorderableList.onAddCallback += list => {
                objPaths.Add(null);
            };

            reorderableList.onRemoveCallback += list => {
                objPaths.RemoveAt(list.index);
            };
        }

        private void DrawHeader(Rect rect) {
            EditorGUI.LabelField(rect, "检测路径");
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused) {
            rect.height -= 2;
            rect.y += 1;
            objPaths[index] = EditorGUI.ObjectField(rect, objPaths[index], typeof(UnityEngine.Object), false);
        }

        private async void OnGUI() {
            reorderableList.DoLayoutList();

            Debug.Log($"设置个数: {counter}");

            if (GUILayout.Button("选择路径")) {
                filePath = EditorUtility.OpenFolderPanel("Select Folder", filePath, "");
            }

            layerIndex = EditorGUILayout.Popup("选择 Layer", layerIndex, UnityEditorInternal.InternalEditorUtility.layers);

            if (GUILayout.Button("设置层级")) {
                if (string.IsNullOrEmpty(filePath) == false) {
                    var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                    var assetPath = filePath.Substring(assetsIndex);
                    var guids = AssetDatabase.FindAssets("t:Prefab", new[] {
                        assetPath
                    });
                    foreach (var guid in guids) {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        var root = prefab.transform;
                        root.gameObject.layer = layerIndex;

                        var children = root.GetComponentsInChildren<Transform>();
                        foreach (var child in children) {
                            child.gameObject.layer = layerIndex;
                        }

                        EditorUtility.SetDirty(prefab);
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("生成全部预制体")) {
                var assetPath = PathUtil.GetAssetPath(filePath);
                var guids = AssetDatabase.FindAssets("t:Prefab", new[] {
                    assetPath
                });
                DebugUtil.Log($"一共找到了 {guids.Length} 个预制体", null, "yellow");

                var counterX = 20;
                var counterY = 20;

                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    var prefab = Instantiate(asset);
                    var transform = prefab.transform;
                    transform.SetPositionAndRotation(new Vector3(counterX, counterY, 0), Quaternion.Euler(0, 90, 0));

                    counterY++;
                    if (counterY % 19 == 0) {
                        counterY = 20;
                        counterX += 2;
                    }
                }
            }

            compress = (ModelImporterMeshCompression) EditorGUILayout.EnumPopup("选择压缩格式", compress);

            if (GUILayout.Button("设置压缩等级")) {
                counter = 0;
                var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = filePath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:mesh", new[] {
                    assetPath
                });
                Debug.Log($"个数: {guids.Length}");

                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    MeshUtility.SetMeshCompression(mesh, compress);
                    EditorUtility.SetDirty(mesh);
                    counter++;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Select File")) {
                filePath = EditorUtility.OpenFilePanel("Select File", filePath, "");
            }

            if (GUILayout.Button("Material")) {
                var toSets = System.IO.File.ReadAllLines(filePath);

                foreach (var set in toSets) {
                    var modelImporter = AssetImporter.GetAtPath(set) as ModelImporter;

                    if (modelImporter != null) {
                        modelImporter.RemoveMaterial();
                    }
                }
            }

            if (GUILayout.Button("设置 Mesh")) {
                counter = 0;
                var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = filePath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:mesh", new[] {
                    assetPath
                });
                Debug.Log($"个数: {guids.Length}");

                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);

                    await mesh.SetReadable(false);
                    AssetDatabase.Refresh();
                    counter++;
                }
            }

            if (GUILayout.Button("动画表情")) {
                var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = filePath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:Prefab", new[] {
                    assetPath
                });
                Debug.Log($"Prefab 个数: {guids.Length}");

                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    var animators = obj.GetComponentsInChildren<Animator>();
                    if (animators != null && animators.Length > 0) {
                        counter++;
                        Debug.Log($"动画表情 {counter}: {obj.name}", obj);
                    }
                }
            }

            sceneObject = EditorGUILayout.ObjectField("场景物体: ", sceneObject, typeof(GameObject), true) as GameObject;

            if (sceneObject != null && GUILayout.Button("场景物体: 分离 Collider 和 Renderer")) {
                // ----------------------------------- 创建所有渲染器的父物体 ----------------------------------------------
                var parent = new GameObject("Mesh Renderer Parent").transform;
                parent.SetParent(sceneObject.transform, true);

                // 获得所有的 Mesh Renderer
                var renderers = sceneObject.GetComponentsInChildren<MeshRenderer>(true);

                // 更改层级
                foreach (var renderer in renderers) {
                    renderer.transform.SetParent(parent, true);
                }

                // ----------------------------------- 创建所有碰撞器的父物体 ----------------------------------------------
                parent = new GameObject("Mesh Collider Parent").transform;
                parent.SetParent(sceneObject.transform, true);

                // 获得所有的 Mesh Collider
                var colliders = sceneObject.GetComponentsInChildren<MeshCollider>(true);

                // 更改层级
                foreach (var collider in colliders) {
                    collider.transform.SetParent(parent, true);
                }
            }

            if (sceneObject != null && GUILayout.Button("场景物体: 关闭全部的光照探针和反射探针")) {
                // 获得所有的 Renderer
                var renderers = sceneObject.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers) {
                    renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    renderer.lightProbeUsage = LightProbeUsage.Off;
                }

                EditorUtility.SetDirty(sceneObject);
            }

            if (sceneObject != null) {
                culling = EditorGUILayout.IntField("设置 Culling 层的占比:", culling);
            }

            if (sceneObject != null && GUILayout.Button("场景物体: 批量添加 Part LodGroup")) {
                var trans = sceneObject.transform;
                for (var index = 0; index < trans.childCount; index++) {
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

            if (Selection.transforms.Length > 1 && GUILayout.Button("场景物体: 划分 Room Part 1F")) {
                var selects = Selection.transforms;
                var root = selects[0].parent.parent;
                var partRoot = new GameObject($"Building_{roomLayer}Part_{roomPartIndex}").transform;
                partRoot.SetParent(root);

                foreach (var sel in selects) {
                    sel.parent.SetParent(partRoot);
                }
            }

            rendererObject = EditorGUILayout.ObjectField("rendererObject: ", rendererObject, typeof(GameObject), true) as GameObject;
            colliderObject = EditorGUILayout.ObjectField("colliderObject: ", colliderObject, typeof(GameObject), true) as GameObject;
            if (rendererObject != null && colliderObject != null && rendererObject.transform.childCount == colliderObject.transform.childCount && GUILayout.Button("场景物体: 还原")) {
                for (var index = 0; index < colliderObject.transform.childCount; index++) {
                    rendererObject.transform.GetChild(0).SetParent(colliderObject.transform.GetChild(index));
                }
            }

            if (GUILayout.Button("模型位置检测")) {
                var path = AssetDatabase.GetAssetPath(sceneObject);
                if (AssetImporter.GetAtPath(path) is ModelImporter modelImporter) {
                    foreach (var transformPath in modelImporter.transformPaths) {
                        Debug.Log($"transformPath: {transformPath}");
                    }
                }

                Debug.Log($"sceneObject.transform: {sceneObject.transform.localPosition.ToString()}");
                Debug.Log($"sceneObject.transform: {sceneObject.transform.localRotation.ToString()}");
                Debug.Log($"sceneObject.transform: {sceneObject.transform.localScale.ToString()}");
            }

            if (GUILayout.Button("检查预制体 Missing")) {
                var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = filePath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:Prefab", new[] {
                    assetPath
                });

                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    预制体Missing(obj);
                }
            }

            if (GUILayout.Button("预制体仅 LOD0 开启阴影, 批量设置 LOD, 关闭全部探针和动态遮挡剔除")) {
                ModifyPrefab();
            }

            filterUV2 = EditorGUILayout.Toggle("含有 UV2 信息剔除", filterUV2);
            if (GUILayout.Button("检测重复 Mesh")) {
                var meshesRepeat = new List<Mesh>();
                var meshesNoRepeat = new List<Mesh>();

                var paths = objPaths.Select(AssetDatabase.GetAssetPath).ToArray();
                var guids = AssetDatabase.FindAssets("t:mesh", paths);
                var meshes = guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Mesh>).ToList();

                foreach (var mesh in meshes) {
                    if (filterUV2 && mesh.uv2 != null && mesh.uv2.Length > 0) {
                        continue;
                    }

                    if (mesh.triangles.Length < 500 || mesh.vertices.Length < 500) {
                        continue;
                    }

                    if (IsRepeat(mesh, meshesRepeat, out _)) {
                        meshesRepeat.Add(mesh);
                    } else {
                        if (IsRepeat(mesh, meshesNoRepeat, out var index)) {
                            meshesRepeat.Add(mesh);
                            meshesRepeat.Add(meshesNoRepeat[index]);
                            meshesNoRepeat.RemoveAt(index);
                        } else {
                            meshesNoRepeat.Add(mesh);
                        }
                    }
                }

                bool IsRepeat(Mesh mesh, in List<Mesh> meshList, out int index) {
                    index = 0;

                    for (var i = 0; i < meshList.Count; i++) {
                        if (mesh.triangles.Length == meshList[i].triangles.Length && mesh.vertices.Length == meshList[i].vertices.Length && mesh.uv.Length == meshList[i].uv.Length) {
                            index = i;
                            return true;
                        }
                    }

                    return false;
                }

                var resultFilePath = $"{Application.dataPath}/重复 Mesh.txt";
                var outputPaths = meshesRepeat.Select(AssetDatabase.GetAssetPath).ToList();
                System.IO.File.WriteAllLines(resultFilePath, outputPaths);
            }

            if (GUILayout.Button("场景物体静态批处理 Mesh")) {
                var children = sceneObject.GetComponentsInChildren<MeshFilter>().Select(t => t.gameObject).ToArray();
                StaticBatchingUtility.Combine(children, sceneObject);
                var mesh = children[0].GetComponent<MeshFilter>().sharedMesh;
                if (mesh.name.Contains("Combined Mesh")) { // 仅判断开头也可以
                    AssetDatabase.CreateAsset(mesh, "Assets/StaticBatchingUtility.asset");
                }
            }

            if (GUILayout.Button("检查当前场景的预制体 Missing")) {
                var transforms = FindObjectsOfType<Transform>();
                foreach (var child in transforms) {
                    if (child.name.IndexOf("Missing Prefab", StringComparison.OrdinalIgnoreCase) >= 0) {
                        DebugUtil.LogError($"Missing Prefab: {child.name}", child, "yellow");
                    }
                }
            }
        }

        private void ModifyPrefab() {
            var assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
            var assetPath = filePath.Substring(assetsIndex);
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] {
                assetPath
            });

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                预制体关闭全部探针和动态遮挡剔除(obj);
                预制体批量设置LOD(obj);
                预制体仅LOD0开启阴影(obj);

                EditorUtility.SetDirty(obj);
                AssetDatabase.SaveAssets();
            }
        }

        private void 预制体关闭全部探针和动态遮挡剔除(GameObject obj) {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers) {
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.allowOcclusionWhenDynamic = false;
            }
        }

        private void 预制体批量设置LOD(GameObject obj) {
            var lodGroups = obj.GetComponentsInChildren<LODGroup>();
            foreach (var lodGroup in lodGroups) {
                var lods = lodGroup.GetLODs();
                if (lods.Length == 4) {
                    lods[0].screenRelativeTransitionHeight = 0.4f;
                    lods[1].screenRelativeTransitionHeight = 0.3f;
                    lods[2].screenRelativeTransitionHeight = 0.05f;
                    lods[3].screenRelativeTransitionHeight = 0.02f;
                    lodGroup.SetLODs(lods);
                } else if (lods.Length == 3) {
                    lods[0].screenRelativeTransitionHeight = 0.3f;
                    lods[1].screenRelativeTransitionHeight = 0.05f;
                    lods[2].screenRelativeTransitionHeight = 0.02f;
                    lodGroup.SetLODs(lods);
                } else if (lods.Length == 2) {
                    lods[0].screenRelativeTransitionHeight = 0.2f;
                    lods[1].screenRelativeTransitionHeight = 0.02f;
                    lodGroup.SetLODs(lods);
                } else {
                    Debug.Log($"{obj.name} 预制体中的 LODGroup 仅有 {lods.Length} 层!", obj);
                }
            }
        }

        private void 预制体仅LOD0开启阴影(GameObject obj) {
            var lodGroups = obj.GetComponentsInChildren<LODGroup>();
            foreach (var lodGroup in lodGroups) {
                var lods = lodGroup.GetLODs();
                foreach (var lod in lods) {
                    var renderers = lod.renderers;
                    foreach (var renderer in renderers) {
                        if (renderer == null) {
                            Debug.LogError($"renderer = null ! {obj.name}/{lodGroup.name}", obj);
                        } else {
                            renderer.shadowCastingMode = ShadowCastingMode.Off;
                        }
                    }
                }

                var renderers0 = lods[0].renderers;
                foreach (var renderer0 in renderers0) {
                    if (renderer0 == null) {
                        Debug.LogError($"renderer0 = null ! {obj.name}/{lodGroup.name}", obj);
                    } else {
                        renderer0.shadowCastingMode = ShadowCastingMode.On;
                    }
                }
            }
        }

        private void 预制体Missing(GameObject obj) {
            var children = obj.GetComponentsInChildren<Transform>();
            foreach (var child in children) {
                if (child.name.IndexOf("Missing Prefab", StringComparison.OrdinalIgnoreCase) >= 0) {
                    DebugUtil.LogError($"Missing Prefab: {obj}", obj, "yellow");
                }
            }
        }
    }
}
