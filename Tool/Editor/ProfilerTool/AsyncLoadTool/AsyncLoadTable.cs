using System.Collections.Generic;
using Script.Effect.Editor.AssetTool.GUI.Editor.Table;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.ProfilerTool.AsyncLoadTool {
    public class AsyncLoadTable : CustomTable<AsyncLoadData> {
        public AsyncLoadTable(Vector2 space, Vector2 minSize, List<AsyncLoadData> dataList, bool isDrawFilter, bool isDrawExport, bool isDeduplicate,
            CustomTableColumn<AsyncLoadData>[] columns,
            CustomTableDelegate.FilterMethod<AsyncLoadData> onFilterFunction,
            CustomTableDelegate.ExportMethod<AsyncLoadData> onExportFunction,
            CustomTableDelegate.SelectMethod<AsyncLoadData> onSelectFunction,
            CustomTableDelegate.DeduplicateMethod<AsyncLoadData> onDeduplicateFunction)
            : base(space, minSize, dataList, isDrawFilter, isDrawExport, isDeduplicate, columns,
                onFilterFunction,
                onExportFunction,
                onSelectFunction,
                onDeduplicateFunction) {
        }
    }
}