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
using UnityEngine.UI;

namespace Script.Effect.Editor.AssetTool.Menu {
    public class BatchEditorWindow : EditorWindow {
        private string selectFullPath;
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

        private readonly List<Transform> hideSeekList = new List<Transform>();
        private Transform[] rendererList;

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

            if (GUILayout.Button("特效检测")) {
                EffectDetect();
            }

            if (GUILayout.Button("时装材质球的纹理引用检测")) {
                DetectFashionMaterial();
            }

            Debug.Log($"设置个数: {counter}");

            if (GUILayout.Button("选择路径")) {
                selectFullPath = EditorUtility.OpenFolderPanel("Select Folder", selectFullPath, "");
            }

            layerIndex = EditorGUILayout.Popup("选择 Layer", layerIndex, UnityEditorInternal.InternalEditorUtility.layers);

            if (GUILayout.Button("设置层级")) {
                if (string.IsNullOrEmpty(selectFullPath) == false) {
                    var assetsIndex = selectFullPath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                    var assetPath = selectFullPath.Substring(assetsIndex);
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
                var assetPath = PathUtil.GetAssetPath(selectFullPath);
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
                var assetsIndex = selectFullPath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = selectFullPath.Substring(assetsIndex);
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
                selectFullPath = EditorUtility.OpenFilePanel("Select File", selectFullPath, "");
            }

            if (GUILayout.Button("Material")) {
                var toSets = System.IO.File.ReadAllLines(selectFullPath);

                foreach (var set in toSets) {
                    var modelImporter = AssetImporter.GetAtPath(set) as ModelImporter;

                    if (modelImporter != null) {
                        modelImporter.RemoveMaterial();
                    }
                }
            }

            if (string.IsNullOrEmpty(selectFullPath) == false && GUILayout.Button("设置 Mesh")) {
                counter = 0;

                var assetsIndex = selectFullPath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = selectFullPath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:mesh", new[] {
                    assetPath
                });

