using System.Collections.Generic;
using Kuroha.GUI.Editor.Table;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.SceneAnalysisTool
{
    public class SceneAnalysisTable : CommonTable<SceneAnalysisData>
    {
        public SceneAnalysisTable(
            Vector2 space,
            Vector2 minSize,
            List<SceneAnalysisData> dataList,
            bool isDrawFilter,
            bool isDrawExport,
            float exportWidth,
            float filterHeight,
            CommonTableColumn<SceneAnalysisData>[] columns, CommonTableDelegate.FilterMethod<SceneAnalysisData> onFilterFunction, CommonTableDelegate.ExportMethod<SceneAnalysisData> onExportFunction, CommonTableDelegate.SelectMethod<SceneAnalysisData> onSelectFunction)
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
                onSelectFunction)
        {
        }
    }
}
