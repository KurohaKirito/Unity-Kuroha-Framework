using System.Collections.Generic;
using Kuroha.GUI.Editor.Table;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.ProfilerTool.AsyncLoadTool
{
    public class LoadTimeRecordTable : CommonTable<LoadTimeRecordData>
    {
        public LoadTimeRecordTable(
            Vector2 space,
            Vector2 minSize,
            List<LoadTimeRecordData> dataList,
            bool isDrawFilter,
            bool isDrawExport,
            float exportWidth,
            float filterHeight,
            CommonTableColumn<LoadTimeRecordData>[] columns,
            CommonTableDelegate.FilterMethod<LoadTimeRecordData> onFilterFunction,
            CommonTableDelegate.ExportMethod<LoadTimeRecordData> onExportFunction,
            CommonTableDelegate.SelectMethod<LoadTimeRecordData> onSelectFunction)
            : base(space,
                minSize,
                dataList,
                isDrawFilter,
                isDrawExport,
                exportWidth,
                filterHeight,
                columns,
                onFilterFunction,
                onExportFunction,
                onSelectFunction) { }
    }
}