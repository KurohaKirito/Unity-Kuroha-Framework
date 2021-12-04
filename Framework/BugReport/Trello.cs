using System.Collections;
using System.Collections.Generic;
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
        
        private readonly string token;
        private readonly string key;
        
        /// <summary>
        /// 当前用户全部的看板
        /// </summary>
        private List<object> boards;
        
        private List<object> lists;
        private List<object> cards;
        private string currentBoardId = "";
        // private string currentListId = "";

        // Dictionary<ListName, listId>
        public readonly Dictionary<string, string> cachedLists = new Dictionary<string, string>();

        public Trello(string key, string token)
        {
            this.key = key;
            this.token = token;
        }
        
        public void SetCurrentBoard(string name)
        {
            if (boards == null)
            {
                throw new System.Exception("You have not yet populated the list of boards, so one cannot be selected.");
            }

            for (var i = 0; i < boards.Count; i++)
            {
                var board = (Dictionary<string, object>)boards[i];
                if ((string)board["name"] == name)
                {
                    currentBoardId = (string)board["id"];
                    return;
                }
            }

            currentBoardId = "";
            throw new System.Exception("No such board found.");
        }
        
        public TrelloCard NewCard(string title, string description, string listName, bool newCardsOnTop = true)
        {
            string currentListId;

            if (cachedLists.ContainsKey(listName))
            {
                currentListId = cachedLists[listName];
            }
            else
            {
                throw new System.Exception("List specified not found.");
            }

            var card = new TrelloCard
            {
                listID = currentListId,
                name = title,
                description = description
            };

            if (newCardsOnTop)
            {
                card.position = "top";
            }
            
            return card;
        }

        public TrelloList NewList(string name)
        {
            if (currentBoardId == "")
            {
                throw new System.Exception("Cannot create a list if there is no board selected.");
            }

            var list = new TrelloList
            {
                boardID = currentBoardId,
                name = name
            };

            return list;
        }

        /// <summary>
        /// 获取当前用户的全部看板
        /// </summary>
        public IEnumerator PopulateBoardsRoutine()
        {
            boards = null;
            
            var url = MEMBER_BASE_URL + "?" + "key=" + key + "&token=" + token + "&boards=all";
            var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            Debug.Log("所有的看板: " + request.downloadHandler.text);
            
            if (Json.Deserialize(request.downloadHandler.text) is Dictionary<string, object> dict)
            {
                boards = dict["boards"] as List<object>;
            }
        }

        // Populate
        public IEnumerator PopulateListsRoutine()
        {
            lists = null;
            if (currentBoardId == "")
            {
                throw new System.Exception("Cannot the lists, you have not selected a board yet.");
            }

            var www = UnityWebRequest.Get(BOARD_BASE_URL + currentBoardId + "?" + "key=" + key + "&token=" + token + "&lists=all");
            yield return www.SendWebRequest();

            if (Json.Deserialize(www.downloadHandler.text) is Dictionary<string, object> dict)
            {
                lists = (List<object>)dict["lists"];
            }

            // cache the lists
            if (lists != null)
            {
                for (var i = 0; i < lists.Count; i++)
                {
                    var list = (Dictionary<string, object>)lists[i];
                    if (cachedLists.ContainsKey((string)list["name"])) continue;
                    cachedLists.Add((string)list["name"], (string)list["id"]);
                }
            }
        }
        
        // 上传
        public IEnumerator UploadCardRoutine(TrelloCard card)
        {
            var post = new WWWForm();
            post.AddField("name", card.name);
            post.AddField("desc", card.description);
            post.AddField("pos", card.position);
            post.AddField("due", card.due);
            post.AddField("idList", card.listID);

            var request = UnityWebRequest.Post(CARD_BASE_URL + "?" + "key=" + key + "&token=" + token, post);
            yield return request.SendWebRequest();
            
            if (Json.Deserialize(request.downloadHandler.text) is Dictionary<string, object> dict)
            {
                yield return dict["id"].ToString();
            }
        }

        // 上传
        public IEnumerator UploadListRoutine(TrelloList list)
        {
            var post = new WWWForm();
            post.AddField("name", list.name);
            post.AddField("idBoard", list.boardID);
            post.AddField("pos", list.position);
            
            var request = UnityWebRequest.Post(LIST_BASE_URL + "?" + "key=" + key + "&token=" + token, post);
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
            var www = UnityWebRequest.Post(CARD_BASE_URL + cardId + "/attachments" + "?" + "key=" + key + "&token=" + token, formData);
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
            var www = UnityWebRequest.Post(CARD_BASE_URL + cardId + "/attachments" + "?" + "key=" + key + "&token=" + token, formData);
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
            
            var www = UnityWebRequest.Post(CARD_BASE_URL + cardId + "/attachments" + "?" + "key=" + key + "&token=" + token, formData);
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
