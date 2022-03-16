using System;
using System.Reflection;

namespace Kuroha.Framework.Utility.RunTime
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    public static class ReflectionUtil
    {
        /// <summary>
        /// 获取程序集
        /// </summary>
        public static Assembly GetAssembly(Type type)
        {
            if (type != null)
            {
                return type.Assembly;
            }
            
            DebugUtil.LogError("类为空, 无法获得类所在的程序集", null, "red");
            return null;
        }
        
        /// <summary>
        /// 获取指定命名空间下指定名称的类
        /// </summary>
        public static Type GetClass(Assembly assembly, string className)
        {
            if (assembly != null)
            {
                return assembly.GetType(className);
            }
            
            DebugUtil.LogError("程序集为空, 无法获得程序集中指定名称的类", null, "red");
            return null;
        }
        
        /// <summary>
        /// 获取指定命名空间下全部的类
        /// </summary>
        public static Type[] GetAllClass(Assembly assembly)
        {
            if (assembly != null)
            {
                return assembly.GetTypes();
            }
            
            DebugUtil.LogError("程序集为空, 无法获得程序集中的类", null, "red");
            return null;
        }

        /// <summary>
        /// 获取方法
        /// </summary>
        public static MethodInfo GetMethod(Type currentClass, string methodName, BindingFlags targetFlags, Type[] parameterTypes)
        {
            if (currentClass != null)
            {
                var method = currentClass.GetMethod(methodName, targetFlags, Type.DefaultBinder, parameterTypes, null);
                
                if (method != null)
                {
                    return method;
                }
                
                DebugUtil.LogError($"{methodName} 方法获取失败, 请检查 '名称', '访问权限', '参数类型' 是否正确", null, "red");
            }
            else
            {
                DebugUtil.LogError("类为空, 无法获得类中指定名称, 指定参数类型的类", null, "red");
            }

            return null;
        }
        
        /// <summary>
        /// 获取方法
        /// </summary>
        public static MethodInfo GetMethod(Type currentClass, string methodName, BindingFlags targetFlags)
        {
            if (currentClass != null)
            {
                var method = currentClass.GetMethod(methodName, targetFlags);
                
                if (method != null)
                {
                    return method;
                }
                
                DebugUtil.LogError($"{methodName} 方法获取失败, 请检查 '名称', '访问权限' 是否正确", null, "red");
            }
            else
            {
                DebugUtil.LogError("类为空, 无法获得类中的方法", null, "red");
            }

            return null;
        }
        
        /// <summary>
        /// 获取方法
        /// </summary>
        public static MethodInfo GetMethod(Type currentClass, string methodName)
        {
            if (currentClass != null)
            {
                var method = currentClass.GetMethod(methodName);
                
                if (method != null)
                {
                    return method;
                }
                
                DebugUtil.LogError($"{methodName} 方法获取失败, 请检查 '名称', '访问权限' 是否正确", null, "red");
            }
            else
            {
                DebugUtil.LogError("类为空, 无法获得类中的方法", null, "red");
            }

            return null;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        public static object CallMethod(MethodInfo method, object classInstance, object[] args)
        {
            if (method != null)
            {
                return method.Invoke(classInstance, args);
            }

            DebugUtil.LogError("方法为空, 无法执行方法", null, "red");
            return null;
        }
        
        /// <summary>
        /// 调用方法
        /// </summary>
        public static object CallMethod(MethodInfo method, object[] args)
        {
            if (method != null)
            {
                return method.Invoke(null, args);
            }

            DebugUtil.LogError("方法为空, 无法执行方法", null, "red");
            return null;
        }

        /// <summary>
        /// 获取字段
        /// </summary>
        public static FieldInfo GetField(Type currentClass, string fieldName, BindingFlags targetFlags)
        {
            if (currentClass != null)
            {
                return currentClass.GetField(fieldName, targetFlags);
            }

            DebugUtil.LogError("类为空, 无法获得类中的字段", null, "red");
            return null;
        }
        
        /// <summary>
        /// 获取字段
        /// </summary>
        public static FieldInfo GetField(Type currentClass, string fieldName)
        {
            if (currentClass != null)
            {
                return currentClass.GetField(fieldName);
            }

            DebugUtil.LogError("类为空, 无法获得类中的字段", null, "red");
            return null;
        }
        
        /// <summary>
        /// 获取字段的值
        /// </summary>
        public static object GetValueField(FieldInfo field, object classInstance)
        {
            if (field != null)
            {
                return field.GetValue(classInstance);
            }
            
            DebugUtil.LogError("字段为空, 无法获得字段的值", null, "red");
            return null;
        }
        
        /// <summary>
        /// 获取字段的值
        /// </summary>
        public static object GetValueField(FieldInfo field)
        {
            if (field != null)
            {
                return field.GetValue(null);
            }
            
            DebugUtil.LogError("字段为空, 无法获得字段的值", null, "red");
            return null;
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        public static PropertyInfo GetProperty(Type currentClass, string propertyName, BindingFlags targetFlags)
        {
            if (currentClass != null)
            {
                return currentClass.GetProperty(propertyName, targetFlags);
            }
            
            DebugUtil.LogError("类为空, 无法获得类中的属性", null, "red");
            return null;
        }
        
        /// <summary>
        /// 获取属性
        /// </summary>
        public static PropertyInfo GetProperty(Type currentClass, string propertyName)
        {
            if (currentClass != null)
            {
                return currentClass.GetProperty(propertyName);
            }
            
            DebugUtil.LogError("类为空, 无法获得类中的属性", null, "red");
            return null;
        }
        
        /// <summary>
        /// 获取属性的值
        /// </summary>
        public static object GetValueProperty(PropertyInfo property, object classInstance)
        {
            if (property != null)
            {
                return property.GetValue(classInstance);
            }
            
            DebugUtil.LogError("属性为空, 无法获得属性的值", null, "red");
            return null;
        }
        
        /// <summary>
        /// 获取属性的值
        /// </summary>
        public static object GetValueProperty(PropertyInfo property)
        {
            if (property != null)
            {
                return property.GetValue(null);
            }
            
            DebugUtil.LogError("属性为空, 无法获得属性的值", null, "red");
            return null;
        }

        /// <summary>
        /// 从源实例中取出与目标实例 "同名字段" 的值
        /// 可以实现一次性快速地从反射出的实例中取出大量基础类型的值
        /// </summary>
        /// <param name="targetInstance">目标实例</param>
        /// <param name="targetFlags">目标实例中字段值的类型</param>
        /// <param name="sourceInstance">源实例</param>
        /// <param name="sourceFlags">源实例中字段值的类型</param>
        public static void GetAllValuesOfSameName(object targetInstance, BindingFlags targetFlags, object sourceInstance, BindingFlags sourceFlags)
        {
            if (targetInstance == null || sourceInstance == null)
            {
                return;
            }

            // 取出两个实例的类型
            var sourceType = sourceInstance.GetType();
            var targetType = targetInstance.GetType();

            // 取出目标实例中的字段
            var targetFields = targetType.GetFields(targetFlags);
            foreach (var targetFieldInfo in targetFields)
            {
                // 得到源实例中的 "相同字段"
                var sourceFieldInfo = sourceType.GetField(targetFieldInfo.Name, sourceFlags);
                if (sourceFieldInfo != null)
                {
                    if (targetFieldInfo.FieldType == sourceFieldInfo.FieldType)
                    {
                        // 取出源字段的值
                        var value = sourceFieldInfo.GetValue(sourceInstance);

                        // 赋值给目标字段
                        targetFieldInfo.SetValue(targetInstance, value);
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定类型的自定义特性
        /// </summary>
        /// <param name="classInfo">类信息</param>
        /// <param name="inherit">是否查询继承链</param>
        public static T GetCustomAttribute<T>(Type classInfo, bool inherit) where T : Attribute
        {
            if (classInfo != null)
            {
                return classInfo.GetCustomAttribute<T>(inherit);
            }
            
            DebugUtil.LogError("类信息为空, 无法获得类中的特性", null, "red");
            
            return null;
        }
    }
}
