using System;
using System.Collections.Generic;
using System.Linq;

namespace Script.Effect.Editor.AssetTool.Util.Editor {
    public static class PathUtil {
        /// <summary>
        /// 将 Absolute Path 转换为 AssetPath
        /// </summary>
        /// <param name="absolutePaths">待转换路径</param>
        /// <returns></returns>
        public static List<string> GetAssetPath(in string[] absolutePaths)
        {
            return (from path in absolutePaths
                where string.IsNullOrEmpty(path) == false && path.IndexOf(".meta", StringComparison.OrdinalIgnoreCase) < 0
                let assetsIndex = path.IndexOf("Assets", StringComparison.OrdinalIgnoreCase)
                let assetPath = path.Substring(assetsIndex)
                select assetPath).ToList();
        }

        /// <summary>
        /// 将 Absolute Path 转换为 AssetPath
        /// </summary>
        /// <param name="absolutePath">待转换路径</param>
        /// <returns></returns>
        public static string GetAssetPath(string absolutePath) {
            string result = null;

            if (string.IsNullOrEmpty(absolutePath) == false) {
                var assetsIndex = absolutePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                var assetPath = absolutePath.Substring(assetsIndex);
                result = assetPath;
            }

            return result;
        }
    }
}