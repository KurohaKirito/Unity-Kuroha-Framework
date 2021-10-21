using System;
using System.Reflection;

public class DynamicClass
{
    private const BindingFlags PUBLIC_INSTANCE_FIELD_FLAG = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField;

    private const BindingFlags PRIVATE_INSTANCE_FIELD_FLAG = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField;

    private const BindingFlags PRIVATE_STATIC_FIELD_FLAG = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField;

    private const BindingFlags PUBLIC_INSTANCE_METHOD_FLAG = BindingFlags.Instance | BindingFlags.Public;

    private const BindingFlags PRIVATE_INSTANCE_METHOD_FLAG = BindingFlags.Instance | BindingFlags.NonPublic;

    public readonly Type InnerType;
    
    public object InnerObject { get; private set; }

    public DynamicClass(Type innerType)
    {
        InnerType = innerType;
    }

    public DynamicClass(object obj)
    {
        if (null == obj) return;
        InnerType = obj.GetType();
        InnerObject = obj;
    }

    public static void CopyFrom(object dst, object src, BindingFlags flags)
    {
        if (dst == null || src == null) return;
        var srcType = src.GetType();
        var dstType = dst.GetType();
        var dstFields = dstType.GetFields(flags);
        var dstArray = dstFields;
        foreach (var dstFieldInfo in dstArray)
        {
            var srcFieldInfo = srcType.GetField(dstFieldInfo.Name, flags);
            if (srcFieldInfo != null && dstFieldInfo.FieldType == srcFieldInfo.FieldType)
            {
                dstFieldInfo.SetValue(dst, srcFieldInfo.GetValue(src));
            }
        }
    }

    public void SetObject(object obj)
    {
        if (obj.GetType() == InnerType)
        {
            InnerObject = obj;
        }
    }

    public object PrivateStaticField(string fieldName)
    {
        return _GetFiled(fieldName, PRIVATE_STATIC_FIELD_FLAG);
    }

    public T PrivateStaticField<T>(string fieldName) where T : class
    {
        return PrivateStaticField(fieldName) as T;
    }

    public object PrivateInstanceField(string fieldName)
    {
        return _GetFiled(fieldName, PRIVATE_INSTANCE_FIELD_FLAG);
    }

    public T PrivateInstanceField<T>(string fieldName) where T : class
    {
        return PrivateInstanceField(fieldName) as T;
    }

    public object PublicInstanceField(string fieldName)
    {
        return _GetFiled(fieldName, PUBLIC_INSTANCE_FIELD_FLAG);
    }

    public T PublicInstanceField<T>(string fieldName) where T : class
    {
        return PublicInstanceField(fieldName) as T;
    }

    public void CallPublicInstanceMethod(string methodName, params object[] args)
    {
        _InvokeMethod(methodName, PUBLIC_INSTANCE_METHOD_FLAG, args);
    }

    public void CallPrivateInstanceMethod(string methodName, params object[] args)
    {
        _InvokeMethod(methodName, PRIVATE_INSTANCE_METHOD_FLAG, args);
    }

    private object _GetFiled(string fieldName, BindingFlags flags)
    {
        if (null == InnerType) return null;
        var field = InnerType.GetField(fieldName, flags);
        return field != null ? field.GetValue(InnerObject) : null;
    }

    private void _InvokeMethod(string methodName, BindingFlags flags, params object[] args)
    {
        if (InnerType == null) return;
        var method = InnerType.GetMethod(methodName, flags);
        if (method == null) return;
        method.Invoke(InnerObject, args);
    }
}
