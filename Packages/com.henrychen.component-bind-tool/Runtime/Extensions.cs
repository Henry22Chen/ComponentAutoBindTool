using UnityEngine;

namespace ComponentBind
{
    public static class StringExtension
    {
        public static string StartWithLower(this string input)
        {
            return input[0].ToString().ToLower() + input[1..];
        }

        public static string StartWithUpper(this string input)
        {
            return input[0].ToString().ToUpper() + input[1..];
        }
    
        /// <summary>
        /// Get the path of this transform in hierarchy from the given parent
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static string GetHierarchyPath(this Transform transform, Transform parent)
        {
            if (transform == parent) return transform.name;
            return transform.parent == null ? transform.name : transform.parent.GetHierarchyPath(parent) + "/" + transform.name;
        }
    }
}