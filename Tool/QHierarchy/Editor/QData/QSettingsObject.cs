using UnityEngine;
using UnityEditor;
using System;
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
            if (value is bool)
            {
                settingBoolValues[settingBoolNames.IndexOf(settingName)] = (bool)value;
            }
            else if (value is string)
            {
                settingStringValues[settingStringNames.IndexOf(settingName)] = (string)value;
            }
            else if (value is float)
            {
                settingFloatValues[settingFloatNames.IndexOf(settingName)] = (float)value;
            }
            else if (value is int)
            {
                settingIntValues[settingIntNames.IndexOf(settingName)] = (int)value;
            }
            EditorUtility.SetDirty(this);
        }

        public object Get(string settingName, object defaultValue)
        {
            if (defaultValue is bool b)
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
            
            if (defaultValue is string s)
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
            
            if (defaultValue is float f)
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
            
            if (defaultValue is int i)
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
            return null;
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
