namespace Kuroha.Tool.Editor.TextureAnalysisTool
{
    public class TextureAnalysisData
    {
        /// <summary>
        /// 检测类型
        /// </summary>
        public enum DetectType
        {
            /// <summary>
            /// 检测当前场景
            /// </summary>
            Scene,

            /// <summary>
            /// 检测指定路径
            /// </summary>
            Path
        }

        public int id;
        public int width;
        public int height;
        public bool isSolid;
        public string repeatInfo;
        public string textureName;
        public string texturePath;
    }
}