using System.Collections.Generic;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.GUI;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.ItemListView;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.Report;

namespace Kuroha.Tool.AssetTool.EffectCheckTool.Editor.Check 
{
    public static class CheckAssetRoot
    {
        private static CheckTextureImporter checkTextureImporter;
        private static CheckModelImporter checkModelImporter;
        
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos)
        {
            switch (itemData.checkAssetType)
            {
                case EffectToolData.AssetsType.TextureImporter:
                {
                    checkTextureImporter = new CheckTextureImporter(itemData);
                    checkTextureImporter.Check(ref reportInfos);
                    break;
                }

                case EffectToolData.AssetsType.ModelImporter: {
                    checkModelImporter = new CheckModelImporter(itemData);
                    checkModelImporter.Check(ref reportInfos);
                    break;
                }
            }
        }
    }
}
