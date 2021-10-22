using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public static class ProfilerWindow
{
    private static bool stage1;
    private static bool stage2;
    private static bool stage3;
    
    private static List<DynamicClass> windows;

    private static DynamicClass GetWindow(ProfilerArea area)
    {
        if (null == windows)
        {
            var dynamicType = new DynamicAssembly(typeof(EditorWindow));
            var type = dynamicType.GetClass("UnityEditor.ProfilerWindow");
            var list = type.PrivateStaticField<IList>("m_ProfilerWindows");
            windows = new List<DynamicClass>();
            foreach (var window in list)
            {
                windows.Add(new DynamicClass(window));
            }
        }
        foreach (var dynamic in windows)
        {
            var val = (ProfilerArea)dynamic.PrivateInstanceField("m_CurrentArea");
            if (val == area)
            {
                return dynamic;
            }
        }
        return null;
    }

    public static MemoryElement GetMemoryDetailRoot(int filterDepth, float filterSize)
    {
        var windowDynamic = GetWindow(ProfilerArea.Memory);
        if (windowDynamic == null) return null;
        var listViewDynamic = new DynamicClass(windowDynamic.PrivateInstanceField("m_MemoryListView"));
        var rootDynamic = listViewDynamic.PrivateInstanceField("m_Root");
        return rootDynamic != null ? MemoryElement.Create(new DynamicClass(rootDynamic), 0, filterDepth, filterSize) : null;
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
        
        foreach (var memoryElement in root.children)
        {
            if (memoryElement != null)
            {
                WriteMemoryDetail(filterName, writer, memoryElement);
            }
        }
    }

    public static void RefreshMemoryData()
    {
        var dynamic = GetWindow(ProfilerArea.Memory);
        
        if (null != dynamic)
        {
            dynamic.CallPrivateInstanceMethod("RefreshMemoryData");
        }
        else
        {
            Debug.Log("请打开Profiler 窗口的 Memory 视图");
        }
    }
}
