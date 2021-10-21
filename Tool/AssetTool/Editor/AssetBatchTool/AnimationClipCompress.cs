using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.AssetBatchTool
{
    public static class AnimationClipCheck
    {
        [MenuItem("Tools/Check Animation")]
        public static void Check()
        {
            var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] {
                "Assets"
            });

            var index = 0;
            var total = guids.Length;
            try
            {
                foreach (var guid in guids)
                {
                    EditorUtility.DisplayProgressBar("anim", "", (float)++index / total);
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var extension = Path.GetExtension(assetPath);
                    if (extension == ".anim")
                    {
                        var filePath = Application.dataPath + assetPath.Substring(6);
                        var content = File.ReadAllText(filePath);
                        content = Regex.Replace(content, "[0-9]{1,10}\\.[0-9]{1,15}", x => {
                                var dotIndex = x.Value.IndexOf(".", StringComparison.Ordinal);
                                var delta = x.Value.Length - dotIndex;
                                return delta > 5 ? x.Value.Remove(dotIndex + 5) : x.Value;
                            }
                        );
                        File.WriteAllText(filePath, content);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
