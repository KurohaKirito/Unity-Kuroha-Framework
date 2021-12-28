using UnityEditor.IMGUI.Controls;

namespace Kuroha.GUI.Editor.Table {
    public class CustomTableColumn<T> : MultiColumnHeaderState.Column {
        public CustomTableDelegate.DrawCellMethod<T> DrawCell { get; set; }

        public CustomTableDelegate.CompareMethod<T> Compare { get; set; }
    }
}