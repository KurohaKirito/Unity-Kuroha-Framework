using System;
using System.Collections.Generic;
using System.IO;
using Kuroha.Util.Release;
using UnityEngine;

#if UNITY_2019_2_OR_NEWER == false
using System.Text.RegularExpressions;
#endif

public static class AutoCheckTool
{
    /// <summary>
    /// 检查项
    /// </summary>
    private static event Action detectItems;

    /// <summary>
    /// 检测结果
    /// </summary>
    public static readonly List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

    /// <summary>
    /// CICD 检测项初始化
    /// </summary>
    private static void Init()
    {
        detectItems += EffectDetect.Detect;
        detectItems += SolidTextureDetect.Detect;
        detectItems += RepeatTextureDetect.Detect;

        detectItems += FbxUVDetect.Detect;
        detectItems += UnusedTextureDetect.Detect;
        detectItems += UnusedModelDetect.Detect;
        detectItems += UnusedMaterialDetect.Detect;
    }
    
    // [MenuItem("Funny/AutoCheckTest")]
    public static void AutoCheck()
    {
        try
        {
            if (detectItems == null)
            {
                Init();
            }
            results.Clear();
    
            // 检测
            detectItems?.Invoke();

            #if UNITY_2019_2_OR_NEWER == false
            
            // 将检测结果序列化为 json 文本
            var resultList = new List<string>();
            foreach (var result in results)
            {
                // string 转 json
                var jsonUnicode = XUPorterJSON.MiniJSON.jsonEncode(result);
                
                // 
                var jsonStr = Regex.Unescape(jsonUnicode);
                resultList.Add(jsonStr);
            }
    
            // 将 json 文本写入文本文件
            var resultFilePath = StringUtil.Concat(Application.dataPath, "/CICDDetectResult.txt");
            File.WriteAllLines(resultFilePath, resultList);
            
            #endif
            
            DebugUtil.Log("CICD Detect Completed!");
        }
        catch (Exception e)
        {
            DebugUtil.Log($"CICD Detect Error: {e}");
            throw;
        }
    }
}