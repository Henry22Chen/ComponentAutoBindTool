using System;

namespace ComponentBind
{
    /// <summary>
    /// 自定义组件的绑定前缀
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoBindableAttribute : Attribute
    {
        public string PrefixName { get; set; }

        public AutoBindableAttribute(string prefixName)
        {
            PrefixName = prefixName;
        }
    }
}