using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ComponentBind
{
    /// <summary>
    /// 默认自动绑定规则辅助器
    /// </summary>
#if UNITY_EDITOR
    public class DefaultAutoBindRuleHelper : IAutoBindRuleHelper
    {
        public virtual Dictionary<Type, string> CustomTypeMap => _customTypeMap;

        private readonly Dictionary<Type, string> _customTypeMap = new();
        private Dictionary<string, string> _fieldName2TypeMap = new();
        private bool _isInitialized = false;

        public void RegisterAutoBind(IEnumerable<string> assemblyNames)
        {
            var attributeType = typeof(AutoBindableAttribute);
            foreach (var assemblyName in assemblyNames)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch
                {
                    continue;
                }

                if (assembly == null)
                {
                    continue;
                }

                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsDefined(attributeType)) continue;
                    
                    var attribute = (AutoBindableAttribute)type.GetCustomAttribute(attributeType);
                    var prefixName = attribute.PrefixName;
                    if (!string.IsNullOrEmpty(prefixName))
                    {
                        RegisterTypeMap(type, prefixName);
                    }
                    else
                    {
                        Debug.LogError($"prefixName cannot be null or empty in {type.Name}");
                    }
                }
            }
        }

        protected virtual void RegisterDefaultTypeMap()
        {
            RegisterTypeMap(typeof(List<>), "List");
        }

        protected void RegisterTypeMap(Type type, string name)
        {
            _customTypeMap.TryAdd(type, name);
        }

        private Dictionary<string, string> GetMapping()
        {
            if (_isInitialized)
                return _fieldName2TypeMap;
            
            RegisterDefaultTypeMap();

            var dict = _customTypeMap
                .Concat(ComponentAutoBindTool.NamePrefixDict.Where(pair => !_customTypeMap.ContainsKey(pair.Key)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            _fieldName2TypeMap = new Dictionary<string, string>();
            foreach (var pair in dict)
            {
                if (_fieldName2TypeMap.ContainsKey(pair.Value))
                {
                    _fieldName2TypeMap[pair.Value] = $"{_fieldName2TypeMap[pair.Value]}\n{pair.Key.Name}";
                }
                else
                {
                    _fieldName2TypeMap[pair.Value] = pair.Key.Name;
                }
            }

            _isInitialized = true;
            return _fieldName2TypeMap;
        }

        public virtual void Initialize(IEnumerable<string> assemblyNames)
        {
            if (_isInitialized)
                return;

            RegisterDefaultTypeMap();
            RegisterAutoBind(assemblyNames);
            
            var dict = _customTypeMap
                .Concat(ComponentAutoBindTool.NamePrefixDict.Where(pair => !_customTypeMap.ContainsKey(pair.Key)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            _fieldName2TypeMap = new Dictionary<string, string>();
            foreach (var pair in dict)
            {
                if (_fieldName2TypeMap.ContainsKey(pair.Value))
                {
                    _fieldName2TypeMap[pair.Value] = $"{_fieldName2TypeMap[pair.Value]}\n{pair.Key.Name}";
                }
                else
                {
                    _fieldName2TypeMap[pair.Value] = pair.Key.Name;
                }
            }
            
            
            _isInitialized = true;
        }

        public virtual bool IsValidBind(Transform target, List<string> fieldNames, List<string> componentTypeNames)
        {
            var typeDict = _fieldName2TypeMap;

            var strArray = target.name.Split('_');

            if (strArray.Length == 1)
            {
                return false;
            }

            var fieldName = strArray[^1].Trim();

            for (var i = 0; i < strArray.Length - 1; i++)
            {
                var str = strArray[i];

                var genericTypes = str.Split('`');
                const string genericFieldName = "@List";
                if (genericTypes.Length == 2 &&
                    string.Equals(genericTypes[0], genericFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    if (typeDict.TryGetValue(genericTypes[1], out var genericArgs))
                    {
                        fieldNames.Add(
                            $"{genericTypes[1].StartWithLower()}{fieldName.StartWithUpper()}{genericFieldName}");

                        componentTypeNames.Add(genericArgs);
                    }
                    else
                    {
                        Debug.LogError($"{target.name} 的命名中 {str} 不存在对应的组件类型，绑定失败");
                        return false;
                    }

                    continue;
                }

                if (typeDict.TryGetValue(str, out var comName))
                {
                    if (componentTypeNames.Contains(comName)) continue;

                    fieldNames.Add($"{str.StartWithLower()}{fieldName.StartWithUpper()}");
                    componentTypeNames.Add(comName);
                }
                else
                {
                    Debug.LogError($"{target.name}的命名中{str}不存在对应的组件类型，绑定失败");
                    return false;
                }
            }
            
            return true;
        }
    }
}
#endif