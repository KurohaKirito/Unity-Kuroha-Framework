using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ExtractMemoryEditor: EditorWindow
{
    /// <summary>
    /// 筛选条件: 内存占用大小, 单位: byte
    /// </summary>
    private float memorySize = 1024;
    
    /// <summary>
    /// 筛选条件: 树形结构深度
    /// </summary>
    private int memoryDepth = 3;

    /// <summary>
    /// 筛选条件: 资源名称
    /// </summary>
    private string assetName = "jetcar";
    
    /// <summary>
    /// 树形结构根节点
    /// </summary>
    private MemoryElement memoryElementRoot;
    
    [MenuItem("Window/Extract Profiler Memory")]
    public static void Open()
    {
        EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
        GetWindow<ExtractMemoryEditor>();
    }

    /// <summary>
    /// 绘制界面
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.LabelField("Current Target: " + ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));

            if (GUILayout.Button("Take Sample"))
            {
                ProfilerWindow.RefreshMemoryData();
            }

            assetName = EditorGUILayout.TextField("Name: ", assetName);
            memorySize = EditorGUILayout.FloatField("Memory Size(B) >= ", memorySize);
            memoryDepth = EditorGUILayout.IntField("Memory Depth(>=1) ", memoryDepth);

            if (GUILayout.Button("Extract Memory"))
            {
                if (memoryDepth <= 0)
                {
                    memoryDepth = 1;
                }
                
                ExtractMemory(assetName, memorySize, memoryDepth - 1);
            }
        }
        EditorGUILayout.EndVertical();
    }
    
    private void ExtractMemory(string assetName, float memSize, int memDepth)
    {
        var parent = Directory.GetParent(Application.dataPath);
        if (parent != null)
        {
            var outputPath = $"{parent.FullName}/MemoryDetailed{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";
            File.Create(outputPath).Dispose();
            memoryElementRoot = ProfilerWindow.GetMemoryDetailRoot(memDepth, memSize);

            if (null != memoryElementRoot)
            {
                var writer = new StreamWriter(outputPath);
                writer.WriteLine("Memory Size: >= {0}MB", memorySize);
                writer.WriteLine("Memory Depth: {0}", memoryDepth);
                writer.WriteLine("Current Target: {0}", ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));
                writer.WriteLine("**********************");
                ProfilerWindow.WriteMemoryDetail(assetName, writer, memoryElementRoot);
                writer.Flush();
                writer.Close();
            }
        
            Process.Start(outputPath);
        }
    }
}
