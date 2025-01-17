using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ComponentBind
{
    /// <summary>
    /// 组件自动绑定工具
    /// </summary>
    public class ComponentAutoBindTool : MonoBehaviour
    {
#if UNITY_EDITOR
        public static readonly Dictionary<Type, string> NamePrefixDict = new()
        {
            { typeof(Transform), "Node" },
            { typeof(GameObject), "Go" },
            { typeof(Animation), "Anim" },
            { typeof(Animator), "Animator" },

            { typeof(RectTransform), "Rect" },
            { typeof(Canvas), "Canvas" },
            { typeof(CanvasGroup), "Group" },
            { typeof(VerticalLayoutGroup), "VLayout" },
            { typeof(HorizontalLayoutGroup), "HLayout" },
            { typeof(GridLayoutGroup), "GLayout" },
            { typeof(ToggleGroup), "TGroup" },

            { typeof(Button), "Btn" },
            { typeof(Image), "Img" },
            { typeof(RawImage), "RImg" },
            { typeof(Text), "Txt" },
            { typeof(InputField), "Input" },
            { typeof(Slider), "Slider" },
            { typeof(Mask), "Mask" },
            { typeof(RectMask2D), "Mask2D" },
            { typeof(Toggle), "Tog" },
            { typeof(Scrollbar), "Scrollbar" },
            { typeof(ScrollRect), "Scroll" },
            { typeof(Dropdown), "Drop" },
            { typeof(TMP_Text), "Txt" },
            { typeof(TMP_InputField), "Input" },
            { typeof(TMP_Dropdown), "Drop" },
            { typeof(TextMeshProUGUI), "Txt" },
            { typeof(TextMeshPro), "Txt" },
        };


        [Serializable]
        public class BindData
        {
            public BindData()
            {
            }

            public BindData(string name, Object bindCom, bool isList, string path)
            {
                this.name = name;
                this.bindCom = bindCom;
                this.isList = isList;
                this.path = path;
            }

            public string name;
            public Object bindCom;
            public string path;
            public bool isList;
        }

        public List<BindData> bindData = new List<BindData>();

        [SerializeField]
        private string className;

        [SerializeField]
        private string rootNamespace;

        [SerializeField]
        private string codePath;

        public string ClassName
        {
            get { return className; }
        }

        public string RootNamespace
        {
            get { return rootNamespace; }
        }

        public string CodePath
        {
            get { return codePath; }
        }

        public IAutoBindRuleHelper RuleHelper { get; set; }
#endif

        [SerializeField]
        public List<Object> bindComponents = new();


        public T GetBindComponent<T>(int index) where T : Object
        {
            if (index >= bindComponents.Count)
            {
                Debug.LogError("索引无效");
                return null;
            }

            var bindCom = bindComponents[index] as T;

            if (bindCom == null)
            {
                Debug.LogError("类型无效");
                return null;
            }

            return bindCom;
        }
    }
}