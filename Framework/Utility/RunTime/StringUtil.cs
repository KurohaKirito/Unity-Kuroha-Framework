using System.Collections.Generic;

namespace Kuroha.Framework.Utility.RunTime
{
    public static class StringUtil
    {
        /// <summary>
        /// 返回整个字符串中指定字符的全部索引
        /// </summary>
        public static List<int> GetAllIndexOfChar(string str, char target)
        {
            var result = new List<int>();
            var allChar = str.ToCharArray();

            for (var index = 0; index < allChar.Length; index++)
            {
                if (allChar[index] == target)
                {
                    result.Add(index);
                }
            }

            return result;
        }
        
        /// <summary>
        /// 包含数字的字符串比较
        /// </summary>
        /// <param name="strA">待比较串 A</param>
        /// <param name="strB">待比较串 B</param>
        /// <param name="isAsc">是否是升序排序</param>
        /// <returns></returns>
        public static int CompareByNumber(string strA, string strB, bool isAsc)
        {
            if (strA == strB)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(strA))
            {
                return isAsc ? 1 : -1;
            }

            if (string.IsNullOrEmpty(strB))
            {
                return isAsc ? -1 : 1;
            }

            var charArrayA = strA.ToCharArray();
            var charArrayB = strB.ToCharArray();

            var indexA = 0;
            var indexB = 0;

            while (indexA < charArrayA.Length && indexB < charArrayB.Length)
            {
                // 判断是否是数字
                if (char.IsDigit(charArrayA[indexA]) && char.IsDigit(charArrayB[indexB]))
                {
                    var numberA = string.Empty;
                    var numberB = string.Empty;

                    while (indexA < charArrayA.Length && char.IsDigit(charArrayA[indexA]))
                    {
                        numberA += charArrayA[indexA];
                        indexA++;
                    }

                    while (indexB < charArrayB.Length && char.IsDigit(charArrayB[indexB]))
                    {
                        numberB += charArrayB[indexB];
                        indexB++;
                    }

                    if (int.Parse(numberA) > int.Parse(numberB))
                    {
                        return 1;
                    }

                    if (int.Parse(numberA) < int.Parse(numberB))
                    {
                        return -1;
                    }
                }
                else
                {
                    if (charArrayA[indexA] > charArrayB[indexB])
                    {
                        return 1;
                    }

                    if (charArrayA[indexA] < charArrayB[indexB])
                    {
                        return -1;
                    }

                    indexA++;
                    indexB++;
                }
            }

            if (charArrayA.Length == charArrayB.Length)
            {
                return 0;
            }

            return charArrayA.Length > charArrayB.Length ? 1 : -1;
        }

        /// <summary>
        /// 对 Bool 类型进行排序后, true 在前, false 在后
        /// 之后对 string 类型排序
        /// </summary>
        /// <param name="boolA"></param>
        /// <param name="boolB"></param>
        /// <param name="strA"></param>
        /// <param name="strB"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public static int CompareByBoolAndString(bool boolA, bool boolB, string strA, string strB, bool isAsc)
        {
            if (boolA == false)
            {
                return isAsc ? 1 : -1;
            }

            if (boolB == false)
            {
                return isAsc ? -1 : 1;
            }

            return CompareByNumber(strA, strB, isAsc);
        }
    }
}
