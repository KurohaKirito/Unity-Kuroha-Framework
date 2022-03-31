using System.Collections.Generic;
using Script.Effect.Editor.AssetTool.GUI.Editor.Table;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.ProfilerTool.AsyncLoadTool {
    public class AsyncLoadTable : CustomTable<AsyncLoadData> {
        public AsyncLoadTable(Vector2 space, Vector2 minSize, List<AsyncLoadData> dataList, bool isDrawFilter, bool isDrawExport, bool isDistinct,
            CustomTableColumn<AsyncLoadData>[] columns,
            CustomTableDelegate.FilterMethod<AsyncLoadData> onFilterFunction,
            CustomTableDelegate.AfterFilterMethod<AsyncLoadData> afterFilterMethod,
            CustomTableDelegate.ExportMethod<AsyncLoadData> onExportFunction,
            CustomTableDelegate.SelectMethod<AsyncLoadData> onSelectFunction,
            CustomTableDelegate.DistinctMethod<AsyncLoadData> onDistinctFunction)
            : base(space, minSize, dataList, isDrawFilter, isDrawExport, isDistinct, columns,
                onFilterFunction,
                afterFilterMethod,
                onExportFunction,
                onSelectFunction,
                onDistinctFunction) {
        }
    }
}