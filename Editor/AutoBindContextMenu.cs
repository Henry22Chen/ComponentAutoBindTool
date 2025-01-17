using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using AutoBindTool = ComponentBind.ComponentAutoBindTool;

namespace ComponentBind.Editor
{
    public class AutoBindContextMenu
    {
        private static IAutoBindRuleHelper _ruleHelper = null;

        public static IAutoBindRuleHelper RuleHelper
        {
            get
            {
                if (_ruleHelper == null)
                {
                    Debug.Log("Load Settings");
                    string[] paths = AssetDatabase.FindAssets("t:AutoBindGlobalSetting");
                    if (paths.Length == 0)
                    {
                        Debug.LogError(
                            "不存在 AutoBindGlobalSetting.asset，通过菜单 Tools/UI Bind/Create AutoBindGlobalSetting 创建");
                        return null;
                    }

                    var existPath = paths.Select(AssetDatabase.GUIDToAssetPath)
                        .FirstOrDefault(p => p.StartsWith("Assets"));
                    if (paths.Length > 2 && existPath != default)
                    {
                        Debug.LogError("Assets 目录下 AutoBindGlobalSetting.asset 数量大于 1");
                        return null;
                    }

                    string path = existPath ?? AssetDatabase.GUIDToAssetPath(paths[0]);
                    var setting = AssetDatabase.LoadAssetAtPath<AutoBindGlobalSetting>(path);

                    _ruleHelper =
                        (IAutoBindRuleHelper)ComponentAutoBindToolInspector
                            .CreateHelperInstance(setting.RuleHelperName);

                    var assemblies = setting.SearchAssemblies
                        .Where(t => t != null)
                        .Select(t => t.name);
                    _ruleHelper.Initialize(assemblies);
                }

                return _ruleHelper;
            }
            private set { _ruleHelper = value; }
        }

        public static Dictionary<Type, string> NamePrefixDict { get; private set; }

