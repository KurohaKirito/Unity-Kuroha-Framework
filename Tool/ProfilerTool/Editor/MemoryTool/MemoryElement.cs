using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kuroha.Util.RunTime;

public class MemoryElement
{
    private int depth;
    public readonly List<MemoryElement> children = new List<MemoryElement>();

    #region 下列字段使用了反射, 需要保持 "字段名称" 与 DLL 中一致
    #pragma warning disable 649
    private string name;
    private long totalMemory;
    #pragma warning restore 649
    #endregion

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
            depth = depth
        };

        // 赋值
        const BindingFlags DST_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
        const BindingFlags SRC_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
        DynamicClass.Copy(dstMemoryElement, DST_FLAGS,
            srcMemoryElement.GetInstance(), SRC_FLAGS);

        // 得到源实例中的 children 字段值
        var srcChildren = srcMemoryElement.GetFieldValue_Public<IList>("children");
        if (srcChildren != null)
        {
            foreach (var srcChild in srcChildren)
            {
                var memoryElement = Create(new DynamicClass(srcChild), depth + 1, filterDepth, filterSize);
                if (memoryElement != null)
                {
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
            }
        }
        
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

        var tab = new string('\t', depth);
        var resultString = $"{tab} {text}, {num}{text2}";
        return resultString;
    }
}