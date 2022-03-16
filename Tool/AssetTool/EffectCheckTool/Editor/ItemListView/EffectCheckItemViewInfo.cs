using System;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.GUI;

namespace Kuroha.Tool.AssetTool.EffectCheckTool.Editor.ItemListView
{
    public class CheckItemInfo
    {
        /// <summary>
        /// 检查项唯一标识
        /// </summary>
        public readonly string guid;

        /// <summary>
        /// 检查项标题
        /// </summary>
        public string title;

        /// <summary>
        /// 表示检查什么类型的资源
        /// </summary>
        public EffectToolData.AssetsType checkAssetType;

        /// <summary>
        /// 表示获取哪里的资源
        /// </summary>
        public int getAssetType;

        /// <summary>
        /// 表示检查资源的什么选项
        /// </summary>
        public int checkOption;

        /// <summary>
        /// 表示检查时的指标和参数
        /// </summary>
        public string parameter;
        
        /// <summary>
        /// 表示是否检查子目录
        /// </summary>
        public bool isCheckSubFile;

        /// <summary>
        /// 待检查路径
        /// </summary>
        public string checkPath;
        
        /// <summary>
        /// 资源白名单规则
        /// </summary>
        public string assetWhiteRegex;
        
        /// <summary>
        /// 物体白名单规则
        /// </summary>
        public string objectWhiteRegex;

        /// <summary>
        /// 危险等级
        /// </summary>
        public int dangerLevel;

        /// <summary>
        /// 启用标志
        /// </summary>
        public bool effectEnable;

        /// <summary>
        /// 是否参与 CICD 检测
        /// </summary>
        public bool cicdEnable;

        /// <summary>
        /// 检查项的备注
        /// </summary>
        public string remark;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CheckItemInfo(string guid, string title, EffectToolData.AssetsType checkAssetType, int getAssetType, int checkOption, string checkPath, string assetWhiteRegex, string objectWhiteRegex, string parameter, int dangerLevel, bool effectEnable, bool cicdEnable, bool isCheckSubFile, string remark)
        {
            this.guid = guid;
            this.title = title;
            this.checkAssetType = checkAssetType;
            this.getAssetType = getAssetType;
            this.checkOption = checkOption;
            this.checkPath = checkPath;
            this.assetWhiteRegex = assetWhiteRegex;
            this.objectWhiteRegex = objectWhiteRegex;
            this.parameter = parameter;
            this.dangerLevel = dangerLevel;
            this.effectEnable = effectEnable;
            this.cicdEnable = cicdEnable;
            this.isCheckSubFile = isCheckSubFile;
            this.remark = remark;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="item"></param>
        public CheckItemInfo(in CheckItemInfo item)
        {
            if (item == null)
            {
                return;
            }

            guid = item.guid;
            title = item.title;
            checkAssetType = item.checkAssetType;
            getAssetType = item.getAssetType;
            checkOption = item.checkOption;
            checkPath = item.checkPath;
            assetWhiteRegex = item.assetWhiteRegex;
            objectWhiteRegex = item.objectWhiteRegex;
            parameter = item.parameter;
            dangerLevel = item.dangerLevel;
            effectEnable = item.effectEnable;
            cicdEnable = item.cicdEnable;
            isCheckSubFile = item.isCheckSubFile;
            remark = item.remark;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="data"></param>
        public CheckItemInfo(in string[] data)
        {
            if (data == null)
            {
                return;
            }

            guid = data[0];
            title = data[1];
            checkAssetType = (EffectToolData.AssetsType)Convert.ToInt32(data[2]);
            getAssetType = Convert.ToInt32(data[3]);
            checkOption = Convert.ToInt32(data[4]);
            checkPath = data[5];
            assetWhiteRegex = data[6];
            objectWhiteRegex = data[7];
            parameter = data[8];
            dangerLevel = Convert.ToInt32(data[9]);
            effectEnable = bool.Parse(data[10]);
            cicdEnable = bool.Parse(data[11]);
            isCheckSubFile = Convert.ToBoolean(data[12]);
            remark = data[13];
        }
    }
}