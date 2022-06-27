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
        
        public static void RemoveMaterial(this ModelImporter modelImporter)
        {
            var path = AssetDatabase.GetAssetPath(modelImporter);
            var remove = path.Substring(0, path.LastIndexOf('/')) + "/Materials";
        
            #region 删除模型的内嵌材质

            // 开启材质导入, 提取出模型的内嵌材质到 Materials 文件夹
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            modelImporter.materialLocation = ModelImporterMaterialLocation.External;
            modelImporter.SaveAndReimport();

            // 删除提取出来的材质球
            AssetDatabase.DeleteAsset(remove);

            // 修改模型材质引用类型为内嵌材质
            modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;
            modelImporter.SaveAndReimport();

            #endregion
        }
    }
}
