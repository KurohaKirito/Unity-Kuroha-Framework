using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kuroha.Util.RunTime;
using UnityEngine;
using UnityEngine.Networking;

namespace Kuroha.Framework.BugReport
{
    [Serializable]
    public class Trello
    {
        #region 网址

        private const string MEMBER_BASE_URL = "https://api.trello.com/1/members/me";
        private const string BOARD_BASE_URL = "https://api.trello.com/1/boards/";
        private const string LIST_BASE_URL = "https://api.trello.com/1/lists/";
        private const string CARD_BASE_URL = "https://api.trello.com/1/cards/";

        #endregion

        [Header("用户密钥")] [SerializeField]
        private string userKey;
        
        [Header("用户令牌")] [SerializeField]
        private string userToken;

        [Header("当前看板 ID")] [SerializeField]
        private string currentBoardId = string.Empty;
        
        [Header("当前用户全部的看板")] [SerializeField]
        private List<TrelloBoard> userAllBoards;
        
        [Header("当前用户的当前看板中的全部列表")] [SerializeField]
        private List<TrelloList> userAllLists;
        
        /// <summary>
        /// [临时变量] URI: Uniform Resource Identifier [统一资源标识符]
        /// URL: Uniform Resource Locator [统一资源定位符]
        /// URN: Uniform Resource Name [统一资源名称]
        /// </summary>
        private string uri = string.Empty;
        
        /// <summary>
        /// [缓存] 当前用户的当前看板中的全部列表 [Key: name] [Value: id]
        /// </summary>
        public readonly Dictionary<string, string> cachedUserLists = new Dictionary<string, string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public Trello(string key, string token)
        {
            userKey = key;
            userToken = token;
        }

        /// <summary>
        /// 从 Json 中取出特定名称的数组
        /// </summary>
        /// <returns></returns>
        private static string GetTrelloJson_Array(string originalJson, string arrayName)
        {
            var regex = new Regex("");
            switch (arrayName)
            {
                case "boards":
                    regex = new Regex(@"(?<=""boards"":)\[([^\]])+\]");
                    break;
                case "lists":
                    regex = new Regex(@"(?<=""lists"":)\[([^\]])+\]");
                    break;
            }
            var match = regex.Match(originalJson);
            
            var json = string.Empty;
            if (match is { Success: true })
            {
                json = @$"{{""data"":{match.Value}}}";
            }

            return json;
        }
        
        /// <summary>
        /// 从 Json 中取出特定名称的字符串
        /// </summary>
        /// <returns></returns>
        private static string GetTrelloJson_String(string originalJson, string stringName)
        {
            var regex = new Regex("");
            switch (stringName)
            {
                case "id":
                    regex = new Regex(@"(?<=""id"":"")([0-9,a-z,A-Z]+)(?="")");
                    break;
                case "board":
                    regex = new Regex(@"(?<=""board"":"")([0-9,a-z,A-Z]+)(?="")");
                    break;
            }
            var match = regex.Match(originalJson);
            
            var json = string.Empty;
            if (match is { Success: true })
            {
                json = match.Value;
            }

            return json;
        }
        
        /// <summary>
        /// 获取当前用户的全部看板
        /// </summary>
        public async Task<KeyValuePair<bool, string>> WebRequest_GetUserAllBoards()
        {
            var isSuccess = true;
            var message = string.Empty;
            
            uri = $"{MEMBER_BASE_URL}?key={userKey}&token={userToken}&boards=all";
            var request = UnityWebRequest.Get(uri);
            await request.SendWebRequest();

            var downloadText = request.downloadHandler.text;
            if (string.IsNullOrEmpty(downloadText))
            {
                isSuccess = false;
                message = request.error;
            }
            else
            {
                var json = GetTrelloJson_Array(downloadText, "boards");
                userAllBoards = JsonUtility.FromJson<JsonSerialization<TrelloBoard>>(json).ToList();
            }

            return new KeyValuePair<bool, string>(isSuccess, message);
        }
        
        /// <summary>
        /// 设置当前看板
        /// </summary>
        public void SetCurrentBoard(string name)
        {
            foreach (var board in userAllBoards)
            {
                if (board.name == name)
                {
                    currentBoardId = board.id;
                    return;
                }
            }
            
            DebugUtil.LogError("错误: 请填写正确的看板名称!", null, "red");
        }
        
        /// <summary>
        /// 获取当前用户的当前看板下的全部列表
        /// </summary>
        public async Task<KeyValuePair<bool, string>> WebRequest_GetUserAllLists()
        {
            var isSuccess = true;
            var message = string.Empty;
            
            uri = $"{BOARD_BASE_URL}{currentBoardId}?key={userKey}&token={userToken}&lists=all";
            var request = UnityWebRequest.Get(uri);
            
            await request.SendWebRequest();

            var downloadText = request.downloadHandler.text;
            if (string.IsNullOrEmpty(downloadText))
            {
                isSuccess = false;
                message = request.error;
            }
            else
            {
                var json = GetTrelloJson_Array(downloadText, "lists");
                userAllLists = JsonUtility.FromJson<JsonSerialization<TrelloList>>(json).ToList();
                CacheUserAllList();
            }
            
            return new KeyValuePair<bool, string>(isSuccess, message);
        }

        /// <summary>
        /// 缓存当前用户的当前看板下的全部列表
        /// </summary>
        private void CacheUserAllList()
        {
            foreach (var trelloList in userAllLists)
            {
                var listName = trelloList.name;
                var listID = trelloList.id;
                if (cachedUserLists.ContainsKey(listName) == false)
                {
                    cachedUserLists.Add(listName, listID);
                }
            }
        }
        
