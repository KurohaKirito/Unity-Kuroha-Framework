using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.GUI.Editor.Table;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.TextureAnalysisTool {
    public class TextureAnalysisTableWindow : EditorWindow {
        /// <summary>
        /// 表格
        /// </summary>
        private TextureAnalysisTable table;

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
        /// 内存警告线
        /// </summary>
        private static int memoryWarn;

        /// <summary>
        /// 内存错误线
        /// </summary>
        private static int memoryError;

        private const int WARN_ERROR_TEXT_WIDTH = 100;
        private const int WARN_ERROR_TEXT_NUMBER_SPACE = 10;
        private const int WARN_ERROR_NUMBER_WIDTH = 60;

        /// <summary>
        /// 是否是对场景进行检测
        /// </summary>
        private static TextureAnalysisData.DetectType detectType;
        
        /// <summary>
        /// 对路径中资源检测的类型
        /// </summary>
        private static TextureAnalysisData.DetectTypeAtPath detectTypeAtPath;

        /// <summary>
        /// 待检测路径
        /// </summary>
        private static string detectPath;

        /// <summary>
        /// 待检测游戏物体
        /// </summary>
        private static GameObject detectGameObject;

        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void Open(TextureAnalysisData.DetectType type, TextureAnalysisData.DetectTypeAtPath typeAtPath, string path, GameObject obj) {
            detectType = type;
            detectPath = path;
            detectGameObject = obj;
            detectTypeAtPath = typeAtPath;
            var window = GetWindow<TextureAnalysisTableWindow>("纹理资源分析", true);
            window.minSize = new Vector2(1200, 1000);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable() {
            // 初始化界限值
            if (widthWarn == 0)   { widthWarn = 250; }
            if (widthError == 0)  { widthError = 500; }
            if (heightWarn == 0)  { heightWarn = 250; }
            if (heightError == 0) { heightError = 500; }
            if (memoryWarn == 0)  { memoryWarn = 512; }
            if (memoryError == 0) { memoryError = 1024; }

            // 初始化表格
            InitTable();
        }

        /// <summary>
        /// 绘制界面
        /// </summary>
        protected void OnGUI() {
            // 顶部留白
            GUILayout.Space(20);
            GUILayout.BeginHorizontal("Box");
            {
                // 左侧留白
                GUILayout.Space(20);
                {
                    GUILayout.BeginHorizontal("Box");
                    EditorGUILayout.LabelField("Width Warning", GUILayout.Width(WARN_ERROR_TEXT_WIDTH));
                    GUILayout.Space(WARN_ERROR_TEXT_NUMBER_SPACE);
                    widthWarn = EditorGUILayout.IntField(widthWarn, GUILayout.Width(WARN_ERROR_NUMBER_WIDTH));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginHorizontal("Box");
                    EditorGUILayout.LabelField("Width Error", GUILayout.Width(WARN_ERROR_TEXT_WIDTH));
                    GUILayout.Space(WARN_ERROR_TEXT_NUMBER_SPACE);
                    widthError = EditorGUILayout.IntField(widthError, GUILayout.Width(WARN_ERROR_NUMBER_WIDTH));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginHorizontal("Box");
                    EditorGUILayout.LabelField("Height Warning", GUILayout.Width(WARN_ERROR_TEXT_WIDTH));
                    GUILayout.Space(WARN_ERROR_TEXT_NUMBER_SPACE);
                    heightWarn = EditorGUILayout.IntField(heightWarn, GUILayout.Width(WARN_ERROR_NUMBER_WIDTH));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginHorizontal("Box");
                    EditorGUILayout.LabelField("Height Error", GUILayout.Width(WARN_ERROR_TEXT_WIDTH));
                    GUILayout.Space(WARN_ERROR_TEXT_NUMBER_SPACE);
                    heightError = EditorGUILayout.IntField(heightError, GUILayout.Width(WARN_ERROR_NUMBER_WIDTH));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.FlexibleSpace();
                    
                    GUILayout.BeginHorizontal("Box");
                    EditorGUILayout.LabelField("Memory Warning", GUILayout.Width(WARN_ERROR_TEXT_WIDTH));
                    GUILayout.Space(WARN_ERROR_TEXT_NUMBER_SPACE);
                    memoryWarn = EditorGUILayout.IntField(memoryWarn, GUILayout.Width(WARN_ERROR_NUMBER_WIDTH));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.FlexibleSpace();
                    
                    GUILayout.BeginHorizontal("Box");
                    EditorGUILayout.LabelField("Memory Error", GUILayout.Width(WARN_ERROR_TEXT_WIDTH));
                    GUILayout.Space(WARN_ERROR_TEXT_NUMBER_SPACE);
                    memoryError = EditorGUILayout.IntField(memoryError, GUILayout.Width(WARN_ERROR_NUMBER_WIDTH));
                    GUILayout.EndHorizontal();
                }
                // 右侧留白
                GUILayout.Space(18);
            }
            GUILayout.EndHorizontal();

            table?.OnGUI();
        }

        /// <summary>
        /// 初始化表格
        /// </summary>
        /// <param name="forceUpdate">是否强制刷新</param>
        private void InitTable(bool forceUpdate = false) {
            if (forceUpdate || table == null) {
                var dataList = InitData();
                if (dataList != null) {
                    var columns = InitColumns();
                    if (columns != null) {
                        var space = new Vector2(20, 20);
                        var min = new Vector2(300, 300);
                        table = new TextureAnalysisTable(space, min, dataList, true, true, true, columns, OnFilterEnter, OnExportPressed, OnRowSelect, OnDistinctPressed);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        private static List<TextureAnalysisData> InitData() {
            var dataList = new List<TextureAnalysisData>();
            var counter = 0;

            // 获取全部的纹理
            GetAllTexture(detectType, detectTypeAtPath, detectPath, out var textures, out var paths);

            // 遍历每一张贴图进行检测
            for (var index = 0; index < textures.Count; index++) {
                ProgressBar.DisplayProgressBar("纹理分析工具", $"纹理检测中: {index + 1}/{textures.Count}", index + 1, textures.Count);

                // 判断后缀
                if (paths[index].EndsWith(".png") == false && paths[index].EndsWith(".tga") == false) {
                    DebugUtil.Log($"文件后缀非法: {paths[index]}", AssetDatabase.LoadAssetAtPath<Texture>(paths[index]));
                }

                // 执行检测
                DetectTexture(ref counter, in dataList, paths[index], textures[index]);
            }

            DebugUtil.Log($"共检测了 {counter} 张贴图");

            #region 处理重复纹理的检测结果数据

            var repeatTextureInfos = TextureRepeatChecker.GetResult();
            foreach (var data in dataList) {
                foreach (var repeatTextureInfo in repeatTextureInfos) {
                    foreach (var assetPath in repeatTextureInfo.assetPaths) {
                        if (assetPath == data.texturePath) {
                            data.repeatInfo = $"第 {repeatTextureInfo.id} 组重复";
                        }
                    }
                }
            }

            #endregion

            return dataList;
        }

        /// <summary>
        /// 获取全部的纹理
        /// </summary>
        private static void GetAllTexture(TextureAnalysisData.DetectType type, TextureAnalysisData.DetectTypeAtPath typeAtPath, string texturesPath, out List<Texture> assets, out List<string> assetPaths) {
            assets = new List<Texture>();
            assetPaths = new List<string>();

            switch (type) {
                case TextureAnalysisData.DetectType.Scene:
                    TextureUtil.GetTexturesInScene(out assets, out assetPaths);
                    break;

                case TextureAnalysisData.DetectType.Path:
                    switch (typeAtPath) {
                        case TextureAnalysisData.DetectTypeAtPath.Textures:
                            TextureUtil.GetTexturesInPath(new[] { texturesPath }, out assets, out assetPaths);
                            break;
                        
                        case TextureAnalysisData.DetectTypeAtPath.Prefabs:
                            var allPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { texturesPath });
                            var allPath = allPrefabGuids.Select(AssetDatabase.GUIDToAssetPath);
                            var allPrefab = allPath.Select(AssetDatabase.LoadAssetAtPath<GameObject>);

                            foreach (var prefab in allPrefab) {
                                TextureUtil.GetTexturesInGameObject(prefab, out var assetsNew, out var assetPathsNew);
                                assets.AddRange(assetsNew);
                                assetPaths.AddRange(assetPathsNew);
                            }
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException(nameof(typeAtPath), typeAtPath, null);
                    }
                    break;

                case TextureAnalysisData.DetectType.GameObject:
                    TextureUtil.GetTexturesInGameObject(detectGameObject, out assets, out assetPaths);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// 检测单张贴图
        /// </summary>
        /// <param name="counter">序号</param>
        /// <param name="dataList">表格数据</param>
        /// <param name="assetPath">贴图路径</param>
        /// <param name="asset">贴图资源</param>
        private static void DetectTexture(ref int counter, in List<TextureAnalysisData> dataList, in string assetPath, in Texture asset) {
            // 去重
            var isHad = false;
            foreach (var data in dataList) {
                if (data.texturePath == assetPath) {
                    isHad = true;
                }
            }
            if (isHad) {
                return;
            }
            
            // 判断是否可以进行特殊检测
            var isSolid = false;
            if (assetPath.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) > 0 ||
                assetPath.IndexOf(".png", StringComparison.OrdinalIgnoreCase) > 0 ||
                assetPath.IndexOf(".tga", StringComparison.OrdinalIgnoreCase) > 0 ||
                assetPath.IndexOf(".psd", StringComparison.OrdinalIgnoreCase) > 0 ||
                assetPath.IndexOf(".tif", StringComparison.OrdinalIgnoreCase) > 0) {
                
                // 纯色纹理判断
                var textureImporter = (TextureImporter)AssetImporter.GetAtPath(assetPath);
                if (!ReferenceEquals(textureImporter, null)) {
                    if (textureImporter.textureShape == TextureImporterShape.Texture2D && TextureUtil.IsSolidColor(asset)) {
                        isSolid = true;
                    }
                }

                // 重复纹理检测
                var isBegin = counter == 1;
                TextureRepeatChecker.CheckOneTexture(assetPath, isBegin);
            }

            // 计数
            counter++;
            
            // 统计内存占用
            var memoryLong = TextureUtil.GetTextureStorageMemorySize(asset);
            
            // 汇总数据
            dataList.Add(new TextureAnalysisData {
                id = counter,
                width = asset.width,
                height = asset.height,
                memory = memoryLong / 1024f,
                isSolid = isSolid,
                textureName = asset.name,
                texturePath = assetPath
            });
        }

        /// <summary>
        /// 初始化列
        /// </summary>
        /// <returns></returns>
        private static CustomTableColumn<TextureAnalysisData>[] InitColumns() {
            return new[] {
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("ID"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 50,
                    minWidth = 50,
                    maxWidth = 120,
                    allowToggleVisibility = true,
                    canSort = true,
                    autoResize = false,
                    Compare = (dataA, dataB, sortType) => dataA.id.CompareTo(dataB.id), // 排序
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;

                        EditorGUI.LabelField(cellRect, data.id.ToString());
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 240,
                    minWidth = 240,
                    maxWidth = 500,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => string.Compare(dataA.textureName, dataB.textureName, StringComparison.Ordinal),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("RawImage Icon"));
                        EditorGUI.LabelField(cellRect, data.textureName.Contains("/")? data.textureName.Split('/').Last() : data.textureName.Split('\\').Last());
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Width"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.width.CompareTo(dataB.width),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        if (data.width > widthError) {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.errorIcon.sml"));
                            EditorGUI.LabelField(cellRect, data.width.ToString());
                        } else if (data.width > widthWarn) {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnIcon.sml"));
                            EditorGUI.LabelField(cellRect, data.width.ToString());
                        } else {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.infoIcon.sml"));
                            EditorGUI.LabelField(cellRect, data.width.ToString());
                        }
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Height"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.height.CompareTo(dataB.height),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        if (data.height > heightError) {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.errorIcon.sml"));
                            EditorGUI.LabelField(cellRect, data.height.ToString());
                        } else if (data.height > heightWarn) {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnIcon.sml"));
                            EditorGUI.LabelField(cellRect, data.height.ToString());
                        } else {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.infoIcon.sml"));
                            EditorGUI.LabelField(cellRect, data.height.ToString());
                        }
                    }
                },
                new CustomTableColumn<TextureAnalysisData>
                {
                    headerContent = new GUIContent("Memory"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 100,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.memory.CompareTo(dataB.memory),
                    DrawCell = (cellRect, data) =>
                    {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        if (data.memory > memoryError)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.errorIcon.sml"));
                            EditorGUI.LabelField(cellRect, $"{data.memory:F1} KB");
                        }
                        else if (data.memory > memoryWarn)
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnIcon.sml"));
                            EditorGUI.LabelField(cellRect, $"{data.memory:F1} KB");
                        }
                        else
                        {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.infoIcon.sml"));
                            EditorGUI.LabelField(cellRect, $"{data.memory:F1} KB");
                        }
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Solid"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 140,
                    allowToggleVisibility = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => AssetTool.Util.RunTime.StringUtil.CompareByBoolAndString(dataA.isSolid, dataB.isSolid, dataA.textureName, dataB.textureName, sortType),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        if (data.isSolid) {
                            if (data.width > 32 && data.height > 32) {
                                EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.errorIcon.sml"));
                            } else {
                                EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("d_FilterSelectedOnly"));
                            }
                            UnityEngine.GUI.Label(cellRect, "纯色纹理");
                        }
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Repeat"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 400,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => AssetTool.Util.RunTime.StringUtil.CompareByNumber(dataA.repeatInfo, dataB.repeatInfo, sortType),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;

                        if (!string.IsNullOrEmpty(data.repeatInfo)) {
                            EditorGUI.LabelField(cellRect, data.repeatInfo);
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 行选中事件
        /// </summary>
        /// <param name="dataList"></param>
        private static void OnRowSelect(in List<TextureAnalysisData> dataList) {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dataList[0].texturePath);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }

        /// <summary>
        /// 导出按钮事件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dataList"></param>
        private static void OnExportPressed(string file, in List<TextureAnalysisData> dataList) {
            if (dataList.Count <= 0) {
                EditorUtility.DisplayDialog("Warning", "No Data!", "Ok");
                return;
            }

            if (File.Exists(file)) {
                File.Delete(file);
            }

            foreach (var data in dataList) {
                File.AppendAllText(file, $"{data.id}\t{data.textureName}\t{data.width}\t{data.height}\n");
            }
        }

        /// <summary>
        /// 查找按钮事件
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="data"></param>
        /// <param name="filterText"></param>
        /// <returns></returns>
        private static bool OnFilterEnter(int mask, TextureAnalysisData data, string filterText) {
            var isMatched = false;
            var maskChars = Convert.ToString(mask, 2).Reverse().ToArray();

            if (ColumnFilter1() || ColumnFilter2() || ColumnFilter3() || ColumnFilter4() || ColumnFilter5() || ColumnFilter6()) {
                isMatched = true;
            }

            #region Local Function

            bool ColumnFilter1() {
                if (maskChars.Length < 1 || maskChars[0] != '1') {
                    return false;
                }

                return data.id.ToString().ToLower().Contains(filterText.ToLower());
            }

            bool ColumnFilter2() {
                if (maskChars.Length < 2 || maskChars[1] != '1') {
                    return false;
                }

                return data.textureName.ToLower().Contains(filterText.ToLower());
            }

            bool ColumnFilter3() {
                if (maskChars.Length < 3 || maskChars[2] != '1') {
                    return false;
                }

                if (int.TryParse(filterText, out var verts)) {
                    if (data.width > verts) {
                        return true;
                    }
                } else if (data.width.ToString().ToLower().Contains(filterText.ToLower())) {
                    return true;
                }

                return false;
            }

            bool ColumnFilter4() {
                if (maskChars.Length < 4 || maskChars[3] != '1') {
                    return false;
                }

                if (int.TryParse(filterText, out int tris)) {
                    if (data.height > tris) {
                        return true;
                    }
                } else if (data.height.ToString().ToLower().Contains(filterText.ToLower())) {
                    return true;
                }

                return false;
            }

            bool ColumnFilter5() {
                if (maskChars.Length < 5 || maskChars[4] != '1') {
                    return false;
                }

                return filterText.ToLower().Contains('纯') && data.isSolid;
            }

            bool ColumnFilter6() {
                if (maskChars.Length < 6 || maskChars[5] != '1') {
                    return false;
                }

                if (string.IsNullOrEmpty(data.repeatInfo)) {
                    data.repeatInfo = string.Empty;
                }

                return data.repeatInfo.ToLower().Contains(filterText.ToLower());
            }

            #endregion

            return isMatched;
        }

        /// <summary>
        /// 数据去重事件
        /// </summary>
        private static void OnDistinctPressed(ref List<TextureAnalysisData> dataList) {
            var newList = new List<TextureAnalysisData>();
            foreach (var data in dataList) {
                if (newList.Exists(analysisData => analysisData.Equal(data)) == false) {
                    newList.Add(data);
                }
            }

            dataList = newList;

            // 重新编号
            for (var index = 0; index < dataList.Count; ++index) {
                dataList[index].id = index + 1;
            }
        }
    }
}