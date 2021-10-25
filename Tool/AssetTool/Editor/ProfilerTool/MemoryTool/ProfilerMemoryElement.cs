using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kuroha.Util.RunTime;

namespace Kuroha.Tool.Editor.ProfilerTool
{
    public class ProfilerMemoryElement
    {
        public const string DELIMITER = ":";
        private int depth;
        public readonly List<ProfilerMemoryElement> children = new List<ProfilerMemoryElement>();

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
        public static ProfilerMemoryElement Create(DynamicClass srcMemoryElement, int depth, int filterDepth, float filterSize)
        {
            // src = source 源
            if (srcMemoryElement == null)
            {
                return null;
            }
            
            // dst = destination 目的
            var dstMemoryElement = new ProfilerMemoryElement
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
        
        /// <summary>
        /// 结点转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var tabs = new string('\t', depth);
            var assetName = string.IsNullOrEmpty(name) ? "-" : name;
            var size = totalMemory / 1024f;
            
            return $"{tabs}{assetName}{DELIMITER}{size}KB";
        }
    }
}
