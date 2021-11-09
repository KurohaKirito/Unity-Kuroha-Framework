﻿using System;
using Kuroha.Tool.AssetTool.Editor.AssetSearchTool.Data;
using Kuroha.Tool.AssetTool.Editor.AssetSearchTool.Searcher;
using UnityEditor;
using UnityEngine;
using Kuroha.Util.RunTime;

namespace Kuroha.Tool.AssetTool.Editor.AssetSearchTool.GUI
{
    public static class GUIDependenceSearcher
    {
        /// <summary>
        /// 过滤器的默认值都是 -1, 默认为全选
        /// </summary>
        private static int dependenceAssetFilter = -1;

        /// <summary>
        /// 滑动条
        /// </summary>
        private static Vector2 dependenceSearchScrollPosition = Vector2.zero;

        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(UI_DEFAULT_MARGIN);

            #region 显示出当前所有选中的游戏物体

            EditorGUILayout.LabelField("请选择需要查找引用的资源文件.");

            if (Selection.assetGUIDs.IsNotNullAndEmpty())
            {
                // 每 1 行显示物体的数量
                const int COUNT_PER_ROW = 5;
                // 每个物体之间的间隔
                const float ITEM_OFFSET = 5f;

                var index = 0;
                var countAll = Selection.assetGUIDs.Length;
                var windowWidth = AssetSearchWindow.windowCurrentRect.width;
                var objectWidth = (windowWidth - (COUNT_PER_ROW - 1) * ITEM_OFFSET) / COUNT_PER_ROW - ITEM_OFFSET;
                while (index < countAll)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (var i = 0; i < COUNT_PER_ROW && index < countAll; i++, index++)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[index]);
                        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        EditorGUILayout.ObjectField(asset, typeof(UnityEngine.Object), true,
                            GUILayout.Width(objectWidth));
                        if (i != COUNT_PER_ROW - 1)
                        {
                            GUILayout.Space(ITEM_OFFSET);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            #endregion

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            #region Search 按钮 与 过滤器

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("过滤器", GUILayout.Width(100));
                dependenceAssetFilter =
                    EditorGUILayout.MaskField(dependenceAssetFilter, Enum.GetNames(typeof(AssetType)));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Search", GUILayout.Width(100)))
                {
                    DependenceSearcher.FindSelectionDependencies(Selection.assetGUIDs);
                }
            }
            EditorGUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            #region 显示查询结果

            dependenceSearchScrollPosition = EditorGUILayout.BeginScrollView(dependenceSearchScrollPosition);
            {
                foreach (var key in DependenceSearcher.dependencies.Keys)
                {
                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);

                    #region 显示被检查的游戏物体

                    var path = AssetDatabase.GUIDToAssetPath(key);
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    EditorGUILayout.ObjectField(asset, typeof(UnityEngine.Object), true);

                    #endregion

                    // 获取全部的引用
                    var referenceAssets = DependenceSearcher.dependencies[key];

                    // 增加 UI 缩进
                    EditorGUI.indentLevel++;

                    #region 显示 数量 以及 排序按钮

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"引用对象:  共 {referenceAssets.Count} 个");
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("按名称排序", GUILayout.Width(100)))
                    {
                        referenceAssets.Sort((x, y) =>
                        {
                            var xAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(x);
                            var yAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(y);
                            return string.Compare(xAsset.name, yAsset.name, StringComparison.Ordinal);
                        });
                    }

                    if (GUILayout.Button("按类型排序", GUILayout.Width(100)))
                    {
                        referenceAssets.Sort((x, y) =>
                        {
                            var xAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(x);
                            var yAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(y);
                            return AssetData.GetAssetType(xAsset, x).CompareTo(AssetData.GetAssetType(yAsset, y));
                        });
                    }

                    EditorGUILayout.EndHorizontal();

                    #endregion

                    #region 显示引用列表

                    foreach (var assetPath in referenceAssets)
                    {
                        var referenceAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                        if (AssetSearchWindow.IsDisplay(referenceAsset, assetPath, dependenceAssetFilter))
                        {
                            EditorGUILayout.ObjectField(referenceAsset, typeof(UnityEngine.Object), true);
                        }
                    }

                    #endregion

                    // 减少 UI 缩进
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndScrollView();

            #endregion
        }
    }
}