using System.Collections.Generic;
using Kuroha.GUI.Editor.Table;
using UnityEngine;

namespace Kuroha.Tool.Editor.FashionAnalysisTool
{
    public class FashionAnalysisTable : CommonTable<FashionAnalysisData>
    {
        public FashionAnalysisTable(
            Vector2 space,
            Vector2 minSize,
            List<FashionAnalysisData> dataList,
            bool isDrawFilter,
            bool isDrawExport,
            float exportWidth,
            float filterHeight,
            CommonTableColumn<FashionAnalysisData>[] columns,
            CommonTableDelegate.FilterMethod<FashionAnalysisData> onFilterFunction,
            CommonTableDelegate.ExportMethod<FashionAnalysisData> onExportFunction,
            CommonTableDelegate.SelectMethod<FashionAnalysisData> onSelectFunction) : base(
            space,
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