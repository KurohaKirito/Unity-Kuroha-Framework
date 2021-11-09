using UnityEditor.IMGUI.Controls;

namespace Script.Effect.Editor.AssetTool.GUI.Editor.Table {
    public class CommonTableColumn<T> : MultiColumnHeaderState.Column {
        public CommonTableDelegate.DrawCellMethod<T> DrawCell { get; set; }

        public CommonTableDelegate.CompareMethod<T> Compare { get; set; }
    }
}