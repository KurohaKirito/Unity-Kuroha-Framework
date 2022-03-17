using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kuroha.Framework.GUI.Editor;
using Kuroha.Framework.Utility.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetViewer.Editor
{
    public class UnityIcon : EditorWindow
    {
        private static int searchBeginIndex;
        private static int searchEndIndex;
        private static int curPageIndex;

        private static Vector2 scrollPos;
        private static readonly List<Texture2D> builtInTextures = new List<Texture2D>();

        private const int UI_COUNT_ROW = 13;
        private const int UI_SPACE = 5;
        private const float UI_WIDTH = 50f;
        private const float UI_HEIGHT = 30f;

        public static int windowWidth = 705;
        public static int windowHeight = 492;
        public static UnityIcon window;

        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void Open()
        {
            window = GetWindow<UnityIcon>();
            window.minSize = new Vector2(windowWidth, windowHeight);
            window.maxSize = window.minSize;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            curPageIndex = 1;
            GetBuiltInAsset();
        }

        /// <summary>
        /// 绘制窗口
        /// </summary>
        private void OnGUI()
        {
            PageManager.Pager(builtInTextures.Count, 100, ref curPageIndex, out searchBeginIndex, out searchEndIndex);

            EditorGUILayout.BeginVertical();
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                {
                    #region Unity 内置图标

                    for (var i = searchBeginIndex; i <= searchEndIndex; i += UI_COUNT_ROW)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            for (var j = 0; j < UI_COUNT_ROW; j++)
                            {
                                var index = i + j;
                                if (index >= builtInTextures.Count)
                                {
                                    continue;
                                }

                                var content = EditorGUIUtility.IconContent(builtInTextures[index].name);
                                if (content.image == null)
                                {
                                    GUILayout.Button("Error", GUILayout.Width(UI_WIDTH), GUILayout.Height(UI_HEIGHT));
                                }
                                else
                                {
                                    if (GUILayout.Button(content, GUILayout.Width(UI_WIDTH),
                                        GUILayout.Height(UI_HEIGHT)))
                                    {
                                        DebugUtil.Log($"获取方法为调用: EditorGUIUtility.IconContent(\"{builtInTextures[index].name}\")");
                                    }
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    #endregion

                    #region Unity 鼠标样式

                    var mouseCursorArray = Enum.GetValues(typeof(MouseCursor));
                    if (mouseCursorArray is MouseCursor[] cursors)
                    {
                        for (var i = 0; i <= cursors.Length; i += 2)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                for (var j = 0; j < 2; j++)
                                {
                                    var index = i + j;
                                    if (index < cursors.Length)
                                    {
                                        GUILayout.Button(Enum.GetName(typeof(MouseCursor), cursors[index]), GUILayout.Width(327));
                                        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), cursors[index]);
                                        GUILayout.Space(UI_SPACE);
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }

                    #endregion
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 获取所有的图标
        /// </summary>
        private static void GetBuiltInAsset()
        {
            if (builtInTextures.Count <= 0)
            {
                const BindingFlags FLAGS = BindingFlags.Static | BindingFlags.NonPublic;
                var info = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", FLAGS);
                var bundle = info?.Invoke(null, null) as AssetBundle;

                if (bundle != null)
                {
                    var objects = bundle.LoadAllAssets();
                    if (objects != null)
                    {
                        var count = objects.Length;
                        for (var index = 0; index < count; index++)
                        {
                            ProgressBar.DisplayProgressBar("Unity 图标显示工具", $"搜索图标中: {index + 1}/{count}", index + 1, count);

                            if (objects[index] is Texture2D texture2D)
                            {
                                // 非法的不添加 (不同版本间有差异)
                                if (texture2D.name != "DialArrow_Texture" &&
                                    texture2D.name != "builtin_brush_1" &&
                                    texture2D.name != "builtin_brush_2" &&
                                    texture2D.name != "builtin_brush_3" &&
                                    texture2D.name != "builtin_brush_4" &&
                                    texture2D.name != "builtin_brush_5" &&
                                    texture2D.name != "builtin_brush_6" &&
                                    texture2D.name != "builtin_brush_7" &&
                                    texture2D.name != "builtin_brush_8" &&
                                    texture2D.name != "builtin_brush_9" &&
                                    texture2D.name != "builtin_brush_10" &&
                                    // 重复的不添加
                                    builtInTextures.Count(builtInTexture => builtInTexture.name == texture2D.name) < 1)
                                {
                                    builtInTextures.Add(texture2D);
                                }
                            }
                        }

                        DebugUtil.Log($"Asset Bundle : 一共有 {builtInTextures.Count} 个图标");
                    }
                }
            }
        }
    }
}
