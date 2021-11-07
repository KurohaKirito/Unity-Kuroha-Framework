using System;
using System.Collections;
using System.Collections.Generic;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine.Profiling;

namespace Kuroha.Tool.AssetTool.Editor.ProfilerTool.MemoryTool
{
    public static class ProfilerWindow
    {
        private static bool stage1;
        private static bool stage2;
        private static bool stage3;
        private static List<DynamicClass> profilerWindows;

        /// <summary>
        /// 获取到 ProfilerWindow 类
        /// </summary>
        /// <param name="targetArea"></param>
        /// <returns></returns>
        private static DynamicClass GetClass_ProfilerWindow(ProfilerArea targetArea)
        {
            if (profilerWindows == null)
            {
                // 获取到 UnityEditor 程序集
                var dynamicAssembly = new DynamicAssembly(typeof(EditorWindow));

                // 获取到 ProfilerWindow 类
                var dynamicClass = dynamicAssembly.GetClass("UnityEditor.ProfilerWindow");

                // 获取到 m_ProfilerWindows 变量, 其类型为: List<ProfilerWindow>
                var list = dynamicClass.GetFieldValue_PrivateStatic<IList>("m_ProfilerWindows");

                profilerWindows = new List<DynamicClass>();
                foreach (var window in list)
                {
                    profilerWindows.Add(new DynamicClass(window));
                }
            }

            if (profilerWindows != null)
            {
                foreach (var dynamicClass in profilerWindows)
                {
                    var currentArea = (ProfilerArea)dynamicClass.GetFieldValue_Private("m_CurrentArea");
                    if (currentArea == targetArea)
                    {
                        return dynamicClass;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 刷新内存页面数据
        /// </summary>
        public static void RefreshMemoryData()
        {
            var memoryDetailWindow = GetClass_ProfilerWindow(ProfilerArea.Memory);

            if (memoryDetailWindow != null)
            {
                // 调用 RefreshMemoryData() 函数
                memoryDetailWindow.CallMethod_Private("RefreshMemoryData");
            }
            else
            {
                DebugUtil.Log("请打开 Profiler 窗口的 Memory 视图, 并切换到 Detail 页面", null, "red");
            }
        }

        /// <summary>
        /// 获取到 Memory Detail 页面的根节点
        /// </summary>
        /// <param name="filterDepth"></param>
        /// <param name="filterSize"></param>
        /// <returns></returns>
        public static ProfilerMemoryElement GetMemoryDetailRoot(int filterDepth, float filterSize)
        {
            ProfilerMemoryElement element = null;

            var memoryDetailWindow = GetClass_ProfilerWindow(ProfilerArea.Memory);
            if (memoryDetailWindow != null)
            {
                // 得到 m_MemoryListView 变量, 其类型为: MemoryTreeListClickable
                var listViewDynamic = new DynamicClass(memoryDetailWindow.GetFieldValue_Private("m_MemoryListView"));

                // 得到 m_Root 变量, 其类型为: ProfilerMemoryElement
                var rootDynamic = listViewDynamic.GetFieldValue_Private("m_Root");
                if (rootDynamic != null)
                {
                    element = ProfilerMemoryElement.Create(new DynamicClass(rootDynamic), 0, filterDepth, filterSize);
                }
            }
            else
            {
                DebugUtil.Log("请打开 Profiler 窗口的 Memory 视图, 并切换到 Detail 页面", null, "red");
            }

            return element;
        }

        /// <summary>
        /// 得到内存占用细节
        /// </summary>
        /// <param name="root">内存细节页面的数据根节点</param>
        /// <param name="filterName">名称筛选</param>
        /// <returns></returns>
        public static IEnumerable<string> GetMemoryDetail(ProfilerMemoryElement root, string filterName)
        {
            const StringComparison COMPARISON = StringComparison.OrdinalIgnoreCase;
            var texts = new List<string>(100);
            var nodes = new Stack<ProfilerMemoryElement>(7000);

            nodes.Push(root);
            while (nodes.Count > 0)
            {
                var currentNode = nodes.Pop();
                var currentText = currentNode.ToString();

                #region 筛选处理分析

                // 筛选 3 级
                if (currentText.IndexOf("\t\t\t", COMPARISON) >= 0)
                {
                    stage3 = currentText.IndexOf(filterName, COMPARISON) >= 0;
                    if (stage3 && stage2 && stage1)
                    {
                        texts.Add(currentText);
                    }
                }
                // 筛选 2 级
                else if (currentText.IndexOf("\t\t", COMPARISON) >= 0)
                {
                    stage2 = currentText.IndexOf($"Texture2D{ProfilerMemoryElement.DELIMITER}", COMPARISON) >= 0 ||
                             currentText.IndexOf($"Mesh{ProfilerMemoryElement.DELIMITER}", COMPARISON) >= 0;
                    if (stage2 && stage1)
                    {
                        texts.Add(currentText);
                    }
                }
                // 筛选 1 级
                else if (currentText.IndexOf("\t", COMPARISON) >= 0)
                {
                    stage1 = currentText.IndexOf($"Assets{ProfilerMemoryElement.DELIMITER}", COMPARISON) >= 0;
                    if (stage1)
                    {
                        texts.Add(currentText);
                    }
                }
                // 筛选 0 级
                else
                {
                    texts.Add(currentText);
                }

                #endregion

                var currentChildren = currentNode.children;
                for (var index = currentChildren.Count - 1; index >= 0; --index)
                {
                    nodes.Push(currentChildren[index]);
                }
            }

            return texts;
        }
    }
}