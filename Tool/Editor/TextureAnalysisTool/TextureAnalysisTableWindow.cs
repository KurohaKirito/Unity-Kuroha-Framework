﻿using System;
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
        private static TextureAnalysisTableWindow window;
        private const int WARN_ERROR_TEXT_WIDTH = 100;
        private const int WARN_ERROR_TEXT_NUMBER_SPACE = 10;
        private const int WARN_ERROR_NUMBER_WIDTH = 60;

        private static TextureAnalysisTable table;
        private static List<TextureAnalysisData> originList;
        private static List<TextureAnalysisData> distinctList;
        private static CustomTableColumn<TextureAnalysisData>[] columns;

        private static int widthWarn;
        private static int widthError;
        private static int heightWarn;
        private static int heightError;
        private static int memoryWarn;
        private static int memoryError;

        private static bool isFashion;
        private static TextureAnalysisData.DetectType detectType;
        private static TextureAnalysisData.DetectTypeAtPath detectTypeAtPath;
        private static string detectPath;
        private static GameObject detectGameObject;

        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void Open(TextureAnalysisData.DetectType type, TextureAnalysisData.DetectTypeAtPath typeAtPath, string path, GameObject obj, bool fashionDetect) {
            isFashion = fashionDetect;
            detectType = type;
            detectPath = path;
            detectGameObject = obj;
            detectTypeAtPath = typeAtPath;

            if (window == null) {
                window = GetWindow<TextureAnalysisTableWindow>("纹理资源分析", true);
                window.minSize = new Vector2(1200, 1000);
            }

            InitTable(true);
            window.Repaint();
        }

        /// <summary>
        /// 绘制界面
        /// </summary>
        protected void OnGUI() {
            // 顶部留白
            GUILayout.Space(10);

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
        /// 初始化
        /// </summary>
        /// <param name="forceUpdate">是否强制刷新</param>
        private static void InitTable(bool forceUpdate = false) {
            // 初始化界限值
            if (widthWarn == 0) {
                widthWarn = 250;
            }

            if (widthError == 0) {
                widthError = 500;
            }

            if (heightWarn == 0) {
                heightWarn = 250;
            }

            if (heightError == 0) {
                heightError = 500;
            }

            if (memoryWarn == 0) {
                memoryWarn = 512;
            }

            if (memoryError == 0) {
                memoryError = 1024;
            }

            if (forceUpdate || table == null) {
                originList = InitData();
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
        private static void GenerateTable(List<TextureAnalysisData> data, CustomTableColumn<TextureAnalysisData>[] column) {
            var space = new Vector2(10, 10);
            var min = new Vector2(300, 300);
            table = new TextureAnalysisTable(space, min, data, true, true, true, column, OnFilterEnter, null, OnExportPressed, OnRowSelect, OnDistinctPressed);
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
                            TextureUtil.GetTexturesInPath(new[] {
                                texturesPath
                            }, out assets, out assetPaths);
                            break;

                        case TextureAnalysisData.DetectTypeAtPath.Prefabs:
                            var allPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] {
                                texturesPath
                            });
                            var allPath = allPrefabGuids.Select(AssetDatabase.GUIDToAssetPath);
                            var allPrefab = allPath.Select(AssetDatabase.LoadAssetAtPath<GameObject>);

                            foreach (var prefab in allPrefab) {
                                if (prefab.transform.GetComponentsInChildren<FashionEffect>().Length > 0) {
                                    Debug.LogError($"预制体 {prefab} 中挂载有 Fashion Effect 脚本!", prefab);
                                }

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
            if (AssetImporter.GetAtPath(assetPath) is TextureImporter textureImporter) {
                if (assetPath.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) > 0 || assetPath.IndexOf(".png", StringComparison.OrdinalIgnoreCase) > 0 || assetPath.IndexOf(".tga", StringComparison.OrdinalIgnoreCase) > 0 || assetPath.IndexOf(".psd", StringComparison.OrdinalIgnoreCase) > 0 || assetPath.IndexOf(".tif", StringComparison.OrdinalIgnoreCase) > 0) {
                    // 纯色纹理判断
                    if (textureImporter.textureShape == TextureImporterShape.Texture2D && TextureUtil.IsSolidColor(asset)) {
                        isSolid = true;
                    }
                }

                // 重复纹理检测
                var isBegin = counter == 1;
                TextureRepeatChecker.CheckOneTexture(assetPath, isBegin);

                // 计数
                counter++;

                // 统计内存占用
                var memoryLong = TextureUtil.GetTextureStorageMemorySize(asset);

                // 获取压缩格式
                textureImporter.GetPlatformTextureSettings("Android", out var androidSize, out var androidFormat);
                textureImporter.GetPlatformTextureSettings("iPhone", out var iOSSize, out var iOSFormat);
                textureImporter.GetPlatformTextureSettings("Standalone", out var pcSize, out var pcFormat);

                // 汇总数据
                dataList.Add(new TextureAnalysisData {
                    id = counter,
                    width = asset.width,
                    height = asset.height,
                    
                    readable = textureImporter.isReadable,
                    mipMaps = textureImporter.mipmapEnabled,
                    streaming = textureImporter.streamingMipmaps,
                    
                    androidFormat = androidFormat,
                    iOSFormat = iOSFormat,
                    pcFormat = pcFormat,
                    androidFormatSize = androidSize,
                    iOSFormatSize = iOSSize,
                    pcFormatSize = pcSize,
                    
                    hadAlpha = textureImporter.DoesSourceTextureHaveAlpha(),
                    importAlpha = textureImporter.alphaSource,
                    
                    memory = memoryLong / 1024f,
                    isSolid = isSolid,
                    textureName = asset.name,
                    texturePath = assetPath
                });
            }
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
                    maxWidth = 70,
                    allowToggleVisibility = false,
                    canSort = true,
                    autoResize = true,
                    Compare = (dataA, dataB, sortType) => dataA.id.CompareTo(dataB.id), // 排序
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;

                        EditorGUI.LabelField(cellRect, data.id.ToString());
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Asset"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 600,
                    minWidth = 600,
                    maxWidth = 1200,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => string.Compare(dataA.texturePath, dataB.texturePath, StringComparison.Ordinal),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("RawImage Icon"));
                        EditorGUI.LabelField(cellRect, data.texturePath);
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Width"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 70,
                    minWidth = 70,
                    maxWidth = 100,
                    allowToggleVisibility = false,
                    autoResize = true,
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
                    width = 70,
                    minWidth = 70,
                    maxWidth = 100,
                    allowToggleVisibility = false,
                    autoResize = true,
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
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("R/W"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 50,
                    minWidth = 50,
                    maxWidth = 70,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.readable.CompareTo(dataB.readable),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;

                        EditorGUI.LabelField(cellRect, data.readable.ToString());
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Mip Maps"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 70,
                    minWidth = 70,
                    maxWidth = 70,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.mipMaps.CompareTo(dataB.mipMaps),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;

                        EditorGUI.LabelField(cellRect, data.mipMaps.ToString());
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Streaming"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 80,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.streaming.CompareTo(dataB.streaming),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;

                        EditorGUI.LabelField(cellRect, data.streaming.ToString());
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Had Alpha"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 80,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.hadAlpha.CompareTo(dataB.hadAlpha),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        EditorGUI.LabelField(cellRect, data.hadAlpha.ToString());
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Alpha Source"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 100,
                    minWidth = 100,
                    maxWidth = 100,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.importAlpha.CompareTo(dataB.importAlpha),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        EditorGUI.LabelField(cellRect, data.importAlpha.ToString());
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Format : Android"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 175,
                    minWidth = 175,
                    maxWidth = 240,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.androidFormatSize - dataB.androidFormatSize,
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        string iconName;
                        if (data.androidFormat == TextureImporterFormat.ETC2_RGB4 || data.androidFormat == TextureImporterFormat.ETC2_RGBA8) {
                            iconName = "console.infoIcon.sml";
                        } else if (data.androidFormat == TextureImporterFormat.ETC_RGB4) {
                            iconName = "console.warnIcon.sml";
                        } else {
                            iconName = "console.errorIcon.sml";
                        }

                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent(iconName));
                        EditorGUI.LabelField(cellRect, $"{data.androidFormatSize, 4} | {data.androidFormat.ToString()}");
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Format : iOS"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 175,
                    minWidth = 175,
                    maxWidth = 240,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.iOSFormatSize - dataB.iOSFormatSize,
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        string iconName;
                        if (data.iOSFormat == TextureImporterFormat.PVRTC_RGB4 || data.iOSFormat == TextureImporterFormat.PVRTC_RGBA4) {
                            iconName = "console.infoIcon.sml";
                        } else if (data.iOSFormat == TextureImporterFormat.ASTC_RGB_6x6 || data.iOSFormat == TextureImporterFormat.ASTC_RGBA_6x6) {
                            iconName = isFashion? "console.warnIcon.sml" : "console.infoIcon.sml";
                        } else {
                            iconName = "console.errorIcon.sml";
                        }

                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent(iconName));
                        EditorGUI.LabelField(cellRect, $"{data.iOSFormatSize, 4} | {data.iOSFormat.ToString()}");
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Format : PC"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 175,
                    minWidth = 175,
                    maxWidth = 240,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.pcFormatSize - dataB.pcFormatSize,
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        string iconName;
                        if (data.pcFormat == TextureImporterFormat.DXT1 || data.pcFormat == TextureImporterFormat.DXT5) {
                            iconName = "console.infoIcon.sml";
                        } else {
                            iconName = "console.errorIcon.sml";
                        }

                        EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent(iconName));
                        EditorGUI.LabelField(cellRect, $"{data.pcFormatSize, 4} | {data.pcFormat.ToString()}");
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Memory"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 100,
                    minWidth = 100,
                    maxWidth = 120,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.memory.CompareTo(dataB.memory),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;

                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        if (data.memory > memoryError) {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.errorIcon.sml"));
                            EditorGUI.LabelField(cellRect, $"{data.memory:F1} KB");
                        } else if (data.memory > memoryWarn) {
                            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnIcon.sml"));
                            EditorGUI.LabelField(cellRect, $"{data.memory:F1} KB");
                        } else {
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
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => AssetTool.Util.RunTime.StringUtil.CompareByBoolAndStringOfTable(dataA.isSolid, dataB.isSolid, dataA.textureName, dataB.textureName),
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

                            EditorGUI.LabelField(cellRect, "纯色纹理");
                        }
                    }
                },
                new CustomTableColumn<TextureAnalysisData> {
                    headerContent = new GUIContent("Repeat"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => AssetTool.Util.RunTime.StringUtil.CompareByNumberOfTable(dataA.repeatInfo, dataB.repeatInfo),
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
                EditorUtility.DisplayDialog("Warning", "No Data!", "OK");
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

            if (FilterString(0, data.id.ToString()) ||
                FilterString(1, data.textureName) ||
                FilterInt(2, data.width) ||
                FilterInt(3, data.height) ||
                FilterBool(4, data.readable) ||
                FilterBool(5, data.mipMaps) ||
                FilterBool(6, data.streaming) ||
                FilterBool(7, data.hadAlpha) ||
                FilterString(8, data.importAlpha.ToString()) ||
                FilterInt(12, (int)data.memory) ||
                FilterBool(13, data.isSolid)) {
                isMatched = true;
            }

            #region Local Function

            bool FilterString(int index, string str) {
                if (maskChars.Length < index + 1 || maskChars[index] != '1') {
                    return false;
                }
                
                return str.ToLower().Contains(filterText.ToLower());
            }
            
            bool FilterInt(int index, int num) {
                if (maskChars.Length < index + 1 || maskChars[index] != '1') {
                    return false;
                }
                
                if (int.TryParse(filterText, out var filterNum)) {
                    if (num > filterNum) {
                        return true;
                    }
                } else if (num.ToString().ToLower().Contains(filterText.ToLower())) {
                    return true;
                }

                return false;
            }

            bool FilterBool(int index, bool flag) {
                if (maskChars.Length < index + 1 || maskChars[index] != '1') {
                    return false;
                }
                return filterText.ToLower().Equals(flag.ToString());
            }

            #endregion

            return isMatched;
        }

        /// <summary>
        /// 数据去重事件
        /// </summary>
        private static void OnDistinctPressed(List<TextureAnalysisData> dataList) {
            var newList = new List<TextureAnalysisData>();
            foreach (var data in dataList) {
                if (newList.Exists(analysisData => analysisData.IsEqual(data))) {
                    continue;
                }

                newList.Add(data);
            }

            // 重新编号
            for (var index = 0; index < newList.Count; ++index) {
                newList[index].id = index + 1;
            }

            GenerateTable(newList, columns);
        }
    }
}
