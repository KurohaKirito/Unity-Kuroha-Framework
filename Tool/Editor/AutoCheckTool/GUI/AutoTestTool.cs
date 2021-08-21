using System;
using System.IO;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

public static class AutoTestTool
{
    public static void AutoPlayUnity()
    {
        const string ANDROID_DEFINE = "AUTO_TEST";
        
        try
        {
            var savePath = $"{Application.dataPath}/PocoSDK";
            
            if (Directory.Exists(savePath) == false)
            {
                DebugUtil.LogError("PocoSDK 文件夹不存在!");
            }
            else
            {
                // 新增宏
                var defineSymbolsAndroid = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                if (defineSymbolsAndroid.IndexOf(ANDROID_DEFINE, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    defineSymbolsAndroid += $";{ANDROID_DEFINE}";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defineSymbolsAndroid);
                }
                
                // 播放 Unity
                if (EditorApplication.isPlaying == false)
                {
                    EditorApplication.isPlaying = true;
                    DebugUtil.Log("执行启动命令成功");
                }
            }
        }
        catch
        {
            DebugUtil.LogError("执行启动命令失败");
            throw;
        }
    }

    public static bool UnityIsPlay()
    {
        var isPlay = EditorApplication.isPlaying;
        
        return isPlay;
    }
}