using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Kuroha.GUI.Editor.Table
{
    public class CommonTable<T> where T : class
    {
        #region private field

        private const int MAX_FILTER_HEIGHT = 20;
        private const int FILTER_EXPORT_HEIGHT_LESS = 2;

        private bool isInitialized;
        private readonly Vector2 minRect;
        private readonly float filterHeight;
        private readonly float exportWidth;
        private readonly string[] displayedOptions;
        private CommonTreeView<T> treeView;
        private TreeViewState treeViewState;

        #endregion

        #region private property

        private List<T> Data { get; }
        private float WidthSpace { get; }
        private float HeightSpace { get; }
        private bool IsDrawExport { get; }
        private bool IsDrawFilter { get; }
        private CommonTableDelegate.FilterMethod<T> FilterFunction { get; }
        private CommonTableDelegate.ExportMethod<T> ExportFunction { get; }
        private CommonTableDelegate.SelectMethod<T> SelectFunction { get; }
        private MultiColumnHeaderState MultiColumnHeaderState { get; }

        #endregion

        #region Constructor

        protected CommonTable(
            Vector2 space,
            Vector2 minSize,
            List<T> dataList,
            bool isDrawFilter,
            bool isDrawExport,
            float exportWidth,
            float filterHeight,
            CommonTableColumn<T>[] columns,
            CommonTableDelegate.FilterMethod<T> onFilterFunction,
            CommonTableDelegate.ExportMethod<T> onExportFunction,
            CommonTableDelegate.SelectMethod<T> onSelectFunction)
        {
            Data = dataList;
            minRect = minSize;
            WidthSpace = space.x;
            HeightSpace = space.y;
            this.exportWidth = exportWidth;
            this.filterHeight = Math.Min(filterHeight, MAX_FILTER_HEIGHT);
            displayedOptions = new string[columns.Length];
            for (var i = 0; i < columns.Length; i++)
            {
                displayedOptions[i] = columns[i].headerContent.text;
            }

            IsDrawFilter = isDrawFilter;
            IsDrawExport = isDrawExport;
            FilterFunction = onFilterFunction;
            ExportFunction = onExportFunction;
            SelectFunction = onSelectFunction;
            // ReSharper disable once CoVariantArrayConversion
            MultiColumnHeaderState = new MultiColumnHeaderState(columns);
        }

        #endregion

        private void Init()
        {
            if (isInitialized == false)
            {
                if (treeViewState == null)
                {
                    treeViewState = new TreeViewState();
                }

                treeView = new CommonTreeView<T>(
                    treeViewState,
                    new MultiColumnHeader(MultiColumnHeaderState),
                    Data,
                    FilterFunction,
                    ExportFunction,
                    SelectFunction);

                treeView.Reload();
                isInitialized = true;
            }
        }

        public void OnGUI()
        {
            Init();

            // 定义表格 Rect
            var tableRect = GUILayoutUtility.GetRect(minRect.x, Screen.width, minRect.y, Screen.height);

            if (Event.current.type != EventType.Layout)
            {
                // Left Space
                tableRect.x += WidthSpace;
                // Up Space
                tableRect.y += HeightSpace;
                // Export Button Position
                var exportPosition = new Vector2(tableRect.x, tableRect.y);

                // Right Space
                tableRect.width -= WidthSpace * 2;
                // Filter Rect
                var filterRect = new Rect(tableRect.x, tableRect.y, tableRect.width, filterHeight);

                // Move Down Filter Height
                tableRect.y += filterHeight;
                // Down Space
                tableRect.height = tableRect.height - filterHeight - HeightSpace * 2;

                treeView.OnGUI(tableRect);
                treeView.OnFilterGUI(filterRect, IsDrawFilter, WidthSpace, displayedOptions);
                treeView.OnExportGUI(exportPosition, IsDrawExport, exportWidth,
                    filterHeight - FILTER_EXPORT_HEIGHT_LESS, Data);
            }
        }
    }
}