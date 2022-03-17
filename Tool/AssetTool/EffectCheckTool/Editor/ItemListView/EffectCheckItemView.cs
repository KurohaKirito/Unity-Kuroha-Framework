using System.Collections.Generic;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.ItemSetView;

namespace Kuroha.Tool.AssetTool.EffectCheckTool.Editor.ItemListView
{
    public static class EffectCheckItemView
    {
        /// <summary>
        /// 当前正在展示的检查项
        /// </summary>
        private static CheckItemInfo currentShowItemInfo;

        /// <summary>
        /// 检查项列表
        /// </summary>
        public static List<CheckItemInfo> CheckItemInfoList { get; set; }

        /// <summary>
        /// 新增检查项
        /// </summary>
        /// <param name="itemInfo">新检查项</param>
        /// <param name="isEditMode">是否是编辑模式</param>
        public static void SaveCheckItem(CheckItemInfo itemInfo, bool isEditMode)
        {
            // 读出配置
            var checkItemList = EffectCheckItemSetView.LoadConfig();

            if (isEditMode)
            {
                var checkItemIndex = int.Parse(itemInfo.guid) - 1;
                checkItemList[checkItemIndex] = itemInfo;
            }
            else
            {
                checkItemList.Add(itemInfo);
            }

            EffectCheckItemSetView.SaveConfig(checkItemList, isEditMode ? "修改成功" : "保存成功!");
            EffectCheckItemViewWindow.isRefresh = true;
        }

        /// <summary>
        /// 移除特定检查项
        /// </summary>
        /// <param name="info">检查项</param>
        public static void Remove(CheckItemInfo info)
        {
            // 这里并不能直接对 CheckItemInfoList 进行修改, 因为 Window 中有一个 Foreach 在对其遍历, 因此需要 Copy 一份数据
            var newCheckItemInfoList = new List<CheckItemInfo>(CheckItemInfoList);

            if (newCheckItemInfoList.Contains(info) == false)
            {
                return;
            }

            newCheckItemInfoList.Remove(info);
            EffectCheckItemSetView.SaveConfig(newCheckItemInfoList, "删除成功!");
            EffectCheckItemViewWindow.isRefresh = true;
        }
    }
}
