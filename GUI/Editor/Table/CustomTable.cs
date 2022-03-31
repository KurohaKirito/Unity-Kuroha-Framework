using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.GUI.Editor.Table {
    public class CustomTable<T> where T : class {
        #region private field

        private const float BUTTON_SPACE = 2;
        private const float BUTTON_HEIGHT = 20;
        private const float BUTTON_WIDTH = 100;

        private readonly Vector2 minRect;
        private readonly string[] displayedOptions;
        private readonly CustomTreeView<T> treeView;

        #endregion

        #region private property

        private Rect filterRect;
        private Vector2 exportVector2;
        private Vector2 distinctVector2;

        private float WidthSpace { get; }
        private float HeightSpace { get; }

        private bool IsDrawExport { get; }
        private bool IsDrawFilter { get; }
        private bool IsDrawDistinct { get; }

        private CustomTableDelegate.FilterMethod<T> FilterFunction { get; }
        private CustomTableDelegate.AfterFilterMethod<T> AfterFilterMethod { get; }
        private CustomTableDelegate.ExportMethod<T> ExportFunction { get; }
        private CustomTableDelegate.SelectMethod<T> SelectFunction { get; }
        private CustomTableDelegate.DistinctMethod<T> DistinctFunction { get; }

        private MultiColumnHeaderState MultiColumnHeaderState { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        protected CustomTable(Vector2 space, Vector2 minSize, List<T> dataList,
            bool isDrawFilter, bool isDrawExport, bool isDrawDistinct,
            CustomTableColumn<T>[] columns,
            CustomTableDelegate.FilterMethod<T> onFilterFunction, CustomTableDelegate.AfterFilterMethod<T> afterFilterMethod,
            CustomTableDelegate.ExportMethod<T> onExportFunction, CustomTableDelegate.SelectMethod<T> onSelectFunction,
            CustomTableDelegate.DistinctMethod<T> onDistinctFunction)
        {
            minRect = minSize;
            WidthSpace = space.x;
            HeightSpace = space.y;
            displayedOptions = new string[columns.Length];
            for (var i = 0; i < columns.Length; i++) {
                displayedOptions[i] = columns[i].headerContent.text;
            }

            IsDrawFilter = isDrawFilter;
            IsDrawExport = isDrawExport;
            IsDrawDistinct = isDrawDistinct;

            FilterFunction = onFilterFunction;
            AfterFilterMethod = afterFilterMethod;
            ExportFunction = onExportFunction;
            SelectFunction = onSelectFunction;
            DistinctFunction = onDistinctFunction;

            // ReSharper disable once CoVariantArrayConversion
            MultiColumnHeaderState = new MultiColumnHeaderState(columns);
            treeView = new CustomTreeView<T>(new TreeViewState(), new MultiColumnHeader(MultiColumnHeaderState), dataList,
                FilterFunction, AfterFilterMethod, ExportFunction, SelectFunction, DistinctFunction);
            treeView.Reload();
        }

        public void OnGUI() {
            // 定义表格 Rect
            var tableRect = GUILayoutUtility.GetRect(minRect.x, Screen.width, minRect.y, Screen.height);

            if (Event.current.type != EventType.Layout) {
                // Left Space
                tableRect.x += WidthSpace;

                // Up Space
                tableRect.y += HeightSpace;

                // Export Button
                if (IsDrawExport) {
                    exportVector2 = new Vector2(tableRect.x, tableRect.y);
                }

                // Distinct Button
                if (IsDrawDistinct) {
                    distinctVector2 = exportVector2 == Vector2.zero? new Vector2(tableRect.x, tableRect.y) : new Vector2(exportVector2.x + BUTTON_WIDTH + BUTTON_SPACE, exportVector2.y);
                }

                // Right Space
                tableRect.width -= WidthSpace * 2;

                // Filter
                if (IsDrawFilter) {
                    filterRect = new Rect(tableRect.x, tableRect.y, tableRect.width, BUTTON_HEIGHT);

                    // Table Move Down The Filter Height
                    tableRect.y += BUTTON_HEIGHT;
                }

                // Down Space
                tableRect.height = tableRect.height - BUTTON_HEIGHT - HeightSpace * 2;

                treeView.OnGUI(tableRect);
                treeView.OnFilterGUI(filterRect, IsDrawFilter, WidthSpace, displayedOptions);
                treeView.OnExportGUI(exportVector2, IsDrawExport, BUTTON_WIDTH, BUTTON_HEIGHT - BUTTON_SPACE);
                treeView.OnDistinctGUI(distinctVector2, IsDrawDistinct, BUTTON_WIDTH, BUTTON_HEIGHT - BUTTON_SPACE);
            }
        }
    }
}