using System;

namespace Kuroha.Framework.BugReport
{
    /// <summary>
    /// Trello 看板
    /// </summary>
    [Serializable]
    public class TrelloBoard
    {
        public string name;
        public bool closed;
        public string idOrganization;
        public string pinned;
        public string id;
    }
}
