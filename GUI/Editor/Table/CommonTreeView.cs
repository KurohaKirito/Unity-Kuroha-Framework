using System.Collections.Generic;
using System.Linq;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.GUI.Editor.Table {
    public class CommonTreeView<T> : TreeView where T : class {
        private int filterMask = -1;
        private string filterText;
        private List<CommonTreeViewItem<T>> items;

        #region private property

        private List<T> DataList { get; }

        private CommonTableDelegate.FilterMethod<T> MethodFilter { get; }

        private CommonTableDelegate.ExportMethod<T> MethodExport { get; }

        private CommonTableDelegate.SelectMethod<T> MethodSelect { get; }

        #endregion

        #region Constructor

        public CommonTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, List<T> dataList, CommonTableDelegate.FilterMethod<T> methodFilter, CommonTableDelegate.ExportMethod<T> methodExport, CommonTableDelegate.SelectMethod<T> methodSelect) : base(state, multiColumnHeader) {
            DataList = dataList;

            MethodFilter = methodFilter;
            MethodExport = methodExport;
            MethodSelect = methodSelect;

            multiColumnHeader.sortingChanged += OnSortingChanged;
            multiColumnHeader.visibleColumnsChanged += OnVisibleColumnChanged;

            showBorder = true;
            showAlternatingRowBackgrounds = true;
            rowHeight = EditorGUIUtility.singleLineHeight;
        }

        #endregion

        public void OnExportGUI(Vector2 exportPosition, bool isDrawExport, float exportWidth, float filterHeight, List<T> dataList) {
            const float EXPORT_OFFSET = -1;

            if (MethodExport == null) {
                return;
            }

            if (!isDrawExport) {
                return;
            }

            if (!UnityEngine.GUI.Button(new Rect(exportPosition.x, exportPosition.y + EXPORT_OFFSET, exportWidth, filterHeight), "Export")) {
                return;
            }

            var path = EditorUtility.SaveFilePanel("Export DataList", Application.dataPath, "dataList.txt", "");
            MethodExport(path, dataList);
        }

        public void OnFilterGUI(Rect rect, bool drawFilter, float rightSpace, string[] displayedOptions) {
            if (!drawFilter) {
                return;
            }

            EditorGUI.BeginChangeCheck();

            var width = rect.width;
            rect.width = UnityEngine.GUI.skin.label.CalcSize(CommonTableStyles.filterSelection).x;
            rect.x = width - rect.width + rightSpace;
            FilterGUI(rect, displayedOptions);

            if (EditorGUI.EndChangeCheck()) {
                Reload();
            }
        }

        private void FilterGUI(Rect rect, string[] displayedOptions) {
            const float FILTER_TYPE_WIDTH = 80;
            const float FILTER_TYPE_OFFSET = -1;
            const float FILTER_NONE_BUTTON_WIDTH = 16;

            // Filter Type
            rect.x -= FILTER_TYPE_WIDTH;
            filterMask = EditorGUI.MaskField(new Rect(rect.x, rect.y + FILTER_TYPE_OFFSET, FILTER_TYPE_WIDTH, rect.height), filterMask, displayedOptions);
            rect.x += FILTER_TYPE_WIDTH;

            // Filter GUI
            rect.width -= FILTER_NONE_BUTTON_WIDTH;
            filterText = EditorGUI.DelayedTextField(rect, GUIContent.none, filterText, CommonTableStyles.searchField);

            // Filter Clear Button
            rect.x += rect.width;
            rect.width = FILTER_NONE_BUTTON_WIDTH;
            var flag = !string.IsNullOrEmpty(filterText);
            var style = !flag? CommonTableStyles.searchFieldCancelButtonEmpty : CommonTableStyles.searchFieldCancelButton;
            if (!UnityEngine.GUI.Button(rect, GUIContent.none, style) || !flag) {
                return;
            }

            filterText = "";
            GUIUtility.keyboardControl = 0;
        }

        private void CellGUI(Rect cellRect, T item, int columnIndex) {
            CenterRectUsingSingleLineHeight(ref cellRect);
            var column = (CommonTableColumn<T>)multiColumnHeader.GetColumn(columnIndex);
            column.DrawCell?.Invoke(cellRect, item);
        }

        #region private function

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="rows">所有行的全部具体信息</param>
        /// <returns></returns>
        private List<CommonTreeViewItem<T>> Filter(IEnumerable<CommonTreeViewItem<T>> rows) {
            var enumerable = rows;

            if (multiColumnHeader.state.visibleColumns.Any(visible => visible == 0) && MethodFilter != null) {
                enumerable = enumerable.Where(item => MethodFilter(filterMask, item.Data, filterText));
            }

            return enumerable.ToList();
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="rows">所有行的全部具体信息</param>
        /// <param name="sortColumnIndex">触发排序的列</param>
        private void Sort(IList<TreeViewItem> rows, int sortColumnIndex) {
            // 获取排序类型
            // 升序: 箭头朝上, flag: true
            // 降序: 箭头朝下, flag: false
            var sortType = multiColumnHeader.IsSortedAscending(sortColumnIndex);

            // 获取排序
            var compare = ((CommonTableColumn<T>)multiColumnHeader.state.columns[sortColumnIndex]).Compare;
            var list = (List<TreeViewItem>)rows;
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
                var itemA = (CommonTreeViewItem<T>)rowA;
                var itemB = (CommonTreeViewItem<T>)rowB;
                return compare(itemA.Data, itemB.Data, true);
            }

            // 降序排序
            int ComparisonDesc(TreeViewItem rowA, TreeViewItem rowB) {
                var itemA = (CommonTreeViewItem<T>)rowA;
                var itemB = (CommonTreeViewItem<T>)rowB;
                return -compare(itemA.Data, itemB.Data, false);
            }
        }

        #endregion

        #region override

        protected override void RowGUI(RowGUIArgs args) {
            var item = (CommonTreeViewItem<T>)args.item;
            for (var i = 0; i < args.GetNumVisibleColumns(); i++) {
                CellGUI(args.GetCellRect(i), item.Data, args.GetColumn(i));
            }
        }

        protected override TreeViewItem BuildRoot() {
            return new CommonTreeViewItem<T>(-1, -1, null);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
            if (items == null) {
                items = new List<CommonTreeViewItem<T>>();
                for (var i = 0; i < DataList.Count; i++) {
                    var data = DataList[i];
                    items.Add(new CommonTreeViewItem<T>(i, 0, data));
                }
            }

            var itemList = items;
            if (!string.IsNullOrEmpty(filterText)) {
                itemList = Filter(itemList);
            }

            var list = itemList.Cast<TreeViewItem>().ToList();

            if (multiColumnHeader.sortedColumnIndex >= 0) {
                Sort(list, multiColumnHeader.sortedColumnIndex);
            }

            return itemList.Cast<TreeViewItem>().ToList();
        }

        protected override void KeyEvent() {
            if (Event.current.type != EventType.KeyDown) {
                return;
            }

            if (Event.current.character != '\t') {
                return;
            }

            UnityEngine.GUI.FocusControl(CommonTableStyles.FOCUS_HELPER);

            Event.current.Use();
        }

        protected override void SelectionChanged(IList<int> selectedIds) {
            var list = new List<T>();

            foreach (var id in selectedIds) {
                if (id < 0 || id > this.DataList.Count) {
                    DebugUtil.LogError(id + "out of range");
                    continue;
                }

                var data = DataList[id];
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