#if UNITY_2019_2_OR_NEWER == false
using UnityEngine;
#endif

// ReSharper disable once CheckNamespace
public static class UnityUtil
{
    #if UNITY_2019_2_OR_NEWER == false
    
    public static bool TryGetComponent<T>(this Component source, out T component) where T : Component
    {
        return (component = source.GetComponent<T>()) != null;
    }

    public static bool TryGetComponent<T>(this Transform source, out T component) where T : Component
    {
        return (component = source.GetComponent<T>()) != null;
    }

    public static bool TryGetComponent<T>(this GameObject source, out T component) where T : Component
    {
        return (component = source.GetComponent<T>()) != null;
    }
    
    #endif
}