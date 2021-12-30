using System.Collections.Generic;
using Kuroha.GUI.Editor;
using Kuroha.Tool.AssetTool.Editor.AutoCheckTool.GUI;
using Kuroha.Tool.AssetTool.Editor.TextureAnalysisTool;
using Kuroha.Util.Editor;
using Kuroha.Util.RunTime;

namespace Kuroha.Tool.AssetTool.Editor.AutoCheckTool.CheckItem
{
    public static class RepeatTextureDetect
    {
        /// <summary>
        /// 执行检测
        /// </summary>
        /// <param name="path">检测路径</param>
        /// <param name="principal">负责人</param>
        /// <param name="isExportFile">是否导出文件, 默认导出文件</param>
        public static AutoCheckToolCheckResult Check(string path, string principal, bool isExportFile = true)
        {
            var results = new AutoCheckToolCheckResult();
        
            // 获取全部纹理
            TextureUtil.GetTexturesInPath(new[] { path }, out var textures, out var texturePaths);

            // 检测每一张纹理
            for (var index = 0; index < textures.Count; index++)
            {
                ProgressBar.DisplayProgressBar("重复纹理检测工具", $"纹理检测中: {index + 1}/{textures.Count}", index + 1, textures.Count);
            
                var isBegin = index == 0;
                
                // 判断后缀
                if (texturePaths[index].EndsWith(".png") == false &&
                    texturePaths[index].EndsWith(".tga") == false) {
                    DebugUtil.Log($"文件后缀非法: {texturePaths[index]}");
                }
                
                TextureRepeatChecker.CheckOneTexture(texturePaths[index], isBegin);
            }
        
            // 处理重复纹理的检测结果数据
            var repeatTextureInfos = TextureRepeatChecker.GetResult();
            foreach (var repeatTextureInfo in repeatTextureInfos)
            {
                var result = new Dictionary<string, string>
                {
                    { "错误名称", "存在一组重复纹理" },
                    { "资源路径", repeatTextureInfo.assetPaths[0] },
                    { "错误等级", "Error" },
                    { "负责人", principal },
                    { "备注", "请使用贴图统计分析工具查看详细的检测结果, 进行人工确认并修复!" }
                };
            
                results.resultList.Add(result);
            }
        
            if (isExportFile)
            {
                AutoCheckToolGUI.ExportResult(results);
                DebugUtil.Log("Repeat Texture Check Completed!");
            }

            return results;
        }
    }
}
