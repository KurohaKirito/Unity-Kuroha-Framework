using System.Collections.Generic;
using System.IO;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

public static class AutoCheckTool
{
    /// <summary>
    /// 自动检测使用
    /// </summary>
    #if Kuroha == false
    [MenuItem("Funny/资源检测工具/Auto Check Tool")]
    #endif
    public static void AutoCheck()
    {
        var results = new List<Dictionary<string, string>>();
        
        // 执行特效检测工具
        results.AddRange(EffectDetect.Check(false));
        
        // 检测 Assets/Art/Effects/Textures 下纹理是否存在重复纹理
        results.AddRange(RepeatTextureDetect.Check(false));
        
        // 检测 Assets/Art/Effects/Textures 下纹理是否存在纯色纹理
        results.AddRange(SolidTextureDetect.Check(false));
        
        // 检测 Assets/Art/Effects/Textures 下是否存在无引用纹理
        results.AddRange(UnusedTextureDetect.Check(false));
        
        // 检测 Assets/Art/Effects/Textures 下的资源与文件夹同级问题
        results.AddRange(FolderAndAssetsDetect.Check("Assets/Art/Effects/Textures", 50, false));
        
        // 检测 Assets/Art/Effects/Materials 下是否存在无引用材质球
        results.AddRange(UnusedMaterialDetect.Check(false));
        
        // 检测 Assets/Art/Effects/Materials 下哪些材质球使用了 LWRP 着色器
        results.AddRange(MaterialShaderDetect.Check(false));
        
        // 检测 Assets/Art/Effects/Materials 下的资源与文件夹同级问题
        results.AddRange(FolderAndAssetsDetect.Check("Assets/Art/Effects/Materials", 50, false));

        // 检测 Assets/Art/Effects/Materials 下材质球的冗余纹理引用问题
        results.AddRange(RedundantTextureReferencesDetect.Check(false));

        ExportResult(results);
        
        DebugUtil.Log("Auto Check Completed!");
    }

    /// <summary>
    /// 导出检测结果到文件
    /// </summary>
    /// <param name="results"></param>
    public static void ExportResult(in List<Dictionary<string,string>> results)
    {
        #if Kuroha == false

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