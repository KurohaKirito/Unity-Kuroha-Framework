using System.Collections.Generic;
using System.Linq;

namespace Kuroha.Util.Release
{
    public static class NullUtil
    {
        /// <summary>
        /// 数组
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNotNullAndEmpty<T>(this IEnumerable<T> self)
        {
            return self != null && self.Any();
        }

        /// <summary>
        /// 数组
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> self)
        {
            return self == null || self.Any() == false;
        }

        /// <summary>
        /// 字典
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNotNullAndEmpty<T1, T2>(this IDictionary<T1, T2> self)
        {
            return self != null && self.Count > 0;
        }

        /// <summary>
        /// 字典
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T1, T2>(this IDictionary<T1, T2> self)
        {
            return self == null || self.Count <= 0;
        }
    }
}