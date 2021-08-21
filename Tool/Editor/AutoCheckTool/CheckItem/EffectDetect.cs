using System.Collections.Generic;
using Kuroha.Tool.Editor.EffectCheckTool.GUI;

public static class EffectDetect
{
    public static void Detect(List<Dictionary<string, string>> results)
    {
        var reportInfos = EffectCheckToolGUI.Detect(true);
        
        foreach (var reportInfo in reportInfos)
        {
            results.Add(new Dictionary<string, string>
            {
                { "错误名称", "纹理尺寸超出限制大小" },
                { "资源路径", reportInfo.assetPath },
                { "错误等级", "Error" },
                { "负责人", "傅佳亿" },
                { "备注", $"人工确认并修复, {reportInfo.content}" }
            });
        }
    }
}
