using System;
using Kuroha.Tool.AssetTool.Editor.AssetSearchTool.Data;
using Kuroha.Tool.AssetTool.Editor.AssetSearchTool.Searcher;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.AssetSearchTool.GUI
{
    public static class GUIDependenceSearcher
    {
        /// <summary>
        /// 公共依赖分析
        /// </summary>
        private static bool isPublicDependenceSearch;
        
        /// <summary>
        /// 过滤器的默认值都是 -1, 默认为全选
        /// </summary>
        private static int dependenceAssetFilter = -1;
        
        /// <summary>
        /// 公共依赖名称过滤
        /// </summary>
        private static string publicDependenceFilterName = "^@";
        
        /// <summary>
        /// 公共依赖数量过滤
        /// </summary>
        private static string publicDependenceFilterCount = ">1";

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

            EditorGUILayout.LabelField("请选择需要查找引用的资源文件.");

            if (Selection.assetGUIDs != null)
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

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            #region Search 按钮 与 过滤器

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("筛选引用对象", GUILayout.Width(100));
                dependenceAssetFilter = EditorGUILayout.MaskField(dependenceAssetFilter, Enum.GetNames(typeof(AssetType)));

                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("搜索依赖", GUILayout.Width(75)))
                {
                    isPublicDependenceSearch = false;
                    DependenceSearcher.FindSelectionDependencies(Selection.assetGUIDs);
                }
                
                if (GUILayout.Button("公共依赖分析", GUILayout.Width(100)))
                {
                    isPublicDependenceSearch = true;
                    DependenceSearcher.FindPublicDependencies();
                }
            }
            EditorGUILayout.EndHorizontal();

            #endregion
            
            GUILayout.Space(UI_DEFAULT_MARGIN / 2);
            
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("筛选物体-名称", GUILayout.Width(100));
                publicDependenceFilterName = EditorGUILayout.TextField(publicDependenceFilterName);
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(UI_DEFAULT_MARGIN / 2);
            
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("筛选物体-数量", GUILayout.Width(100));
                publicDependenceFilterCount = EditorGUILayout.TextField(publicDependenceFilterCount);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(UI_DEFAULT_MARGIN);

            dependenceSearchScrollPosition = EditorGUILayout.BeginScrollView(dependenceSearchScrollPosition);
            {
                if (isPublicDependenceSearch)
                {
                    GUIPublicDependence();
                }
                else
                {
                    GUIDependence();
                }
            }
            EditorGUILayout.EndScrollView();

            void GUIDependence()
            {
                foreach (var key in DependenceSearcher.dependencies.Keys)
                {
                    var path = AssetDatabase.GUIDToAssetPath(key);
                    var keyAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    
                    if (string.IsNullOrEmpty(publicDependenceFilterName) == false)
                    {
                        if (publicDependenceFilterName.StartsWith("^"))
                        {
                            var filter = publicDependenceFilterName.Substring(1);
                            if (string.IsNullOrEmpty(filter) == false && keyAsset.name.Contains(filter))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (keyAsset.name.Contains(publicDependenceFilterName) == false)
                            {
                                continue;
                            }
                        }
                    }
                    
                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);
                    
                    EditorGUILayout.ObjectField(keyAsset, typeof(UnityEngine.Object), true);

                    // 获取全部的引用
                    var referenceAssets = DependenceSearcher.dependencies[key];

                    // 增加 UI 缩进
                    EditorGUI.indentLevel++;

                    #region 显示 数量 以及 排序按钮

                    EditorGUILayout.BeginHorizontal();
                    
                    if (referenceAssets.Count <= 0)
                    {
                        var oldColor = UnityEngine.GUI.color;
                        UnityEngine.GUI.color = Color.red;
                        GUILayout.Label($"引用对象:  共 {referenceAssets.Count} 个");
                        UnityEngine.GUI.color = oldColor;
                    }
                    else
                    {
                        GUILayout.Label($"引用对象:  共 {referenceAssets.Count} 个");
                    }
                    
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

            void GUIPublicDependence()
            {
                foreach (var keyAsset in DependenceSearcher.publicDependencies.Keys)
                {
                    if (string.IsNullOrEmpty(publicDependenceFilterName) == false)
                    {
                        if (publicDependenceFilterName.StartsWith("^"))
                        {
                            var filter = publicDependenceFilterName.Substring(1);
                            if (string.IsNullOrEmpty(filter) == false && keyAsset.name.Contains(filter))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (keyAsset.name.Contains(publicDependenceFilterName) == false)
                            {
                                continue;
                            }
                        }
                    }
                    
                    if (string.IsNullOrEmpty(publicDependenceFilterCount) == false)
                    {
                        if (publicDependenceFilterCount.StartsWith(">"))
                        {
                            var filter = publicDependenceFilterCount.Substring(1);
                            if (int.TryParse(filter, out var result))
                            {
                                if (DependenceSearcher.publicDependencies[keyAsset].Count <= result)
                                {
                                    continue;
                                }
                            }
                        }
                        else if (publicDependenceFilterCount.StartsWith("<"))
                        {
                            var filter = publicDependenceFilterCount.Substring(1);
                            if (int.TryParse(filter, out var result))
                            {
                                if (DependenceSearcher.publicDependencies[keyAsset].Count >= result)
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (int.TryParse(publicDependenceFilterCount, out var result))
                            {
                                if (DependenceSearcher.publicDependencies[keyAsset].Count != result)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    
                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);
                    
                    EditorGUILayout.ObjectField(keyAsset, typeof(UnityEngine.Object), true);

                    // 获取全部的引用
                    var referenceAssets = DependenceSearcher.publicDependencies[keyAsset];

                    // 增加 UI 缩进
                    EditorGUI.indentLevel++;

                    #region 显示 数量 以及 排序按钮

                    EditorGUILayout.BeginHorizontal();
                    if (referenceAssets.Count > 1)
                    {
                        var oldColor = UnityEngine.GUI.color;
                        UnityEngine.GUI.color = Color.yellow;
                        GUILayout.Label($"引用对象:  共 {referenceAssets.Count} 个");
                        UnityEngine.GUI.color = oldColor;
                    }
                    else
                    {
                        GUILayout.Label($"引用对象:  共 {referenceAssets.Count} 个");
                    }
                    
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("按名称排序", GUILayout.Width(100)))
                    {
                        referenceAssets.Sort((x, y) => string.Compare(x.name, y.name, StringComparison.Ordinal));
                    }

                    if (GUILayout.Button("按类型排序", GUILayout.Width(100)))
                    {
                        referenceAssets.Sort((x, y) =>
                        {
                            var xPath = AssetDatabase.GetAssetPath(x);
                            var yPath = AssetDatabase.GetAssetPath(y);
                            return AssetData.GetAssetType(x, xPath).CompareTo(AssetData.GetAssetType(y, yPath));
                        });
                    }

                    EditorGUILayout.EndHorizontal();

                    #endregion

                    #region 显示引用列表

                    foreach (var referenceAsset in referenceAssets)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(referenceAsset);
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
        }
    }
}