                Debug.Log($"个数: {guids.Length}");
                selectFullPath = null;

                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.IndexOf(".mesh", StringComparison.OrdinalIgnoreCase) > 0 || path.IndexOf(".asset", StringComparison.OrdinalIgnoreCase) > 0) {
                        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                        await mesh.SetReadableAsync(false);
                        counter++;
                        Repaint();
                    }
                }

                AssetDatabase.Refresh();
            }

            if (string.IsNullOrEmpty(selectFullPath) == false && GUILayout.Button("优化 Mesh")) {
                counter = 0;

                var assetsIndex = selectFullPath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = selectFullPath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:mesh", new[] {
                    assetPath
                });

                Debug.Log($"个数: {guids.Length}");
                selectFullPath = null;

                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    MeshUtility.Optimize(mesh);
                    EditorUtility.SetDirty(mesh);
                    counter++;
                    Repaint();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("动画表情")) {
                var assetsIndex = selectFullPath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = selectFullPath.Substring(assetsIndex);
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

            if (sceneObject != null && GUILayout.Button("场景物体: 关闭全部的光照探针和反射探针和剔除")) {
                // 获得所有的 Renderer
                var renderers = sceneObject.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers) {
                    renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    renderer.lightProbeUsage = LightProbeUsage.Off;
                    renderer.allowOcclusionWhenDynamic = false;
                }

                EditorUtility.SetDirty(sceneObject);
            }

            if (sceneObject != null && GUILayout.Button("场景物体: 关闭全部阴影投射")) {
                var renderers = sceneObject.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers) {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
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
                var assetsIndex = selectFullPath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = selectFullPath.Substring(assetsIndex);
                var guids = AssetDatabase.FindAssets("t:Prefab", new[] {
                    assetPath
                });

                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    PrefabMissing(obj);
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

            if (GUILayout.Button("移除多余的 ScenePart")) {
                RemoveScenePart();
            }

            if (GUILayout.Button("移除多余的 Mesh Filter")) {
                RemoveMeshFilter();
            }

            if (GUILayout.Button("自动添加 Culling 层 LOD", GUILayout.Height(50))) {
                AddLODGroup();
            }

            if (GUILayout.Button("躲猫猫")) {
                if (sceneObject != null) {
                    for (var index = 0; index < sceneObject.transform.childCount; index++) {
                        var child = sceneObject.transform.GetChild(index);
                        hideSeekList.Add(child);
                    }
                }

                var renderer = GameObject.Find("Renderer");
                if (renderer != null) {
                    rendererList = renderer.transform.GetComponentsInChildren<Transform>();
                }

                Debug.Log($"hideSeekList: {hideSeekList.Count}, rendererList: {rendererList.Length}");

                foreach (var collider in hideSeekList) {

                    var flag = false;

                    foreach (var render in rendererList) {
                        if (PositionE(render.position, collider.position)) {
                            var part = render.parent;
                            collider.SetParent(part);
                            render.SetParent(collider); // Debug.Log($"hideSeekList: {collider.name}", collider); // Debug.Log($"rendererList: {render.name}", render);
                            flag = true;
                            render.localPosition = new Vector3(0, 0, 0);
                            render.localScale = new Vector3(1, 1, 1);
                            break;
                        }
                    }

                    if (flag == false) {
                        DebugUtil.Log($"hideSeekList 没有找到: {collider.name}", collider, "red");
                    }
                }
            }
        }

        private bool PositionE(Vector3 a, Vector3 b) {
            var aX = Math.Round(a.x, 2);
            var aY = Math.Round(a.y, 2);
            var aZ = Math.Round(a.z, 2);
            var bX = Math.Round(b.x, 2);
            var bY = Math.Round(b.y, 2);
            var bZ = Math.Round(b.z, 2);
            return Math.Abs(aX - bX) < 0.01f && Math.Abs(aY - bY) < 0.01f && Math.Abs(aZ - bZ) < 0.01f;
        }

        private void ModifyPrefab() {
            var assetsIndex = selectFullPath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
            var assetPath = selectFullPath.Substring(assetsIndex);
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] {
                assetPath
            });

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                // PrefabCloseProbeOcclusion(obj);
                // PrefabSetLOD(obj);
                // if (path.Contains("Environmental")) {
                //     PrefabOpenShadow(obj);
                // }
                PrefabCloseShadow(obj);

                AssetDatabase.SaveAssets();
            }
        }

        private void PrefabCloseProbeOcclusion(GameObject obj) {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers) {
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.allowOcclusionWhenDynamic = false;
            }
        }

        private void PrefabSetLOD(GameObject obj) {
            var lodGroups = obj.GetComponentsInChildren<LODGroup>();
            foreach (var lodGroup in lodGroups) {
                var lods = lodGroup.GetLODs();
                if (lods.Length == 4) {
                    lods[0].screenRelativeTransitionHeight = 0.35f;
                    lods[1].screenRelativeTransitionHeight = 0.05f;
                    lods[2].screenRelativeTransitionHeight = 0.02f;
                    lods[3].screenRelativeTransitionHeight = 0.01f;
                    lodGroup.SetLODs(lods);
                }
                // else if (lods.Length == 3) {
                //     lods[0].screenRelativeTransitionHeight = 0.3f;
                //     lods[1].screenRelativeTransitionHeight = 0.05f;
                //     lods[2].screenRelativeTransitionHeight = 0.02f;
                //     lodGroup.SetLODs(lods);
                // } else if (lods.Length == 2) {
                //     lods[0].screenRelativeTransitionHeight = 0.2f;
                //     lods[1].screenRelativeTransitionHeight = 0.02f;
                //     lodGroup.SetLODs(lods);
                //}
                else if (lods.Length != 1) {
                    Debug.Log($"{obj.name} 预制体中的 LODGroup 仅有 {lods.Length} 层!", obj);
                }
            }
        }

        private void PrefabOpenShadow(GameObject obj) {
            var lodGroups = obj.GetComponentsInChildren<LODGroup>();
            foreach (var lodGroup in lodGroups) {
                var lods = lodGroup.GetLODs();
                // foreach (var lod in lods) {
                //     var renderers = lod.renderers;
                //     foreach (var renderer in renderers) {
                //         if (renderer == null) {
                //             Debug.LogError($"renderer = null ! {obj.name}/{lodGroup.name}", obj);
                //         } else {
                //             renderer.shadowCastingMode = ShadowCastingMode.Off;
                //         }
                //     }
                // }
                //
                // var renderers0 = lods[0].renderers;
                // foreach (var renderer0 in renderers0) {
                //     if (renderer0 == null) {
                //         Debug.LogError($"renderer0 = null ! {obj.name}/{lodGroup.name}", obj);
                //     } else {
                //         renderer0.shadowCastingMode = ShadowCastingMode.On;
                //     }
                // }

                if (lods.Length >= 3) {
                    var renderers1 = lods[1].renderers;
                    foreach (var renderer1 in renderers1) {
                        if (renderer1 == null) {
                            Debug.LogError($"renderer1 = null ! {obj.name}/{lodGroup.name}", obj);
                        } else {
                            if (renderer1.shadowCastingMode != ShadowCastingMode.On) {
                                renderer1.shadowCastingMode = ShadowCastingMode.On;
                            }
                        }
                    }
                }
            }
        }

        private void PrefabCloseShadow(GameObject obj) {
            var transforms = obj.GetComponentsInChildren<Transform>();
            foreach (var transform in transforms) {
                if (transform.name.EndsWith("_LBP")) {
                    var renderer = transform.GetComponent<MeshRenderer>();
                    if (renderer != null && renderer.shadowCastingMode != ShadowCastingMode.Off) {
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                        EditorUtility.SetDirty(obj);
                    }
                }
            }
        }

        private void PrefabMissing(GameObject obj) {
            var children = obj.GetComponentsInChildren<Transform>();
            foreach (var child in children) {
                if (child.name.IndexOf("Missing Prefab", StringComparison.OrdinalIgnoreCase) >= 0) {
                    DebugUtil.LogError($"Missing Prefab: {obj}", obj, "yellow");
                }
            }
        }

        private void RemoveScenePart() {
            var renderers = AssetUtil.GetAllComponentsInScene<Renderer>(AssetUtil.FindType.All);
            foreach (var renderer in renderers) {
                if (renderer != null) {
                    renderer.allowOcclusionWhenDynamic = false;
                }
            }

            var sceneParts = AssetUtil.GetAllComponentsInScene<ScenePart>(AssetUtil.FindType.All);
            foreach (var scenePart in sceneParts) {
                if (scenePart != null) {
                    if (scenePart.partType != ScenePartType.House) {
                        //scenePart.SetPrefabName(null);
                        EditorUtility.SetDirty(scenePart);
                    }
                }
            }
        }

        private void RemoveMeshFilter() {
            var filters = AssetUtil.GetAllComponentsInScene<MeshFilter>(AssetUtil.FindType.All);
            foreach (var filter in filters) {
                if (filter.gameObject.TryGetComponent<MeshRenderer>(out var renderer) == false) {
                    EditorUtility.SetDirty(filter.gameObject);
                    DestroyImmediate(filter);
                } else if (renderer.enabled == false) {
                    EditorUtility.SetDirty(filter.gameObject);
                    DestroyImmediate(filter);
                    DestroyImmediate(renderer);
                }
            }
        }

        private void AddLODGroup() {
            if (Selection.transforms != null) {
                foreach (var transform in Selection.transforms) {
                    if (transform != null) {
                        var obj = transform.gameObject;
                        var lodGroup = obj.AddComponent<LODGroup>();
                        lodGroup.SetLODs(new[] {
                            new LOD {
                                renderers = new[] {
                                    obj.GetComponent<Renderer>()
                                },
                                screenRelativeTransitionHeight = 0.02f
                            }
                        });
                        EditorUtility.SetDirty(obj);
                    }
                }
            }
        }

        private void EffectDetect() {
            var fullPath = System.IO.Path.GetFullPath("Assets/ToBundle/Config/Txt/FashionEffect.txt");
            var allLines = System.IO.File.ReadAllLines(fullPath);
            for (var index = 0; index < allLines.Length; index++) {
                if (index >= 3) {
                    var line = allLines[index];
                    var lineParts = line.Split('\t');
                    var effectNamePart = lineParts[1];
                    var isCombinePart = lineParts[3];

                    var effectNames = effectNamePart.Split('|');
                    var isCombines = isCombinePart.Split('|');

                    for (var combineIndex = 0; combineIndex < isCombines.Length; combineIndex++) {
                        var isCombine = isCombines[combineIndex];
                        if (isCombine == "1") {
                            var effectPrefabName = effectNames[combineIndex];
                            var effectPrefabGuids = AssetDatabase.FindAssets($"{effectPrefabName} t:Prefab");
                            if (effectPrefabGuids.Length != 1) {
                                foreach (var guid in effectPrefabGuids) {
                                    var effectPrefabPath = AssetDatabase.GUIDToAssetPath(guid);
                                    var effectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(effectPrefabPath);
                                    if (effectPrefab.name.Equals(effectPrefabName)) {
                                        EffectDetect(effectPrefabName, effectPrefabGuids);
                                    }
                                }
                            } else {
                                EffectDetect(effectPrefabName, effectPrefabGuids);
                            }
                        }
                    }
                }
            }
        }

        private void EffectDetect(string effectPrefabName, in string[] effectPrefabGuids) {
            var effectPrefabPath = AssetDatabase.GUIDToAssetPath(effectPrefabGuids[0]);
            var effectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(effectPrefabPath);
            
            var allRenderer = effectPrefab.GetComponentsInChildren<MeshRenderer>();
            EffectDetect(allRenderer, effectPrefabName);
            
            var allRenderer2 = effectPrefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            EffectDetect(allRenderer2, effectPrefabName);
        }

        private void EffectDetect(IEnumerable<Renderer> allRenderer, string effectPrefabName) {
            foreach (var renderer in allRenderer) {
                var materials = renderer.sharedMaterials;
                if (materials.Length != 1) {
                    foreach (var material in materials) {
                        DebugUtil.LogError($"特效 {effectPrefabName} 中的 Renderer {renderer.name} 有 {materials.Length} 个材质球: {material.name}", material);
                    }
                } else {
                    TextureUtil.GetAllTexturesInMaterial(materials[0], out var textureDataList);
                    if (textureDataList != null && textureDataList.Count > 0) {
                        foreach (var textureData in textureDataList) {
                            if (textureData.asset.isReadable == false) {
                                if (textureData.path.ToLower().Contains("sfx_")) {
                                    DebugUtil.LogError($"特效纹理 {textureData.path} 没有开启读写");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DetectFashionMaterial() {
            var mainTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/Characters/Textures/Fashion_Low/Suit/Suit_1/Suit_1_D.psd");
            
            var export = new List<string>();
            var guids = AssetDatabase.FindAssets("t:Material", new [] { "Assets/ToBundle/Fashion" });
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                // TextureUtil.GetAllTexturesInMaterial(material, out var textureDataList);
                // if (textureDataList.Any(textureData => textureData.path.ToLower().Contains("sfx_"))) {
                //     export.Adds($"材质球 {material.name} 引用了特效纹理!");
                //     DebugUtil.LogError($"材质球 {material.name} 引用了特效纹理!", material, "red");
                // }
                if (material.mainTexture == null) {
                    export.Add($"材质球 {material.name} 主纹理为空!");
                    DebugUtil.LogError($"材质球 {material.name} 主纹理为空!", material, "red");
                    material.mainTexture = mainTexture;
                    material.SetTexture("_MaskTex", null);
                    material.SetTexture("_NormalMap", null);
                    material.SetTexture("_EmissionTex", null);
                    material.SetTexture("_MatCap", null);
                    EditorUtility.SetDirty(material);
                }
            }
            
            System.IO.File.WriteAllLines("C:/123.txt", export);
        }
    }
}
