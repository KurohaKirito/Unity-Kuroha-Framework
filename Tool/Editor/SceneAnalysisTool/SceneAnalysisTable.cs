using System.Collections.Generic;
using Script.Effect.Editor.AssetTool.GUI.Editor.Table;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.SceneAnalysisTool {
    public class SceneAnalysisTable : CustomTable<SceneAnalysisData> {
        public SceneAnalysisTable(Vector2 space, Vector2 minSize, List<SceneAnalysisData> dataList, bool isDrawFilter, bool isDrawExport, bool isDistinct,
            CustomTableColumn<SceneAnalysisData>[] columns,
            CustomTableDelegate.FilterMethod<SceneAnalysisData> onFilterFunction,
            CustomTableDelegate.ExportMethod<SceneAnalysisData> onExportFunction,
            CustomTableDelegate.SelectMethod<SceneAnalysisData> onSelectFunction,
            CustomTableDelegate.DistinctMethod<SceneAnalysisData> onDistinctFunction)
            : base(space, minSize, dataList, isDrawFilter, isDrawExport, isDistinct, columns,
                onFilterFunction,
                onExportFunction,
                onSelectFunction,
                onDistinctFunction) {
        }
    }
}
