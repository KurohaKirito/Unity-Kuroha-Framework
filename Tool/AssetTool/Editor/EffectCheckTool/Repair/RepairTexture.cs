using System;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report;
using UnityEditor;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Repair
{
    public static class RepairTexture
    {
        /// <summary>
        /// 自动修复问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">问题项</param>
        public static void Repair(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modeType = (CheckTexture.CheckOptions) effectCheckReportInfo.modeType;

            switch (modeType)
            {
                case CheckTexture.CheckOptions.Size:
                    break;
                
                case CheckTexture.CheckOptions.MipMaps:
                    RepairMipMaps(effectCheckReportInfo);
                    break;
                
                case CheckTexture.CheckOptions.ReadWriteEnable:
                    RepairReadWrite(effectCheckReportInfo);
                    break;
                
                case CheckTexture.CheckOptions.CompressFormat:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// 修复 Mip Maps
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairMipMaps(EffectCheckReportInfo effectCheckReportInfo)
        {
            var enable = Convert.ToBoolean(effectCheckReportInfo.parameter);

            var textureImporter = AssetImporter.GetAtPath(effectCheckReportInfo.assetPath) as TextureImporter;

            if (ReferenceEquals(textureImporter, null) == false)
            {
                if (textureImporter.mipmapEnabled != enable)
                {
                    textureImporter.mipmapEnabled = enable;
                    AssetDatabase.ImportAsset(effectCheckReportInfo.assetPath);
                    EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                }
            }
        }
        
        /// <summary>
        /// 修复 Read Write
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairReadWrite(EffectCheckReportInfo effectCheckReportInfo)
        {
            var enable = Convert.ToBoolean(effectCheckReportInfo.parameter);
            var textureImporter = AssetImporter.GetAtPath(effectCheckReportInfo.assetPath) as TextureImporter;

            if (ReferenceEquals(textureImporter, null) == false)
            {
                if (textureImporter.isReadable != enable)
                {
                    textureImporter.isReadable = enable;
                    AssetDatabase.ImportAsset(effectCheckReportInfo.assetPath);
                    EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                }
            }
        }
    }
}
