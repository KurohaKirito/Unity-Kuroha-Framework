using System.Collections.Generic;
using Kuroha.GUI.Editor.Table;
using UnityEngine;

namespace Kuroha.Tool.Editor.ModelAnalysisTool
{
    public class ModelAnalysisTable : CommonTable<ModelAnalysisData>
    {
        public ModelAnalysisTable(
            Vector2 space,
            Vector2 minSize,
            List<ModelAnalysisData> dataList,
            bool isDrawFilter,
            bool isDrawExport,
            float exportWidth,
            float filterHeight,
            CommonTableColumn<ModelAnalysisData>[] columns, CommonTableDelegate.FilterMethod<ModelAnalysisData> onFilterFunction, CommonTableDelegate.ExportMethod<ModelAnalysisData> onExportFunction, CommonTableDelegate.SelectMethod<ModelAnalysisData> onSelectFunction)
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