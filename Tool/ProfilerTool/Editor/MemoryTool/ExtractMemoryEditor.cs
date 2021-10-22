using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ExtractMemoryEditor: EditorWindow
{
    private const string PROFILER_PATH = "Window/Analysis/Profiler";
    
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
    private string memoryName = "jeep";
    
    /// <summary>
    /// 树形结构根节点
    /// </summary>
    private MemoryElement memoryElementRoot;
    
    [MenuItem("Window/Extract Profiler Memory")]
    public static void Open()
    {
        // 打开 Profiler 窗口
        EditorApplication.ExecuteMenuItem(PROFILER_PATH);
        
        // 打开此窗口
        GetWindow<ExtractMemoryEditor>();
    }

    /// <summary>
    /// 绘制界面
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.LabelField("当前连接设备: " + ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));

            if (GUILayout.Button("截取内存细节快照"))
            {
                // 刷新数据
                ProfilerWindow.RefreshMemoryData();
            }

            // 刷选条件
            memoryName = EditorGUILayout.TextField("Name: ", memoryName);
            memorySize = EditorGUILayout.FloatField("Memory Size(B) >= ", memorySize);
            memoryDepth = EditorGUILayout.IntField("Memory Depth(>=1) ", memoryDepth);

            if (GUILayout.Button("导出内存细节详情"))
            {
                if (memoryDepth <= 0)
                {
                    memoryDepth = 1;
                }
                
                // 导出内存数据
                ExtractMemory(memoryName, memorySize, memoryDepth - 1);
            }
        }
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 导出内存细节详情
    /// </summary>
    /// <param name="memName"></param>
    /// <param name="memSize"></param>
    /// <param name="memDepth"></param>
    private void ExtractMemory(string memName, float memSize, int memDepth)
    {
        // 输出文件路径
        var outputPath = $"C:/MemoryDetail_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";
        
        // 创建文件
        File.Create(outputPath).Dispose();
        
        // 获取到根节点
        memoryElementRoot = ProfilerWindow.GetMemoryDetailRoot(memDepth, memSize);
        if (memoryElementRoot != null)
        {
            var writer = new StreamWriter(outputPath);
            writer.WriteLine("Memory Size: >= {0}B", memorySize);
            writer.WriteLine("Memory Depth: {0}", memoryDepth);
            writer.WriteLine("Current Target: {0}", ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));
            writer.WriteLine("****************************************************************************************");
            
            ProfilerWindow.WriteMemoryDetail(memName, writer, memoryElementRoot);
            
            writer.Flush();
            writer.Close();
        }
        
        Process.Start(outputPath);
    }
}
