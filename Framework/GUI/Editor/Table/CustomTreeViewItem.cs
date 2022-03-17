using UnityEditor.IMGUI.Controls;

namespace Kuroha.Framework.GUI.Editor.Table
{
    internal class CustomTreeViewItem<T> : TreeViewItem where T : class
    {
        public T Data{ get; }

        public CustomTreeViewItem(int id, int depth, T data) : base(id, depth, data == null? "Root" : data.ToString())
        {
            Data = data;
        }
    }
}
