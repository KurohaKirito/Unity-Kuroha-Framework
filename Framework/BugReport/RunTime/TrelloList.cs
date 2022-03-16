using System;

namespace Kuroha.Framework.BugReport.RunTime
{
    /// <summary>
    /// Trello 列表
    /// </summary>
    [Serializable]
    public class TrelloList
    {
        public string name;
        public bool closed;
        public string pos;
        public string softLimit;
        public string idBoard;
        public string subscribed;
        public string id;
    }
}
