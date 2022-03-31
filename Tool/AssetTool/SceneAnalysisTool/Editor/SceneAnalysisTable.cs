using System.Collections.Generic;
using Kuroha.Framework.GUI.Editor.Table;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.SceneAnalysisTool.Editor
{
    public class SceneAnalysisTable : CustomTable<SceneAnalysisData>
    {
        public SceneAnalysisTable(
            Vector2 space,
            Vector2 minSize,
            List<SceneAnalysisData> dataList,
            bool isDrawFilter,
            bool isDrawExport,
            bool isDrawDistinct,
            CustomTableColumn<SceneAnalysisData>[] columns,
            CustomTableDelegate.FilterMethod<SceneAnalysisData> onFilterFunction,
            CustomTableDelegate.AfterFilterMethod<SceneAnalysisData> onAfterFilterFunction,
            CustomTableDelegate.ExportMethod<SceneAnalysisData> onExportFunction,
            CustomTableDelegate.SelectMethod<SceneAnalysisData> onSelectFunction,
            CustomTableDelegate.DistinctMethod<SceneAnalysisData> onDistinctFunction)
            : base(space,
                minSize,
                dataList,
                isDrawFilter,
                isDrawExport,
                isDrawDistinct,
                columns,
                onFilterFunction,
                onAfterFilterFunction,
                onExportFunction,
                onSelectFunction,
                onDistinctFunction)
        {
        }
    }
}
