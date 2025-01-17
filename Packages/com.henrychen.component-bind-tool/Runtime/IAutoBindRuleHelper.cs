using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ComponentBind
{
    /// <summary>
    /// 自动绑定规则辅助器接口
    /// </summary>
    public interface IAutoBindRuleHelper
    {
        /// <summary>
        /// 自定义的 (类型, 前缀) 字典，如果 key 与默认的相同，将覆盖默认设置
        /// </summary>
        Dictionary<Type, string> CustomTypeMap { get; }
        
        /// <summary>
        /// 定义一个物体名称是一个合法的需要绑定的符号前缀，不以该符号开头的物体会被忽略
        /// </summary>
        char ValidSymbol { get; }

        /// <summary>
        /// 初始化，用于注册 TypeMap
        /// </summary>
        /// <param name="assemblyNames">需要搜索的程序集</param>
        void Initialize(IEnumerable<string> assemblyNames);
        
        /// <summary>
        /// 是否为有效绑定
        /// </summary>
        bool IsValidBind(Transform target, ref List<AutoBindField> fields);

        /// <summary>
        /// 获取列表（数组）元素
        /// </summary>
        /// <param name="target"></param>
        /// <param name="field"></param>
        /// <param name="elements"></param>
        void FindListElements(Transform target, AutoBindField field, ref List<Object> elements);
    }

    public class AutoBindField
    {
        public string Name { get; }
        public bool IsList { get; }
        /// <summary>
        /// 原始的类型名，可能由多个类型名组成，如："Text\nTMP_Text"
        /// </summary>
        public string ComponentType { get; }
        /// <summary>
        /// 类型列表，可能会有一个前缀对应多个类型的情况，如：txt 对应 Text 和 TMP_Text
        /// </summary>
        public List<string> PossibleTypes { get; } = new();
        
        public AutoBindField(string name, bool isList, string componentType)
        {
            Name = name;
            IsList = isList;
            ComponentType = componentType;
        }
    }
}