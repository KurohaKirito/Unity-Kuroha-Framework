using System;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.GUI;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemListView
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
        /// 待检查资源的类型
        /// </summary>
        public EffectToolData.AssetsType assetsType;

        /// <summary>
        /// 子检查项类型
        /// </summary>
        public int checkType;

        /// <summary>
        /// 子检查项参数
        /// </summary>
        public string parameter;

        /// <summary>
        /// 待检查资源的类型路径
        /// </summary>
        public string path;
        
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
        /// 检测子目录标志
        /// </summary>
        public bool isCheckSubFile;

        /// <summary>
        /// 检查项的备注
        /// </summary>
        public string remark;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CheckItemInfo(string guid, string title, EffectToolData.AssetsType assetsType, int checkType, string path,
            string assetWhiteRegex, string objectWhiteRegex, string parameter, int dangerLevel,
            bool effectEnable, bool cicdEnable, bool isCheckSubFile, string remark)
        {
            this.guid = guid;
            this.title = title;
            this.assetsType = assetsType;
            this.checkType = checkType;
            this.path = path;
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
            assetsType = item.assetsType;
            checkType = item.checkType;
            path = item.path;
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
            assetsType = (EffectToolData.AssetsType)Convert.ToInt32(data[2]);
            checkType = Convert.ToInt32(data[3]);
            path = data[4];
            assetWhiteRegex = data[5];
            objectWhiteRegex = data[6];
            parameter = data[7];
            dangerLevel = Convert.ToInt32(data[8]);
            effectEnable = bool.Parse(data[9]);
            cicdEnable = bool.Parse(data[10]);
            isCheckSubFile = Convert.ToBoolean(data[11]);
            remark = data[12];
        }
    }
}