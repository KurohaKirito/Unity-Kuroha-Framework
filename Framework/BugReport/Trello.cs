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
        
        private List<object> cards;
        // private string currentListId = "";

        // Dictionary<ListName, listId>
        public readonly Dictionary<string, string> cachedLists = new Dictionary<string, string>();

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

            var url = $"{MEMBER_BASE_URL}?key={userKey}&token={userToken}&boards=all";
            var request = UnityWebRequest.Get(url);
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

            var url = $"{BOARD_BASE_URL}{currentBoardId}?key={userKey}&token={userToken}&lists=all";
            var request = UnityWebRequest.Get(url);
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
                        if (cachedLists.ContainsKey(listName) == false)
                        {
                            cachedLists.Add(listName, listID);
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

            var url = $"{LIST_BASE_URL}?key={userKey}&token={userToken}";
            var request = UnityWebRequest.Post(url, post);
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

            if (cachedLists.ContainsKey(listName))
            {
                currentListId = cachedLists[listName];
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
            
            var url = $"{CARD_BASE_URL}?key={userKey}&token={userToken}";
            var request = UnityWebRequest.Post(url, post);
            yield return request.SendWebRequest();
            
            if (Json.Deserialize(request.downloadHandler.text) is Dictionary<string, object> dict)
            {
                yield return dict["id"].ToString();
            }
        }

        // 设置
        public IEnumerator SetUpAttachmentInCardRoutine(string cardId, string attachmentName, Texture2D image)
        {
            var bytes = image.EncodeToPNG();
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", bytes, attachmentName, "image/png")
            };
            var www = UnityWebRequest.Post(CARD_BASE_URL + cardId + "/attachments" + "?" + "key=" + userKey + "&token=" + userToken, formData);
            yield return www.SendWebRequest();
        }

        // 设置
        public IEnumerator SetUpAttachmentInCardRoutine(string cardId, string attachmentName, string data)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(data.ToCharArray());
            
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", bytes, attachmentName, "text/plain")
            };
            var www = UnityWebRequest.Post(CARD_BASE_URL + cardId + "/attachments" + "?" + "key=" + userKey + "&token=" + userToken, formData);
            yield return www.SendWebRequest();
        }
        
        // 设置
        public IEnumerator SetUpAttachmentInCardFromFileRoutine(string cardId, string attachmentName, string path)
        {
            Debug.Assert(System.IO.File.Exists(path), "The path to the log file specified is not correct");

            var bytes = System.IO.File.ReadAllBytes(path);

            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", bytes, attachmentName, "text/plain")
            };
            
            var www = UnityWebRequest.Post(CARD_BASE_URL + cardId + "/attachments" + "?" + "key=" + userKey + "&token=" + userToken, formData);
            yield return www.SendWebRequest();
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        

        
        
        
        
        
        // private void CheckWwwStatus(string errorMessage, WWW www)
        // {
        //     if (!string.IsNullOrEmpty(www.error))
        //     {
        //         throw new System.Exception(errorMessage + ": " + www.error);
        //     }
        // }
        
        // public IEnumerator PopulateCardsFromListRoutine(string listId)
        // {
        //     cards = null;
        //     if (listId == "")
        //     {
        //         throw new System.Exception("Cannot retreive the cards, you have not selected a list yet.");
        //     }
        //
        //     WWW www = new WWW(LIST_BASE_URL + listId + "?" + "key=" + key + "&token=" + token + "&cards=all");
        //
        //     yield return www;
        //     CheckWwwStatus("Something went wrong: ", www);
        //
        //     var dict = Json.Deserialize(www.text) as Dictionary<string, object>;
        //     cards = (List<object>)dict["cards"];
        // }

        // public TrelloCard UploadExceptionCard(System.Exception e)
        // {
        //     var card = NewCard("Bug");
        //     card.name = e.GetType().ToString();
        //     card.description = e.Message;
        //     return card; // uploadCard(card);
        // }
    }
}
