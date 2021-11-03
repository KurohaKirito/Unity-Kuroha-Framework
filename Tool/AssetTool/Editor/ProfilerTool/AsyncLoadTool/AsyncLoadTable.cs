using System.Collections.Generic;
using Kuroha.GUI.Editor.Table;
using UnityEngine;

namespace Kuroha.Tool.Editor.ProfilerTool
{
    public class AsyncLoadTable : CommonTable<AsyncLoadData>
    {
        public AsyncLoadTable(
            Vector2 space,
            Vector2 minSize,
            List<AsyncLoadData> dataList,
            bool isDrawFilter,
            bool isDrawExport,
            float exportWidth,
            float filterHeight,
            CommonTableColumn<AsyncLoadData>[] columns,
            CommonTableDelegate.FilterMethod<AsyncLoadData> onFilterFunction,
            CommonTableDelegate.ExportMethod<AsyncLoadData> onExportFunction,
            CommonTableDelegate.SelectMethod<AsyncLoadData> onSelectFunction)
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