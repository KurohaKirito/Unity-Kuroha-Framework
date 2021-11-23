using System;
using System.Collections.Generic;

namespace Kuroha.Util.Editor
{
    public static class PathUtil
    {
        /// <summary>
        /// 将 Absolute Path 转换为 AssetPath
        /// </summary>
        /// <param name="absolutePaths">待转换路径</param>
        /// <returns></returns>
        public static List<string> GetAssetPath(in string[] absolutePaths)
        {
            var result = new List<string>();

            foreach (var path in absolutePaths)
            {
                if (string.IsNullOrEmpty(path) == false)
                {
                    var assetsIndex = path.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                    var assetPath = path.Substring(assetsIndex);
                    if (assetPath.IndexOf(".meta", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        result.Add(assetPath);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 将 Absolute Path 转换为 AssetPath
        /// </summary>
        /// <param name="absolutePath">待转换路径</param>
        /// <returns></returns>
        public static string GetAssetPath(string absolutePath)
        {
            string result = null;

            if (string.IsNullOrEmpty(absolutePath) == false)
            {
                var assetsIndex = absolutePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = absolutePath.Substring(assetsIndex);
                result = assetPath;
            }

            return result;
        }
    }
}
