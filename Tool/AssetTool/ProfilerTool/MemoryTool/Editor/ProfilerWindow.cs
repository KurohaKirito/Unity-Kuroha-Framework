using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kuroha.Framework.Utility.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.ProfilerTool.MemoryTool.Editor
{
    public static class ProfilerWindow
    {
        private static bool stage1;
        private static bool stage2;
        private static bool stage3;
        private static IList profilerWindows;
        private static readonly Assembly assemblyInfo = ReflectionUtil.GetAssembly(typeof(EditorWindow));
        private static readonly Type classInfoProfilerWindow = ReflectionUtil.GetClass(assemblyInfo, "UnityEditor.ProfilerWindow");
        private static readonly Type classInfoMemoryProfilerModule = ReflectionUtil.GetClass(assemblyInfo, "UnityEditorInternal.Profiling.MemoryProfilerModule");

        /// <summary>
        /// 获取到对应面板的 ProfilerModule 类
        /// </summary>
        private static object GetClassProfilerModule(string areaName)
        {
            // areaName: 'CPU Usage', 'Memory', 'Rendering',
            if (profilerWindows == null)
            {
                // private static List<ProfilerWindow> s_ProfilerWindows = new List<ProfilerWindow>();
                var fieldInfo = ReflectionUtil.GetField(classInfoProfilerWindow, "s_ProfilerWindows", BindingFlags.NonPublic | BindingFlags.Static);
                profilerWindows = ReflectionUtil.GetValueField(fieldInfo) as IList;
            }

            if (profilerWindows != null)
            {
                foreach (var profilerWindowInstance in profilerWindows)
                {
                    // private List<ProfilerModuleBase> m_Modules;
                    var fieldInfo = ReflectionUtil.GetField(classInfoProfilerWindow, "m_Modules", BindingFlags.NonPublic | BindingFlags.Instance);
                    var result2 = ReflectionUtil.GetValueField(fieldInfo, profilerWindowInstance) as IList;
                    Debug.Log($"当前 Modules 一共有 {result2?.Count} 个");
                    
                    // public ProfilerModuleBase SelectedModule
                    // 返回值的真实类型为: UnityEditorInternal.Profiling.MemoryProfilerModule
                    var propertyInfo = ReflectionUtil.GetProperty(classInfoProfilerWindow, "SelectedModule", BindingFlags.Public | BindingFlags.Instance);
                    var memoryProfilerModuleInstance = ReflectionUtil.GetValueProperty(propertyInfo, profilerWindowInstance);
                    Debug.Log($"当前选中的 Modules 名称 {memoryProfilerModuleInstance}");
                    
                    // protected string m_Name
                    var fieldInfo2 = ReflectionUtil.GetField(classInfoMemoryProfilerModule, "m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (ReflectionUtil.GetValueField(fieldInfo2, memoryProfilerModuleInstance) is string result3) {
                        if (result3.Contains(areaName)) {
                            return memoryProfilerModuleInstance;
                        }
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
            var memoryDetailWindowInstance = GetClassProfilerModule("Memory");
            if (memoryDetailWindowInstance != null)
            {
                // private void RefreshMemoryData()
                var methodInfo = ReflectionUtil.GetMethod(classInfoMemoryProfilerModule, "RefreshMemoryData", BindingFlags.NonPublic | BindingFlags.Instance);
                ReflectionUtil.CallMethod(methodInfo, memoryDetailWindowInstance, null);
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

            var memoryDetailWindow = GetClassProfilerModule("Memory");
            if (memoryDetailWindow != null)
            {
                // private MemoryTreeListClickable m_MemoryListView
                var fieldInfo = ReflectionUtil.GetField(classInfoMemoryProfilerModule, "m_MemoryListView", BindingFlags.NonPublic | BindingFlags.Instance);
                var memoryListViewInstance = ReflectionUtil.GetValueField(fieldInfo, memoryDetailWindow);
                
                // protected MemoryElement m_Root
                var classInfoMemoryTreeListClickable = memoryListViewInstance.GetType();
                var fieldInfoRoot = ReflectionUtil.GetField(classInfoMemoryTreeListClickable, "m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                var rootInstance = ReflectionUtil.GetValueField(fieldInfoRoot, memoryListViewInstance);
                if (rootInstance != null)
                {
                    element = ProfilerMemoryElement.Create(rootInstance, 0, filterDepth, filterSize);
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