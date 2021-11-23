using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Util.Unity {
    public static class UnityUtil {
        public static bool TryGetComponent<T>(this Component source, out T component) where T : Component {
            return (component = source.GetComponent<T>()) != null;
        }
        
        public static bool TryGetComponent<T>(this GameObject source, out T component) where T : Component {
            return (component = source.GetComponent<T>()) != null;
        }
    }
}