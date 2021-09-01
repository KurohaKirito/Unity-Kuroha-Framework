#if Kuroha
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.AssetCheckTool;
using Kuroha.Tool.Editor.AssetSearchTool.GUI;
using Kuroha.Tool.Editor.AssetSearchTool.Searcher;
using Kuroha.Tool.Editor.AssetViewer;
using UnityEditor;

namespace Kuroha.Editor
{
    public class KurohaToolMenu : UnityEditor.Editor
    {
        [MenuItem("Kuroha/开启日志输出", false, 0)]
        public static void OpenDebugLog()
        {
            Kuroha.Util.Release.DebugUtil.LogEnable = true;
        }

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

        [MenuItem("Kuroha/消息弹窗/错误", false, 40)]
        public static void DialogError()
        {
            Dialog.SetListener(
                () => { Kuroha.Util.Release.DebugUtil.Log("错误: 您点击了确定按钮"); },
                () => { Kuroha.Util.Release.DebugUtil.Log("错误: 您点击了取消按钮"); },
                () => { Kuroha.Util.Release.DebugUtil.Log("错误: 您点击了功能按钮"); });
            Dialog.Display("这是弹窗的内容", Dialog.DialogType.Error, "确定按钮");
        }

        [MenuItem("Kuroha/消息弹窗/警告", false, 40)]
        public static void DialogWarning()
        {
            Dialog.SetListener(
                () => { Kuroha.Util.Release.DebugUtil.Log("警告: 您点击了确定按钮"); },
                () => { Kuroha.Util.Release.DebugUtil.Log("警告: 您点击了取消按钮"); },
                () => { Kuroha.Util.Release.DebugUtil.Log("警告: 您点击了功能按钮"); });
            Dialog.Display(
                "消息内容: 当前一共检测出了 1035 个文件!  " +
                "消息内容: 当前一共检测出了 1035 个文件!  " +
                "消息内容: 当前一共检测出了 1035 个文件!  " +
                "消息内容: 当前一共检测出了 1035 个文件!  ",
                Dialog.DialogType.Warn,
                "OK",
                "Cancel",
                "Alt");
        }

        [MenuItem("Kuroha/消息弹窗/消息", false, 40)]
        public static void DialogInfo()
        {
            // 弹窗使用方法:
            // 如果有需要在点击按钮后执行的回调, 则需要事先注册回调事件
            // 如果点击按钮后仅关闭窗口而已, 则不需要注册回调事件
            Dialog.Display("这是一条消息, 阅读完, 点击确定即可!", Dialog.DialogType.Message, "确定");
        }

        [MenuItem("Kuroha/资源分析工具", false, 60)]
        public static void AssetAnalysis()
        {
            AssetCheckToolWindow.Open();
        }

        [MenuItem("Kuroha/资源查找工具", false, 60)]
        public static void AssetSearchTool()
        {
            AssetSearchWindow.Open(0);
        }

        [MenuItem("Assets/查找资源引用")]
        public static void FindAssetReference()
        {
            ReferenceSearcher.OpenWindow();
        }
    }
}
#endif