namespace Kuroha.Framework.BugReport
{
    /// <summary>
    /// Trello 列表
    /// </summary>
    public class TrelloList
    {
        /// <summary>
        /// 列表名称
        /// </summary>
        public string name = string.Empty;
        
        /// <summary>
        /// 列表所属看板 ID
        /// </summary>
        public string boardID = string.Empty;
        
        /// <summary>
        /// 列表位置
        /// </summary>
        public string position = "bottom";
    }
}
