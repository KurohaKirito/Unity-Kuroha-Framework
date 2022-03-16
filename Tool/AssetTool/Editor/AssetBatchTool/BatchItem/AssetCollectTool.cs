using System;
using System.Collections.Generic;
using System.Linq;
using Kuroha.Framework.AsyncLoad.RunTime;
using UnityEditor;
using UnityEngine;
using Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchGUI;

namespace Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchItem
{
    public static class AssetCollectTool
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        private enum AssetType
        {
            音频片段,
            物品数据,
            物品特效,
            物品类型,
            职业数据,
            地图数据,
            角色数据,
            技能数据
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
        private static AssetType assetType = AssetType.音频片段;
        
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
                AssetType.音频片段 =>    "Assets/Resources/DataBase/Audio",
                AssetType.物品数据 =>    "Assets/Resources/DataBase/Item",
                AssetType.物品特效 => "Assets/Resources/DataBase/ItemBuff",
                AssetType.物品类型 => "Assets/Resources/DataBase/ItemType",
                AssetType.职业数据 =>    "Assets/Resources/DataBase/Job",
                AssetType.地图数据 =>    "Assets/Resources/DataBase/Map",
                AssetType.角色数据 =>    "Assets/Resources/DataBase/Role",
                AssetType.技能数据 =>    "Assets/Resources/DataBase/Skill",
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
                AssetType.音频片段 =>    "Assets/Resources/Configs/Assets/Audios.asset",
                AssetType.物品数据 =>    "Assets/Resources/Configs/Assets/Item.asset",
                AssetType.物品特效 => "Assets/Resources/Configs/Assets/ItemBuff.asset",
                AssetType.物品类型 => "Assets/Resources/Configs/Assets/ItemType.asset",
                AssetType.职业数据 =>    "Assets/Resources/Configs/Assets/Job.asset",
                AssetType.地图数据 =>    "Assets/Resources/Configs/Assets/Map.asset",
                AssetType.角色数据 =>    "Assets/Resources/Configs/Assets/Role.asset",
                AssetType.技能数据 =>    "Assets/Resources/Configs/Assets/Skill.asset",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// 收集
        /// </summary>
        private static void Collect()
        {
            var paths = new List<string>();
            var guids = AssetDatabase.FindAssets(GetFilterStr(), new[] {
                collectPath
            });
            paths.AddRange(guids.Select(AssetDatabase.GUIDToAssetPath));

            var config = AssetDatabase.LoadAssetAtPath<ScriptableObjectAsyncLoadAsset>(savePath);
            config.assetPaths = paths;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 得到查找条件
        /// </summary>
        private static string GetFilterStr()
        {
            return assetType switch
            {
                AssetType.音频片段 => "t:SingleClip",
                AssetType.物品数据 => "t:DS_Item",
                AssetType.物品特效 => "t:DS_ItemBuff",
                AssetType.物品类型 => "t:DS_ItemType",
                AssetType.职业数据 => "t:DS_Job",
                AssetType.地图数据 => "t:DS_Map",
                AssetType.角色数据 => "t:DS_Role",
                AssetType.技能数据 => "t:DS_Skill",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
