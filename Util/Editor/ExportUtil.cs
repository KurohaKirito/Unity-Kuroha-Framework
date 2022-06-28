using System.Collections.Generic;
using System.Text;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using UnityEditor;

namespace Script.Effect.Editor.AssetTool.Util.Editor {
    public static class ExportUtil {
        /// <summary>
        /// 导出 Dictionary List 数据到文件
        /// </summary>
        public static void ExportDictionaryList(string filePath, List<Dictionary<string, string>> texts) {
            var rowCount = texts.Count;
            if (rowCount > 0) {
                var columnCount = texts[0].Keys.Count;
                if (columnCount > 0) {
                    DebugUtil.Log($"一共需要导出 {rowCount} 行, {columnCount} 列数据");

                    // 标题 key
                    var title = new StringBuilder();
                    var lines = new List<string>();
                    foreach (var key in texts[0].Keys) {
                        title.Append($"{key},");
                    }

                    title.Remove(title.Length - 1, 1);
                    lines.Add(title.ToString());

                    // 内容 value
                    for (var lineIndex = 1; lineIndex < texts.Count; lineIndex++) {
                        var line = new StringBuilder();
                        foreach (var value in texts[lineIndex].Values) {
                            line.Append($"{value},");
                        }

                        line.Remove(line.Length - 1, 1);
                        lines.Add(line.ToString());
                    }

                    // 写入文件
                    System.IO.File.WriteAllLines(filePath, lines);
                }
            } else {
                DebugUtil.Log("没有需要导出的内容");
            }
        }
    }
}