        public static bool TryGetPrefix(Type type, out string prefix)
        {
            prefix = default;

            // if (RuleHelper == null)
            // {
            //     Debug.Log("Load Settings");
            //     string[] paths = AssetDatabase.FindAssets("t:AutoBindGlobalSetting");
            //     if (paths.Length == 0)
            //     {
            //         Debug.LogError("不存在 AutoBindGlobalSetting.asset，通过菜单 Tools/UI Bind/Create AutoBindGlobalSetting 创建");
            //         return false;
            //     }
            //
            //     var existPath = paths.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => p.StartsWith("Assets"));
            //     if (paths.Length > 2 && existPath != default)
            //     {
            //         Debug.LogError("Assets 目录下 AutoBindGlobalSetting.asset 数量大于 1");
            //         return false;
            //     }
            //
            //     string path = existPath ?? AssetDatabase.GUIDToAssetPath(paths[0]);
            //     var setting = AssetDatabase.LoadAssetAtPath<AutoBindGlobalSetting>(path);
            //
            //     RuleHelper =
            //         (IAutoBindRuleHelper)ComponentAutoBindToolInspector.CreateHelperInstance(setting.RuleHelperName);
            //
            //     var assemblies = setting.SearchAssemblies
            //         .Where(t => t != null)
            //         .Select(t => t.name);
            //     RuleHelper.Initialize(assemblies);
            // }

            if (RuleHelper == null)
                return false;

            if (NamePrefixDict == null)
            {
                Debug.Log("Load Dict");
                NamePrefixDict = RuleHelper.CustomTypeMap
                    .Concat(AutoBindTool.NamePrefixDict.Where(pair =>
                        !RuleHelper.CustomTypeMap.ContainsKey(pair.Key)))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            return NamePrefixDict.TryGetValue(type, out prefix);
        }


        public static void Bind(Object target, Type type = default)
        {
            type ??= target.GetType();
            if (TryGetPrefix(type, out var prefix))
            {
                var bindSymbol = RuleHelper.ValidSymbol;
                var originName = target.name;
                var nameParts = originName.Split('_');
                if (nameParts.Length <= 1 || !nameParts.Any(p => p.Equals(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    if (originName.StartsWith(bindSymbol))
                    {
                        originName = originName[1..];
                    }

                    Undo.RecordObject(target, $"auto_bind_{target.name}_{target.GetInstanceID()}");
                    //Undo.RegisterCompleteObjectUndo(target, $"auto_bind_{target.GetInstanceID()}");
                    target.name = $"{bindSymbol}{prefix}_{originName}";
                }
            }
            else
            {
                Debug.LogWarning("试图绑定到 ComponentAutoBindTool 的组件未注册到绑定规则 <color=cyan>IAutoBindRuleHelper</color> 中");
            }
        }

        //[MenuItem("CONTEXT/Animation/绑定控件")]
        //private static void BindAnimationContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Animator/绑定控件")]
        //private static void BindAnimatorContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Transform/绑定控件")]
        //private static void BindTransformContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/GameObject/绑定控件")]
        //private static void BindGameObjectContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/RectTransform/绑定控件")]
        //private static void BindRectTransformContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Canvas/绑定控件")]
        //private static void BindCanvasContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/CanvasGroup/绑定控件")]
        //private static void BindCanvasGroupContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/VerticalLayoutGroup/绑定控件")]
        //private static void BindVerticalLayoutGroupContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/HorizontalLayoutGroup/绑定控件")]
        //private static void BindHorizontalLayoutGroupContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/GridLayoutGroup/绑定控件")]
        //private static void BindGridLayoutGroupContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/ToggleGroup/绑定控件")]
        //private static void BindToggleGroupContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Button/绑定控件")]
        //private static void BindButtonContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Image/绑定控件")]
        //private static void BindImageContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/RawImage/绑定控件")]
        //private static void BindRawImageContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Text/绑定控件")]
        //private static void BindTextContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/InputField/绑定控件")]
        //private static void BindInputFieldContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Slider/绑定控件")]
        //private static void BindSliderContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Mask/绑定控件")]
        //private static void BindMaskContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/RectMask2D/绑定控件")]
        //private static void BindRectMask2DContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Toggle/绑定控件")]
        //private static void BindToggleContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Scrollbar/绑定控件")]
        //private static void BindScrollbarContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/ScrollRect/绑定控件")]
        //private static void BindScrollRectContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/Dropdown/绑定控件")]
        //private static void BindDropdownContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/TMP_Text/绑定控件")]
        //private static void BindTMP_TextContextMenu(MenuCommand command) => Bind(command.context, typeof(TMP_Text));

        //[MenuItem("CONTEXT/TMP_InputField/绑定控件")]
        //private static void BindTMP_InputFieldContextMenu(MenuCommand command) => Bind(command.context);

        //[MenuItem("CONTEXT/TMP_Dropdown/绑定控件")]
        //private static void BindTMP_DropdownContextMenu(MenuCommand command) => Bind(command.context);

        [MenuItem("CONTEXT/Object/UIBind/Component", false, 20)]
        private static void BindObjectContextMenu(MenuCommand command)
        {
            if (command.context.GetType().IsSubclassOf(typeof(TMP_Text)))
                Bind(command.context, typeof(TMP_Text));
            else
                Bind(command.context);
        }

        [MenuItem("CONTEXT/Object/UIBind/Component", true, 20)]
        private static bool BindObjectContextMenuValidate(MenuCommand command)
        {
            return IsBindable(command.context);
        }

        [MenuItem("CONTEXT/Object/UIBind/GameObject", false, 20)]
        private static void BindGameObjectObjectContextMenu(MenuCommand command)
        {
            if (command.context is Component comp)
            {
                var go = comp.gameObject;
                Bind(go);
            }
        }

        [MenuItem("CONTEXT/Object/UIBind/GameObject", true, 20)]
        private static bool BindGameObjectObjectContextMenuValidate(MenuCommand command)
        {
            return IsBindable(command.context);
        }

        [MenuItem("CONTEXT/Object/UIBind/Unbind Component", false, 20)]
        private static void UnBindObjectContextMenu(MenuCommand command)
        {
            if (command.context is Component component)
            {
                var gameObject = component.gameObject;
                if (TryGetPrefix(component.GetType(), out var prefix))
                {
                    var fullPrefix = $"{RuleHelper.ValidSymbol}{prefix}_";
                    if (gameObject.name.StartsWith(fullPrefix))
                    {
                        gameObject.name = gameObject.name.Replace($"{prefix}_", "");
                        if (!gameObject.name.Contains('_'))
                            gameObject.name = gameObject.name[1..];
                    }
                    else
                        gameObject.name = gameObject.name.Replace($"{prefix}_", "");
                }
            }
        }

        [MenuItem("CONTEXT/Object/UIBind/Unbind Component", true, 20)]
        private static bool UnBindObjectContextMenuValidate(MenuCommand command)
        {
            return IsUnbindable(command.context);
        }

        [MenuItem("CONTEXT/Object/UIBind/Unbind GameObject", false, 20)]
        private static void UnBindGameObjectContextMenu(MenuCommand command)
        {
            if (command.context is Component component && RuleHelper != null)
            {
                var gameObject = component.gameObject;
                var prefix = $"{RuleHelper.ValidSymbol}Go_";
                if (gameObject.name.StartsWith(prefix))
                {
                    gameObject.name = gameObject.name.Replace($"Go_", "");
                    if (!gameObject.name.Contains('_'))
                        gameObject.name = gameObject.name[1..];
                }
                else
                    gameObject.name = gameObject.name.Replace($"Go_", "");
            }
        }

        [MenuItem("CONTEXT/Object/UIBind/Unbind GameObject", true, 20)]
        private static bool UnBindGameObjectContextMenuValidate(MenuCommand command)
        {
            if (command.context is Component component && RuleHelper != null)
            {
                var gameObject = component.gameObject;
                return gameObject.name.StartsWith(RuleHelper.ValidSymbol) &&
                       gameObject.name.Contains($"Go_");
            }

            return false;
        }

        private static bool IsUnbindable(Object target)
        {
            if (target is Component component)
            {
                if (TryGetPrefix(component.GetType(), out var prefix))
                {
                    return component.gameObject.name.StartsWith(RuleHelper.ValidSymbol) &&
                           component.gameObject.name.Contains($"{prefix}_");
                }
            }

            return false;
        }

        private static bool IsBindable(Object target)
        {
            if (target is Component component)
            {
                var autoBindTools = component.GetComponentsInParent<AutoBindTool>(true);
                if (autoBindTools.Length > 0)
                {
                    return autoBindTools.Any(tool => tool.gameObject != component.gameObject);
                }

                // var autoBindTool = component.GetComponentInParent<AutoBindTool>(true);
                // return autoBindTool != null && autoBindTool.gameObject != component.gameObject;
            }

            return false;
        }
    }
}