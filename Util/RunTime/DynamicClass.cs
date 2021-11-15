using System;
using System.Reflection;

namespace Script.Effect.Editor.AssetTool.Util.RunTime {
    /// <summary>
    /// 动态类
    /// </summary>
    public class DynamicClass {
        #region 反射筛选标志

        /// <summary>
        /// Public 字段
        /// </summary>
        private const BindingFlags PUBLIC_INSTANCE_FIELD = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;

        /// <summary>
        /// Private 字段
        /// </summary>
        private const BindingFlags PRIVATE_INSTANCE_FIELD = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;

        /// <summary>
        /// Private Static 字段
        /// </summary>
        private const BindingFlags PRIVATE_STATIC_FIELD = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField;

        /// <summary>
        /// Public 方法
        /// </summary>
        private const BindingFlags PUBLIC_INSTANCE_METHOD = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Public Static 方法
        /// </summary>
        private const BindingFlags PUBLIC_STATIC_INSTANCE_METHOD = BindingFlags.Public | BindingFlags.Static;

        /// <summary>
        /// Private 方法
        /// </summary>
        private const BindingFlags PRIVATE_INSTANCE_METHOD = BindingFlags.NonPublic | BindingFlags.Instance;

        #endregion

        /// <summary>
        /// 动态类当前所指代的类
        /// </summary>
        private readonly Type currentClass;

        /// <summary>
        /// 动态类当前所指代的类的实例
        /// </summary>
        private object currentInstance;

        /// <summary>
        /// 设置实例
        /// </summary>
        /// <param name="obj"></param>
        public void SetInstance(object obj) {
            if (obj.GetType() == currentClass) {
                currentInstance = obj;
            }
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public object GetInstance() {
            return currentInstance;
        }

        /// <summary>
        /// 构造函数 (参数: 类型)
        /// </summary>
        /// <param name="currentClass"></param>
        public DynamicClass(Type currentClass) {
            this.currentClass = currentClass;
        }

        /// <summary>
        /// 构造函数 (参数: 类型的实例)
        /// </summary>
        /// <param name="obj"></param>
        public DynamicClass(object obj) {
            if (obj == null) {
                return;
            }

            currentClass = obj.GetType();
            currentInstance = obj;
        }

        #region 获取字段接口

        #region private static

        /// <summary>
        /// 获取类中的 private static 字段的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFieldValue_PrivateStatic<T>(string fieldName) where T : class {
            return GetFieldValue_PrivateStatic(fieldName) as T;
        }

        /// <summary>
        /// 获取类中的 private static 字段的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public object GetFieldValue_PrivateStatic(string fieldName) {
            return GetFiledValue(fieldName, PRIVATE_STATIC_FIELD);
        }

        #endregion

        #region Private

        /// <summary>
        /// 获取类中的 private 字段的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFieldValue_Private<T>(string fieldName) where T : class {
            return GetFieldValue_Private(fieldName) as T;
        }

        /// <summary>
        /// 获取类中的 private 字段的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public object GetFieldValue_Private(string fieldName) {
            return GetFiledValue(fieldName, PRIVATE_INSTANCE_FIELD);
        }

        #endregion

        #region Public

        /// <summary>
        /// 获取类中的 public 字段的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFieldValue_Public<T>(string fieldName) where T : class {
            return GetFieldValue_Public(fieldName) as T;
        }

        /// <summary>
        /// 获取类中的 public 字段的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public object GetFieldValue_Public(string fieldName) {
            return GetFiledValue(fieldName, PUBLIC_INSTANCE_FIELD);
        }

        #endregion

        /// <summary>
        /// 获取类中的字段的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private object GetFiledValue(string fieldName, BindingFlags flags) {
            if (currentClass == null) {
                return null;
            }

            var dynamicField = currentClass.GetField(fieldName, flags);
            return dynamicField == null? null : dynamicField.GetValue(currentInstance);
        }

        #endregion

        #region 调用函数接口

        /// <summary>
        /// 调用 public 函数
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public object CallMethod_Public(string methodName, params object[] args) {
            return InvokeMethod(methodName, PUBLIC_INSTANCE_METHOD, args);
        }

        /// <summary>
        /// 调用 public static 函数
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public object CallMethod_PublicStatic(string methodName, params object[] args) {
            return InvokeMethod(methodName, PUBLIC_STATIC_INSTANCE_METHOD, args);
        }

        /// <summary>
        /// 调用 private 函数
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public object CallMethod_Private(string methodName, params object[] args) {
            return InvokeMethod(methodName, PRIVATE_INSTANCE_METHOD, args);
        }

        /// <summary>
        /// 调用类中的函数
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <param name="args"></param>
        private object InvokeMethod(string methodName, BindingFlags flags, params object[] args) {
            var method = currentClass?.GetMethod(methodName, flags);
            return method?.Invoke(currentInstance, args);
        }

        #endregion

        /// <summary>
        /// 从源实例中取出与目标实例 "同名字段" 的值
        /// </summary>
        /// <param name="dstInstance">目标实例</param>
        /// <param name="dstFlags">目标实例中字段值的类型</param>
        /// <param name="srcInstance">源实例</param>
        /// <param name="srcFlags">源实例中字段值的类型</param>
        public static void Copy(object dstInstance, BindingFlags dstFlags, object srcInstance, BindingFlags srcFlags) {
            if (dstInstance == null || srcInstance == null) {
                return;
            }

            // 取出两个实例的类型
            var srcType = srcInstance.GetType();
            var dstType = dstInstance.GetType();

            // 取出目标实例中的字段
            var dstFields = dstType.GetFields(dstFlags);
            foreach (var dstFieldInfo in dstFields) {
                // 得到源实例中的 "相同字段"
                var srcFieldInfo = srcType.GetField(dstFieldInfo.Name, srcFlags);
                if (srcFieldInfo != null) {
                    if (dstFieldInfo.FieldType == srcFieldInfo.FieldType) {
                        // 取出源字段的值
                        var value = srcFieldInfo.GetValue(srcInstance);

                        // 赋值给目标字段
                        dstFieldInfo.SetValue(dstInstance, value);
                    }
                }
            }
        }
    }
}