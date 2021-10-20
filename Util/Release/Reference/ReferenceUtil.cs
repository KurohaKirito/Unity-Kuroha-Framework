using System;
using System.Reflection;

namespace Kuroha.Util.Release
{
    public static class ReferenceUtil
    {
        /// <summary>
        /// 获取指定类所在的程序集信息
        /// </summary>
        /// <param name="type">类</param>
        /// <returns></returns>
        public static Assembly GetAssembly(Type type)
        {
            return type.Assembly;
        }

        /// <summary>
        /// 获取指定程序集中指定的类信息
        /// </summary>
        /// <param name="assembly">指定的程序集</param>
        /// <param name="className">类的名字</param>
        /// <returns></returns>
        public static Type GetClass(Assembly assembly, string className)
        {
            return assembly.GetType(className);
        }
        
        /// <summary>
        /// 获取指定类的指定字段信息
        /// </summary>
        /// <param name="dynamicClass">指定的类</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="fieldFlags">字段修饰类型</param>
        /// <returns></returns>
        public static FieldInfo GetField(Type dynamicClass, string fieldName, BindingFlags fieldFlags)
        {
            if (ReferenceEquals(dynamicClass, null) == false)
            {
                var currentField = dynamicClass.GetField(fieldName, fieldFlags);
                return currentField;
            }
            
            return null;
        }

        /// <summary>
        /// 获取指定字段所保存的值
        /// </summary>
        /// <param name="dynamicField">指定的字段信息</param>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <returns></returns>
        public static TValue GetValue<TValue>(FieldInfo dynamicField) where TValue : class
        {
            var fieldValue = dynamicField.GetValue(dynamicField);
            return fieldValue as TValue;
        }
        
        /// <summary>
        /// 获取指定字段所保存的值
        /// </summary>
        /// <param name="dynamicField">指定的字段信息</param>
        /// <returns></returns>
        public static object GetValue(FieldInfo dynamicField)
        {
            return dynamicField.GetValue(dynamicField);
        }
        
        
        
        /*
        // 获取到 UnityEditor 程序集信息
        var dynamicAssembly = Kuroha.Util.Release.ReferenceUtil.GetAssembly(typeof(EditorWindow));
            
        // 获取到 UnityEditor.ProfilerWindow 类信息
        const string CLASS_NAME = "UnityEditor.ProfilerWindow";
        var dynamicClass = Kuroha.Util.Release.ReferenceUtil.GetClass(dynamicAssembly, CLASS_NAME);
            
        // 获取到 m_ProfilerWindows 字段信息
        var dynamicFieldProfilerWindows = Kuroha.Util.Release.ReferenceUtil.GetField(dynamicClass, "m_ProfilerWindows", 
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.GetField);
        
        // 获取字段中保存的值
        var profilerWindowList = Kuroha.Util.Release.ReferenceUtil.GetValue<IList>(dynamicFieldProfilerWindows);
        Debug.Log($"获取到了 list 字段, 字段内的数量为 {profilerWindowList.Count} ");
        
        // 获取到 m_CurrentArea 字段信息
        var dynamicFieldCurrentArea = Kuroha.Util.Release.ReferenceUtil.GetField(dynamicClass, "m_CurrentArea",
            System.Reflection.BindingFlags.Instance|
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.GetField);
        // 获取字段中保存的值
        var currentArea = Kuroha.Util.Release.ReferenceUtil.GetValue(dynamicFieldCurrentArea);
        Debug.Log($"当前选中的页面是 {(UnityEngine.Profiling.ProfilerArea)currentArea}");
        */
    }
}



