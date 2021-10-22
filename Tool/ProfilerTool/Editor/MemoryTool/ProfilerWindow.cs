using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public static class ProfilerWindow
{
    private static bool stage1;
    private static bool stage2;
    private static bool stage3;
    
    private static List<DynamicClass> profilerWindows;

    /// <summary>
    /// 获取到 ProfilerWindow 类
    /// </summary>
    /// <param name="area"></param>
    /// <returns></returns>
    private static DynamicClass GetClass_ProfilerWindow(ProfilerArea area)
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
        
        foreach (var dynamicClass in profilerWindows)
        {
            var val = (ProfilerArea)dynamicClass.GetFieldValue_Private("m_CurrentArea");
            if (val == area)
            {
                return dynamicClass;
            }
        }
        return null;
    }

    public static MemoryElement GetMemoryDetailRoot(int filterDepth, float filterSize)
    {
        var windowDynamic = GetClass_ProfilerWindow(ProfilerArea.Memory);
        if (windowDynamic == null) return null;
        var listViewDynamic = new DynamicClass(windowDynamic.GetFieldValue_Private("m_MemoryListView"));
        var rootDynamic = listViewDynamic.GetFieldValue_Private("m_Root");
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

    /// <summary>
    /// 刷新内存页面数据
    /// </summary>
    public static void RefreshMemoryData()
    {
        var dynamic = GetClass_ProfilerWindow(ProfilerArea.Memory);
        
        if (null != dynamic)
        {
            dynamic.CallMethod_Private("RefreshMemoryData");
        }
        else
        {
            Debug.Log("请打开Profiler 窗口的 Memory 视图");
        }
    }
}
