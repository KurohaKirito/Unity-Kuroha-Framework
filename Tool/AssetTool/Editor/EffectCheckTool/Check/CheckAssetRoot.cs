using System.Collections.Generic;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.GUI;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Check 
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
