using System.Collections.Generic;
using Kuroha.Framework.Singleton;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.BugReport
{
    public class BugReport : Singleton<BugReport>
    {
        /// <summary>
        /// Trello API
        /// </summary>
        [SerializeField]
        private Trello trello;
        
        /// <summary>
        /// 单例
        /// </summary>
        public static BugReport Instance => InstanceBase as BugReport;
        
        [Header("用户密钥")] [SerializeField]
        private string trelloUserKey = "ac263348103d7880336bc34541819cfa";
        
        [Header("用户令牌")] [SerializeField]
        private string trelloUserToken = "be9d7b29f6141bb281afb08ed43a12862ecf26a803198a325d0f0dfe08856b70";
        
        [Header("看板名称")] [SerializeField]
        private string trelloUserTokenBoard = "魔剑镇魂曲";
        
        [Header("卡片列表 [可自动同步看板列表] [可自动创建新列表到看板]")] [SerializeField]
        private List<string> userListName;
        
        /// <summary>
        /// 初始化 BugReport
        /// </summary>
        /// <returns></returns>
        public override async void OnLaunch()
        {
            DebugUtil.Log("初始化 Bug Report 中...", this, "yellow");
            
            // 初始化 Trello
            trello = new Trello(trelloUserKey, trelloUserToken);

            // 网络请求用户所有的看板
            await trello.WebRequest_GetUserAllBoards();

            // 设置当前看板
            trello.SetCurrentBoard(trelloUserTokenBoard);
            
            // 网络请求当前用户的当前看板下的全部列表
            await trello.WebRequest_GetUserAllLists();

            #region 自动同步看板列表

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

            #endregion
            
            #region 自动创建新列表到看板

            foreach (var listName in userListName)
            {
                if (trello.cachedUserLists.ContainsKey(listName) == false)
                {
                    var newList = trello.NewList(listName);
                    await trello.WebRequest_UploadNewUserList(newList);
                }
            }

            #endregion

            // 再次网络请求当前用户的当前看板下的全部列表
            await trello.WebRequest_GetUserAllLists();
            
            DebugUtil.Log("Bug Report 初始化完成!", this, "green");
        }

        /// <summary>
        /// 上传报错
        /// </summary>
        /// <param name="cardTitle">卡片标题</param>
        /// <param name="cardDescription">卡片描述</param>
        /// <param name="cardList">卡片所属列表</param>
        /// <returns></returns>
        public async void ReportError(string cardTitle, string cardDescription, string cardList)
        {
            // 新建卡片
            var card = trello.NewCard(cardTitle, cardDescription, cardList);

            // 上传卡片, 成功后返回 CardID
            var newCardID = await trello.WebRequest_UploadNewUserCard(card);
            DebugUtil.Log($"新建卡片: {cardTitle}, ID 为: {newCardID}", this, "green");

            // 上传附件 [截图]
            var screenshot = ScreenshotUtil.Instance.CaptureCameraShot(new Rect(0, 0, Screen.width, Screen.height), Camera.main);
            await trello.WebRequest_UploadAttachmentToCard_Image(newCardID, "ErrorScreenshot.png", screenshot);
            
            // 上传附件 [字符串]
            await trello.WebRequest_UploadAttachmentToCard_String(newCardID, "ErrorInfo.txt", "这里是详细的报错信息, 使用字符串的形式进行上传");
            
            // 上传附件 [文本类文件]
            await trello.WebRequest_UploadAttachmentToCard_TextFile(newCardID, "这是报错日志.json", @"C:\Users\Kuroha\Desktop\Untitled-1.json");
            
            DebugUtil.Log("报错上传成功!", this, "green");
        }
    }
}
