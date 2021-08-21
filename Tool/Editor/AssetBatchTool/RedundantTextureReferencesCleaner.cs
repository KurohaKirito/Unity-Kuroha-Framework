using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kuroha.GUI.Editor;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.AssetBatchTool
{
    public static class RedundantTextureReferencesCleaner
    {
        /// <summary>
        /// 是否自动修复
        /// </summary>
        private static bool isAutoRepair;
        
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool unusedMaterialFoldout = true;

        /// <summary>
        /// 检测路径
        /// </summary>
        private static string unusedMaterialPath = "Assets/Art";

        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_INPUT_AREA_WIDTH = 400;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;
        
        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;

        /// <summary>
        /// 界面绘制
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            unusedMaterialFoldout = EditorGUILayout.Foldout(unusedMaterialFoldout, "材质球冗余纹理引用清除器", true);
            if (unusedMaterialFoldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    GUILayout.Label("1. 输入待检测的材质球资源的路径. (Assets/相对路径)");
                    GUILayout.BeginVertical("Box");
                    {
                        unusedMaterialPath = EditorGUILayout.TextField("Material Path:", unusedMaterialPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    }
                    GUILayout.EndVertical();

                    GUILayout.Space(UI_DEFAULT_MARGIN);
                    GUILayout.Label("2. 请选择是否自动修复, 可以自动清除保存的多余纹理引用.");
                    GUILayout.BeginVertical("Box");
                    {
                        isAutoRepair = EditorGUILayout.Toggle("Auto Repair", isAutoRepair, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    }
                    GUILayout.EndVertical();

                    GUILayout.Space(UI_DEFAULT_MARGIN);
                    GUILayout.Label("3. 点击按钮, 开始检测.");
                    GUILayout.BeginVertical("Box");
                    {
                        if (GUILayout.Button("Start", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            Check();
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 开始检测
        /// </summary>
        private static void Check()
        {
            // 获取相对目录下所有的材质球文件
            var guids = AssetDatabase.FindAssets("t:Material", new []{unusedMaterialPath});
            var materials = new List<Material>();
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                materials.Add(AssetDatabase.LoadAssetAtPath<Material>(path));
            }
            Kuroha.Util.Release.DebugUtil.Log($"Find {materials.Count} Materials!");
            
            // 遍历材质
            var repairCounter = 0;
            for (var index = 0; index < materials.Count; index++)
            {
                ProgressBar.DisplayProgressBar("检测中", $"{index + 1}/{materials.Count}", index + 1, materials.Count);
                if (RepairMaterial(materials[index]))
                {
                    repairCounter++;
                }
            }

            Kuroha.Util.Release.DebugUtil.Log($"RepairCounter: 共检测出 {repairCounter} 个问题.");
        }

        /// <summary>
        /// 修复纹理中冗余的纹理引用问题
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        private static bool RepairMaterial(Material material)
        {
            var isError = false;
            var strBuilder = new StringBuilder();

            // 获取材质球中引用的全部纹理的 GUID (不包含冗余的引用)
            Kuroha.Util.Editor.TextureUtil.GetTexturesInMaterial(material, out var textures);
            var textureGUIDs = textures.Select(textureData => textureData.guid).ToList();
            
            // 直接以文本形式逐行读取 Material 文件 (包含全部的纹理引用)
            var materialPathName = Path.GetFullPath(AssetDatabase.GetAssetPath(material));
            using (var reader = new StreamReader(materialPathName))
            {
                var regex = new Regex(@"\s+guid:\s+(\w+),");
                var line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Contains("m_Texture:"))
                    {
                        // 包含纹理贴图引用的行，使用正则表达式获取纹理贴图的 guid
                        var match = regex.Match(line);
                        if (match.Success)
                        {
                            var guid = match.Groups[1].Value;
                            if (textureGUIDs.Contains(guid) == false)
                            {
                                // 是冗余引用
                                isError = true;
                                
                                // 将 fileID 赋值为 0 来清除引用关系
                                var guidLength = line.IndexOf("fileID:", StringComparison.Ordinal) + 7;
                                strBuilder.AppendLine(line.Substring(0, guidLength) + " 0}");
                            }
                        }
                    }

                    strBuilder.AppendLine(line);
                    line = reader.ReadLine();
                }
            }

            if (isAutoRepair)
            {
                using (var writer = new StreamWriter(materialPathName))
                {
                    writer.Write(strBuilder.ToString());
                }
            }

            return isError;
        }
    }
}