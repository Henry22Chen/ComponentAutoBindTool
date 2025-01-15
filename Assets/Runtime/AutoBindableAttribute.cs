using System;

namespace ComponentBind
{
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