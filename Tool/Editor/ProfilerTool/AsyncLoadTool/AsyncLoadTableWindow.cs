﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Script.Effect.Editor.AssetTool.GUI.Editor.Table;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.ProfilerTool.AsyncLoadTool {
    public class AsyncLoadTableWindow : EditorWindow {
        private AsyncLoadTable table;
        private static string asyncLoadRecordPath;

        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void Open(ref string path) {
            asyncLoadRecordPath = path;
            var window = GetWindow<AsyncLoadTableWindow>("统计资源包加载时长", true);
            window.minSize = new Vector2(1200, 1000);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable() {
            InitTable();
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
                        table = new AsyncLoadTable(space, min, dataList, true, true, true, columns,
                            OnFilterEnter, null, null, null);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        private static List<AsyncLoadData> InitData() {
            var counter = 0;
            var results = new List<AsyncLoadData>();
            var dataList = File.ReadAllLines(asyncLoadRecordPath);

            foreach (var data in dataList) {
                var allData = data.Split(';');
                var bundlePathIndex = allData[0].IndexOf("assets/AssetBundle/", StringComparison.Ordinal);
                var bundlePath = allData[0].Replace('\\', '/').Substring(bundlePathIndex);
                var startTime = allData[1].Substring(11, 8);
                var endTime = allData[2].Substring(11, 8);
                var useTime = float.Parse(allData[3].Substring(8)) * 1000;
                results.Add(new AsyncLoadData {
                    id = counter++,
                    bundlePath = bundlePath,
                    startTime = startTime,
                    endTime = endTime,
                    useTime = useTime
                });
            }

            return results;
        }

        /// <summary>
        /// 初始化列
        /// </summary>
        /// <returns></returns>
        private static CustomTableColumn<AsyncLoadData>[] InitColumns() {
            return new[] {
                new CustomTableColumn<AsyncLoadData> {
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
                new CustomTableColumn<AsyncLoadData> {
                    headerContent = new GUIContent("Bundle"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 600,
                    minWidth = 240,
                    maxWidth = 700,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => string.Compare(dataA.bundlePath, dataB.bundlePath, StringComparison.Ordinal),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        var iconRect = cellRect;
                        iconRect.width = 20f;
                        cellRect.xMin += 20f;

                        EditorGUI.LabelField(iconRect, new GUIContent(AssetDatabase.GetCachedIcon(data.bundlePath)));
                        EditorGUI.LabelField(cellRect, data.bundlePath);
                    }
                },
                new CustomTableColumn<AsyncLoadData> {
                    headerContent = new GUIContent("StartTime"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => string.Compare(dataA.startTime, dataB.startTime, StringComparison.Ordinal),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        EditorGUI.LabelField(cellRect, data.startTime);
                    }
                },
                new CustomTableColumn<AsyncLoadData> {
                    headerContent = new GUIContent("EndTime"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => string.Compare(dataA.endTime, dataB.endTime, StringComparison.Ordinal),
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        EditorGUI.LabelField(cellRect, data.endTime);
                    }
                },
                new CustomTableColumn<AsyncLoadData> {
                    headerContent = new GUIContent("UseTime (ms)"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 120,
                    minWidth = 80,
                    maxWidth = 140,
                    allowToggleVisibility = true,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => dataA.useTime.CompareTo(dataB.useTime), // 排序
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 30f;
                        EditorGUI.LabelField(cellRect, data.useTime.ToString("F1"));
                    }
                },
                new CustomTableColumn<AsyncLoadData> {
                    headerContent = new GUIContent("Stack"),
                    headerTextAlignment = TextAlignment.Center,
                    width = 300,
                    minWidth = 80,
                    maxWidth = 400,
                    allowToggleVisibility = false,
                    autoResize = false,
                    canSort = true,
                    Compare = (dataA, dataB, sortType) => 1,
                    DrawCell = (cellRect, data) => {
                        cellRect.height += 5f;
                        cellRect.xMin += 3f;
                        EditorGUI.LabelField(cellRect, data.stackInfo);
                    }
                }
            };
        }

        /// <summary>
        /// 绘制界面
        /// </summary>
        protected void OnGUI() {
            table?.OnGUI();
        }

        /// <summary>
        /// 查找按钮事件
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="data"></param>
        /// <param name="filterText"></param>
        /// <returns></returns>
        private static bool OnFilterEnter(int mask, AsyncLoadData data, string filterText) {
            var isMatched = false;
            var maskChars = Convert.ToString(mask, 2).Reverse().ToArray();

            if (ColumnFilter1() || ColumnFilter2() || ColumnFilter5()) {
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

                return data.bundlePath.ToLower().Contains(filterText.ToLower());
            }

            bool ColumnFilter5() {
                if (maskChars.Length < 5 || maskChars[4] != '1') {
                    return false;
                }

                if (float.TryParse(filterText, out var tris)) {
                    if (data.useTime > tris) {
                        return true;
                    }
                }

                return false;
            }

            #endregion

            return isMatched;
        }
    }
}