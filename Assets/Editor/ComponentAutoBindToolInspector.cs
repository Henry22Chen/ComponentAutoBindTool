using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using BindData = ComponentBind.ComponentAutoBindTool.BindData;
using Object = UnityEngine.Object;


namespace ComponentBind.Editor
{
    [CustomEditor(typeof(ComponentAutoBindTool))]
    public class ComponentAutoBindToolInspector : UnityEditor.Editor
    {
        private ComponentAutoBindTool _target;

        private SerializedProperty _bindData;
        private SerializedProperty _bindComponents;
        private readonly List<BindData> _tempList = new();
        private readonly List<string> _tempFieldNames = new();
        private readonly List<string> _tempComponentTypeNames = new();

        private IEnumerable<string> _searchAssemblies;
        private string[] _helperTypeNames;
        private string _helperTypeName;
        private int _helperTypeNameIndex;

        private static List<Type> _pageTypes;

        private AutoBindGlobalSetting _setting;

        private SerializedProperty _namespace;
        private SerializedProperty _className;
        private SerializedProperty _codePath;

        private void OnEnable()
        {
            _target = (ComponentAutoBindTool)target;
            _bindData = serializedObject.FindProperty("bindData");
            _bindComponents = serializedObject.FindProperty("bindComponents");

            var paths = AssetDatabase.FindAssets("t:AutoBindGlobalSetting");
            if (paths.Length == 0)
            {
                Debug.LogError("不存在 AutoBindGlobalSetting.asset，通过菜单 Tools/UI Bind/Create AutoBindGlobalSetting 创建");
                return;
            }

            var existPath = paths.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => p.StartsWith("Assets"));
            if (paths.Length > 2 && existPath != default)
            {
                Debug.LogError("Assets 目录下 AutoBindGlobalSetting.asset 数量大于 1");
                return;
            }

            var path = existPath ?? AssetDatabase.GUIDToAssetPath(paths[0]);
            _setting = AssetDatabase.LoadAssetAtPath<AutoBindGlobalSetting>(path);

            _searchAssemblies = _setting.SearchAssemblies
                .Where(t => t != null)
                .Select(t => t.name).ToArray();
            if (_setting.SearchAssemblies.Length == 0)
            {
                _helperTypeNames = GetTypeNames(typeof(IAutoBindRuleHelper));
                if (_pageTypes == null || _pageTypes.Count == 0)
                {
                    _pageTypes = GetTypes(typeof(IAutoBindPage));
                    _pageTypes.AddRange(GetTypes(typeof(AutoBindBehaviour)));
                }
            }
            else
            {
                _helperTypeNames = GetTypeNames(typeof(IAutoBindRuleHelper), _searchAssemblies);
                if (_pageTypes == null || _pageTypes.Count == 0)
                {
                    _pageTypes = GetTypes(typeof(IAutoBindPage), _searchAssemblies);
                    _pageTypes.AddRange(GetTypes(typeof(AutoBindBehaviour), _searchAssemblies));
                }
            }

            _helperTypeNameIndex = 0;
            for (var i = 0; i < _helperTypeNames.Length; i++)
            {
                if (!string.Equals(_helperTypeNames[i], _setting.RuleHelperName)) continue;
                _helperTypeNameIndex = i;
                break;
            }

            _namespace = serializedObject.FindProperty("rootNamespace");
            _className = serializedObject.FindProperty("className");
            _codePath = serializedObject.FindProperty("codePath");

            _namespace.stringValue = string.IsNullOrEmpty(_namespace.stringValue)
                ? _setting.RootNamespace
                : _namespace.stringValue;
            _className.stringValue = string.IsNullOrEmpty(_className.stringValue)
                ? _target.gameObject.name
                : _className.stringValue;
            _codePath.stringValue = string.IsNullOrEmpty(_codePath.stringValue)
                ? _setting.CodePath
                : _codePath.stringValue;

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawScriptField();

            DrawTopButton();

            DrawHelperSelect();

            DrawSetting();

