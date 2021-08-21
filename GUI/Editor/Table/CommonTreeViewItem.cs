using UnityEditor.IMGUI.Controls;

namespace Kuroha.GUI.Editor.Table
{
    internal class CommonTreeViewItem<T> : TreeViewItem where T : class
    {
        public T Data { get; }

        public CommonTreeViewItem(int id, int depth, T data) : base(id, depth, data == null ? "Root" : data.ToString())
        {
            Data = data;
        }
    }
}