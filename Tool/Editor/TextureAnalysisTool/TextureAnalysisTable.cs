using System.Collections.Generic;
using Script.Effect.Editor.AssetTool.GUI.Editor.Table;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.TextureAnalysisTool {
    public class TextureAnalysisTable : CommonTable<TextureAnalysisData> {
        public TextureAnalysisTable(Vector2 space, Vector2 minSize, List<TextureAnalysisData> dataList, bool isDrawFilter, bool isDrawExport, float exportWidth, float filterHeight, CommonTableColumn<TextureAnalysisData>[] columns, CommonTableDelegate.FilterMethod<TextureAnalysisData> onFilterFunction, CommonTableDelegate.ExportMethod<TextureAnalysisData> onExportFunction, CommonTableDelegate.SelectMethod<TextureAnalysisData> onSelectFunction) : base(space, minSize, dataList, isDrawFilter, isDrawExport, exportWidth, filterHeight, columns, onFilterFunction, onExportFunction, onSelectFunction) {
        }
    }
}