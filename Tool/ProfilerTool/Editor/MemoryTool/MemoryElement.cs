using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kuroha.Util.RunTime;

public class MemoryElement : IComparable<MemoryElement>
{
    private int depth;
    public List<MemoryElement> children;
    
    // 下列字段使用了反射, name, totalMemory 需要保持命名与 DLL 中 MemoryElement 类里面的一致
    private string name;
    private long totalMemory;

    /// <summary>
    /// 创建一个 Memory Element
    /// </summary>
    /// <param name="srcMemoryElement"></param>
    /// <param name="depth"></param>
    /// <param name="filterDepth"></param>
    /// <param name="filterSize"></param>
    /// <returns></returns>
    public static MemoryElement Create(DynamicClass srcMemoryElement, int depth, int filterDepth, float filterSize)
    {
        // src = source 源
        if (srcMemoryElement == null)
        {
            return null;
        }
        
        // dst = destination 目的
        var dstMemoryElement = new MemoryElement
        {
            depth = depth,
            children = new List<MemoryElement>(),
        };

        // 赋值
        const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
        DynamicClass.Copy(dstMemoryElement, srcMemoryElement.GetInstance(), FLAGS);

        // 得到源实例中的 children 字段值
        var srcChildren = srcMemoryElement.GetFieldValue_Public<IList>("children");
        if (srcChildren == null)
        {
            return dstMemoryElement;
        }
        
        foreach (var srcChild in srcChildren)
        {
            var memoryElement = Create(new DynamicClass(srcChild), depth + 1, filterDepth, filterSize);
            if (memoryElement == null)
            {
                continue;
            }

            if (depth > filterDepth)
            {
                continue;
            }

            if (memoryElement.totalMemory < filterSize)
            {
                continue;
            }
            
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

        var resultString = string.Format(new string('\t', depth) + " {0}, {1}{2}", text, num, text2);
        return resultString;
    }

    public int CompareTo(MemoryElement other)
    {
        if (other.totalMemory != totalMemory)
        {
            return (int) (other.totalMemory - totalMemory);
        }

        if (string.IsNullOrEmpty(name))
        {
            return -1;
        }

        if (string.IsNullOrEmpty(other.name))
        {
            return 1;
        }

        return string.Compare(name, other.name, StringComparison.Ordinal);
    }
}