using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.GUI.Editor.Table {
    public class CustomTable<T> where T : class {
        #region private field

        private const float BUTTON_SPACE = 2;
        private const float BUTTON_HEIGHT = 20;
        private const float BUTTON_WIDTH = 100;
        private bool isInitialized;
        private readonly Vector2 minRect;
        private readonly string[] displayedOptions;
        private CustomTreeView<T> treeView;
        private TreeViewState treeViewState;

        #endregion

        #region private property

        private Rect filterRect;
        private Vector2 exportVector2;
        private Vector2 deduplicateVector2;

        private List<T> Data { get; }
        
        private float WidthSpace { get; }
        private float HeightSpace { get; }
        
        private bool IsDrawExport { get; }
        private bool IsDrawFilter { get; }
        private bool IsDrawDeduplicate { get; }
        
        private CustomTableDelegate.FilterMethod<T> FilterFunction { get; }
        private CustomTableDelegate.ExportMethod<T> ExportFunction { get; }
        private CustomTableDelegate.SelectMethod<T> SelectFunction { get; }
        private CustomTableDelegate.DeduplicateMethod<T> DeduplicateFunction { get; }
        
        private MultiColumnHeaderState MultiColumnHeaderState { get; }

        #endregion

        #region Constructor

        protected CustomTable(Vector2 space, Vector2 minSize, List<T> dataList, bool isDrawFilter, bool isDrawExport, bool isDrawDeduplicate,
            CustomTableColumn<T>[] columns,
            CustomTableDelegate.FilterMethod<T> onFilterFunction,
            CustomTableDelegate.ExportMethod<T> onExportFunction,
            CustomTableDelegate.SelectMethod<T> onSelectFunction,
            CustomTableDelegate.DeduplicateMethod<T> onDeduplicateFunction) {
            Data = dataList;
            minRect = minSize;
            WidthSpace = space.x;
            HeightSpace = space.y;
            displayedOptions = new string[columns.Length];
            for (var i = 0; i < columns.Length; i++) {
                displayedOptions[i] = columns[i].headerContent.text;
            }

            IsDrawFilter = isDrawFilter;
            IsDrawExport = isDrawExport;
            IsDrawDeduplicate = isDrawDeduplicate;
            
            FilterFunction = onFilterFunction;
            ExportFunction = onExportFunction;
            SelectFunction = onSelectFunction;
            DeduplicateFunction = onDeduplicateFunction;
            
            // ReSharper disable once CoVariantArrayConversion
            MultiColumnHeaderState = new MultiColumnHeaderState(columns);
        }

        #endregion

        private void Init() {
            if (isInitialized == false) {
                if (treeViewState == null) {
                    treeViewState = new TreeViewState();
                }

                treeView = new CustomTreeView<T>(treeViewState, new MultiColumnHeader(MultiColumnHeaderState), Data,
                    FilterFunction, ExportFunction, SelectFunction, DeduplicateFunction);

                treeView.Reload();
                isInitialized = true;
            }
        }

        public void OnGUI() {
            Init();

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
                
                // Deduplicate Button
                if (IsDrawDeduplicate) {
                    deduplicateVector2 = exportVector2 == Vector2.zero
                        ? new Vector2(tableRect.x, tableRect.y)
                        : new Vector2(exportVector2.x + BUTTON_WIDTH + BUTTON_SPACE, exportVector2.y);
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
                treeView.OnExportGUI(exportVector2, IsDrawExport, BUTTON_WIDTH, BUTTON_HEIGHT - BUTTON_SPACE, Data);
                treeView.OnDeduplicateGUI(deduplicateVector2, IsDrawDeduplicate, BUTTON_WIDTH, BUTTON_HEIGHT - BUTTON_SPACE, Data);
            }
        }
    }
}