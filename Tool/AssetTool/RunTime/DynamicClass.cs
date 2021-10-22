using System;
using System.Reflection;

/// <summary>
/// 动态类
/// </summary>
public class DynamicClass
{
    #region 反射筛选标志

    /// <summary>
    /// Public 字段
    /// </summary>
    private const BindingFlags PUBLIC_INSTANCE_FIELD =
        BindingFlags.Public |
        BindingFlags.Instance |
        BindingFlags.GetField;
    
    /// <summary>
    /// Private 字段
    /// </summary>
    private const BindingFlags PRIVATE_INSTANCE_FIELD =
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.GetField;
    
    /// <summary>
    /// Private Static 字段
    /// </summary>
    private const BindingFlags PRIVATE_STATIC_FIELD =
        BindingFlags.NonPublic |
        BindingFlags.Static |
        BindingFlags.GetField;
    
    /// <summary>
    /// Public 方法
    /// </summary>
    private const BindingFlags PUBLIC_INSTANCE_METHOD =
        BindingFlags.Public |
        BindingFlags.Instance;
    
    /// <summary>
    /// Private 方法
    /// </summary>
    private const BindingFlags PRIVATE_INSTANCE_METHOD =
        BindingFlags.NonPublic |
        BindingFlags.Instance;

    #endregion
    
    /// <summary>
    /// 动态类当前所指代的类
    /// </summary>
    private readonly Type currentClass;
    
    /// <summary>
    /// 动态类当前所指代的类的实例
    /// </summary>
    public object CurrentInstance { get; private set; }

    /// <summary>
    /// 构造函数 (参数: 类型)
    /// </summary>
    /// <param name="currentClass"></param>
    public DynamicClass(Type currentClass)
    {
        this.currentClass = currentClass;
    }

    /// <summary>
    /// 构造函数 (参数: 类型的实例)
    /// </summary>
    /// <param name="obj"></param>
    public DynamicClass(object obj)
    {
        if (obj == null)
        {
            return;
        }
        
        currentClass = obj.GetType();
        CurrentInstance = obj;
    }

    public static void CopyFrom(object dst, object src, BindingFlags flags)
    {
        if (dst == null || src == null)
        {
            return;
        }
        
        var srcType = src.GetType();
        var dstType = dst.GetType();
        
        var dstFields = dstType.GetFields(flags);
        foreach (var dstFieldInfo in dstFields)
        {
            var srcFieldInfo = srcType.GetField(dstFieldInfo.Name, flags);
            if (srcFieldInfo != null && dstFieldInfo.FieldType == srcFieldInfo.FieldType)
            {
                dstFieldInfo.SetValue(dst, srcFieldInfo.GetValue(src));
            }
        }
    }

    public void SetInstance(object obj)
    {
        if (obj.GetType() == currentClass)
        {
            CurrentInstance = obj;
        }
    }

    public object PrivateStaticField(string fieldName)
    {
        return _GetFiled(fieldName, PRIVATE_STATIC_FIELD);
    }

    public T PrivateStaticField<T>(string fieldName) where T : class
    {
        return PrivateStaticField(fieldName) as T;
    }

    public object PrivateInstanceField(string fieldName)
    {
        return _GetFiled(fieldName, PRIVATE_INSTANCE_FIELD);
    }

    public T PrivateInstanceField<T>(string fieldName) where T : class
    {
        return PrivateInstanceField(fieldName) as T;
    }

    public object PublicInstanceField(string fieldName)
    {
        return _GetFiled(fieldName, PUBLIC_INSTANCE_FIELD);
    }

    public T PublicInstanceField<T>(string fieldName) where T : class
    {
        return PublicInstanceField(fieldName) as T;
    }

    public void CallPublicInstanceMethod(string methodName, params object[] args)
    {
        _InvokeMethod(methodName, PUBLIC_INSTANCE_METHOD, args);
    }

    public void CallPrivateInstanceMethod(string methodName, params object[] args)
    {
        _InvokeMethod(methodName, PRIVATE_INSTANCE_METHOD, args);
    }

    private object _GetFiled(string fieldName, BindingFlags flags)
    {
        if (null == currentClass) return null;
        var field = currentClass.GetField(fieldName, flags);
        return field != null ? field.GetValue(CurrentInstance) : null;
    }

    private void _InvokeMethod(string methodName, BindingFlags flags, params object[] args)
    {
        if (currentClass == null) return;
        var method = currentClass.GetMethod(methodName, flags);
        if (method == null) return;
        method.Invoke(CurrentInstance, args);
    }
}
