
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public class MemoryElement : IComparable<MemoryElement>
{
    //反射 name, totalMemory, children 需要保持命名与dll里面一致
    public string name;
    public long totalMemory;
    public List<MemoryElement> children = new List<MemoryElement>();


    private int _depth;

    private MemoryElement()
    {
    }

    public static MemoryElement Create(DynamicClass srcMemoryElement, int depth, int filterDepth, float filterSize)
    {
        if (srcMemoryElement == null) return null;
        var dstMemoryElement = new MemoryElement { _depth = depth };
        DynamicClass.CopyFrom(dstMemoryElement, srcMemoryElement.CurrentInstance,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);

        var srcChildren = srcMemoryElement.PublicInstanceField<IList>("children");
        if (srcChildren == null) return dstMemoryElement;
        foreach (var srcChild in srcChildren)
        {
            var memoryElement = Create(new DynamicClass(srcChild), depth + 1, filterDepth, filterSize);
            if (memoryElement == null) continue;
            if (depth > filterDepth) continue;
            if (!(memoryElement.totalMemory >= filterSize)) continue;
            dstMemoryElement.children.Add(memoryElement);
        }

        //dstMemoryElement.children.Sort();
        return dstMemoryElement;
    }


    public override string ToString()
    {
        var text = string.IsNullOrEmpty(name) ? "-" : name;
        var text2 = "KB";
        var num = totalMemory / 1024f;
        if (num > 512f)
        {
            num /= 1024f;
            text2 = "MB";
        }

        var resultString = string.Format(new string('\t', _depth) + " {0}, {1}{2}", text, num, text2);
        return resultString;
    }

    public int CompareTo(MemoryElement other)
    {
        if (other.totalMemory != totalMemory)
        {
            return (int) (other.totalMemory - totalMemory);
        }

        if (string.IsNullOrEmpty(name)) return -1;
        return !string.IsNullOrEmpty(other.name) ? string.Compare(name, other.name, StringComparison.Ordinal) : 1;

    }
}