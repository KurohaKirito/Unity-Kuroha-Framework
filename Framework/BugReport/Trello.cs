using System.Collections;
using System.Collections.Generic;
using Kuroha.Util.RunTime;
using MiniJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace Kuroha.Framework.BugReport
{
    public class Trello
    {
        #region 网址

        private const string MEMBER_BASE_URL = "https://api.trello.com/1/members/me";
        private const string BOARD_BASE_URL = "https://api.trello.com/1/boards/";
        private const string LIST_BASE_URL = "https://api.trello.com/1/lists/";
        private const string CARD_BASE_URL = "https://api.trello.com/1/cards/";

        #endregion
        
        /// <summary>
        /// 用户密钥
        /// </summary>
        private readonly string userKey;
        
        /// <summary>
        /// 用户令牌
        /// </summary>
        private readonly string userToken;

        /// <summary>
        /// URI: Uniform Resource Identifier [统一资源标识符]
        /// URL: Uniform Resource Locator [统一资源定位符]
        /// URN: Uniform Resource Name [统一资源名称]
        /// </summary>
        private string uri = string.Empty;
        
        /// <summary>
        /// 当前看板 ID
        /// </summary>
        private string currentBoardId = string.Empty;
        
        /// <summary>
        /// 当前用户全部的看板
        /// </summary>
        private List<object> userAllBoards;
        
        /// <summary>
        /// 当前用户的当前看板中的全部列表
        /// </summary>
        private List<object> userAllLists;
        
        /// <summary>
        /// [缓存] 当前用户的当前看板中的全部列表
        /// Key: 列表名
        /// Value: 列表 ID
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
        /// 获取当前用户的全部看板
        /// </summary>
        public IEnumerator WebRequest_GetUserAllBoards()
        {
            userAllBoards = null;

            uri = $"{MEMBER_BASE_URL}?key={userKey}&token={userToken}&boards=all";
            var request = UnityWebRequest.Get(uri);
            yield return request.SendWebRequest();

            var requestResult = request.downloadHandler.text;
            if (Json.Deserialize(requestResult) is Dictionary<string, object> dict)
            {
                userAllBoards = dict["boards"] as List<object>;
            }
        }
        
        /// <summary>
        /// 设置当前看板
        /// </summary>
        public string SetCurrentBoard(string name)
        {
            currentBoardId = string.Empty;
            
            for (var index = 0; index < userAllBoards.Count; ++index)
            {
                if (userAllBoards[index] is Dictionary<string, object> board)
                {
                    if (board["name"].ToString() == name)
                    {
                        currentBoardId = board["id"].ToString();
                        return currentBoardId;
                    }
                }
            }
            
            DebugUtil.LogError("错误: 请填写正确的看板名称!", null, "red");
            
            return currentBoardId;
        }
        
        /// <summary>
        /// 获取当前用户的当前看板下的全部列表
        /// </summary>
        public IEnumerator WebRequest_GetUserAllLists()
        {
            userAllLists = null;

            uri = $"{BOARD_BASE_URL}{currentBoardId}?key={userKey}&token={userToken}&lists=all";
            var request = UnityWebRequest.Get(uri);
            yield return request.SendWebRequest();
            
            var requestResult = request.downloadHandler.text;
            if (Json.Deserialize(requestResult) is Dictionary<string, object> dict)
            {
                userAllLists = dict["lists"] as List<object>;
            }

            CacheUserAllList();
        }

        /// <summary>
        /// 缓存当前用户的当前看板下的全部列表
        /// </summary>
        private void CacheUserAllList()
        {
            if (userAllLists != null)
            {
                for (var index = 0; index < userAllLists.Count; ++index)
                {
                    if (userAllLists[index] is Dictionary<string, object> list)
                    {
                        var listName = list["name"].ToString();
                        var listID = list["id"].ToString();
                        if (cachedUserLists.ContainsKey(listName) == false)
                        {
                            cachedUserLists.Add(listName, listID);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 在当前用户的当前看板中上传一个新列表
        /// </summary>
        public IEnumerator WebRequest_UploadNewUserList(TrelloList list)
        {
            var post = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("name", list.name),
                new MultipartFormDataSection("idBoard", list.boardID),
                new MultipartFormDataSection("pos", list.position),
            };

            uri = $"{LIST_BASE_URL}?key={userKey}&token={userToken}";
            var request = UnityWebRequest.Post(uri, post);
            yield return request.SendWebRequest();

            if (Json.Deserialize(request.downloadHandler.text) is Dictionary<string, object> dict)
            {
                yield return dict["id"].ToString();
            }
        }
        
        /// <summary>
        /// 新建一张卡片
        /// </summary>
        /// <param name="title">卡片标题</param>
        /// <param name="description">卡片描述</param>
        /// <param name="listName">所属的列表名</param>
        /// <param name="newCardsOnTop">是否添加在列表的顶部</param>
        /// <returns></returns>
        public TrelloCard NewCard(string title, string description, string listName, bool newCardsOnTop = true)
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
                position = newCardsOnTop ? "top" : "bottom",
            };
            
            return card;
        }
        
        /// <summary>
        /// 在当前用户的当前看板中的特定列表中上传一张新卡片
        /// </summary>
        public IEnumerator WebRequest_UploadNewUserCard(TrelloCard card)
        {
            var post = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("name", card.name),
                new MultipartFormDataSection("desc", card.description),
                new MultipartFormDataSection("pos", card.position),
                new MultipartFormDataSection("due", card.due),
                new MultipartFormDataSection("idList", card.listID),
            };
            
            uri = $"{CARD_BASE_URL}?key={userKey}&token={userToken}";
            var request = UnityWebRequest.Post(uri, post);
            yield return request.SendWebRequest();
            
            if (Json.Deserialize(request.downloadHandler.text) is Dictionary<string, object> dict)
            {
                yield return dict["id"].ToString();
            }
        }

        /// <summary>
        /// 上传附件到指定 ID 的卡片 [图片]
        /// </summary>
        /// <param name="cardId">指定的卡片 ID</param>
        /// <param name="attachmentFileName">附件文件名称</param>
        /// <param name="image">图片</param>
        /// <returns></returns>
        public IEnumerator WebRequest_UploadAttachmentToCard_Image(string cardId, string attachmentFileName, Texture2D image)
        {
            var bytes = image.EncodeToPNG();
            yield return WebRequest_UploadAttachmentToCard_Bytes(cardId, attachmentFileName, bytes);
        }

        /// <summary>
        /// 上传附件到指定 ID 的卡片 [字符串]
        /// </summary>
        /// <param name="cardId">指定的卡片 ID</param>
        /// <param name="attachmentFileName">附件文件名称</param>
        /// <param name="text">文本字符串</param>
        /// <returns></returns>
        public IEnumerator WebRequest_UploadAttachmentToCard_String(string cardId, string attachmentFileName, string text)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text.ToCharArray());
            yield return WebRequest_UploadAttachmentToCard_Bytes(cardId, attachmentFileName, bytes);
        }
        
        /// <summary>
        /// 上传附件到指定 ID 的卡片 [文本文件]
        /// </summary>
        /// <param name="cardId">指定的卡片 ID</param>
        /// <param name="attachmentFileName">附件文件名称</param>
        /// <param name="textFilePath">文本文件路径</param>
        /// <returns></returns>
        public IEnumerator WebRequest_UploadAttachmentToCard_TextFile(string cardId, string attachmentFileName, string textFilePath)
        {
            var bytes = System.IO.File.ReadAllBytes(textFilePath);
            yield return WebRequest_UploadAttachmentToCard_Bytes(cardId, attachmentFileName, bytes);
        }
        
        /// <summary>
        /// 上传附件到指定 ID 的卡片 [字节流]
        /// </summary>
        /// <param name="cardId">指定的卡片 ID</param>
        /// <param name="attachmentFileName">附件文件名称</param>
        /// <param name="bytes">字节流</param>
        /// <returns></returns>
        private IEnumerator WebRequest_UploadAttachmentToCard_Bytes(string cardId, string attachmentFileName, byte[] bytes)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", bytes, attachmentFileName, "text/plain")
            };
            
            uri = $"{CARD_BASE_URL}{cardId}/attachments?key={userKey}&token={userToken}";
            var request = UnityWebRequest.Post(uri, formData);
            yield return request.SendWebRequest();
        }
    }
}
