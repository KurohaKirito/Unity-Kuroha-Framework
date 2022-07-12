using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor.Table;
using Script.Effect.Editor.AssetTool.Tool.Editor.MeshAnalysisTool;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.SceneAnalysisTool {
    public class SceneAnalysisTableWindow : EditorWindow {
        private static string detectPath;
        private static GameObject detectGameObject;
        private static MeshAnalysisData.DetectType detectType;
        private static MeshAnalysisData.DetectTypeAtPath detectTypeAtPath;
        private static MeshAnalysisData.DetectMeshType detectMeshType;

        private int resultTris;
        private int resultVerts;
        private int resultUV;
        private int resultUV2;
        private int resultUV3;
        private int resultUV4;
        private int resultColors;
        private int resultTangents;
        private int resultNormals;
        private float resultMemory;

        private SceneAnalysisTable table;

        private static int vertsWarn;
        private static int vertsError;
        private static int trisWarn;
        private static int trisError;

        private static bool isDistinct;
        private static List<SceneAnalysisData> originList = new List<SceneAnalysisData>();
        private static readonly List<SceneAnalysisData> distinctList = new List<SceneAnalysisData>();
        private static CustomTableColumn<SceneAnalysisData>[] columns;

        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void Open(MeshAnalysisData.DetectType pDetectType, MeshAnalysisData.DetectTypeAtPath pDetectTypeAtPath, MeshAnalysisData.DetectMeshType pDetectMeshType, GameObject asset, string path) {
            detectPath = path;
            detectGameObject = asset;
            detectType = pDetectType;
            detectTypeAtPath = pDetectTypeAtPath;
            detectMeshType = pDetectMeshType;

            var window = GetWindow<SceneAnalysisTableWindow>("场景分析", true);
            window.minSize = new Vector2(1000, 600);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable() {
            if (vertsWarn == 0) {
                vertsWarn = 500;
            }

            if (vertsError == 0) {
                vertsError = 1000;
            }

            if (trisWarn == 0) {
                trisWarn = 500;
            }

            if (trisError == 0) {
                trisError = 1000;
            }

            resultTris = 0;
            resultVerts = 0;

            InitTable();
        }

        /// <summary>
        /// 绘制界面
        /// </summary>
        protected void OnGUI() {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical("Box");
            vertsWarn = EditorGUILayout.IntField("Enter Verts Warning Line", vertsWarn, GUILayout.Width(200));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical("Box");
            vertsError = EditorGUILayout.IntField("Enter Verts Error Line", vertsError, GUILayout.Width(200));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical("Box");
            trisWarn = EditorGUILayout.IntField("Enter Tris Warning Line", trisWarn, GUILayout.Width(200));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical("Box");
            trisError = EditorGUILayout.IntField("Enter Tris Error Line", trisError, GUILayout.Width(200));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();

            table?.OnGUI();
        }

        /// <summary>
        /// 初始化表格
        /// </summary>
        /// <param name="forceUpdate">是否强制刷新</param>
        private void InitTable(bool forceUpdate = false) {
            if (forceUpdate || table == null) {
                originList = InitRows();
                if (originList != null) {
                    columns = InitColumns();
                    if (columns != null) {
                        GenerateTable(originList, columns);
                    }
                }
            }
        }

        /// <summary>
        /// 生成表格
        /// </summary>
        /// <param name="data"></param>
        /// <param name="column"></param>
        private void GenerateTable(List<SceneAnalysisData> data, CustomTableColumn<SceneAnalysisData>[] column) {
            table = new SceneAnalysisTable(new Vector2(20, 20), new Vector2(300, 300), data, true, true, true, column, OnFilterEnter, OnAfterFilter, OnExportPressed, OnRowSelect, OnDistinctPressed);
        }

        /// <summary>
        /// 初始化行数据
        /// </summary>
        /// <returns></returns>
        private List<SceneAnalysisData> InitRows() {
            var dataList = new List<SceneAnalysisData>();
            var meshCount = 0;

            switch (detectType) {
                case MeshAnalysisData.DetectType.Scene:
                    if (detectMeshType == MeshAnalysisData.DetectMeshType.RendererMesh) {
                        var meshFilters = FindObjectsOfType<MeshFilter>();
                        DetectMeshFilter(in dataList, in meshFilters);
                        meshCount += meshFilters.Length;

                        var skinnedMeshRenderers = FindObjectsOfType<SkinnedMeshRenderer>();
                        DetectSkinnedMeshRenderer(in dataList, in skinnedMeshRenderers);
                        meshCount += skinnedMeshRenderers.Length;

                        var particleSystems = FindObjectsOfType<ParticleSystem>();
                        DetectParticleSystem(in dataList, in particleSystems);
                        meshCount += particleSystems.Length;
                    } else if (detectMeshType == MeshAnalysisData.DetectMeshType.ColliderMesh) {
                        var meshColliders = FindObjectsOfType<MeshCollider>();
                        DetectMeshCollider(in dataList, in meshColliders);
                        meshCount += meshColliders.Length;
                    }

                    break;
                case MeshAnalysisData.DetectType.Path:
                    DetectPath(in dataList, ref meshCount);
                    break;
                case MeshAnalysisData.DetectType.GameObject:
                    if (detectGameObject != null) {
                        DetectPrefab(in dataList, ref meshCount, detectGameObject);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AddRowsSum(dataList);

            if (meshCount <= 0) {
                return null;
            }

            DebugUtil.Log($"共检测出 {meshCount} 个 mesh 资源");
            return dataList;
        }

        private void DetectPrefab(in List<SceneAnalysisData> dataList, ref int meshCount, GameObject obj) {
            if (detectMeshType == MeshAnalysisData.DetectMeshType.RendererMesh) {
                var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
                DetectMeshFilter(in dataList, in meshFilters);
                meshCount += meshFilters.Length;

                var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
                DetectSkinnedMeshRenderer(in dataList, in skinnedMeshRenderers);
                meshCount += skinnedMeshRenderers.Length;

                var particleSystems = obj.GetComponentsInChildren<ParticleSystem>();
                DetectParticleSystem(in dataList, in particleSystems);
                meshCount += particleSystems.Length;
            } else if (detectMeshType == MeshAnalysisData.DetectMeshType.ColliderMesh) {
                var meshColliders = obj.GetComponentsInChildren<MeshCollider>();
                DetectMeshCollider(in dataList, in meshColliders);
                meshCount += meshColliders.Length;
            }
        }

        /// <summary>
        /// 增加一行: 总和
        /// </summary>
        private void AddRowsSum(in List<SceneAnalysisData> dataList) {
            dataList.Add(new SceneAnalysisData {
                id = dataList.Count + 1,
                tris = resultTris,
                verts = resultVerts,
                readwrite = "/",
                uv = resultUV,
                uv2 = resultUV2,
                uv3 = resultUV3,
                uv4 = resultUV4,
                colors = resultColors,
                normals = resultNormals,
                tangents = resultTangents,
                assetName = "Sum",
                assetPath = "Sum",
                assetType = "/",
                importCameras = "/",
                importLights = "/",
                importNormals = "/",
                meshCompression = "/",
                weldVertices = "/",
                meshOptimizationFlags = "/",
            });
        }

        /// <summary>
        /// 增加一条检测结果
        /// </summary>
        private void AddResult(in List<SceneAnalysisData> dataList, Mesh mesh, string readWriteEnable) {
            var path = AssetDatabase.GetAssetPath(mesh);
            var modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;

            resultVerts += mesh.vertices.Length;
            resultTris += mesh.triangles.Length / 3;
            resultUV += mesh.uv.Length;
            resultUV2 += mesh.uv2.Length;
            resultUV3 += mesh.uv3.Length;
            resultUV4 += mesh.uv4.Length;
            resultColors += mesh.colors.Length;
            resultTangents += mesh.tangents.Length;
            resultNormals += mesh.normals.Length;

            var newData = new SceneAnalysisData {
                id = dataList.Count + 1,
                tris = mesh.triangles.Length / 3,
                verts = mesh.vertices.Length,
                readwrite = readWriteEnable,
                uv = mesh.uv.Length,
                uv2 = mesh.uv2.Length,
                uv3 = mesh.uv3.Length,
                uv4 = mesh.uv4.Length,
                colors = mesh.colors.Length,
                normals = mesh.normals.Length,
                tangents = mesh.tangents.Length,
                assetType = "网格",
                assetPath = path,
                assetName = mesh.name,
                importCameras = "/",
                importLights = "/",
                importNormals = "/",
                meshCompression = MeshUtility.GetMeshCompression(mesh).ToString(),
                weldVertices = "/",
                meshOptimizationFlags = "/",
            };

            if (modelImporter != null) {
                newData.assetType = "模型";
                newData.meshCompression = modelImporter.meshCompression.ToString();
                newData.meshOptimizationFlags = modelImporter.meshOptimizationFlags.ToString();
                newData.importNormals = modelImporter.importNormals.ToString();
                newData.importLights = modelImporter.importLights.ToString();
                newData.importCameras = modelImporter.importCameras.ToString();
                newData.weldVertices = modelImporter.weldVertices.ToString();
            }
            
            dataList.Add(newData);
        }

        #region 检测

        /// <summary>
        /// 检测 MeshCollider
        /// </summary>
        /// <param name="dataList">行数据</param>
        /// <param name="meshColliders">待检测组件</param>
        private void DetectMeshCollider(in List<SceneAnalysisData> dataList, in MeshCollider[] meshColliders) {
            const MeshColliderCookingOptions DEFAULT_OPTIONS = MeshColliderCookingOptions.EnableMeshCleaning & MeshColliderCookingOptions.WeldColocatedVertices & MeshColliderCookingOptions.CookForFasterSimulation;

            foreach (var meshCollider in meshColliders) {
                if (meshCollider != null) {
                    var sharedMesh = meshCollider.sharedMesh;
                    if (sharedMesh == null) {
                        DebugUtil.LogError("使用了 MeshCollider 却没有指定 Mesh!", meshCollider.gameObject, "red");
                    } else {
                        var readWriteEnable = false;

                        // 负缩放的凸多面体
                        if (meshCollider.transform.localScale.x < 0 || meshCollider.transform.localScale.y < 0 || meshCollider.transform.localScale.z < 0) {
                            if (meshCollider.convex) {
                                readWriteEnable = true;
                                if (sharedMesh.isReadable == false) {
                                    DebugUtil.Log($"{meshCollider.name} 是一个负缩放的凸多面体, 需要开启读写!", meshCollider.gameObject, "red");
                                }
                            }
                        }

                        // 倾斜
                        else if (meshCollider.transform.parent != null && meshCollider.transform.localRotation != Quaternion.identity && meshCollider.transform.parent.localScale != Vector3.one) {
                            readWriteEnable = true;
                            if (sharedMesh.isReadable == false) {
                                DebugUtil.Log($"{meshCollider.name} 的旋转是倾斜的, 需要开启读写!", meshCollider.gameObject, "red");
                            }
                        }

                        // 烘焙选项非默认
                        else if (meshCollider.cookingOptions != DEFAULT_OPTIONS) {
                            readWriteEnable = true;
                            if (sharedMesh.isReadable == false) {
                                DebugUtil.Log($"{meshCollider.name} 的烘焙选项非默认, 需要开启读写!", meshCollider.gameObject, "red");
                            }
                        }

                        var readWriteEnableStr = $"{sharedMesh.isReadable} => {readWriteEnable}";
                        AddResult(dataList, sharedMesh, readWriteEnableStr);
                    }
                }
            }
        }

        /// <summary>
        /// 检测 MeshFilter
        /// </summary>
        /// <param name="dataList">行数据</param>
        /// <param name="meshFilters">待检测组件</param>
        private void DetectMeshFilter(in List<SceneAnalysisData> dataList, in MeshFilter[] meshFilters) {
            foreach (var meshFilter in meshFilters) {
                if (ReferenceEquals(meshFilter, null)) {
                    continue;
                }

                var sharedMesh = meshFilter.sharedMesh;
                if (ReferenceEquals(sharedMesh, null)) {
                    DebugUtil.LogError("使用了 MeshFilter 却没有指定 Mesh!", meshFilter.gameObject);
                    continue;
                }

                AddResult(dataList, sharedMesh, "非碰撞");
            }
        }

        /// <summary>
        /// 检测 ParticleSystem
        /// </summary>
        /// <param name="dataList">行数据</param>
        /// <param name="particleSystems">待检测组件</param>
        private void DetectParticleSystem(in List<SceneAnalysisData> dataList, in ParticleSystem[] particleSystems) {
            foreach (var particleSystem in particleSystems) {
                if (ReferenceEquals(particleSystem, null) == false) {
                    var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                    var mesh = renderer.mesh;

                    if (ReferenceEquals(mesh, null)) {
                        if (renderer.renderMode == ParticleSystemRenderMode.Mesh) {
                            DebugUtil.LogError($"粒子系统 {particleSystem.transform.name} 使用 Mesh 方式渲染粒子, 却没有指定 Mesh!", particleSystem.gameObject, "red");
                        }
                    } else {
                        AddResult(dataList, mesh, "非碰撞");
                    }
                }
            }
        }

        /// <summary>
        /// 检测 SkinnedMeshRenderer
        /// </summary>
        /// <param name="dataList">行数据</param>
        /// <param name="skinnedMeshRenderers">待检测组件</param>
        private void DetectSkinnedMeshRenderer(in List<SceneAnalysisData> dataList, in SkinnedMeshRenderer[] skinnedMeshRenderers) {
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers) {
                if (ReferenceEquals(skinnedMeshRenderer, null)) {
                    continue;
                }

                var mesh = skinnedMeshRenderer.sharedMesh;
                if (ReferenceEquals(mesh, null)) {
                    DebugUtil.LogError("使用了 SkinnedMeshRenderer 却没有指定 Mesh!", skinnedMeshRenderer.gameObject);
                    continue;
                }

                AddResult(dataList, mesh, "非碰撞");
            }
        }

        #endregion

        #region 创建数据列

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_ID() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("ID"),
                headerTextAlignment = TextAlignment.Center,
                width = 50,
                minWidth = 50,
                maxWidth = 120,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.id.CompareTo(dataB.id), // 排序
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.id.ToString());
                },
            };
        }
        
        private static CustomTableColumn<SceneAnalysisData> CreateColumn_AssetType() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Type"),
                headerTextAlignment = TextAlignment.Center,
                width = 50,
                minWidth = 50,
                maxWidth = 120,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => string.Compare(dataA.assetType, dataB.assetType, StringComparison.Ordinal), // 排序
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.assetType);
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_Name() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Center,
                width = 600,
                minWidth = 600,
                maxWidth = 1200,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => string.Compare(dataA.assetPath, dataB.assetPath, StringComparison.Ordinal),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    var iconRect = cellRect;
                    iconRect.width = 20f;
                    EditorGUI.LabelField(iconRect, data.assetName.Equals("Sum")? EditorGUIUtility.IconContent("console.InfoIcon.sml") : EditorGUIUtility.IconContent("PrefabModel Icon"));
                    cellRect.xMin += 20f;
                    EditorGUI.LabelField(cellRect, data.assetPath);
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_Verts() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Verts"),
                headerTextAlignment = TextAlignment.Center,
                width = 80,
                minWidth = 80,
                maxWidth = 120,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.verts.CompareTo(dataB.verts),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    var iconRect = cellRect;
                    iconRect.width = 20f;
                    cellRect.xMin += 20f;
                    if (data.verts > vertsError) {
                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.ErrorIcon.sml"));
                        EditorGUI.LabelField(cellRect, data.verts.ToString());
                    } else if (data.verts > vertsWarn) {
                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.WarnIcon.sml"));
                        EditorGUI.LabelField(cellRect, data.verts.ToString());
                    } else {
                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.InfoIcon.sml"));
                        EditorGUI.LabelField(cellRect, data.verts.ToString());
                    }
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_Tris() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Tris"),
                headerTextAlignment = TextAlignment.Center,
                width = 80,
                minWidth = 80,
                maxWidth = 120,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.tris.CompareTo(dataB.tris),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    var iconRect = cellRect;
                    iconRect.width = 20f;
                    cellRect.xMin += 20f;
                    if (data.tris > trisError) {
                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.ErrorIcon.sml"));
                        EditorGUI.LabelField(cellRect, data.tris.ToString());
                    } else if (data.tris > trisWarn) {
                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnIcon.sml"));
                        EditorGUI.LabelField(cellRect, data.tris.ToString());
                    } else {
                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.infoIcon.sml"));
                        EditorGUI.LabelField(cellRect, data.tris.ToString());
                    }
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_ReadWrite() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Readable"),
                headerTextAlignment = TextAlignment.Center,
                width = 70,
                minWidth = 70,
                maxWidth = 120,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => string.Compare(dataA.readwrite, dataB.readwrite, StringComparison.Ordinal),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.readwrite.ToString());
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_UV() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("UV"),
                headerTextAlignment = TextAlignment.Center,
                width = 50,
                minWidth = 50,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.uv.CompareTo(dataB.uv),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.uv.ToString());
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_UV2() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("UV2"),
                headerTextAlignment = TextAlignment.Center,
                width = 50,
                minWidth = 50,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.uv2.CompareTo(dataB.uv2),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.uv2.ToString());
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_UV3() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("UV3"),
                headerTextAlignment = TextAlignment.Center,
                width = 40,
                minWidth = 40,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.uv3.CompareTo(dataB.uv3),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.uv3.ToString());
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_UV4() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("UV4"),
                headerTextAlignment = TextAlignment.Center,
                width = 40,
                minWidth = 40,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.uv4.CompareTo(dataB.uv4),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.uv4.ToString());
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_Colors() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Colors"),
                headerTextAlignment = TextAlignment.Center,
                width = 60,
                minWidth = 60,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.colors.CompareTo(dataB.colors),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.colors.ToString());
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_Tangents() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Tangents"),
                headerTextAlignment = TextAlignment.Center,
                width = 70,
                minWidth = 70,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.tangents.CompareTo(dataB.tangents),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.tangents.ToString());
                },
            };
        }

        private static CustomTableColumn<SceneAnalysisData> CreateColumn_Normals() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Normals"),
                headerTextAlignment = TextAlignment.Center,
                width = 70,
                minWidth = 70,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.normals.CompareTo(dataB.normals),
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.normals.ToString());
                },
            };
        }
        
        private static CustomTableColumn<SceneAnalysisData> CreateColumn_MeshCompression() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Compression"),
                headerTextAlignment = TextAlignment.Center,
                width = 100,
                minWidth = 100,
                maxWidth = 100,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => string.Compare(dataA.meshCompression, dataB.meshCompression, StringComparison.Ordinal), // 排序
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.meshCompression.ToString());
                },
            };
        }
        
        private static CustomTableColumn<SceneAnalysisData> CreateColumn_OptimizationFlags() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Optimization"),
                headerTextAlignment = TextAlignment.Center,
                width = 100,
                minWidth = 100,
                maxWidth = 100,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => string.Compare(dataA.meshOptimizationFlags, dataB.meshOptimizationFlags, StringComparison.Ordinal), // 排序
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.meshOptimizationFlags.ToString());
                },
            };
        }
        
        private static CustomTableColumn<SceneAnalysisData> CreateColumn_ImportNormals() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("import Normals"),
                headerTextAlignment = TextAlignment.Center,
                width = 100,
                minWidth = 100,
                maxWidth = 100,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => string.Compare(dataA.importNormals, dataB.importNormals, StringComparison.Ordinal), // 排序
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.importNormals.ToString());
                },
            };
        }
        
        private static CustomTableColumn<SceneAnalysisData> CreateColumn_ImportLights() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("import Lights"),
                headerTextAlignment = TextAlignment.Center,
                width = 100,
                minWidth = 100,
                maxWidth = 100,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => string.Compare(dataB.importLights, dataA.importLights, StringComparison.Ordinal), // 排序
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.importLights.ToString());
                },
            };
        }
        
        private static CustomTableColumn<SceneAnalysisData> CreateColumn_ImportCameras() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Import Cameras"),
                headerTextAlignment = TextAlignment.Center,
                width = 100,
                minWidth = 100,
                maxWidth = 100,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => string.Compare(dataA.importCameras, dataB.importCameras, StringComparison.Ordinal), // 排序
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.importCameras.ToString());
                },
            };
        }
        
        private static CustomTableColumn<SceneAnalysisData> CreateColumn_WeldVertices() {
            return new CustomTableColumn<SceneAnalysisData> {
                headerContent = new GUIContent("Weld Vertices"),
                headerTextAlignment = TextAlignment.Center,
                width = 100,
                minWidth = 100,
                maxWidth = 100,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => string.Compare(dataA.weldVertices, dataB.weldVertices, StringComparison.Ordinal), // 排序
                DrawCell = (cellRect, data) => {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.weldVertices.ToString());
                },
            };
        }

        #endregion

        /// <summary>
        /// 初始化列
        /// </summary>
        /// <returns></returns>
        private static CustomTableColumn<SceneAnalysisData>[] InitColumns() {
            var newColumns = new List<CustomTableColumn<SceneAnalysisData>> {
                CreateColumn_ID(),
                CreateColumn_AssetType(),
                CreateColumn_Name(),
                CreateColumn_Verts(),
                CreateColumn_Tris(),
                CreateColumn_UV(),
                CreateColumn_UV2(),
                CreateColumn_UV3(),
                CreateColumn_UV4(),
                CreateColumn_Colors(),
                CreateColumn_Tangents(),
                CreateColumn_Normals(),
                CreateColumn_ReadWrite(),
                CreateColumn_MeshCompression(),
                CreateColumn_OptimizationFlags(),
                CreateColumn_ImportNormals(),
                CreateColumn_ImportLights(),
                CreateColumn_ImportCameras(),
                CreateColumn_WeldVertices()
            };

            return newColumns.ToArray();
        }

        #region 事件

        /// <summary>
        /// 行选中事件
        /// </summary>
        /// <param name="dataList"></param>
        private static void OnRowSelect(in List<SceneAnalysisData> dataList) {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dataList[0].assetPath);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }

        /// <summary>
        /// 导出按钮事件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dataList"></param>
        private static void OnExportPressed(string file, in List<SceneAnalysisData> dataList) {
            if (dataList.Count <= 0) {
                EditorUtility.DisplayDialog("Warning", "No Data!", "Ok");
                return;
            }

            if (File.Exists(file)) {
                File.Delete(file);
            }

            var stringBuilder = new StringBuilder();
            foreach (var data in dataList) {
                stringBuilder.Clear();
                stringBuilder.Append($"{data.id}\t");
                stringBuilder.Append(data.assetName == "Sum"? "无路径\t" : $"{Path.GetFullPath(data.assetPath)}\t");
                stringBuilder.Append($"{data.tris}\t");
                stringBuilder.Append($"{data.verts}\t");
                stringBuilder.Append($"{data.uv}\t");
                stringBuilder.Append($"{data.uv2}\t");
                stringBuilder.Append($"{data.uv3}\t");
                stringBuilder.Append($"{data.uv4}\t");
                stringBuilder.Append($"{data.colors}\t");
                stringBuilder.Append($"{data.tangents}\t");
                stringBuilder.Append($"{data.normals}\t");
                stringBuilder.Append("\r\n");
                File.AppendAllText(file, stringBuilder.ToString());
            }
        }

        /// <summary>
        /// 查找按钮事件
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="data"></param>
        /// <param name="filterText"></param>
        /// <returns></returns>
        private static bool OnFilterEnter(int mask, SceneAnalysisData data, string filterText) {
            var isMatched = true;
            var maskChars = Convert.ToString(mask, 2).Reverse().ToArray();

            var index = 0;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.id.ToString());
            }
            
            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.assetType);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.assetName);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterNumber(data.verts);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterNumber(data.tris);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterNumber(data.uv);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterNumber(data.uv2);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterNumber(data.uv3);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterNumber(data.uv4);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterNumber(data.colors);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterNumber(data.tangents);
            }

            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterNumber(data.normals);
            }
            
            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.readwrite);
            }
            
            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.meshCompression);
            }
            
            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.meshOptimizationFlags);
            }
            
            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.importNormals);
            }
            
            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.importLights);
            }
            
            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.importCameras);
            }
            
            index++;
            if (maskChars.Length > index && maskChars[index] == '1') {
                isMatched &= ColumnFilterString(data.weldVertices);
            }

            #region Local Function

            bool ColumnFilterString(string sourceData) {
                return sourceData.ToLower().Contains(filterText.ToLower());
            }

            bool ColumnFilterNumber(int sourceData) {
                return int.TryParse(filterText, out var number) && sourceData > number;
            }

            #endregion

            return isMatched;
        }

        /// <summary>
        /// 查找结束事件
        /// </summary>
        private void OnAfterFilter(List<SceneAnalysisData> dataList) {
            if (dataList.Count <= 0) {
                return;
            }

            if (dataList.Count == 1) {
                if (dataList[0].assetName == "Sum") {
                    dataList.Clear();
                    return;
                }
            }

            resultTris = 0;
            resultVerts = 0;
            resultUV = 0;
            resultUV2 = 0;
            resultUV3 = 0;
            resultUV4 = 0;
            resultColors = 0;
            resultTangents = 0;
            resultNormals = 0;

            var sumIndex = 0;

            for (var i = 0; i < dataList.Count; i++) {
                if (dataList[i].assetName == "Sum") {
                    sumIndex = i;
                } else {
                    resultTris += dataList[i].tris;
                    resultVerts += dataList[i].verts;
                    resultUV += dataList[i].uv;
                    resultUV2 += dataList[i].uv2;
                    resultUV3 += dataList[i].uv3;
                    resultUV4 += dataList[i].uv4;
                    resultColors += dataList[i].colors;
                    resultTangents += dataList[i].tangents;
                    resultNormals += dataList[i].normals;
                }
            }

            dataList.RemoveAt(sumIndex);

            for (var i = 0; i < dataList.Count; i++) {
                dataList[i].id = i + 1;
            }

            AddRowsSum(dataList);
        }

        /// <summary>
        /// 数据去重事件
        /// </summary>
        private void OnDistinctPressed(List<SceneAnalysisData> dataList) {
            if (isDistinct) {
                // 恢复去重
                isDistinct = false;
                dataList = originList;

                // 重新编号
                for (var index = 0; index < dataList.Count; ++index) {
                    dataList[index].id = index + 1;
                }
            } else {
                // 去重
                isDistinct = true;
                distinctList.Clear();
                foreach (var data in dataList) {
                    if (data.assetName == "Sum") {
                        continue;
                    }

                    if (distinctList.Exists(t => t.IsEqual(data))) {
                        continue;
                    }

                    distinctList.Add(data);
                }

                dataList = distinctList;

                // 重新编号并重新求和
                resultTris = 0;
                resultVerts = 0;
                resultUV = 0;
                resultUV2 = 0;
                resultUV3 = 0;
                resultUV4 = 0;
                resultColors = 0;
                resultNormals = 0;
                resultTangents = 0;
                for (var index = 0; index < dataList.Count; ++index) {
                    dataList[index].id = index + 1;
                    resultTris += dataList[index].tris;
                    resultVerts += dataList[index].verts;
                    resultUV += dataList[index].uv;
                    resultUV2 += dataList[index].uv2;
                    resultUV3 += dataList[index].uv3;
                    resultUV4 += dataList[index].uv4;
                    resultColors += dataList[index].colors;
                    resultNormals += dataList[index].normals;
                    resultTangents += dataList[index].tangents;
                }

                // 添加 Sum
                AddRowsSum(dataList);
            }

            // 重新生成表格
            GenerateTable(dataList, columns);
        }

        #endregion

        private void DetectPath(in List<SceneAnalysisData> dataList, ref int meshCount) {
            switch (detectTypeAtPath) {
                case MeshAnalysisData.DetectTypeAtPath.Meshes: {
                    var meshGuids = AssetDatabase.FindAssets("t:Mesh", new[] {
                        detectPath
                    });
                    meshCount = meshGuids.Length;
                    foreach (var guid in meshGuids) {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        try {
                            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                            AddResult(dataList, mesh, $"{mesh.isReadable}");
                        } catch {
                            Debug.LogError($"无法读取为 Mesh 资源! {path}");
                        }
                    }
                    break;
                }
                
                case MeshAnalysisData.DetectTypeAtPath.Prefabs:
                    var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] {
                        detectPath
                    });
                    foreach (var guid in prefabGuids) {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (obj != null) {
                            DetectPrefab(in dataList, ref meshCount, obj);
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
