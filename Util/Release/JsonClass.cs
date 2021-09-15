using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Util.Release
{
    /// <summary>
    /// Dictionary
    /// </summary>
    [Serializable]
    public class JsonClass<TKey, TValue> : ISerializationCallbackReceiver
    {
        /// <summary>
        /// 实际序列化的字段
        /// </summary>
        [SerializeField]
        private List<TKey> keys;
        
        /// <summary>
        /// 实际序列化的字段
        /// </summary>
        [SerializeField]
        private List<TValue> values;

        /// <summary>
        /// 待序列化的数据
        /// </summary>
        private Dictionary<TKey, TValue> data;
        
        /// <summary>
        /// 构造方法
        /// </summary>
        public JsonClass(Dictionary<TKey, TValue> data)
        {
            this.data = data;
        }

        /// <summary>
        /// 序列化前调用
        /// </summary>
        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(data.Keys);
            values = new List<TValue>(data.Values);
        }

        /// <summary>
        /// 反序列化后调用
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (keys.Count == values.Count)
            {
                data = new Dictionary<TKey, TValue>(keys.Count);
                for (var index = 0; index < keys.Count; ++index)
                {
                    data.Add(keys[index], values[index]);
                }
            }
        }
        
        /// <summary>
        /// 转换为字典返回
        /// </summary>
        /// <returns></returns>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            return data;
        }
    }
}