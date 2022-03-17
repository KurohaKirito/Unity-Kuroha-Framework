using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Kuroha.Tool.QHierarchy.Editor.QData
{
    [System.Serializable]
    internal class QSettingsObject: ScriptableObject
    {
        [SerializeField] private List<string> settingStringNames  = new List<string>();
        [SerializeField] private List<string> settingStringValues = new List<string>();

        [SerializeField] private List<string> settingFloatNames   = new List<string>();
        [SerializeField] private List<float>  settingFloatValues  = new List<float>();

        [SerializeField] private List<string> settingIntNames     = new List<string>();
        [SerializeField] private List<int>    settingIntValues    = new List<int>();

        [SerializeField] private List<string> settingBoolNames   = new List<string>();
        [SerializeField] private List<bool>   settingBoolValues  = new List<bool>();

        public void Clear()
        {
            settingStringNames.Clear();
            settingStringValues.Clear();
            settingFloatNames.Clear();
            settingFloatValues.Clear();
            settingIntNames.Clear();
            settingIntValues.Clear();
            settingBoolNames.Clear();
            settingBoolValues.Clear();
        }

        public void Set(string settingName, object value)
        {
            switch (value)
            {
                case bool b:
                    settingBoolValues[settingBoolNames.IndexOf(settingName)] = b;
                    break;
                
                case string s:
                    settingStringValues[settingStringNames.IndexOf(settingName)] = s;
                    break;
                
                case float f:
                    settingFloatValues[settingFloatNames.IndexOf(settingName)] = f;
                    break;
                
                case int i:
                    settingIntValues[settingIntNames.IndexOf(settingName)] = i;
                    break;
            }

            EditorUtility.SetDirty(this);
        }

        public object Get(string settingName, object defaultValue)
        {
            switch (defaultValue)
            {
                case bool b:
                {
                    var id = settingBoolNames.IndexOf(settingName);
                    if (id == -1)
                    {
                        settingBoolNames.Add(settingName);
                        settingBoolValues.Add(b);
                        return b;
                    }
                
                    return settingBoolValues[id];
                }
                case string s:
                {
                    var id = settingStringNames.IndexOf(settingName);
                    if (id == -1) 
                    {
                        settingStringNames.Add(settingName);
                        settingStringValues.Add(s);
                        return s;
                    }
                
                    return settingStringValues[id];
                }
                case float f:
                {
                    var id = settingFloatNames.IndexOf(settingName);
                    if (id == -1) 
                    {
                        settingFloatNames.Add(settingName);
                        settingFloatValues.Add(f);
                        return f;
                    }
                
                    return settingFloatValues[id];
                }
                case int i:
                {
                    var id = settingIntNames.IndexOf(settingName);
                    if (id == -1) 
                    {
                        settingIntNames.Add(settingName);
                        settingIntValues.Add(i);
                        return i;
                    }
                
                    return settingIntValues[id];
                }
                default:
                    return null;
            }
        }
        
        public object Get<T>(string settingName)
        {
            if (typeof(T) == typeof(bool))
            {
                var id = settingBoolNames.IndexOf(settingName);
                return id == -1 ? (object) null : settingBoolValues[id];
            }
            
            if (typeof(T) == typeof(string))
            {
                var id = settingStringNames.IndexOf(settingName);
                return id == -1 ? null : settingStringValues[id];
            }
            
            if (typeof(T) == typeof(float))
            {
                var id = settingFloatNames.IndexOf(settingName);
                return id == -1 ? (object) null : settingFloatValues[id];
            }
            
            if (typeof(T) == typeof(int))
            {
                var id = settingIntNames.IndexOf(settingName);
                return id == -1 ? (object) null : settingIntValues[id];
            }
            
            return null;
        }
    }
}
