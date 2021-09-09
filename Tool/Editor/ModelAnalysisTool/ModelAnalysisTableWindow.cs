using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuroha.GUI.Editor.Table;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.ModelAnalysisTool
{
    public class ModelAnalysisTableWindow : EditorWindow
    {
        private static bool isCollider;
        private static GameObject prefab;
        private static bool isDetectCurrentScene;

        private int resultTris;
        private int resultVerts;
        private ModelAnalysisTable table;
        private GUIStyle fontStyleRed;
        private GUIStyle fontStyleYellow;

        private static int vertsWarn;
        private static int vertsError;
        private static int trisWarn;
        private static int trisError;

        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void Open(bool collider, GameObject asset, bool detectCurrentScene)
        {
            prefab = asset;
            isCollider = collider;
            isDetectCurrentScene = detectCurrentScene;

            var window = GetWindow<ModelAnalysisTableWindow>(true);
            window.minSize = new Vector2(1000, 600);
            window.maxSize = new Vector2(1000, 600);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            if (vertsWarn == 0)
            {
                vertsWarn = 500;
            }

            if (vertsError == 0)
            {
                vertsError = 1000;
            }

            if (trisWarn == 0)
            {
                trisWarn = 500;
            }

            if (trisError == 0)
            {
                trisError = 1000;
            }

            fontStyleRed = new GUIStyle();
            fontStyleYellow = new GUIStyle();
            resultTris = 0;
            resultVerts = 0;
            fontStyleRed.normal.textColor = new Color((float)203 / 255, (float)27 / 255, (float)69 / 255);
            fontStyleYellow.normal.textColor = new Color((float)226 / 255, (float)148 / 255, (float)59 / 255);

            InitTable();
        }

        /// <summary>
        /// 绘制界面
        /// </summary>
        protected void OnGUI()
        {
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
        private void InitTable(bool forceUpdate = false)
        {
            if (forceUpdate || table == null)
            {
                if (prefab != null || isDetectCurrentScene)
                {
                    var dataList = InitRows(isCollider);
                    if (dataList != null)
                    {
                        var columns = InitColumns();
                        if (columns != null)
                        {
                            table = new ModelAnalysisTable(new Vector2(20, 20), new Vector2(300, 300),
                                dataList, true, true, 50, 50, columns,
                                OnFilterEnter, OnExportPressed, OnRowSelect);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 初始化行数据
        /// </summary>
        /// <param name="isDetectCollider">是否是检测碰撞 true: 检测碰撞 false: 检测渲染</param>
        /// <returns></returns>
        private List<ModelAnalysisData> InitRows(bool isDetectCollider)
        {
            var dataList = new List<ModelAnalysisData>();
            var meshCount = 0;

            if (isDetectCurrentScene)
            {
                if (isDetectCollider)
                {
                    var meshColliders = FindObjectsOfType<MeshCollider>();
                    DetectMeshCollider(in dataList, in meshColliders);
                    AddRowsSum(dataList);
                    meshCount += meshColliders.Length;
                }
                else
                {
                    var meshFilters = FindObjectsOfType<MeshFilter>();
                    DetectMeshFilter(in dataList, in meshFilters);
                    meshCount += meshFilters.Length;
                    var skinnedMeshRenderers = FindObjectsOfType<SkinnedMeshRenderer>();
                    DetectSkinnedMeshRenderer(in dataList, in skinnedMeshRenderers);
                    AddRowsSum(dataList);
                    meshCount += skinnedMeshRenderers.Length;
                }
            }
            else
            {
                if (isDetectCollider)
                {
                    var meshColliders = prefab.GetComponentsInChildren<MeshCollider>();
                    DetectMeshCollider(in dataList, in meshColliders);
                    AddRowsSum(dataList);
                    meshCount += meshColliders.Length;
                }
                else
                {
                    var meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
                    DetectMeshFilter(in dataList, in meshFilters);
                    meshCount += meshFilters.Length;
                    var skinnedMeshRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
                    DetectSkinnedMeshRenderer(in dataList, in skinnedMeshRenderers);
                    AddRowsSum(dataList);
                    meshCount += skinnedMeshRenderers.Length;
                }
            }

            if (meshCount <= 0)
            {
                return null;
            }

            DebugUtil.Log($"共检测出 {meshCount} 个 mesh 组件");
            return dataList;
        }

        /// <summary>
        /// 增加一行: 总和
        /// </summary>
        private void AddRowsSum(in List<ModelAnalysisData> dataList)
        {
            dataList.Add(new ModelAnalysisData
            {
                id = dataList.Count + 1,
                tris = resultTris,
                verts = resultVerts,
                assetName = "总和",
                assetPath = string.Empty,
            });
        }

        /// <summary>
        /// 检测 MeshCollider
        /// </summary>
        /// <param name="dataList">行数据</param>
        /// <param name="meshColliders">待检测组件</param>
        private void DetectMeshCollider(in List<ModelAnalysisData> dataList, in MeshCollider[] meshColliders)
        {
            var counter = 0;

            foreach (var meshCollider in meshColliders)
            {
                if (ReferenceEquals(meshCollider, null))
                {
                    continue;
                }

                var sharedMesh = meshCollider.sharedMesh;
                if (ReferenceEquals(sharedMesh, null))
                {
                    DebugUtil.LogError("使用了 MeshCollider 却没有指定 Mesh!", meshCollider.gameObject);
                    continue;
                }

                resultVerts += sharedMesh.vertices.Length;
                resultTris += sharedMesh.triangles.Length / 3;

                counter++;
                dataList.Add(new ModelAnalysisData
                {
                    id = counter,
                    tris = sharedMesh.triangles.Length / 3,
                    verts = sharedMesh.vertices.Length,
                    assetName = AssetDatabase.GetAssetPath(sharedMesh),
                    assetPath = AssetDatabase.GetAssetPath(sharedMesh),
                });
            }
        }

        /// <summary>
        /// 检测 MeshFilter
        /// </summary>
        /// <param name="dataList">行数据</param>
        /// <param name="meshFilters">待检测组件</param>
        private void DetectMeshFilter(in List<ModelAnalysisData> dataList, in MeshFilter[] meshFilters)
        {
            var counter = 0;

            foreach (var meshFilter in meshFilters)
            {
                if (ReferenceEquals(meshFilter, null))
                {
                    continue;
                }

                var sharedMesh = meshFilter.sharedMesh;
                if (ReferenceEquals(sharedMesh, null))
                {
                    DebugUtil.LogError("使用了 MeshFilter 却没有指定 Mesh!", meshFilter.gameObject);
                    continue;
                }

                resultVerts += sharedMesh.vertices.Length;
                resultTris += sharedMesh.triangles.Length / 3;

                counter++;
                dataList.Add(new ModelAnalysisData
                {
                    id = counter,
                    tris = sharedMesh.triangles.Length / 3,
                    verts = sharedMesh.vertices.Length,
                    assetName = AssetDatabase.GetAssetPath(sharedMesh),
                    assetPath = AssetDatabase.GetAssetPath(sharedMesh),
                });
            }
        }

        /// <summary>
        /// 检测 SkinnedMeshRenderer
        /// </summary>
        /// <param name="dataList">行数据</param>
        /// <param name="skinnedMeshRenderers">待检测组件</param>
        private void DetectSkinnedMeshRenderer(in List<ModelAnalysisData> dataList, in SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            var counter = 0;

            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (ReferenceEquals(skinnedMeshRenderer, null))
                {
                    continue;
                }

                var sharedMesh = skinnedMeshRenderer.sharedMesh;
                if (ReferenceEquals(sharedMesh, null))
                {
                    DebugUtil.LogError("使用了 SkinnedMeshRenderer 却没有指定 Mesh!", skinnedMeshRenderer.gameObject);
                    continue;
                }

                resultVerts += sharedMesh.vertices.Length;
                resultTris += sharedMesh.triangles.Length / 3;

                counter++;
                dataList.Add(new ModelAnalysisData
                {
                    id = counter,
                    tris = sharedMesh.triangles.Length / 3,
                    verts = sharedMesh.vertices.Length,
                    assetName = AssetDatabase.GetAssetPath(sharedMesh),
                    assetPath = AssetDatabase.GetAssetPath(sharedMesh),
                });
            }
        }

        /// <summary>
        /// 初始化列
        /// </summary>
        /// <returns></returns>
        private CommonTableColumn<ModelAnalysisData>[] InitColumns()
        {
            return new[]
            {
                new CommonTableColumn<ModelAnalysisData>
                {
                    headerContent = new GUIContent("ID"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 50,
                    minWidth = 50,
                    maxWidth = 120,
                    allowToggleVisibility = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => -dataA.id.CompareTo(dataA.id), // 排序
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        EditorGUI.LabelField(cellRect, data.id.ToString());
                    },
                },
                new CommonTableColumn<ModelAnalysisData>
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 300,
                    minWidth = 300,
                    maxWidth = 500,
                    allowToggleVisibility = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) =>
                        -string.Compare(dataA.assetName, dataB.assetName, StringComparison.Ordinal),
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        EditorGUI.LabelField(iconRect,
                            data.assetName.Equals("总和")
                                ? EditorGUIUtility.IconContent("console.infoicon.sml")
                                : EditorGUIUtility.IconContent("PrefabModel Icon"));
                        cellRect.xMin += 20f;
                        EditorGUI.LabelField(cellRect,
                            data.assetName.Contains("/")
                                ? data.assetName.Split('/').Last()
                                : data.assetName.Split('\\').Last());
                    },
                },
                new CommonTableColumn<ModelAnalysisData>
                {
                    headerContent = new GUIContent("Verts"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => -dataA.verts.CompareTo(dataB.verts),
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;
                        if (data.verts > vertsError)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.erroricon.sml"));
                            EditorGUI.LabelField(cellRect, data.verts.ToString(), fontStyleRed);
                        }
                        else if (data.verts > vertsWarn)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnicon.sml"));
                            EditorGUI.LabelField(cellRect, data.verts.ToString(), fontStyleYellow);
                        }
                        else
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.infoicon.sml"));
                            EditorGUI.LabelField(cellRect, data.verts.ToString());
                        }
                    },
                },
                new CommonTableColumn<ModelAnalysisData>
                {
                    headerContent = new GUIContent("Tris"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => -dataA.tris.CompareTo(dataB.tris),
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;
                        if (data.tris > trisError)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.erroricon.sml"));
                            EditorGUI.LabelField(cellRect, data.tris.ToString(), fontStyleRed);
                        }
                        else if (data.tris > trisWarn)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnicon.sml"));
                            EditorGUI.LabelField(cellRect, data.tris.ToString(), fontStyleYellow);
                        }
                        else
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.infoicon.sml"));
                            EditorGUI.LabelField(cellRect, data.tris.ToString());
                        }
                    },
                }
            };
        }

        /// <summary>
        /// 行选中事件
        /// </summary>
        /// <param name="dataList"></param>
        private static void OnRowSelect(in List<ModelAnalysisData> dataList)
        {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dataList[0].assetPath);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }

        /// <summary>
        /// 导出按钮事件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dataList"></param>
        private static void OnExportPressed(string file, in List<ModelAnalysisData> dataList)
        {
            if (dataList.Count <= 0)
            {
                EditorUtility.DisplayDialog("Warning", "No Data!", "Ok");
                return;
            }

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            foreach (var data in dataList)
            {
                File.AppendAllText(file, $"{data.id}\t{data.assetName}\t{data.verts}\t{data.tris}\n");
            }
        }

        /// <summary>
        /// 查找按钮事件
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="data"></param>
        /// <param name="filterText"></param>
        /// <returns></returns>
        private static bool OnFilterEnter(int mask, ModelAnalysisData data, string filterText)
        {
            var isMatched = false;
            var maskChars = Convert.ToString(mask, 2).Reverse().ToArray();

            if (ColumnFilter1() || ColumnFilter2() || ColumnFilter3() || ColumnFilter4())
            {
                isMatched = true;
            }

            #region Local Function

            bool ColumnFilter1()
            {
                if (maskChars.Length < 1 || maskChars[0] != '1')
                {
                    return false;
                }

                return data.id.ToString().ToLower().Contains(filterText.ToLower());
            }

            bool ColumnFilter2()
            {
                if (maskChars.Length < 2 || maskChars[1] != '1')
                {
                    return false;
                }

                return data.assetName.ToLower().Contains(filterText.ToLower());
            }

            bool ColumnFilter3()
            {
                if (maskChars.Length < 3 || maskChars[2] != '1')
                {
                    return false;
                }

                if (int.TryParse(filterText, out int verts))
                {
                    if (data.verts > verts)
                    {
                        return true;
                    }
                }
                else if (data.verts.ToString().ToLower().Contains(filterText.ToLower()))
                {
                    return true;
                }

                return false;
            }

            bool ColumnFilter4()
            {
                if (maskChars.Length < 4 || maskChars[3] != '1')
                {
                    return false;
                }

                if (int.TryParse(filterText, out int tris))
                {
                    if (data.tris > tris)
                    {
                        return true;
                    }
                }
                else if (data.tris.ToString().ToLower().Contains(filterText.ToLower()))
                {
                    return true;
                }

                return false;
            }

            #endregion

            return isMatched;
        }
    }
}