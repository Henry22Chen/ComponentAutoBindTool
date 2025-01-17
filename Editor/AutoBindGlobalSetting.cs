using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace ComponentBind.Editor
{
    /// <summary>
    /// 自动绑定全局设置
    /// </summary>
    public class AutoBindGlobalSetting : ScriptableObject
    {
        [SerializeField]
        private string codePath;

        [SerializeField]
        private string rootNamespace;

        [SerializeField]
        private string ruleHelperName;

        [SerializeField]
        private AssemblyDefinitionAsset[] searchAssemblies;

        [SerializeField, Tooltip("是否在生成的 BindComponents 方法前添加 override 关键字")]
        private bool autoInitialize = true;
        
        public string CodePath => codePath;

        public string RootNamespace => rootNamespace;

        public string RuleHelperName => ruleHelperName;

        public AssemblyDefinitionAsset[] SearchAssemblies => searchAssemblies;

        public bool AutoInitialize => autoInitialize;
        
        [MenuItem("Tools/UI Bind/Create AutoBindGlobalSetting")]
        private static void CreateAutoBindGlobalSetting()
        {
            string[] paths = AssetDatabase.FindAssets("t:AutoBindGlobalSetting");
            //if (paths.Length >= 1)
            var existPath = paths.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => p.StartsWith("Assets"));
            if (existPath != default)
            {
                EditorUtility.DisplayDialog("警告", $"已存在AutoBindGlobalSetting，路径:{existPath}", "确认");
                var asset = AssetDatabase.LoadAssetAtPath<AutoBindGlobalSetting>(existPath);
                EditorGUIUtility.PingObject(asset);
                return;
            }

            AutoBindGlobalSetting setting = CreateInstance<AutoBindGlobalSetting>();
            setting.codePath = ".";
            AssetDatabase.CreateAsset(setting, "Assets/AutoBindGlobalSetting.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(setting);
        }
    }
}
