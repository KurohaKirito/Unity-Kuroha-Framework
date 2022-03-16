using System.Reflection;
using Kuroha.Framework.Utility.Editor;
using Kuroha.Framework.Utility.RunTime;
using Kuroha.Tool.AssetSearchTool.Editor.GUI;
using Kuroha.Tool.AssetSearchTool.Editor.Searcher;
using Kuroha.Tool.AssetTool.AssetCheckTool.Editor;
using Kuroha.Tool.AssetViewer.Editor;
using UnityEditor;

namespace Kuroha.Tool.ToolMenu.Editor
{
    public class ToolMenu : UnityEditor.Editor
    {
        #region 日志开关

        [MenuItem("Kuroha/日志/开启", false, 0)]
        public static void OpenDebugLog()
        {
            DebugUtil.LogEnable = true;
        }

        [MenuItem("Kuroha/日志/开启", true, 0)]
        public static bool OpenDebugLogValidate()
        {
            return DebugUtil.LogEnable == false;
        }

        [MenuItem("Kuroha/日志/关闭", false, 0)]
        public static void CloseDebugLog()
        {
            DebugUtil.LogEnable = false;
        }

        [MenuItem("Kuroha/日志/关闭", true, 0)]
        public static bool CloseDebugLogValidate()
        {
            return DebugUtil.LogEnable;
        }

        [MenuItem("Kuroha/日志/清空", false, 0)]
        public static void ClearDebugLog()
        {
            var dynamicAssembly = ReflectionUtil.GetAssembly(typeof(SceneView));
            var dynamicClass = ReflectionUtil.GetClass(dynamicAssembly, "UnityEditor.LogEntries");

            // public static extern void Clear();
            var dynamicMethod = ReflectionUtil.GetMethod(dynamicClass, "Clear", BindingFlags.Public | BindingFlags.Static);
            ReflectionUtil.CallMethod(dynamicMethod, null);
        }

        #endregion

        #region 调试开关

        [MenuItem("Kuroha/调试模式/开启", false, 1)]
        public static void OpenKurohaDebugMode()
        {
            UnityDefineUtil.AddDefine("KUROHA_DEBUG_MODE", BuildTargetGroup.Standalone);
        }

        [MenuItem("Kuroha/调试模式/开启", true, 1)]
        public static bool OpenKurohaDebugModeValidate()
        {
            return UnityDefineUtil.IsDefine("KUROHA_DEBUG_MODE", BuildTargetGroup.Standalone, out _) == false;
        }

        [MenuItem("Kuroha/调试模式/关闭", false, 1)]
        public static void CloseKurohaDebugMode()
        {
            UnityDefineUtil.RemoveDefine("KUROHA_DEBUG_MODE", BuildTargetGroup.Standalone);
        }

        [MenuItem("Kuroha/调试模式/关闭", true, 1)]
        public static bool CloseKurohaDebugModeValidate()
        {
            return UnityDefineUtil.IsDefine("KUROHA_DEBUG_MODE", BuildTargetGroup.Standalone, out _);
        }

        #endregion

        #region 图标工具

        [MenuItem("Kuroha/UnityIcon/显示所有图标", false, 20)]
        public static void DisplayAllIcon()
        {
            UnityIcon.Open();
        }

        [MenuItem("Kuroha/UnityIcon/调整窗口大小", false, 20)]
        public static void EditWindowSize()
        {
            SizeEdit.Open();
        }

        #endregion

        #region 工具

        [MenuItem("Kuroha/Asset Check Tool", false, 60)]
        public static void AssetAnalysis()
        {
            AssetCheckToolWindow.Open();
        }

        [MenuItem("Kuroha/Asset Search Tool", false, 60)]
        public static void AssetSearchTool()
        {
            AssetSearchWindow.Open(0);
        }

        #endregion

        #region 右键菜单

        [MenuItem("Assets/Find All Reference")]
        public static void FindAssetReference()
        {
            ReferenceSearcher.OpenWindow();
        }

        #endregion
    }
}
