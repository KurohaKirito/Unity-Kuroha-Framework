using System.Collections.Generic;
using System.Linq;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.GUI.Editor.Table {
    public class CustomTreeView<T> : TreeView where T : class {
        private int filterMask = -1;
        private string filterText;
        private bool isReBuildRows;
        private readonly List<T> dataList;
        private List<T> dataListToShow;
        private List<CustomTreeViewItem<T>> allRow;

        #region private property

        private CustomTableDelegate.FilterMethod<T> MethodFilter { get; }
        private CustomTableDelegate.AfterFilterMethod<T> MethodAfterFilter { get; }
        private CustomTableDelegate.ExportMethod<T> MethodExport { get; }
        private CustomTableDelegate.SelectMethod<T> MethodSelect { get; }
        private CustomTableDelegate.DistinctMethod<T> MethodDistinct { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, List<T> dataList, CustomTableDelegate.FilterMethod<T> methodFilter, CustomTableDelegate.AfterFilterMethod<T> methodAfterFilter, CustomTableDelegate.ExportMethod<T> methodExport, CustomTableDelegate.SelectMethod<T> methodSelect, CustomTableDelegate.DistinctMethod<T> methodDistinct) : base(state, multiColumnHeader) {
            this.dataList = dataList;

            MethodFilter = methodFilter;
            MethodAfterFilter = methodAfterFilter;
            MethodExport = methodExport;
            MethodSelect = methodSelect;
            MethodDistinct = methodDistinct;

            multiColumnHeader.sortingChanged += OnSortingChanged;
            multiColumnHeader.visibleColumnsChanged += OnVisibleColumnChanged;

            showBorder = true;
            showAlternatingRowBackgrounds = true;
            rowHeight = EditorGUIUtility.singleLineHeight;
        }

        public void OnFilterGUI(Rect rect, bool isDraw, float rightSpace, string[] displayedOptions) {
            if (isDraw == false) {
                return;
            }

            EditorGUI.BeginChangeCheck();

            var width = rect.width;
            rect.width = UnityEngine.GUI.skin.label.CalcSize(CustomTableStyles.filterSelection).x;
            rect.x = width - rect.width + rightSpace;
            FilterGUI(rect, displayedOptions);

            if (EditorGUI.EndChangeCheck()) {
                Reload();
            }
        }

        public void OnExportGUI(Vector2 exportPosition, bool isDraw, float width, float height) {
            if (MethodExport == null) {
                return;
            }

            if (isDraw == false) {
                return;
            }

            if (UnityEngine.GUI.Button(new Rect(exportPosition.x, exportPosition.y - 1, width, height), "Export")) {
                var path = EditorUtility.SaveFilePanel("Export DataList", Application.dataPath, "Mesh Info.txt", "");
                MethodExport(path, dataListToShow);
            }
        }

        public void OnDistinctGUI(Vector2 position, bool isDraw, float width, float height) {
            if (MethodDistinct == null) {
                return;
            }

            if (isDraw == false) {
                return;
            }

            if (UnityEngine.GUI.Button(new Rect(position.x, position.y - 1, width, height), "Distinct")) {
                MethodDistinct(dataListToShow);
            }
        }

        private void FilterGUI(Rect rect, string[] displayedOptions) {
            const float FILTER_TYPE_WIDTH = 80;
            const float FILTER_TYPE_OFFSET = 1;
            const float FILTER_TYPE_SPACE = 5;
            const float FILTER_NONE_BUTTON_WIDTH = 16;

            // Filter Type
            rect.x -= FILTER_TYPE_WIDTH;
            filterMask = EditorGUI.MaskField(new Rect(rect.x - FILTER_TYPE_SPACE, rect.y + FILTER_TYPE_OFFSET, FILTER_TYPE_WIDTH, rect.height), filterMask, displayedOptions);
            rect.x += FILTER_TYPE_WIDTH;

            // Filter GUI
            rect.width -= FILTER_NONE_BUTTON_WIDTH;
            filterText = EditorGUI.DelayedTextField(rect, GUIContent.none, filterText, CustomTableStyles.searchField);

            // Filter Clear Button [x]
            rect.x += rect.width;
            rect.width = FILTER_NONE_BUTTON_WIDTH;
            var isEmpty = string.IsNullOrEmpty(filterText);
            var buttonStyle = isEmpty? CustomTableStyles.searchFieldCancelButtonEmpty : CustomTableStyles.searchFieldCancelButton;
            if (UnityEngine.GUI.Button(rect, GUIContent.none, buttonStyle) && isEmpty == false) {
                filterText = string.Empty;
                GUIUtility.keyboardControl = 0;
            }
        }

        private void CellGUI(Rect cellRect, T item, int columnIndex) {
            CenterRectUsingSingleLineHeight(ref cellRect);
            var column = (CustomTableColumn<T>) multiColumnHeader.GetColumn(columnIndex);
            column.DrawCell?.Invoke(cellRect, item);
        }

        #region private function

        private List<T> Filter(IEnumerable<T> rows) {
            var isHadColumnsShowing = multiColumnHeader.state.visibleColumns.Any(visible => visible == 0);
            if (isHadColumnsShowing && MethodFilter != null) {
                rows = rows.Where(item => MethodFilter(filterMask, item, filterText));
            }

            isReBuildRows = true;
            return rows.ToList();
        }

        private void Sort(IList<TreeViewItem> rows, int sortColumnIndex) {
            // 获取排序类型
            // 升序: 箭头朝上, flag: true
            // 降序: 箭头朝下, flag: false
            var sortType = multiColumnHeader.IsSortedAscending(sortColumnIndex);

            // 获取排序
            var compare = ((CustomTableColumn<T>) multiColumnHeader.state.columns[sortColumnIndex]).Compare;
            var list = (List<TreeViewItem>) rows;
            if (compare == null) {
                return;
            }

            // 调用排序
            if (sortType) {
                list.Sort(ComparisonAsc);
            } else {
                list.Sort(ComparisonDesc);
            }

            // 升序排序
            int ComparisonAsc(TreeViewItem rowA, TreeViewItem rowB) {
                var itemA = (CustomTreeViewItem<T>) rowA;
                var itemB = (CustomTreeViewItem<T>) rowB;
                return compare(itemA.Data, itemB.Data, true);
            }

            // 降序排序
            int ComparisonDesc(TreeViewItem rowA, TreeViewItem rowB) {
                var itemA = (CustomTreeViewItem<T>) rowA;
                var itemB = (CustomTreeViewItem<T>) rowB;
                return -compare(itemA.Data, itemB.Data, false);
            }
        }

        #endregion

        #region override

        protected override void RowGUI(RowGUIArgs args) {
            var item = (CustomTreeViewItem<T>) args.item;
            for (var i = 0; i < args.GetNumVisibleColumns(); i++) {
                CellGUI(args.GetCellRect(i), item.Data, args.GetColumn(i));
            }
        }

        protected override TreeViewItem BuildRoot() {
            return new CustomTreeViewItem<T>(-1, -1, null);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
            if (dataListToShow == null) {
                dataListToShow = new List<T>();
            }

            if (string.IsNullOrEmpty(filterText)) {
                if (dataListToShow.Count != dataList.Count) {
                    dataListToShow = dataList;
                    isReBuildRows = true;
                }
            } else {
                dataListToShow = Filter(dataList);
                MethodAfterFilter?.Invoke(dataListToShow);
            }

            if (allRow == null) {
                allRow = new List<CustomTreeViewItem<T>>();
            }

            if (isReBuildRows) {
                allRow.Clear();
                for (var i = 0; i < dataListToShow.Count; i++) {
                    var data = dataListToShow[i];
                    allRow.Add(new CustomTreeViewItem<T>(i, 0, data));
                }

                isReBuildRows = false;
            }

            var list = allRow.Cast<TreeViewItem>().ToList();
            if (multiColumnHeader.sortedColumnIndex >= 0) {
                Sort(list, multiColumnHeader.sortedColumnIndex);
            }

            return list;
        }

        protected override void KeyEvent() {
            if (Event.current.type != EventType.KeyDown) {
                return;
            }

            if (Event.current.character != '\t') {
                return;
            }

            UnityEngine.GUI.FocusControl(CustomTableStyles.FOCUS_HELPER);

            Event.current.Use();
        }

        protected override void SelectionChanged(IList<int> selectedIds) {
            var list = new List<T>();

            foreach (var id in selectedIds) {
                if (id < 0 || id > dataListToShow.Count) {
                    DebugUtil.LogError($"CustomTreeView: {id} out of range: {dataListToShow.Count}");
                    continue;
                }

                var data = dataListToShow[id];
                list.Add(data);
            }

            MethodSelect?.Invoke(list);
        }

        #endregion

        #region Event Function

        private void OnVisibleColumnChanged(MultiColumnHeader header) {
            Reload();
        }

        private void OnSortingChanged(MultiColumnHeader header) {
            Sort(GetRows(), multiColumnHeader.sortedColumnIndex);
        }

        #endregion
    }
}
