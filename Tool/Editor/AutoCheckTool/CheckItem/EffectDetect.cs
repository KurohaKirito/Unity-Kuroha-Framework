using System.Collections.Generic;
using Kuroha.Tool.Editor.EffectCheckTool.GUI;

public static class EffectDetect
{
    public static void Detect()
    {
        // 调用特效检测
        var reportInfos = EffectCheckToolGUI.Detect(true);
        
        // 整理数据
        foreach (var reportInfo in reportInfos)
        {
            var result = new Dictionary<string, string>
            {
                {"错误名称", "纹理尺寸超出限制大小"},
                {"资源路径", reportInfo.assetPath},
                {"错误等级", "Error"},
                {"负责人", "傅佳亿"},
                {"备注", $"人工确认并修复, {reportInfo.content}" }
            };

            AutoCheckTool.results.Add(result);
        }
    }
}
