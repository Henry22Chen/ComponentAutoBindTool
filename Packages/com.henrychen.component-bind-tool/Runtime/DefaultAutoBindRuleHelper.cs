using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ComponentBind
{
    /// <summary>
    /// 默认自动绑定规则辅助器
    /// </summary>
#if UNITY_EDITOR
    public class DefaultAutoBindRuleHelper : IAutoBindRuleHelper
    {
        public virtual Dictionary<Type, string> CustomTypeMap => _customTypeMap;
        public char ValidSymbol => '@';

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

        public bool IsValidBind(Transform target, ref List<AutoBindField> fields)
        {
            if (string.IsNullOrEmpty(target.name) || target.name[0] != ValidSymbol)
                return false;
            
            fields ??= new List<AutoBindField>();
            
            var typeDict = _fieldName2TypeMap;

            var strArray = target.name[1..].Split('_');

            if (strArray.Length == 1)
            {
                return false;
            }

            var fieldName = strArray[^1].Trim();
            var tempResultDict = new Dictionary<string, AutoBindField>();
            for (var i = 0; i < strArray.Length - 1; i++)
            {
                var typePrefix = strArray[i];

                var genericTypes = typePrefix.Split('`');
                const string genericFieldName = "List";
                if (genericTypes.Length == 2 &&
                    string.Equals(genericTypes[0], genericFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    if (typeDict.TryGetValue(genericTypes[1], out var genericArgs))
                    {
                        var name = $"{genericTypes[1].StartWithLower()}{fieldName.StartWithUpper()}{genericFieldName}";
                        var field = new AutoBindField(name, true, genericArgs);
                        tempResultDict[genericArgs] = field;
                    }
                    else
                    {
                        Debug.LogError($"{target.name} 的命名中 {typePrefix} 不存在对应的组件类型，绑定失败");
                        return false;
                    }

                    continue;
                }

                if (typeDict.TryGetValue(typePrefix, out var comName))
                {
                    if (tempResultDict.ContainsKey(comName)) continue;

                    var name = $"{typePrefix.StartWithLower()}{fieldName.StartWithUpper()}";
                    var field = new AutoBindField(name, false, comName);
                    tempResultDict[comName] = field;
                }
                else
                {
                    Debug.LogError($"{target.name}的命名中{typePrefix}不存在对应的组件类型，绑定失败");
                    return false;
                }
            }

            foreach (var (typeName, field) in tempResultDict)
            {
                var multiTypes = typeName.Split('\n');
                field.PossibleTypes.AddRange(multiTypes);
                fields.Add(field);
            }
            
            return true;
        }

        public void FindListElements(Transform target, AutoBindField field, ref List<Object> elements)
        {
            elements ??= new List<Object>();
            var componentTypeName = field.ComponentType;
            var possibleTypes = field.PossibleTypes;
            var validType = "";
            for (var i = 0; i < target.childCount; i++)
            {
                var errorCount = 0;
                foreach (var typeName in possibleTypes)
                {
                    if (typeName.Equals("GameObject"))
                    {
                        elements.Add(target.GetChild(i).gameObject);
                        break;
                    }
                    
                    
                    var com = target.GetChild(i).GetComponent(typeName);
                    if (com == null)
                    {
                        errorCount++;
                        if (errorCount == possibleTypes.Count)
                            Debug.LogError($"{target.name} 子节点中不存在 {componentTypeName} 类型的组件");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(validType) && validType != typeName)
                        {
                            Debug.LogError($"{target.name} 子节点中存在多个类型的组件 {validType} 和 {typeName}");
                            continue;
                        }
                        
                        validType = typeName;
                        elements.Add(com);
                        break;
                    }
                }
            }
        }
    }
}
#endif