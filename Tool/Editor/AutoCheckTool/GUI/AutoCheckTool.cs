using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class AutoCheckTool
{
    [UnityEditor.MenuItem("Funny/CICD-AutoCheckTest")]
    public static void AutoCheck()
    {
        var results = new List<Dictionary<string, string>>();
        
        results.AddRange(EffectDetect.Check(false));

        ExportResult(results);
    }

    public static void ExportResult(in List<Dictionary<string,string>> results)
    {
        #if UNITY_2019_2_OR_NEWER == false
            
        // 将检测结果序列化为 json 文本
        var jsonList = new List<string>();
        foreach (var result in results)
        {
            // string 转 json
            var jsonUnicode = XUPorterJSON.MiniJSON.jsonEncode(result);
                
            // 将 UTF8 字符串解码为原字符
            var jsonStr = System.Text.RegularExpressions.Regex.Unescape(jsonUnicode);
            jsonList.Add(jsonStr);
        }
    
        // 将 json 文本写入文本文件
        var resultFilePath = StringUtil.Concat(Application.dataPath, "/AutoCheckResult.txt");
        File.WriteAllLines(resultFilePath, jsonList);
            
        #endif
    }
}