using System;
using System.Collections.Generic;
using UnityEngine;

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
        /// 初始化，用于注册 TypeMap
        /// </summary>
        /// <param name="assemblyNames">需要搜索的程序集</param>
        void Initialize(IEnumerable<string> assemblyNames);
        
        /// <summary>
        /// 是否为有效绑定
        /// </summary>
        bool IsValidBind(Transform target, List<string> fieldNames, List<string> componentTypeNames);
    }
}