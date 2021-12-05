namespace Kuroha.Framework.BugReport
{
    /// <summary>
    /// Trello 卡片
    /// </summary>
    public class TrelloCard
    {
        /// <summary>
        /// 卡片名称
        /// </summary>
        public string name = string.Empty;
        
        /// <summary>
        /// 卡片描述
        /// </summary>
        public string description = string.Empty;
        
        /// <summary>
        /// 卡片新增后放在列表中的位置
        /// bottom : 新增在当前列表的底部
        /// top: 新增在当前列表的顶部
        /// </summary>
        public string position = "bottom";
        
        /// <summary>
        /// 卡片原因
        /// </summary>
        public string due = "null";
        
        /// <summary>
        /// 卡片所属列表 ID
        /// </summary>
        public string listID = string.Empty;
        
        /// <summary>
        /// 链接指向
        /// </summary>
        public string urlSource = "null";
    }
}
