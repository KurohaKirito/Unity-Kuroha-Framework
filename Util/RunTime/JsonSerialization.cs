using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Util.RunTime
{
    /// <summary>
    /// List
    /// </summary>
    [Serializable]
    public class JsonSerialization<T>
    {
        /// <summary>
        /// 实际序列化的字段
        /// </summary>
        [SerializeField]
        private List<T> data;
        
        /// <summary>
        /// 构造方法
        /// </summary>
        public JsonSerialization(List<T> data)
        {
            this.data = data;
        }

        /// <summary>
        /// 转换为字典返回
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            return data;
        }
    }
    
    /// <summary>
    /// Dictionary
    /// </summary>
    [Serializable]
    public class JsonSerialization<TKey, TValue> : ISerializationCallbackReceiver
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
        public JsonSerialization(Dictionary<TKey, TValue> data)
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

    public static class JsonUtil
    {
        /// <summary>
        /// 生成 Json
        /// </summary>
        /// <param name="obj">数据</param>
        /// <param name="filePathAndName">json 路径以及文件名</param>
        public static void ToJsonFile(object obj, string filePathAndName)
        {
            var json = JsonUtility.ToJson(obj);
            System.IO.File.WriteAllText(filePathAndName, json);
        }
    }
}