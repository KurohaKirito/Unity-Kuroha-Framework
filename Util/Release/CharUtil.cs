namespace Kuroha.Util.Release
{
    public static class CharUtil
    {
        /// <summary>
        /// 判断一个字符是否是中文
        /// </summary>
        /// <param name="character">字符</param>
        /// <returns></returns>
        public static bool IsChinese(char character)
        {
            return character >= 0x4E00 && character <= 0x9FA5;
        }

        /// <summary>
        /// 判断一个字符是否是空格
        /// </summary>
        /// <param name="character">字符</param>
        /// <returns></returns>
        public static bool IsSpace(char character)
        {
            return character == ' ';
        }
    }
}