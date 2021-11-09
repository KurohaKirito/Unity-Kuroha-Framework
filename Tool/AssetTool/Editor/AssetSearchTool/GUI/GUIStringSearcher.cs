using System;
using Kuroha.Tool.AssetTool.Editor.AssetSearchTool.Data;
using Kuroha.Tool.AssetTool.Editor.AssetSearchTool.Searcher;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.AssetSearchTool.GUI
{
    /// <summary>
    /// [GUI] 字符串匹配器
    /// </summary>
    public static class GUIStringSearcher
    {
        /// <summary>
        /// 字符串搜索的关键字
        /// </summary>
        private static string stringSearchKeys = string.Empty;

        /// <summary>
        /// 过滤器的默认值都是 -1, 即全选
        /// </summary>
        private static int stringAssetFilter = -1;

        /// <summary>
        /// 滑动条
        /// </summary>
        private static Vector2 stringSearchScrollPosition = Vector2.zero;

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

            #region 输入框, 以及查询按钮

            EditorGUILayout.BeginHorizontal();
            {
                stringSearchKeys = UnityEditor.EditorGUILayout.TextField("请输入查询的字符串:", stringSearchKeys);
                GUILayout.Space(UI_DEFAULT_MARGIN);
                if (GUILayout.Button("Search", GUILayout.Width(80)))
                {
                    var searchStringArray =
                        stringSearchKeys.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    StringSearcher.FindString(searchStringArray);
                }
            }
            EditorGUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            #region 过滤器

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("过滤器", GUILayout.Width(100));
                stringAssetFilter = EditorGUILayout.MaskField(stringAssetFilter, Enum.GetNames(typeof(AssetType)));
            }
            EditorGUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            #region 显示查询结果

            stringSearchScrollPosition = EditorGUILayout.BeginScrollView(stringSearchScrollPosition);
            {
                foreach (var key in StringSearcher.references.Keys)
                {
                    GUILayout.Space(UI_DEFAULT_MARGIN);

                    EditorGUILayout.TextField(key);

                    GUILayout.Space(UI_DEFAULT_MARGIN);

                    var referenceAssets = StringSearcher.references[key];
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label($"引用对象:共 {referenceAssets.Count} 个");
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("按字母排序", GUILayout.Width(100)))
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
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(UI_DEFAULT_MARGIN);

                    EditorGUI.indentLevel++;
                    foreach (var item in referenceAssets)
                    {
                        var referenceAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item);
                        if (AssetSearchWindow.IsDisplay(referenceAsset, item, stringAssetFilter))
                        {
                            EditorGUILayout.ObjectField(referenceAsset, typeof(UnityEngine.Object), true);
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndScrollView();

            #endregion
        }
    }
}