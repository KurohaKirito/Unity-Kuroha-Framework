using System.Collections.Generic;
using Kuroha.GUI.Editor.Table;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.ProfilerTool.LoadTimeRecordTool.Editor
{
    public class LoadTimeRecordTable : CustomTable<LoadTimeRecordData>
    {
        public LoadTimeRecordTable(
            Vector2 space,
            Vector2 minSize,
            List<LoadTimeRecordData> dataList,
            bool isDrawFilter,
            bool isDrawExport,
            bool isDrawDistinct,
            CustomTableColumn<LoadTimeRecordData>[] columns,
            CustomTableDelegate.FilterMethod<LoadTimeRecordData> onFilterFunction,
            CustomTableDelegate.ExportMethod<LoadTimeRecordData> onExportFunction,
            CustomTableDelegate.SelectMethod<LoadTimeRecordData> onSelectFunction,
            CustomTableDelegate.DistinctMethod<LoadTimeRecordData> onDistinctFunction)
            : base(space,
                minSize,
                dataList,
                isDrawFilter,
                isDrawExport,
                isDrawDistinct,
                columns,
                onFilterFunction,
                onExportFunction,
                onSelectFunction,
                onDistinctFunction) { }
    }
}