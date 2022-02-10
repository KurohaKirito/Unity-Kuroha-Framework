using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kuroha.Util.RunTime;

namespace Kuroha.Tool.AssetTool.Editor.ProfilerTool.MemoryTool
{
    public class ProfilerMemoryElement
    {
        public const string DELIMITER = ":";
        private int depth;
        public readonly List<ProfilerMemoryElement> children = new List<ProfilerMemoryElement>();

        #region 下列字段使用了反射, 需要保持 "字段名称" 与 DLL 中完全一致, 位于 MemoryElement 类中
        #pragma warning disable 649
        private string name;
        private long totalMemory;
        #pragma warning restore 649
        #endregion

        /// <summary>
        /// 创建一个 Memory Element
        /// </summary>
        public static ProfilerMemoryElement Create(object sourceInstance, int depth, int filterDepth, float filterSize)
        {
            // src = source 源
            if (sourceInstance == null)
            {
                return null;
            }
            
            // dst = destination 目的
            var dstMemoryElement = new ProfilerMemoryElement
            {
                depth = depth
            };
            
            // Type: MemoryElement
            var classInfo = sourceInstance.GetType();

            // 赋值
            const BindingFlags DST_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
            const BindingFlags SRC_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
            ReflectionUtil.GetAllValuesOfSameName(dstMemoryElement, DST_FLAGS, sourceInstance, SRC_FLAGS);
            
            // public List<MemoryElement> children
            var fieldInfo = ReflectionUtil.GetField(classInfo, "children", BindingFlags.Public | BindingFlags.Instance);
            var srcChildrenValue = ReflectionUtil.GetValueField(fieldInfo, sourceInstance);
            if (srcChildrenValue is IList srcChildren)
            {
                foreach (var srcChild in srcChildren)
                {
                    var memoryElement = Create(srcChild, depth + 1, filterDepth, filterSize);
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
