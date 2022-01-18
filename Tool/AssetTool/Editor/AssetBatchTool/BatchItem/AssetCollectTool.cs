﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.DataStruct.Item;
using Data.DataStruct.Job;
using Data.DataStruct.Map;
using Data.DataStruct.Role;
using Data.DataStruct.Skill;
using Kuroha.Framework.Audio;
using Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchGUI;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchItem
{
    public static class AssetCollectTool
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        private enum AssetType
        {
            音频,
            物品,
            物品特效,
            物品类型,
            职业,
            地图,
            角色,
            技能
        }

        /// <summary>
        /// 收集路径
        /// </summary>
        private static string collectPath;
        
        /// <summary>
        /// 保存路径
        /// </summary>
        private static string savePath;

        /// <summary>
        /// 收集类型
        /// </summary>
        private static AssetType assetType = AssetType.音频;
        
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool foldout = true;
        
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
        private const float UI_INPUT_AREA_WIDTH = 500;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            SetDefaultPath();
            
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            foldout = EditorGUILayout.Foldout(foldout, AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.AssetCollectTool], true);
            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 选择收集的资源类型.");
                    GUILayout.BeginVertical("Box");
                    EditorGUI.BeginChangeCheck();
                    assetType = (AssetType) EditorGUILayout.EnumPopup("Select Collect Type", assetType, GUILayout.Width (UI_INPUT_AREA_WIDTH));
                    if (EditorGUI.EndChangeCheck()) {
                        ResetCollectPath();
                        ResetSavePath();
                    }
                    GUILayout.EndVertical();
                    
                    EditorGUILayout.LabelField("2. 输入待收集资源的路径.");
                    GUILayout.BeginVertical ("Box");
                    collectPath = EditorGUILayout.TextField ("Input Collect Path", collectPath, GUILayout.Width (UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    
                    EditorGUILayout.LabelField("3. 输入待收集资源的路径.");
                    GUILayout.BeginVertical ("Box");
                    savePath = EditorGUILayout.TextField ("Input Save Path", savePath, GUILayout.Width (UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();

                    EditorGUILayout.LabelField("4. 点击按钮, 开始收集.");
                    GUILayout.BeginVertical ("Box");
                    if (GUILayout.Button ("Collect", GUILayout.Height (UI_BUTTON_HEIGHT), GUILayout.Width (UI_BUTTON_WIDTH)))
                    {
                        Collect();
                        AssetDatabase.Refresh();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 设置默认路径
        /// </summary>
        private static void SetDefaultPath()
        {
            if (string.IsNullOrEmpty(collectPath))
            {
                ResetCollectPath();
            }
            
            if (string.IsNullOrEmpty(savePath))
            {
                ResetSavePath();
            }
        }

        /// <summary>
        /// 重置收集路径
        /// </summary>
        private static void ResetCollectPath()
        {
            collectPath = assetType switch
            {
                AssetType.音频 =>    "Assets/Resources/DataBase/Audio",
                AssetType.物品 =>    "Assets/Resources/DataBase/Item",
                AssetType.物品特效 => "Assets/Resources/DataBase/ItemBuff",
                AssetType.物品类型 => "Assets/Resources/DataBase/ItemType",
                AssetType.职业 =>    "Assets/Resources/DataBase/Job",
                AssetType.地图 =>    "Assets/Resources/DataBase/Map",
                AssetType.角色 =>    "Assets/Resources/DataBase/Role",
                AssetType.技能 =>    "Assets/Resources/DataBase/Skill",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        /// <summary>
        /// 重置保存路径
        /// </summary>
        private static void ResetSavePath()
        {
            savePath = assetType switch
            {
                AssetType.音频 =>    "Assets/Resources/Configs/Assets/Audios.txt",
                AssetType.物品 =>    "Assets/Resources/Configs/Assets/Item.txt",
                AssetType.物品特效 => "Assets/Resources/Configs/Assets/ItemBuff.txt",
                AssetType.物品类型 => "Assets/Resources/Configs/Assets/ItemType.txt",
                AssetType.职业 =>    "Assets/Resources/Configs/Assets/Job.txt",
                AssetType.地图 =>    "Assets/Resources/Configs/Assets/Map.txt",
                AssetType.角色 =>    "Assets/Resources/Configs/Assets/Role.txt",
                AssetType.技能 =>    "Assets/Resources/Configs/Assets/Skill.txt",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// 收集
        /// </summary>
        private static void Collect()
        {
            var filterType = assetType switch
            {
                AssetType.音频 =>    $"t:{nameof(SingleClip)}",
                AssetType.物品 =>    $"t:{nameof(DS_Item)}",
                AssetType.物品特效 => $"t:{nameof(DS_ItemBuff)}",
                AssetType.物品类型 => $"t:{nameof(DS_ItemType)}",
                AssetType.职业 =>    $"t:{nameof(DS_Job)}",
                AssetType.地图 =>    $"t:{nameof(DS_Map)}",
                AssetType.角色 =>    $"t:{nameof(DS_Role)}",
                AssetType.技能 =>    $"t:{nameof(DS_Skill)}",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var paths = new List<string>();
            var guids = AssetDatabase.FindAssets(filterType, new[] {
                collectPath
            });
            paths.AddRange(guids.Select(AssetDatabase.GUIDToAssetPath));
            
            System.IO.File.WriteAllLines(savePath, paths, Encoding.UTF8);
        }
    }
}
