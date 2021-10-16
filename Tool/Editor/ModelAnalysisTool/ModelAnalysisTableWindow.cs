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
        private int resultUV;
        private int resultUV2;
        private int resultUV3;
        private int resultUV4;
        private int resultColors;
        private int resultTangents;
        private int resultNormals;
        
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
                if (ReferenceEquals(prefab, null) == false || isDetectCurrentScene)
                {
                    var dataList = InitRows(isCollider);
                    
                    if (dataList != null)
                    {
                        var columns = InitColumns(isCollider);
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

            if (isDetectCollider)
            {
                var meshColliders = isDetectCurrentScene
                    ? FindObjectsOfType<MeshCollider>()
                    : prefab.GetComponentsInChildren<MeshCollider>();
                DetectMeshCollider(in dataList, in meshColliders);
                meshCount += meshColliders.Length;
            }
            else
            {
                var meshFilters = isDetectCurrentScene
                    ? FindObjectsOfType<MeshFilter>()
                    : prefab.GetComponentsInChildren<MeshFilter>();
                DetectMeshFilter(in dataList, in meshFilters);
                meshCount += meshFilters.Length;

                
                var skinnedMeshRenderers = isDetectCurrentScene
                    ? FindObjectsOfType<SkinnedMeshRenderer>()
                    : prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
                DetectSkinnedMeshRenderer(in dataList, in skinnedMeshRenderers);
                meshCount += skinnedMeshRenderers.Length;


                var particleSystems = isDetectCurrentScene
                    ? FindObjectsOfType<ParticleSystem>()
                    : prefab.GetComponentsInChildren<ParticleSystem>();
                DetectParticleSystem(in dataList, in particleSystems);
                meshCount += particleSystems.Length;
            }
            
            AddRowsSum(dataList);

            if (meshCount <= 0)
            {
                return null;
            }

            DebugUtil.Log($"共检测出 {meshCount} 个 mesh 资源");
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
                uv = resultUV,
                uv2 = resultUV2,
                uv3 = resultUV3,
                uv4 = resultUV4,
                colors = resultColors,
                normals = resultNormals,
                tangents = resultTangents,
                assetName = "总和",
                assetPath = string.Empty,
            });
        }

        /// <summary>
        /// 增加一条检测结果
        /// </summary>
        private void AddResult(in List<ModelAnalysisData> dataList, Mesh mesh, string readWriteEnable)
        {
            resultVerts += mesh.vertices.Length;
            resultTris += mesh.triangles.Length / 3;
            resultUV += mesh.uv.Length;
            resultUV2 += mesh.uv2.Length;
            resultUV3 += mesh.uv3.Length;
            resultUV4 += mesh.uv4.Length;
            resultColors += mesh.colors.Length;
            resultTangents += mesh.tangents.Length;
            resultNormals += mesh.normals.Length;

            dataList.Add(new ModelAnalysisData
            {
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
                assetName = AssetDatabase.GetAssetPath(mesh),
                assetPath = AssetDatabase.GetAssetPath(mesh)
            });
        }
        
        /// <summary>
        /// 检测 MeshCollider
        /// </summary>
        /// <param name="dataList">行数据</param>
        /// <param name="meshColliders">待检测组件</param>
        private void DetectMeshCollider(in List<ModelAnalysisData> dataList, in MeshCollider[] meshColliders)
        {
            const MeshColliderCookingOptions DEFAULT_OPTIONS = MeshColliderCookingOptions.EnableMeshCleaning &
                                                               MeshColliderCookingOptions.WeldColocatedVertices &
                                                               MeshColliderCookingOptions.CookForFasterSimulation;
            
            foreach (var meshCollider in meshColliders)
            {
                if (ReferenceEquals(meshCollider, null) == false)
                {
                    var sharedMesh = meshCollider.sharedMesh;
                    if (ReferenceEquals(sharedMesh, null))
                    {
                        DebugUtil.LogError("使用了 MeshCollider 却没有指定 Mesh!", meshCollider.gameObject, "red");
                    }
                    else
                    {
                        var readWriteEnable = false;
                        
                        // 负缩放 + 凸体
                        if (meshCollider.transform.localScale.x < 0 || meshCollider.transform.localScale.y < 0 || meshCollider.transform.localScale.z < 0)
                        {
                            if (meshCollider.convex)
                            {
                                DebugUtil.Log("原因 1");
                                readWriteEnable = true;
                            }
                        }
                        
                        // 旋转
                        // else if (meshCollider.transform.localRotation != Quaternion.identity)
                        // {
                        //     DebugUtil.Log("原因 2");
                        //     readWriteEnable = true;
                        // }
                        
                        // Cooking Options
                        else if (meshCollider.cookingOptions != DEFAULT_OPTIONS)
                        {
                            DebugUtil.Log("原因 3");
                            readWriteEnable = true;
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
        private void DetectMeshFilter(in List<ModelAnalysisData> dataList, in MeshFilter[] meshFilters)
        {
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

                AddResult(dataList, sharedMesh, "非碰撞");
            }
        }
        
        /// <summary>
        /// 检测 ParticleSystem
        /// </summary>
        /// <param name="dataList">行数据</param>
        /// <param name="particleSystems">待检测组件</param>
        private void DetectParticleSystem(in List<ModelAnalysisData> dataList, in ParticleSystem[] particleSystems)
        {
            foreach (var particleSystem in particleSystems)
            {
                if (ReferenceEquals(particleSystem, null) == false)
                {
                    var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                    var mesh = renderer.mesh;
                
                    if (ReferenceEquals(mesh, null))
                    {
                        if (renderer.renderMode == ParticleSystemRenderMode.Mesh)
                        {
                            DebugUtil.LogError($"粒子系统 {particleSystem.transform.name} 使用 Mesh 方式渲染粒子, 却没有指定 Mesh!", particleSystem.gameObject, "red");
                        }
                    }
                    else
                    {
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
        private void DetectSkinnedMeshRenderer(in List<ModelAnalysisData> dataList, in SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (ReferenceEquals(skinnedMeshRenderer, null))
                {
                    continue;
                }

                var mesh = skinnedMeshRenderer.sharedMesh;
                if (ReferenceEquals(mesh, null))
                {
                    DebugUtil.LogError("使用了 SkinnedMeshRenderer 却没有指定 Mesh!", skinnedMeshRenderer.gameObject);
                    continue;
                }

                AddResult(dataList, mesh, "非碰撞");
            }
        }

        #region 创建数据列

        private static CommonTableColumn<ModelAnalysisData> CreateColumn_ID()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("ID"),
                headerTextAlignment = TextAlignment.Center,
                width = 50,
                minWidth = 50,
                maxWidth = 120,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.id.CompareTo(dataB.id), // 排序
                DrawCell = (cellRect, data) =>
                {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.id.ToString());
                },
            };
        }

        private static CommonTableColumn<ModelAnalysisData> CreateColumn_Name()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Center,
                width = 300,
                minWidth = 300,
                maxWidth = 500,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) =>
                    string.Compare(dataA.assetName, dataB.assetName, StringComparison.Ordinal),
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
            };
        }
        
        private CommonTableColumn<ModelAnalysisData> CreateColumn_Verts()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("Verts"),
                headerTextAlignment = TextAlignment.Center,
                width = 80,
                minWidth = 80,
                maxWidth = 120,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.verts.CompareTo(dataB.verts),
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
            };
        }
        
        private CommonTableColumn<ModelAnalysisData> CreateColumn_Tris()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("Tris"),
                headerTextAlignment = TextAlignment.Center,
                width = 80,
                minWidth = 80,
                maxWidth = 120,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.tris.CompareTo(dataB.tris),
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
            };
        }
        
        private static CommonTableColumn<ModelAnalysisData> CreateColumn_ReadWrite()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("R/W"),
                headerTextAlignment = TextAlignment.Center,
                width = 120,
                minWidth = 120,
                maxWidth = 160,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) =>
                    string.Compare(dataA.readwrite, dataB.readwrite, StringComparison.Ordinal),
                DrawCell = (cellRect, data) =>
                {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.readwrite.ToString());
                },
            };
        }
        
        private static CommonTableColumn<ModelAnalysisData> CreateColumn_UV()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("UV"),
                headerTextAlignment = TextAlignment.Center,
                width = 50,
                minWidth = 50,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.uv.CompareTo(dataB.uv),
                DrawCell = (cellRect, data) =>
                {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.uv.ToString());
                },
            };
        }
        
        private static CommonTableColumn<ModelAnalysisData> CreateColumn_UV2()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("UV2"),
                headerTextAlignment = TextAlignment.Center,
                width = 50,
                minWidth = 50,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.uv2.CompareTo(dataB.uv2),
                DrawCell = (cellRect, data) =>
                {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.uv2.ToString());
                },
            };
        }
        
        private static CommonTableColumn<ModelAnalysisData> CreateColumn_UV3()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("UV3"),
                headerTextAlignment = TextAlignment.Center,
                width = 40,
                minWidth = 40,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.uv3.CompareTo(dataB.uv3),
                DrawCell = (cellRect, data) =>
                {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.uv3.ToString());
                },
            };
        }
        
        private static CommonTableColumn<ModelAnalysisData> CreateColumn_UV4()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("UV4"),
                headerTextAlignment = TextAlignment.Center,
                width = 40,
                minWidth = 40,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.uv4.CompareTo(dataB.uv4),
                DrawCell = (cellRect, data) =>
                {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.uv4.ToString());
                },
            };
        }
        
        private static CommonTableColumn<ModelAnalysisData> CreateColumn_Colors()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("Colors"),
                headerTextAlignment = TextAlignment.Center,
                width = 60,
                minWidth = 60,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.colors.CompareTo(dataB.colors),
                DrawCell = (cellRect, data) =>
                {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.colors.ToString());
                },
            };
        }
        
        private static CommonTableColumn<ModelAnalysisData> CreateColumn_Tangents()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("Tangents"),
                headerTextAlignment = TextAlignment.Center,
                width = 70,
                minWidth = 70,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.tangents.CompareTo(dataB.tangents),
                DrawCell = (cellRect, data) =>
                {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.tangents.ToString());
                },
            };
        }
        
        private static CommonTableColumn<ModelAnalysisData> CreateColumn_Normals()
        {
            return new CommonTableColumn<ModelAnalysisData>
            {
                headerContent = new GUIContent("Normals"),
                headerTextAlignment = TextAlignment.Center,
                width = 70,
                minWidth = 70,
                maxWidth = 80,
                allowToggleVisibility = true,
                canSort = true,
                Compare = (dataA, dataB, sortType) => dataA.normals.CompareTo(dataB.normals),
                DrawCell = (cellRect, data) =>
                {
                    cellRect.height += 5f;
                    cellRect.xMin += 3f;
                    EditorGUI.LabelField(cellRect, data.normals.ToString());
                },
            };
        }

        #endregion
        
        /// <summary>
        /// 初始化列
        /// </summary>
        /// <returns></returns>
        private CommonTableColumn<ModelAnalysisData>[] InitColumns(bool collider)
        {
            var columns = new List<CommonTableColumn<ModelAnalysisData>>
            {
                CreateColumn_ID(),
                CreateColumn_Name(),
                CreateColumn_Verts(),
                CreateColumn_Tris(),
                CreateColumn_UV(),
                CreateColumn_UV2(),
                CreateColumn_UV3(),
                CreateColumn_UV4(),
                CreateColumn_Colors(),
                CreateColumn_Tangents(),
                CreateColumn_Normals()
            };
            
            if (collider)
            {
                columns.Add(CreateColumn_ReadWrite());
            }
            
            return columns.ToArray();
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