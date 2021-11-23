using Kuroha.GUI.Editor;
using Kuroha.Tool.AssetTool.Editor.AssetBatchTool.BatchItem;
using Kuroha.Tool.AssetTool.Editor.AssetCheckTool;
using Kuroha.Tool.AssetTool.Editor.AssetSearchTool.GUI;
using Kuroha.Tool.AssetTool.Editor.AssetSearchTool.Searcher;
using Kuroha.Tool.AssetViewer.Editor;
using UnityEditor;

namespace Kuroha.Menu.Editor
{
    public class ToolMenu : UnityEditor.Editor
    {
        #region 日志开关

        [MenuItem("Kuroha/日志/开启", false, 0)]
        public static void OpenDebugLog()
        {
            Kuroha.Util.RunTime.DebugUtil.LogEnable = true;
        }
        
        [MenuItem("Kuroha/日志/开启", true, 0)]
        public static bool OpenDebugLogValidate()
        {
            return Kuroha.Util.RunTime.DebugUtil.LogEnable == false;
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

        #region 弹窗示例

        [MenuItem("Kuroha/消息弹窗示例/错误", false, 40)]
        public static void DialogError()
        {
            Dialog.SetListener(
                () => { Kuroha.Util.RunTime.DebugUtil.Log("错误: 您点击了确定按钮"); },
                () => { Kuroha.Util.RunTime.DebugUtil.Log("错误: 您点击了取消按钮"); },
                () => { Kuroha.Util.RunTime.DebugUtil.Log("错误: 您点击了功能按钮"); });
            Dialog.Display("这是弹窗的内容", Dialog.DialogType.Error, "确定按钮");
        }

        [MenuItem("Kuroha/消息弹窗示例/警告", false, 40)]
        public static void DialogWarning()
        {
            Dialog.SetListener(
                () => { Kuroha.Util.RunTime.DebugUtil.Log("警告: 您点击了确定按钮"); },
                () => { Kuroha.Util.RunTime.DebugUtil.Log("警告: 您点击了取消按钮"); },
                () => { Kuroha.Util.RunTime.DebugUtil.Log("警告: 您点击了功能按钮"); });
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

        [MenuItem("Kuroha/消息弹窗示例/消息", false, 40)]
        public static void DialogInfo()
        {
            // 弹窗使用方法:
            // 如果有需要在点击按钮后执行的回调, 则需要事先注册回调事件
            // 如果点击按钮后仅关闭窗口而已, 则不需要注册回调事件
            Dialog.Display("这是一条消息, 阅读完, 点击确定即可!", Dialog.DialogType.Message, "确定");
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
        
        [MenuItem("GameObject/LODTool", false, 12)]
        public static void Batch()
        {
            LodBatchWindow.Open();
        }

        #endregion
    }
}