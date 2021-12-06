using System;
using UnityEditor;

namespace Kuroha.Util.Editor
{
    public static class UnityDefineUtil
    {
        /// <summary>
        /// 添加宏
        /// </summary>
        /// <param name="defineName">宏的名称</param>
        /// <param name="platform">目标平台</param>
        public static void AddDefine(string defineName, BuildTargetGroup platform)
        {
            if (IsDefine(defineName, platform, out var define) == false)
            {
                define += $";{defineName}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, define);
            }
        }
        
        /// <summary>
        /// 移除宏
        /// </summary>
        /// <param name="defineName">宏的名称</param>
        /// <param name="platform">目标平台</param>
        public static void RemoveDefine(string defineName, BuildTargetGroup platform)
        {
            if (IsDefine(defineName, platform, out var define1))
            {
                define1 = define1.Replace($";{defineName}", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, define1);
            }
            
            if (IsDefine(defineName, platform, out var define2))
            {
                define2 = define2.Replace($"{defineName}", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, define2);
            }
        }

        /// <summary>
        /// 检测是否定义了宏
        /// </summary>
        /// <param name="defineName">宏的名称</param>
        /// <param name="platform">目标平台</param>
        /// <param name="defines">当前宏内容</param>
        public static bool IsDefine(string defineName, BuildTargetGroup platform, out string defines)
        {
            defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);

            var index = defines.IndexOf(defineName, StringComparison.OrdinalIgnoreCase);

            return index >= 0;
        }
    }
}