        /// <summary>
        /// 在当前用户的当前看板中上传一个新列表
        /// </summary>
        public async Task<KeyValuePair<bool, string>> WebRequest_UploadNewUserList(TrelloList list)
        {
            var isSuccess = true;
            string message;
            
            var post = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("name", list.name),
                new MultipartFormDataSection("idBoard", list.idBoard),
                new MultipartFormDataSection("pos", "bottom"),
            };

            uri = $"{LIST_BASE_URL}?key={userKey}&token={userToken}";
            var request = UnityWebRequest.Post(uri, post);
            await request.SendWebRequest();

            var downloadText = request.downloadHandler.text;
            if (string.IsNullOrEmpty(downloadText))
            {
                isSuccess = false;
                message = request.error;
            }
            else
            {
                message = GetTrelloJson_String(downloadText, "id");
            }
            
            return new KeyValuePair<bool, string>(isSuccess, message);
        }
        
        /// <summary>
        /// 新建一个列表
        /// </summary>
        /// <param name="title">列表标题</param>
        /// <param name="isOnRight">是否新增在所有列表的右侧</param>
        /// <returns></returns>
        public TrelloList NewList(string title, bool isOnRight = true)
        {
            var newList = new TrelloList
            {
                name = title,
                idBoard = currentBoardId,
                pos = isOnRight ? "bottom" : "top",
            };
            
            return newList;
        }
        
        /// <summary>
        /// 新建一张卡片
        /// </summary>
        /// <param name="title">卡片标题</param>
        /// <param name="description">卡片描述</param>
        /// <param name="listName">所属的列表名</param>
        /// <param name="isOnTop">是否添加在列表的顶部</param>
        /// <returns></returns>
        public TrelloCard NewCard(string title, string description, string listName, bool isOnTop = true)
        {
            string currentListId;

            if (cachedUserLists.ContainsKey(listName))
            {
                currentListId = cachedUserLists[listName];
            }
            else
            {
                DebugUtil.LogError($"未找到名为 {listName} 的列表, 请检查!", null, "red");
                return null;
            }

            var card = new TrelloCard
            {
                listID = currentListId,
                name = title,
                description = description,
                position = isOnTop ? "top" : "bottom",
            };
            
            return card;
        }
        
        /// <summary>
        /// 在当前用户的当前看板中的特定列表中上传一张新卡片
        /// </summary>
        public async Task<KeyValuePair<bool, string>> WebRequest_UploadNewUserCard(TrelloCard card)
        {
            var isSuccess = true;
            string message;
            
            var post = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("name", card.name),
                new MultipartFormDataSection("desc", card.description),
                new MultipartFormDataSection("pos", card.position),
                new MultipartFormDataSection("idList", card.listID),
            };
            
            uri = $"{CARD_BASE_URL}?key={userKey}&token={userToken}";
            var request = UnityWebRequest.Post(uri, post);
            await request.SendWebRequest();
            
            var downloadText = request.downloadHandler.text;
            if (string.IsNullOrEmpty(downloadText))
            {
                isSuccess = false;
                message = request.error;
            }
            else
            {
                message = GetTrelloJson_String(downloadText, "id");
            }
            
            return new KeyValuePair<bool, string>(isSuccess, message);
        }

        /// <summary>
        /// 上传附件到指定 ID 的卡片 [图片]
        /// </summary>
        /// <param name="cardId">指定的卡片 ID</param>
        /// <param name="attachmentFileName">附件文件名称</param>
        /// <param name="image">图片</param>
        /// <returns></returns>
        public async Task WebRequest_UploadAttachmentToCard_Image(string cardId, string attachmentFileName, Texture2D image)
        {
            var bytes = image.EncodeToPNG();
            await WebRequest_UploadAttachmentToCard_Bytes(cardId, attachmentFileName, bytes);
        }

        /// <summary>
        /// 上传附件到指定 ID 的卡片 [字符串]
        /// </summary>
        /// <param name="cardId">指定的卡片 ID</param>
        /// <param name="attachmentFileName">附件文件名称</param>
        /// <param name="text">文本字符串</param>
        /// <returns></returns>
        public async Task WebRequest_UploadAttachmentToCard_String(string cardId, string attachmentFileName, string text)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text.ToCharArray());
            await WebRequest_UploadAttachmentToCard_Bytes(cardId, attachmentFileName, bytes);
        }
        
        /// <summary>
        /// 上传附件到指定 ID 的卡片 [文本文件]
        /// </summary>
        /// <param name="cardId">指定的卡片 ID</param>
        /// <param name="attachmentFileName">附件文件名称</param>
        /// <param name="textFilePath">文本文件路径</param>
        /// <returns></returns>
        public async Task WebRequest_UploadAttachmentToCard_TextFile(string cardId, string attachmentFileName, string textFilePath)
        {
            var bytes = System.IO.File.ReadAllBytes(textFilePath);
            await WebRequest_UploadAttachmentToCard_Bytes(cardId, attachmentFileName, bytes);
        }
        
        /// <summary>
        /// 上传附件到指定 ID 的卡片 [字节流]
        /// </summary>
        /// <param name="cardId">指定的卡片 ID</param>
        /// <param name="attachmentFileName">附件文件名称</param>
        /// <param name="bytes">字节流</param>
        /// <returns></returns>
        private async Task WebRequest_UploadAttachmentToCard_Bytes(string cardId, string attachmentFileName, byte[] bytes)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", bytes, attachmentFileName, "text/plain")
            };
            
            uri = $"{CARD_BASE_URL}{cardId}/attachments?key={userKey}&token={userToken}";
            var request = UnityWebRequest.Post(uri, formData);
            await request.SendWebRequest();
        }
    }
}
