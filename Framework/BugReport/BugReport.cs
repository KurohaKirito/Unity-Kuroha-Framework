using System.Collections.Generic;
using System.Threading.Tasks;
using Kuroha.Framework.Singleton;
using Kuroha.Util.RunTime;
using UnityEngine;
using UnityEngine.UI;

namespace Kuroha.Framework.BugReport
{
    public class BugReport : Singleton<BugReport>
    {
        #region 编辑器 API

#if KUROHA_DEBUG_MODE

        [UnityEngine.Header("当前收集的全部日志")] [UnityEngine.SerializeField]
        private List<Kuroha.Framework.BugReport.UnityLog> logList;

        private void OnGUI()
        {
            logList ??= new List<Kuroha.Framework.BugReport.UnityLog>();
            unityLogDic ??= new Dictionary<int, Kuroha.Framework.BugReport.UnityLog>();

            logList.Clear();
            foreach (var value in unityLogDic.Values)
            {
                logList.Add(value);
            }
        }

#endif

        #endregion
        
        private Dictionary<int, UnityLog> unityLogDic;
        
        public static BugReport Instance => InstanceBase as BugReport;
        
        [Header("Trello API")]
        [SerializeField]
        private Trello trello;
        
        [Header("初始化成功标志")]
        [SerializeField]
        private bool initSuccess;

        [Header("用户密钥")]
        [SerializeField]
        private string trelloUserKey = "ac263348103d7880336bc34541819cfa";
        
        [Header("用户令牌")]
        [SerializeField]
        private string trelloUserToken = "be9d7b29f6141bb281afb08ed43a12862ecf26a803198a325d0f0dfe08856b70";
        
        [Header("看板名称")]
        [SerializeField]
        private string trelloUserTokenBoard = "魔剑镇魂曲";
        
        [Header("卡片列表 [可自动同步看板列表] [可自动创建新列表到看板]")]
        [SerializeField]
        private List<string> userListName;

        [Header("错误上传按钮")]
        [SerializeField]
        private Button bugReportButton;

        /// <summary>
        /// [Async] 初始化
        /// </summary>
        public sealed override async Task InitAsync()
        {
            unityLogDic ??= new Dictionary<int, UnityLog>();
            if (trello?.cachedUserLists == null || trello.cachedUserLists.Count <= 0)
            {
                trello = new Trello(trelloUserKey, trelloUserToken);
                
                var pair = await trello.WebRequest_GetUserAllBoards();
                if (pair.Key)
                {
                    trello.SetCurrentBoard(trelloUserTokenBoard);
                    pair = await trello.WebRequest_GetUserAllLists();
                    if (pair.Key)
                    {
                        SyncList();
                        pair = await CreateNewList();
                        if (pair.Key)
                        {
                            pair = await trello.WebRequest_GetUserAllLists();
                            if (pair.Key)
                            {
                                initSuccess = true;
                                RegisterLogCollect();
                                DebugUtil.Log("日志上报初始化完成", this, "green");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 同步看板列表
        /// </summary>
        private void SyncList()
        {
            userListName ??= new List<string>();
            
            if (userListName.Count <= 0)
            {
                foreach (var listName in trello.cachedUserLists.Keys)
                {
                    userListName.Add(listName);
                }
            }
            else
            {
                foreach (var listName in trello.cachedUserLists.Keys)
                {
                    if (userListName.Contains(listName) == false)
                    {
                        userListName.Add(listName);
                    }
                }
            }
        }

        /// <summary>
        /// 创建新列表到看板
        /// </summary>
        private async Task<KeyValuePair<bool, string>> CreateNewList()
        {
            var pair = new KeyValuePair<bool, string>(true, string.Empty);
            
            foreach (var listName in userListName)
            {
                if (trello.cachedUserLists.ContainsKey(listName) == false)
                {
                    var newList = trello.NewList(listName);
                    pair = await trello.WebRequest_UploadNewUserList(newList);
                    if (pair.Key == false)
                    {
                        return pair;
                    }
                }
            }
            
            return pair;
        }

        /// <summary>
        /// 上传报错
        /// </summary>
        private async void ReportError()
        {
            if (initSuccess)
            {
                foreach (var pair in unityLogDic)
                {
                    var card = trello.NewCard(pair.Key.ToString(), $"该日志一共出现了 {pair.Value.count} 次", "New_Bug");
                    var request = await trello.WebRequest_UploadNewUserCard(card);
                    var newCardID = request.Value;
                    
                    // 上传附件 [截图]
                    var screenshot = ScreenshotUtil.Instance.CaptureCameraShot(new Rect(0, 0, Screen.width, Screen.height), Camera.main);
                    await trello.WebRequest_UploadAttachmentToCard_Image(newCardID, "ErrorScreenshot.png", screenshot);
            
                    // 上传附件 [字符串]
                    await trello.WebRequest_UploadAttachmentToCard_String(newCardID, "ErrorInfo.txt", $"{pair.Value.condition}\r\n{pair.Value.stacktrace}");
            
                    // 上传附件 [文本类文件]
                    // await trello.WebRequest_UploadAttachmentToCard_TextFile(newCardID, "这是报错日志.json", @"C:\Users\Kuroha\Desktop\Untitled-1.json");
                }
                
                DebugUtil.Log("日志上传完成!", this, "green");
            }
        }
        
        /// <summary>
        /// 注册日志收集
        /// </summary>
        private void RegisterLogCollect()
        {
            Application.logMessageReceivedThreaded += ApplicationOnLogMessageReceived;
        }
        
        /// <summary>
        /// 日志收集
        /// </summary>
        /// <param name="condition">日志信息</param>
        /// <param name="stacktrace">堆栈跟踪</param>
        /// <param name="type">日志类型</param>
        private void ApplicationOnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            // 收集指定特征的异常
            if (type == LogType.Exception && condition.Contains("指定异常"))
            {
                var hash = condition.GetHashCode();
                if (unityLogDic.TryGetValue(hash, out var log))
                {
                    log.count++;
                }
                else
                {
                    unityLogDic[hash] = new UnityLog(condition, stacktrace, type);
                }
            }
        }
    }
}
