using System;
using System.Collections.Generic;
using System.IO;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.EffectCheckTool.ItemSetView
{
    public static class EffectCheckItemSetView
    {
        /// <summary>
        /// 配置文件的路径
        /// </summary>
        #if Kuroha
        private static string ConfigFilePath => $"{Application.dataPath}/Kuroha/Config/EffectToolConfig.txt";
        #else
        private static string ConfigFilePath => $"{Application.dataPath}/Art/Effects/EffectToolConfig.txt";
        #endif
        
        /// <summary>
        /// 危险等级
        /// </summary>
        public static readonly string[] dangerLevelOptions =
        {
            "警告",
            "危险"
        };

        /// <summary>
        /// 读取配置
        /// </summary>
        public static List<CheckItemInfo> LoadConfig()
        {
            var itemDic = new Dictionary<string, string[]>();
            var itemInfoList = new List<CheckItemInfo>();

            // 读取配置
            var allLines = File.ReadAllLines(ConfigFilePath);

            // 读取数据 (不读取标题, 所以下标从 1 开始)
            for (var index = 1; index < allLines.Length; index++)
            {
                if (string.IsNullOrEmpty(allLines[index]))
                {
                    continue;
                }

                // 切割数据
                var lineData = allLines[index].Split('\t');

                // 判断重复
                if (lineData.Length > 0)
                {
                    if (itemDic.ContainsKey(lineData[0]))
                    {
                        DebugUtil.LogError("表中有重复数据需要删除: " + lineData[0]);
                    }
                    else
                    {
                        itemDic.Add(lineData[0], lineData);
                    }
                }

                var info = new CheckItemInfo(lineData);
                itemInfoList.Add(info);
            }

            return itemInfoList;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static void SaveConfig(List<CheckItemInfo> configData, string dialogMessage)
        {
            var lines = new string[configData.Count + 1];

            // 配置文件标题行
            lines[0] = "GUID\t标题\t资源类型\t检查类型\t路径\t白名单规则\t参数\t危险等级\t是否参与特效检测\t是否参与CICD检测\t是否检查子目录\t备注";

            // 配置文件数据行
            for (var i = 0; i < configData.Count; i++)
            {
                var itemData = configData[i];
                var values = new List<string>
                {
                    (i + 1).ToString(),
                    itemData.title,
                    Convert.ToInt32(itemData.assetsType).ToString(),
                    Convert.ToInt32(itemData.checkType).ToString(),
                    itemData.path,
                    itemData.writePathRegex,
                    itemData.parameter,
                    itemData.dangerLevel.ToString(),
                    itemData.effectEnable.ToString(),
                    itemData.cicdEnable.ToString(),
                    itemData.isCheckSubFile.ToString(),
                    itemData.remark
                };

                lines[i + 1] = string.Join("\t", values.ToArray());
            }

            var directory = Path.GetDirectoryName(ConfigFilePath);
            if (directory != null && Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(ConfigFilePath, lines);
            AssetDatabase.Refresh();
            Dialog.Display(dialogMessage, Dialog.DialogType.Message, "OK");
        }
    }
}