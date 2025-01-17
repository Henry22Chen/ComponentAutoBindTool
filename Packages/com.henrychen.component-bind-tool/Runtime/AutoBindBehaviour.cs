using UnityEngine;

namespace ComponentBind
{
    /// <summary>
    /// 绑定组件的基类，帮助完成获取组件引用的工作（也可自己写）
    /// </summary>
    public abstract class AutoBindBehaviour : MonoBehaviour
    {
        protected virtual void BindComponents(GameObject go)
        {
        }

        protected virtual void Awake()
        {
            BindComponents(gameObject);
        }
    }
}