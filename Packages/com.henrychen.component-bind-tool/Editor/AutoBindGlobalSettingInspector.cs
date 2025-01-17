using System.IO;
using UnityEditor;
using UnityEngine;

namespace ComponentBind.Editor
{
    [CustomEditor(typeof(AutoBindGlobalSetting))]
    public class AutoBindGlobalSettingInspector : UnityEditor.Editor
    {
        private SerializedProperty _rootNamespace;
        private SerializedProperty _codePath;
        private SerializedProperty _ruleHelperName;
        private SerializedProperty _searchAssemblies;
        private SerializedProperty _autoInitialize;

        private string[] _helperTypeNames;
        private int _helperTypeNameIndex;

        private void OnEnable()
        {
            _rootNamespace = serializedObject.FindProperty("rootNamespace");
            _codePath = serializedObject.FindProperty("codePath");
            _ruleHelperName = serializedObject.FindProperty("ruleHelperName");
            _searchAssemblies = serializedObject.FindProperty("searchAssemblies");
            _autoInitialize = serializedObject.FindProperty("autoInitialize");

            _helperTypeNames = ComponentAutoBindToolInspector.GetTypeNames(typeof(IAutoBindRuleHelper));

            _helperTypeNameIndex = 0;
            for (var i = 0; i < _helperTypeNames.Length; i++)
            {
                if (!string.Equals(_helperTypeNames[i], _ruleHelperName.stringValue)) continue;
                _helperTypeNameIndex = i;
                break;
            }
        }

        public override void OnInspectorGUI()
        {
            _rootNamespace.stringValue = EditorGUILayout.TextField(new GUIContent("默认命名空间"), _rootNamespace.stringValue);

            EditorGUILayout.LabelField("默认代码保存路径：");
            EditorGUILayout.LabelField(_codePath.stringValue);
            if (GUILayout.Button("选择路径", GUILayout.Width(140f)))
            {
                _codePath.stringValue = EditorUtility.OpenFolderPanel("选择代码保存路径", Application.dataPath, "");
                if (!string.IsNullOrEmpty(_codePath.stringValue))
                {
                    _codePath.stringValue = Path.GetRelativePath(Application.dataPath, _codePath.stringValue);
                }
            }

            _helperTypeNameIndex = EditorGUILayout.Popup("AutoBindRuleHelper", _helperTypeNameIndex, _helperTypeNames);
            _ruleHelperName.stringValue = _helperTypeNames[_helperTypeNameIndex];
            var toggleContent = new GUIContent("自动初始化", "是否在生成的 BindComponents 方法前添加 override 关键字");
            _autoInitialize.boolValue = EditorGUILayout.ToggleLeft(toggleContent, _autoInitialize.boolValue);
            
            EditorGUILayout.PropertyField(_searchAssemblies, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}