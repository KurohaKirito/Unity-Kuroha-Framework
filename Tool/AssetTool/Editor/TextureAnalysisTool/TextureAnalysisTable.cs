﻿using System.Collections.Generic;
using Kuroha.GUI.Editor.Table;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.TextureAnalysisTool
{
    public class TextureAnalysisTable : CustomTable<TextureAnalysisData> {
        public TextureAnalysisTable(Vector2 space, Vector2 minSize, List<TextureAnalysisData> dataList, bool isDrawFilter, bool isDrawExport, bool isDrawDistinct,
            CustomTableColumn<TextureAnalysisData>[] columns,
            CustomTableDelegate.FilterMethod<TextureAnalysisData> onFilterFunction,
            CustomTableDelegate.ExportMethod<TextureAnalysisData> onExportFunction,
            CustomTableDelegate.SelectMethod<TextureAnalysisData> onSelectFunction,
            CustomTableDelegate.DistinctMethod<TextureAnalysisData> onDistinctFunction)
            : base(space, minSize, dataList, isDrawFilter, isDrawExport, isDrawDistinct, columns,
                onFilterFunction,
                onExportFunction,
                onSelectFunction,
                onDistinctFunction) {
        }
    }
}
