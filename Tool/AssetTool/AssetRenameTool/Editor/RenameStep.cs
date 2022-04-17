using System;

namespace Kuroha.Tool.AssetTool.AssetRenameTool.Editor
{
    public enum OperaType
    {
        Delete,
        Remove,
        Insert,
        Replace,
        Sort
    }

    public enum PositionType
    {
        Begin,
        End,
        Index
    }

    public enum SortType
    {
        A_Z,
        Z_A
    }

    [Serializable]
    public struct DeleteStep
    {
        public int beginIndex;
        public int length;
    }

    [Serializable]
    public struct RemoveStep
    {
        public string regex;
    }

    [Serializable]
    public struct ReplaceStep
    {
        public string regex;
        public string newString;
    }

    [Serializable]
    public struct InsertStep
    {
        public PositionType paramType;
        public int index;
        public string content;
    }

    [Serializable]
    public struct SortStep
    {
        public SortType sortType;
        public int BeginNumber;
        public int length;
    }

    [Serializable]
    public class RenameStep
    {
        public OperaType operaType;

        public InsertStep insertStep;
        public DeleteStep deleteStep;
        public RemoveStep removeStep;
        public ReplaceStep replaceStep;
        public SortStep sortStep;
    }
}