            DrawKvData();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScriptField()
        {
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(true);
            var type = GetCurrentType();
            if (type != null)
            {
                var findAsset = AssetDatabase.FindAssets($"t:Script {type.Name}");
                if (findAsset.Length > 0)
                {
                    foreach (var s in findAsset)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var assetPath = AssetDatabase.GUIDToAssetPath(s);
                        if (assetPath.Contains("BindComponents"))
                        {
                            EditorGUILayout.PrefixLabel("BindComponents:");
                        }
                        else
                        {
                            EditorGUILayout.PrefixLabel("Main:");
                        }

                        var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
                        EditorGUILayout.ObjectField(obj, typeof(MonoScript), false);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("未找到脚本");
                }
            }
            else
            {
                EditorGUILayout.LabelField("未找到脚本");
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private Type GetCurrentType()
        {
            var namespaceName = _namespace.stringValue;
            var className = _className.stringValue;
            foreach (var type in _pageTypes)
            {
                if (type.Namespace != namespaceName) continue;

                if (type.Name == className)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// 绘制顶部按钮
        /// </summary>
        private void DrawTopButton()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("自动绑定组件并生成代码"))
            {
                RemoveNull();
                AutoBindComponent();
                serializedObject.ApplyModifiedProperties();
                GenAutoBindCode();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("排序"))
            {
                Sort();
            }

            if (GUILayout.Button("全部删除"))
            {
                RemoveAll();
            }

            if (GUILayout.Button("删除空引用"))
            {
                RemoveNull();
            }

            if (GUILayout.Button("自动绑定组件"))
            {
                AutoBindComponent();
            }

            if (GUILayout.Button("生成绑定代码"))
            {
                GenAutoBindCode();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 排序
        /// </summary>
        private void Sort()
        {
            _tempList.Clear();
            foreach (BindData data in _target.bindData)
            {
                _tempList.Add(new BindData(data.name, data.bindCom, data.isList, data.path));
            }

            _tempList.Sort((x, y) => { return string.Compare(x.name, y.name, StringComparison.Ordinal); });

            _bindData.ClearArray();
            foreach (BindData data in _tempList)
            {
                AddBindData(data.name, data.bindCom, data.path, data.isList);
            }

            SyncBindComs();
        }

        /// <summary>
        /// 全部删除
        /// </summary>
        private void RemoveAll()
        {
            _bindData.ClearArray();

            SyncBindComs();
        }

        /// <summary>
        /// 删除空引用
        /// </summary>
        private void RemoveNull()
        {
            for (int i = _bindData.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty element = _bindData.GetArrayElementAtIndex(i).FindPropertyRelative("BindCom");
                if (element.objectReferenceValue == null)
                {
                    _bindData.DeleteArrayElementAtIndex(i);
                }
            }

            SyncBindComs();
        }

        /// <summary>
        /// 自动绑定组件
        /// </summary>
        private void AutoBindComponent()
        {
            _bindData.ClearArray();

            Transform[] children = _target.gameObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                var autoBind =
                    child.GetComponentInParent<ComponentAutoBindTool>(true);
                if (autoBind != _target && autoBind.transform != child) continue;
                if (autoBind == _target && autoBind.transform == child) continue;

                _tempFieldNames.Clear();
                _tempComponentTypeNames.Clear();

                if (_target.RuleHelper.IsValidBind(child, _tempFieldNames, _tempComponentTypeNames))
                {
                    for (int i = 0; i < _tempFieldNames.Count; i++)
                    {
                        var componentList = _tempComponentTypeNames[i].Split('\n');
                        var componentTypeName = _tempComponentTypeNames[i];
                        var fieldName = _tempFieldNames[i];

                        if (fieldName.EndsWith("@List"))
                        {
                            fieldName = fieldName.Replace("@", "");
                            for (int j = 0; j < child.childCount; j++)
                            {
                                var errorCount = 0;
                                foreach (var compTypeName in componentList)
                                {
                                    Component com = child.GetChild(j).GetComponent(compTypeName);
                                    if (com == null)
                                    {
                                        errorCount++;
                                        if (errorCount == componentList.Length)
                                            Debug.LogError($"{child.name}上不存在{componentTypeName}的组件");
                                    }
                                    else
                                    {
                                        var hierarchyPath = $"{com.transform.GetHierarchyPath(_target.transform)}";
                                        AddBindData($"{fieldName}", com, hierarchyPath, true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var errorCount = 0;
                            foreach (var compTypeName in componentList)
                            {
                                if (compTypeName.Equals("GameObject"))
                                {
                                    var hierarchyPath = $"{child.transform.GetHierarchyPath(_target.transform)}";
                                    AddBindData(fieldName, child.gameObject, hierarchyPath);
                                }
                                else
                                {
                                    Component com = child.GetComponent(compTypeName);
                                    if (com == null)
                                    {
                                        errorCount++;
                                        if (errorCount == componentList.Length)
                                            Debug.LogError($"{child.name}上不存在{componentTypeName}的组件");
                                    }
                                    else
                                    {
                                        var hierarchyPath = $"{child.transform.GetHierarchyPath(_target.transform)}";
                                        AddBindData(fieldName, com, hierarchyPath);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            SyncBindComs();
        }

        /// <summary>
        /// 绘制辅助器选择框
        /// </summary>
        private void DrawHelperSelect()
        {
            _helperTypeName = _helperTypeNames[_helperTypeNameIndex];

            if (_target.RuleHelper != null)
            {
                _helperTypeName = _target.RuleHelper.GetType().Name;

                for (int i = 0; i < _helperTypeNames.Length; i++)
                {
                    if (_helperTypeName == _helperTypeNames[i])
                    {
                        _helperTypeNameIndex = i;
                    }
                }
            }
            else
            {
                IAutoBindRuleHelper helper = (IAutoBindRuleHelper)CreateHelperInstance(_helperTypeName);
                helper.Initialize(_searchAssemblies);
                _target.RuleHelper = helper;
            }

            foreach (GameObject go in Selection.gameObjects)
            {
                var autoBindTool =
                    go.GetComponent<ComponentAutoBindTool>();
                if (autoBindTool != null && autoBindTool.RuleHelper == null)
                {
                    var helper = (IAutoBindRuleHelper)CreateHelperInstance(_helperTypeName);
                    helper.Initialize(_searchAssemblies);
                    autoBindTool.RuleHelper = helper;
                }
            }

            int selectedIndex = EditorGUILayout.Popup("AutoBindRuleHelper", _helperTypeNameIndex, _helperTypeNames);
            if (selectedIndex != _helperTypeNameIndex)
            {
                _helperTypeNameIndex = selectedIndex;
                _helperTypeName = _helperTypeNames[selectedIndex];
                var helper = (IAutoBindRuleHelper)CreateHelperInstance(_helperTypeName);
                helper.Initialize(_searchAssemblies);
                _target.RuleHelper = helper;
            }
        }

        /// <summary>
        /// 绘制设置项
        /// </summary>
        private void DrawSetting()
        {
            EditorGUILayout.BeginHorizontal();
            _namespace.stringValue = EditorGUILayout.TextField(new GUIContent("命名空间："), _namespace.stringValue);
            if (GUILayout.Button("默认设置"))
            {
                _namespace.stringValue = _setting.RootNamespace;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _className.stringValue = EditorGUILayout.TextField(new GUIContent("类名："), _className.stringValue);
            if (GUILayout.Button("物体名"))
            {
                _className.stringValue = _target.gameObject.name;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("代码保存路径：");
            EditorGUILayout.LabelField(_codePath.stringValue);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("选择路径"))
            {
                string temp = _codePath.stringValue;
                _codePath.stringValue = EditorUtility.OpenFolderPanel("选择代码保存路径", Application.dataPath, "");
                if (string.IsNullOrEmpty(_codePath.stringValue))
                {
                    _codePath.stringValue = temp;
                }
                else
                {
                    _codePath.stringValue = Path.GetRelativePath(Application.dataPath, _codePath.stringValue);
                }
            }

            if (GUILayout.Button("默认设置"))
            {
                _codePath.stringValue = _setting.CodePath;
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制键值对数据
        /// </summary>
        private void DrawKvData()
        {
            //绘制key value数据

            int needDeleteIndex = -1;

            EditorGUILayout.BeginVertical();
            SerializedProperty property;

            for (int i = 0; i < _bindData.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(25));
                property = _bindData.GetArrayElementAtIndex(i).FindPropertyRelative("Name");
                property.stringValue = EditorGUILayout.TextField(property.stringValue, GUILayout.Width(150));
                property = _bindData.GetArrayElementAtIndex(i).FindPropertyRelative("BindCom");
                property.objectReferenceValue =
                    EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(Component), true);

                if (GUILayout.Button("X"))
                {
                    //将元素下标添加进删除list
                    needDeleteIndex = i;
                }

                EditorGUILayout.EndHorizontal();
            }

            //删除data
            if (needDeleteIndex != -1)
            {
                _bindData.DeleteArrayElementAtIndex(needDeleteIndex);
                SyncBindComs();
            }

            EditorGUILayout.EndVertical();
        }


        /// <summary>
        /// 添加绑定数据
        /// </summary>
        private void AddBindData(string name, Object bindCom, string path, bool isList = false)
        {
            int index = _bindData.arraySize;
            _bindData.InsertArrayElementAtIndex(index);
            SerializedProperty element = _bindData.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("Name").stringValue = Regex.Replace(name, @"\s", string.Empty);
            element.FindPropertyRelative("BindCom").objectReferenceValue = bindCom;
            element.FindPropertyRelative("IsList").boolValue = isList;
            element.FindPropertyRelative("Path").stringValue = path;
        }

        /// <summary>
        /// 同步绑定数据
        /// </summary>
        private void SyncBindComs()
        {
            _bindComponents.ClearArray();

            for (int i = 0; i < _bindData.arraySize; i++)
            {
                SerializedProperty property = _bindData.GetArrayElementAtIndex(i).FindPropertyRelative("BindCom");
                _bindComponents.InsertArrayElementAtIndex(i);
                _bindComponents.GetArrayElementAtIndex(i).objectReferenceValue = property.objectReferenceValue;
            }
        }

        /// <summary>
        /// 获取指定基类在指定程序集中的所有子类名称
        /// </summary>
        public static string[] GetTypeNames(Type typeBase, IEnumerable<string> assemblyNames)
        {
            List<string> typeNames = new List<string>();
            foreach (string assemblyName in assemblyNames)
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

                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        typeNames.Add(type.FullName);
                    }
                }
            }

            typeNames.Sort();
            return typeNames.ToArray();
        }

        public static string[] GetTypeNames(Type typeBase)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<string> typeNames = new List<string>();
            foreach (var assembly in assemblies)
            {
                if (assembly == null)
                {
                    continue;
                }

                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        typeNames.Add(type.FullName);
                    }
                }
            }

            typeNames.Sort();
            return typeNames.ToArray();
        }

        public static List<Type> GetTypes(Type typeBase)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> typeList = new List<Type>();
            foreach (var assembly in assemblies)
            {
                if (assembly == null)
                {
                    continue;
                }

                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        typeList.Add(type);
                    }
                }
            }

            return typeList;
        }

        public static List<Type> GetTypes(Type baseType, IEnumerable<string> assemblyNames)
        {
            List<Type> typeList = new List<Type>();
            foreach (string assemblyName in assemblyNames)
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

                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
                    {
                        typeList.Add(type);
                    }
                }
            }

            return typeList;
        }

        /// <summary>
        /// 创建辅助器实例
        /// </summary>
        public static object CreateHelperInstance(string helperTypeName, IEnumerable<string> assemblyNames)
        {
            foreach (string assemblyName in assemblyNames)
            {
                Assembly assembly = Assembly.Load(assemblyName);

                object instance = assembly.CreateInstance(helperTypeName);
                if (instance != null)
                {
                    return instance;
                }
            }

            return null;
        }

        public static object CreateHelperInstance(string helperTypeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                object instance = assembly.CreateInstance(helperTypeName);
                if (instance != null)
                {
                    return instance;
                }
            }

            return null;
        }


        /// <summary>
        /// 生成自动绑定代码
        /// </summary>
        private void GenAutoBindCode()
        {
            GameObject go = _target.gameObject;

            string className = !string.IsNullOrEmpty(_target.ClassName) ? _target.ClassName : go.name;
            string codePath = !string.IsNullOrEmpty(_target.CodePath) ? _target.CodePath : _setting.CodePath;
            string assetPath = $"Assets/{codePath}/{className}.BindComponents.cs";
            codePath = Path.Combine(Application.dataPath, codePath);

            if (!Directory.Exists(codePath))
            {
                Debug.LogError($"{go.name}的代码保存路径{codePath}无效");
            }

            var path = $"{codePath}/{className}.BindComponents.cs";
            Debug.Log($"codePath : {path}");
            using (StreamWriter sw = new StreamWriter(path))
            {
                var autoGenComment = @$"//----------------------
// <auto-generated>
//     Generated by 
//     Date:{DateTime.Now}
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//----------------------";
                sw.Write(autoGenComment);
                sw.WriteLine();
                sw.WriteLine("using ComponentBind;");
                sw.WriteLine("using UnityEngine;");
                //sw.WriteLine("using UnityEngine.UI;");
                sw.WriteLine("");


                if (!string.IsNullOrEmpty(_target.RootNamespace))
                {
                    //命名空间
                    sw.WriteLine("namespace " + _target.RootNamespace);
                    sw.WriteLine("{");
                    sw.WriteLine("");
                }

                //类名
                sw.WriteLine($"\tpublic partial class {className}");
                sw.WriteLine("\t{");
                sw.WriteLine("");

                List<string> definedList = new();
                Dictionary<string, List<int>> compListDict = new();
                //组件字段
                for (var i = 0; i < _target.bindData.Count; i++)
                {
                    var data = _target.bindData[i];
                    if (!data.isList)
                    {
                        sw.WriteLine($"\t\t/// <summary>");
                        sw.WriteLine($"\t\t/// {data.path}");
                        sw.WriteLine($"\t\t/// </summary>");
                        sw.WriteLine($"\t\tprivate {data.bindCom.GetType().FullName} _{data.name};");
                    }
                    else
                    {
                        if (!compListDict.ContainsKey(data.name))
                        {
                            sw.WriteLine($"\t\t/// <summary>");
                            sw.WriteLine($"\t\t/// {data.path}");
                            sw.WriteLine($"\t\t/// </summary>");
                            sw.WriteLine(
                                $"\t\tprivate System.Collections.Generic.List<{data.bindCom.GetType().FullName}> _{data.name};");
                            compListDict.Add(data.name, new List<int> { i });
                        }
                        else
                        {
                            compListDict[data.name].Add(i);
                        }
                    }
                }

                sw.WriteLine("");

                sw.WriteLine("\t\tprotected override void BindComponents(GameObject go)");
                sw.WriteLine("\t\t{");

                //获取autoBindTool上的Component
                sw.WriteLine($"\t\t\tComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();");
                sw.WriteLine("");

                //根据索引获取

                for (int i = 0; i < _target.bindData.Count; i++)
                {
                    BindData data = _target.bindData[i];

                    if (data.isList) continue;

                    string filedName = $"_{data.name}";
                    sw.WriteLine(
                        $"\t\t\t{filedName} = autoBindTool.GetBindComponent<{data.bindCom.GetType().FullName}>({i});");
                }

                // 获取List

                foreach (var pair in compListDict)
                {
                    BindData data = _target.bindData[pair.Value[0]];
                    string fieldName = $"_{pair.Key}";
                    sw.WriteLine(
                        $"\t\t\t{fieldName} = new System.Collections.Generic.List<{data.bindCom.GetType().FullName}>");
                    sw.WriteLine("\t\t\t{");
                    foreach (var i in pair.Value)
                    {
                        data = _target.bindData[i];
                        sw.WriteLine($"\t\t\t\tautoBindTool.GetBindComponent<{data.bindCom.GetType().FullName}>({i}),");
                    }

                    sw.WriteLine("\t\t\t};");
                }

                sw.WriteLine("\t\t}");

                sw.WriteLine("\t}");

                if (!string.IsNullOrEmpty(_target.RootNamespace))
                {
                    sw.WriteLine("}");
                }
            }

            AssetDatabase.Refresh();
            var isOk = EditorUtility.DisplayDialog("提示", "代码生成完毕", "OK");
            if (isOk)
            {
                var pingObj = AssetDatabase.LoadMainAssetAtPath(assetPath);
                EditorGUIUtility.PingObject(pingObj);
            }
        }
    }
}