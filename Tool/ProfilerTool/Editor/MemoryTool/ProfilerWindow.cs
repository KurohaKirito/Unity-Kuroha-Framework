using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.Profiling;
using Kuroha.Util.RunTime;

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
            DebugUtil.Log($"获取到了 {profilerWindows.Count} 个 ProfilerWindow 类", null, "green");
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
    public static MemoryElement GetMemoryDetailRoot(int filterDepth, float filterSize)
    {
        MemoryElement element = null;
        
        var memoryDetailWindow = GetClass_ProfilerWindow(ProfilerArea.Memory);
        if (memoryDetailWindow != null)
        {
            // 得到 m_MemoryListView 变量, 其类型为: MemoryTreeListClickable
            var listViewDynamic = new DynamicClass(memoryDetailWindow.GetFieldValue_Private("m_MemoryListView"));
            
            // 得到 m_Root 变量, 其类型为: MemoryElement
            var rootDynamic = listViewDynamic.GetFieldValue_Private("m_Root");
            if (rootDynamic != null)
            {
                element = MemoryElement.Create(new DynamicClass(rootDynamic), 0, filterDepth, filterSize);
            }
        }
        else
        {
            DebugUtil.Log("请打开 Profiler 窗口的 Memory 视图, 并切换到 Detail 页面", null, "red");
        }

        return element;
    }
    
    public static void WriteMemoryDetail(string filterName, StreamWriter writer, MemoryElement root)
    {
        if (null == root)
        {
            return;
        }
        
        var text = root.ToString();
        
        // 筛选 3 级
        if (text.IndexOf("\t\t\t", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            stage3 = text.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) >= 0;
            if (stage3 && stage2 && stage1)
            {
                writer.WriteLine(text);
            }
        }
        // 筛选 2 级
        else if (text.IndexOf("\t\t", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            stage2 = text.IndexOf("Texture2D", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     text.IndexOf("Mesh", StringComparison.OrdinalIgnoreCase) >= 0;
            if (stage2 && stage1)
            {
                writer.WriteLine(text);
            }
        }
        // 筛选 1 级
        else
        {
            stage1 = text.IndexOf("Assets", StringComparison.OrdinalIgnoreCase) >= 0;
            if (stage1)
            {
                writer.WriteLine(text);
            }
        }
        
        foreach (var memoryElement in root.childrenList)
        {
            if (memoryElement != null)
            {
                WriteMemoryDetail(filterName, writer, memoryElement);
            }
        }
    }
}
