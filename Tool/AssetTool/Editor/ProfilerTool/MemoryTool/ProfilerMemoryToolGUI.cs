using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Kuroha.Tool.Editor.ProfilerTool
{
    public static class ProfilerMemoryToolGUI
    {
        /// <summary>
        /// Unity Analysis Profiler
        /// </summary>
        private const string PROFILER_PATH = "Window/Analysis/Profiler";

        /// <summary>
        /// 筛选条件: 内存占用大小, 单位: byte
        /// </summary>
        private static float memorySize = 1024;

        /// <summary>
        /// 筛选条件: 树形结构深度
        /// </summary>
        private static int memoryDepth = 3;

        /// <summary>
        /// 筛选条件: 资源名称
        /// </summary>
        private static string memoryName = "jeep";
        
        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;
        
        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;
        
        /// <summary>
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_INPUT_AREA_WIDTH = 400;
        
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool foldout = true;

        /// <summary>
        /// 树形结构根节点
        /// </summary>
        private static ProfilerMemoryElement profilerMemoryElementRoot;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
            
            foldout = EditorGUILayout.Foldout(foldout, ProfilerToolGUI.tools[(int) ProfilerToolGUI.ToolType.MemoryTool], true);

            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                EditorGUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("当前连接设备: " + ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));

                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);
                    
                    EditorGUILayout.LabelField("1. 请先打开 Profiler 窗口, 并聚焦 Memory 部分的 Detail 窗口.");
                    GUILayout.BeginHorizontal("Box");
                    {
                        if (GUILayout.Button("打开 Profiler 窗口", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            EditorApplication.ExecuteMenuItem(PROFILER_PATH);
                        }
                    }
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);
                    
                    EditorGUILayout.LabelField("2. 点击按钮, 获取设备当前的内存细节信息快照.");
                    GUILayout.BeginHorizontal("Box");
                    {
                        if (GUILayout.Button("Take Sample", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            // 刷新数据
                            ProfilerWindow.RefreshMemoryData();
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);
                    
                    // 绘制筛选条件
                    EditorGUILayout.LabelField("3. 填写筛选条件");
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("1. 按照资源名称筛选");
                    memoryName = EditorGUILayout.TextField("Name: ", memoryName, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    EditorGUI.indentLevel--;
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("2. 按照内存占用大小筛选");
                    memorySize = EditorGUILayout.FloatField("Memory Size(B) >= ", memorySize, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    EditorGUI.indentLevel--;
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("3. 按照资源树深度筛选");
                    memoryDepth = EditorGUILayout.IntField("Memory Depth(>=1) ", memoryDepth, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    EditorGUI.indentLevel--;

                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);
                    
                    EditorGUILayout.LabelField("4. 点击按钮, 导出内存占用细节到文件: C:/MemoryDetail.txt");
                    GUILayout.BeginHorizontal("Box");
                    {
                        if (GUILayout.Button("Export", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            if (memoryDepth <= 0)
                            {
                                memoryDepth = 1;
                            }

                            // 导出内存数据
                            ExtractMemory(memoryName, memorySize, memoryDepth - 1);
                        }
                    }
                    GUILayout.EndHorizontal();
                    
                }
                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 导出内存细节详情
        /// </summary>
        /// <param name="memName"></param>
        /// <param name="memSize"></param>
        /// <param name="memDepth"></param>
        private static void ExtractMemory(string memName, float memSize, int memDepth)
        {
            // 文本内容
            var texts = new List<string>(100);

            // 输出文件路径
            var outputPath = $"C:/MemoryDetail_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";

            // 获取到根节点
            profilerMemoryElementRoot = ProfilerWindow.GetMemoryDetailRoot(memDepth, memSize);
            if (profilerMemoryElementRoot != null)
            {
                var memoryConnect = ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler);
                texts.Add($"Memory Size: >= {memorySize}B)");
                texts.Add($"Memory Depth: {memoryDepth}");
                texts.Add($"Current Target: {memoryConnect}");
                texts.Add("****************************************************************************************");
                texts.AddRange(ProfilerWindow.GetMemoryDetail(profilerMemoryElementRoot, memName));
            }

            System.IO.File.WriteAllLines(outputPath, texts);
            System.Diagnostics.Process.Start(outputPath);
        }
    }
}