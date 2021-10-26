using System;
using System.IO;
using System.Text.RegularExpressions;
using Kuroha.Util.RunTime;
using UnityEditor;

namespace Kuroha.Tool.Editor.AssetBatchTool
{
    /// <summary>
    /// 压缩动画片段
    /// </summary>
    public static class AnimationClipCheck
    {
        [MenuItem("Tools/Check Animation")]
        public static void Check()
        {
            var guids = AssetDatabase.FindAssets("t:AnimationClip", new[]
            {
                "Assets"
            });

            var total = guids.Length;
            for (var index = 0; index < total; index++)
            {
                Kuroha.GUI.Editor.ProgressBar.DisplayProgressBar("动画片段压缩中, 可能遇到动画片段较大的情况, 请耐心等候", $"{index + 1}/{total}", index + 1, total);
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[index]);
                var extension = Path.GetExtension(assetPath);
                if (extension == ".anim")
                {
                    var filePath = Path.GetFullPath(assetPath);
                    var content = File.ReadAllText(filePath);
                    content = Regex.Replace(content, "-?[0-9]{1,10}\\.[0-9]{1,15}", Compress);
                    File.WriteAllText(filePath, content);
                }
            }
        }

        private static string Compress(Match match)
        {
            // 小数点后位数最大限制
            const int DELTA_LIMIT = 4;
            
            // 小数点的位置
            var dotIndex = match.Value.IndexOf(".", StringComparison.Ordinal);
            
            // 小数位的个数
            var delta = match.Value.Length - dotIndex;
            
            // 缩小小数位数
            var result = match.Value;
            if (delta > DELTA_LIMIT + 1)
            {
                result = result.Remove(dotIndex + DELTA_LIMIT + 1);
            }
            if (result == "0.0000" || result == "-0.0000")
            {
                result = "0";
            }
            return result;
        }
    }
}
