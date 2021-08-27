using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuroha.GUI.Editor.Table;
using Kuroha.Util.Editor;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.FashionAnalysisTool
{
    public class FashionAnalysisTableWindow : EditorWindow
    {
        private FashionAnalysisTable table;

        private GUIStyle fontStyleRed;
        private GUIStyle fontStyleYellow;

        /// <summary>
        /// 宽度警告线
        /// </summary>
        private static int widthWarn;

        /// <summary>
        /// 宽度错误线
        /// </summary>
        private static int widthError;

        /// <summary>
        /// 高度警告线
        /// </summary>
        private static int heightWarn;

        /// <summary>
        /// 高度错误线
        /// </summary>
        private static int heightError;

        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void Open()
        {
            var window = GetWindow<FashionAnalysisTableWindow>(true);
            window.minSize = new Vector2(1200, 1000);
            window.maxSize = window.minSize;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            // 初始化界限值
            if (widthWarn == 0)
            {
                widthWarn = 500;
            }

            if (widthError == 0)
            {
                widthError = 1000;
            }

            if (heightWarn == 0)
            {
                heightWarn = 500;
            }

            if (heightError == 0)
            {
                heightError = 1000;
            }

            // 初始化字体风格
            fontStyleRed = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                normal = {textColor = new Color((float) 203 / 255, (float) 27 / 255, (float) 69 / 255)}
            };

            fontStyleYellow = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                normal = {textColor = new Color((float) 226 / 255, (float) 148 / 255, (float) 59 / 255)}
            };
            
            // 初始化表格
            InitTable();
        }

        /// <summary>
        /// 初始化表格
        /// </summary>
        /// <param name="forceUpdate">是否强制刷新</param>
        private void InitTable(bool forceUpdate = false)
        {
            if (forceUpdate || table == null)
            {
                var dataList = InitData();
                if (dataList != null)
                {
                    var columns = InitColumns();
                    if (columns != null)
                    {
                        var space = new Vector2(20, 20);
                        var min = new Vector2(300, 300);
                        table = new FashionAnalysisTable(
                            space, min, dataList, 
                            true, true, 50, 50, columns,
                            OnFilterEnter, OnExportPressed, OnRowSelect);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        private static List<FashionAnalysisData> InitData()
        {
            var idCounter = 0;
            var dataList = new List<FashionAnalysisData>();
            CollectParticleSystemMeshInfo(ref idCounter, dataList);
            CollectMeshFilterMeshInfo(ref idCounter, dataList);
            CollectSkinnedMeshRendererMeshInfo(ref idCounter, dataList);
            
            Debug.LogError($"共检测出了 {dataList.Count} 条数据");

            #region 获取全部的纹理并检测

            // for (var index = 0; index < textures.Count; index++)
            // {
            //     ProgressBar.DisplayProgressBar("Texture", $"纹理检测中: {index + 1}/{textures.Count}", index + 1, textures.Count);
            //
            //     // 判断后缀
            //     if (paths[index].EndsWith(".png") || paths[index].EndsWith(".tga"))
            //     {
            //         DetectTexture(ref counter, in dataList, paths[index], textures[index]);
            //     }
            //     else
            //     {
            //         DebugUtil.Log($"文件后缀非法: {paths[index]}", AssetDatabase.LoadAssetAtPath<Texture>(paths[index]));
            //     }
            // }

            #endregion

            return dataList;
        }

        #region 收集 Mesh 信息

        private static void CollectParticleSystemMeshInfo(ref int id, in List<FashionAnalysisData> dataList)
        {
            var player1 = GameObject.Find("Player1").transform;
            var roleBox1 = player1.GetChild(0);
            var model1 = roleBox1.GetChild(0);
            var role1 = model1.GetChild(0);
            
            var particles = role1.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var particleSystem in particles)
            {
                var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                if (renderer.renderMode == ParticleSystemRenderMode.Mesh)
                {
                    var mesh = renderer.mesh;
                    dataList.Add(new FashionAnalysisData {
                        id = ++id,
                        tris = mesh.triangles.Length / 3,
                        verts = mesh.vertices.Length,
                        uv2 = mesh.uv2.Length,
                        uv3 = mesh.uv3.Length,
                        uv4 = mesh.uv4.Length,
                        colors = mesh.colors.Length,
                        meshName = mesh.name,
                        meshPath = AssetDatabase.GetAssetPath(mesh)
                    });
                }
            }
        }
        
        private static void CollectMeshFilterMeshInfo(ref int id, in List<FashionAnalysisData> dataList) {
            
        }
        
        private static void CollectSkinnedMeshRendererMeshInfo(ref int id, in List<FashionAnalysisData> dataList) {
            
        }

        #endregion

        private static void CopyToFourRole()
        {
            // 获取所有的角色父物体 Player1 Player2 Player3 Player4
            var player1 = GameObject.Find("Player1");
            var player2 = GameObject.Find("Player1");
            var player3 = GameObject.Find("Player1");
            var player4 = GameObject.Find("Player1");
            
            var roleBox1 = player1.transform.GetChild(0);
            var roleBox2 = player2.transform.GetChild(0);
            var roleBox3 = player3.transform.GetChild(0);
            var roleBox4 = player4.transform.GetChild(0);
            
            var model1 = roleBox1.transform.GetChild(0);
        }

        private static void CollectParticleSystemInfo()
        {
            // var animators = role1.GetComponentsInChildren<Animator>(true);
            // var animatorList = new List<Animator>(animators);
            // animatorList.RemoveAt(0);
            // DebugUtil.Log($"共检测了 {animatorList.Count} 个动画");
            // foreach (var animator in animatorList) {
            //     DebugUtil.LogError($"{animator.name}", animator.transform);
            // }

            // var renderers = role1.GetComponentsInChildren<Renderer>(true);
            // var rendererList = new List<Renderer>(renderers);
            // DebugUtil.Log($"共检测了 {rendererList.Count} 个渲染器");
            // foreach (var renderer in rendererList) {
            //     DebugUtil.LogError($"{renderer.name}", renderer.transform);
            // }
        }

        /// <summary>
        /// 获取全部的纹理
        /// </summary>
        /// <param name="texturesPath">纹理贴图所在的路径</param>
        /// <param name="assets">返回的资源</param>
        /// <param name="assetPaths">返回的资源路径</param>
        /// <returns></returns>
        private static void GetAllTexture(string texturesPath, out List<Texture> assets, out List<string> assetPaths)
        {
            assets = new List<Texture>();
            assetPaths = new List<string>();

            TextureUtil.GetTexturesInScene(out assets, out assetPaths);
        }

        /// <summary>
        /// 检测单张贴图
        /// </summary>
        /// <param name="counter">序号</param>
        /// <param name="dataList">表格数据</param>
        /// <param name="assetPath">贴图路径</param>
        /// <param name="asset">贴图资源</param>
        private static void DetectTexture(ref int counter, in List<FashionAnalysisData> dataList, in string assetPath, in Texture asset)
        {
            var isHad = false;
            foreach (var data in dataList)
            {
                if (data.meshPath == assetPath)
                {
                    isHad = true;
                }
            }

            if (isHad)
            {
                return;
            }

            // 计数
            counter++;

            // 纯色纹理判断
            var isSolid = false;
            var textureImporter = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (!ReferenceEquals(textureImporter, null))
            {
                if (textureImporter.textureShape == TextureImporterShape.Texture2D && TextureUtil.IsSolidColor(asset))
                {
                    isSolid = true;
                }
            }

            // 汇总数据
            // dataList.Add(new FashionAnalysisData
            // {
            //     id = counter,
            //     width = asset.width,
            //     height = asset.height,
            //     isSolid = isSolid,
            //     textureName = asset.name,
            //     texturePath = assetPath
            // });
        }

        /// <summary>
        /// 初始化列
        /// </summary>
        /// <returns></returns>
        private CommonTableColumn<FashionAnalysisData>[] InitColumns()
        {
            return new[]
            {
                new CommonTableColumn<FashionAnalysisData>
                {
                    headerContent = new GUIContent("ID"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 50,
                    minWidth = 50,
                    maxWidth = 120,
                    allowToggleVisibility = true,
                    canSort = true,
                    autoResize = false,
                    Compare = (dataA, dataB, sortType) => dataA.id.CompareTo(dataB.id), // 排序
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;

                        EditorGUI.LabelField(cellRect, data.id.ToString());
                    }
                },

                new CommonTableColumn<FashionAnalysisData>
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 240,
                    minWidth = 240,
                    maxWidth = 500,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) =>
                        string.Compare(dataA.meshName, dataB.meshName, StringComparison.Ordinal),
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("RawImage Icon"));
                        EditorGUI.LabelField(cellRect, data.meshName.Contains("/")
                            ? data.meshName.Split('/').Last()
                            : data.meshName.Split('\\').Last());
                    }
                },

                /*
                
                new CommonTableColumn<FashionAnalysisData>
                {
                    headerContent = new GUIContent("Width"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.width.CompareTo(dataB.width),
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        if (data.width > widthError)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.erroricon.sml"));
                            EditorGUI.LabelField(cellRect, data.width.ToString(), fontStyleRed);
                        }
                        else if (data.width > widthWarn)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnicon.sml"));
                            EditorGUI.LabelField(cellRect, data.width.ToString(), fontStyleYellow);
                        }
                        else
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.infoicon.sml"));
                            EditorGUI.LabelField(cellRect, data.width.ToString());
                        }
                    }
                },

                new CommonTableColumn<FashionAnalysisData>
                {
                    headerContent = new GUIContent("Height"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.height.CompareTo(dataB.height),
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        if (data.height > heightError)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.erroricon.sml"));
                            EditorGUI.LabelField(cellRect, data.height.ToString(), fontStyleRed);
                        }
                        else if (data.height > heightWarn)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnicon.sml"));
                            EditorGUI.LabelField(cellRect, data.height.ToString(), fontStyleYellow);
                        }
                        else
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.infoicon.sml"));
                            EditorGUI.LabelField(cellRect, data.height.ToString());
                        }
                    }
                },

                new CommonTableColumn<FashionAnalysisData>
                {
                    headerContent = new GUIContent("Solid"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 140,
                    allowToggleVisibility = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => Kuroha.Util.Release.StringUtil.CompareByBoolAndString(
                        dataA.isSolid, dataB.isSolid, dataA.textureName, dataB.textureName, sortType),
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        if (data.isSolid)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("d_FilterSelectedOnly"));
                            EditorGUI.LabelField(cellRect, "纯色纹理", fontStyleRed);
                        }
                    }
                },

                new CommonTableColumn<FashionAnalysisData>
                {
                    headerContent = new GUIContent("Repeat"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 300,
                    minWidth = 80,
                    maxWidth = 400,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => Kuroha.Util.Release.StringUtil.CompareByNumber(
                        dataA.repeatInfo, dataB.repeatInfo, sortType),
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;

                        if (!string.IsNullOrEmpty(data.repeatInfo))
                        {
                            EditorGUI.LabelField(cellRect, data.repeatInfo);
                        }
                    }
                }
                
                */
            };
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
            widthWarn = EditorGUILayout.IntField("Enter Width Warning Line", widthWarn, GUILayout.Width(200));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical("Box");
            widthError = EditorGUILayout.IntField("Enter Width Error Line", widthError, GUILayout.Width(200));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical("Box");
            heightWarn = EditorGUILayout.IntField("Enter Height Warning Line", heightWarn, GUILayout.Width(200));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical("Box");
            heightError = EditorGUILayout.IntField("Enter Tris Error Line", heightError, GUILayout.Width(200));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();

            //table?.OnGUI();
        }

        /// <summary>
        /// 行选中事件
        /// </summary>
        /// <param name="dataList"></param>
        private static void OnRowSelect(in List<FashionAnalysisData> dataList)
        {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dataList[0].meshPath);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }

        /// <summary>
        /// 导出按钮事件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dataList"></param>
        private static void OnExportPressed(string file, in List<FashionAnalysisData> dataList)
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
                File.AppendAllText(file, $"{data.id}\t{data.meshName}\t{data.tris}\t{data.verts}\n");
            }
        }

        /// <summary>
        /// 查找按钮事件
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="data"></param>
        /// <param name="filterText"></param>
        /// <returns></returns>
        private static bool OnFilterEnter(int mask, FashionAnalysisData data, string filterText)
        {
            var isMatched = false;
            var maskChars = Convert.ToString(mask, 2).Reverse().ToArray();

            // if (ColumnFilter1() || ColumnFilter2() || ColumnFilter3() || ColumnFilter4() || ColumnFilter5() || ColumnFilter6())
            // {
            //     isMatched = true;
            // }

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

                return data.meshName.ToLower().Contains(filterText.ToLower());
            }

            bool ColumnFilter3()
            {
                if (maskChars.Length < 3 || maskChars[2] != '1')
                {
                    return false;
                }

                if (int.TryParse(filterText, out var verts))
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

            /*
            bool ColumnFilter5()
            {
                if (maskChars.Length < 5 || maskChars[4] != '1')
                {
                    return false;
                }

                return filterText.ToLower().Contains('纯') && data.isSolid;
            }

            bool ColumnFilter6()
            {
                if (maskChars.Length < 6 || maskChars[5] != '1')
                {
                    return false;
                }

                if (string.IsNullOrEmpty(data.repeatInfo))
                {
                    data.repeatInfo = string.Empty;
                }

                return data.repeatInfo.ToLower().Contains(filterText.ToLower());
            }

            */
            #endregion

            return isMatched;
        }
    }
}