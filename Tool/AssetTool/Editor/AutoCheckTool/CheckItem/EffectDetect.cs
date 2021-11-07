using System.Collections.Generic;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.GUI;
using Kuroha.Util.RunTime;

public static class EffectDetect
{
    /// <summary>
    /// 自动检测使用
    /// </summary>
    public static void Detect()
    {
        Check();
    }

    /// <summary>
    /// 执行检测
    /// </summary>
    /// <param name="isExportFile">是否导出文件, 默认导出文件</param>
    public static List<Dictionary<string, string>> Check(bool isExportFile = true)
    {
        var results = new List<Dictionary<string, string>>();
        
        // 调用特效检测
        var reportInfos = EffectCheckToolGUI.Detect(true);
        
        // 整理数据
        foreach (var reportInfo in reportInfos)
        {
            var length = reportInfo.content.IndexOf(',');
            
            if (length <= 0)
            {
                length = reportInfo.content.IndexOf('!');
            }
            if (length <= 0)
            {
                length = reportInfo.content.IndexOf(':');
            }
            
            var errorTitle = reportInfo.content.Substring(0, length);
            
            var result = new Dictionary<string, string>
            {
                {"错误名称", errorTitle},
                {"资源路径", reportInfo.assetPath},
                {"错误等级", "Error"},
                {"负责人", "傅佳亿"},
                {"备注", "可使用资源检测工具中的特效检测工具查看详细信息" }
            };

            results.Add(result);
        }

        if (isExportFile)
        {
            AutoCheckToolGUI.ExportResult(results);
            DebugUtil.Log("Effect Check Completed!");
        }

        return results;
    }
}
