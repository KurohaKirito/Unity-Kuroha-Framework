using UnityEngine;

namespace Script.Effect.Editor.AssetTool.GUI.Editor.Table {
    internal static class CommonTableStyles {
        public const string FOCUS_HELPER = "SerializedPropertyTreeViewFocusHelper";
        public const string SERIALIZE_FILTER_SELECTION = "_FilterSelection";
        public const string SERIALIZE_FILTER_DISABLE = "_FilterDisable";
        public const string SERIALIZE_FILTER_INVERT = "_FilterInvert";
        public const string SERIALIZE_TREE_VIEW_STATE = "_TreeViewState";
        public const string SERIALIZE_COLUMN_HEADER_STATE = "_ColumnHeaderState";
        public const string SERIALIZE_FILTER = "_Filter_";
        public static readonly GUIContent filterDisable = new GUIContent("Disable All|Disables all filters.");
        public static readonly GUIContent filterInvert = new GUIContent("Invert Result|Inverts the filtered results.");
        public static readonly GUIContent filterSelection = new GUIContent("Lock Selection|Limits the table contents to the active selection.");
        public static readonly GUIStyle searchField = "SearchTextField";
        public static readonly GUIStyle searchFieldCancelButton = "SearchCancelButton";
        public static readonly GUIStyle searchFieldCancelButtonEmpty = "SearchCancelButtonEmpty";
    }
}