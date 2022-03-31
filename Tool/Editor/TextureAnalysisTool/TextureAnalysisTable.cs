using System.Collections.Generic;
using Script.Effect.Editor.AssetTool.GUI.Editor.Table;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.TextureAnalysisTool {
    public class TextureAnalysisTable : CustomTable<TextureAnalysisData> {
        public TextureAnalysisTable(Vector2 space, Vector2 minSize, List<TextureAnalysisData> dataList, bool isDrawFilter, bool isDrawExport, bool isDistinct,
            CustomTableColumn<TextureAnalysisData>[] columns,
            CustomTableDelegate.FilterMethod<TextureAnalysisData> onFilterFunction,
            CustomTableDelegate.AfterFilterMethod<TextureAnalysisData> afterFilterMethod,
            CustomTableDelegate.ExportMethod<TextureAnalysisData> onExportFunction,
            CustomTableDelegate.SelectMethod<TextureAnalysisData> onSelectFunction,
            CustomTableDelegate.DistinctMethod<TextureAnalysisData> onDistinctFunction)
            : base(space, minSize, dataList, isDrawFilter, isDrawExport, isDistinct, columns,
                onFilterFunction,
                afterFilterMethod,
                onExportFunction,
                onSelectFunction,
                onDistinctFunction) {
        }
    }
}