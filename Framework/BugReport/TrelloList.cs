using System;

namespace Kuroha.Framework.BugReport
{
    /// <summary>
    /// Trello 列表
    /// </summary>
    [Serializable]
    public class TrelloList
    {
        public string id;
        public string name;
        public bool closed = false;
        public string pos;
        public string softLimit;
        public string idBoard;
        public string subscribed;
    }
}
