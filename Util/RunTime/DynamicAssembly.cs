using System;
using System.Reflection;

namespace Script.Effect.Editor.AssetTool.Util.RunTime {
    /// <summary>
    /// 动态程序集
    /// </summary>
    public class DynamicAssembly {
        /// <summary>
        /// 程序集
        /// </summary>
        private readonly Assembly assembly;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type">类型</param>
        public DynamicAssembly(Type type) {
            assembly = type.Assembly;
        }

        /// <summary>
        /// 获取指定的类
        /// </summary>
        /// <param name="className">类名</param>
        /// <returns></returns>
        public DynamicClass GetClass(string className) {
            return new DynamicClass(assembly.GetType(className));
        }
    }
}