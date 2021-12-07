using System.Collections.Generic;
using System.Threading.Tasks;
using Kuroha.Framework.Singleton;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.BugReport
{
    public class BugReport : Singleton<BugReport>
    {
        public static BugReport Instance => InstanceBase as BugReport;
        
        [Header("Trello API")] [SerializeField]
        private Trello trello;
        
        [Header("初始化成功标志")] [SerializeField]
        private bool initSuccess;

        [Header("用户密钥")] [SerializeField]
        private string trelloUserKey = "ac263348103d7880336bc34541819cfa";
        
        [Header("用户令牌")] [SerializeField]
        private string trelloUserToken = "be9d7b29f6141bb281afb08ed43a12862ecf26a803198a325d0f0dfe08856b70";
        
        [Header("看板名称")] [SerializeField]
        private string trelloUserTokenBoard = "魔剑镇魂曲";
        
        [Header("卡片列表 [可自动同步看板列表] [可自动创建新列表到看板]")] [SerializeField]
        private List<string> userListName;
        
        /// <summary>
        /// [Async] 初始化
        /// </summary>
        public sealed override async Task InitAsync()
        {
            if (trello == null)
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
            if (userListName.IsNullOrEmpty())
            {
                userListName = new List<string>();
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
        /// <param name="cardTitle">卡片标题</param>
        /// <param name="cardDescription">卡片描述</param>
        /// <param name="cardList">卡片所属列表</param>
        /// <returns></returns>
        public async Task<bool> ReportError(string cardTitle, string cardDescription, string cardList)
        {
            if (initSuccess == false)
            {
                return false;
            }
            
            var card = trello.NewCard(cardTitle, cardDescription, cardList);
            var pair = await trello.WebRequest_UploadNewUserCard(card);
            var newCardID = pair.Value;

            #region 上传附件代码示例

            // 上传附件 [截图]
            var screenshot = ScreenshotUtil.Instance.CaptureCameraShot(new Rect(0, 0, Screen.width, Screen.height), Camera.main);
            await trello.WebRequest_UploadAttachmentToCard_Image(newCardID, "ErrorScreenshot.png", screenshot);
            
            // 上传附件 [字符串]
            await trello.WebRequest_UploadAttachmentToCard_String(newCardID, "ErrorInfo.txt", "这里是详细的报错信息, 使用字符串的形式进行上传");
            
            // 上传附件 [文本类文件]
            // await trello.WebRequest_UploadAttachmentToCard_TextFile(newCardID, "这是报错日志.json", @"C:\Users\Kuroha\Desktop\Untitled-1.json");

            #endregion

            return true;
        }
    }
}
