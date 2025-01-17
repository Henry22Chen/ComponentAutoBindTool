using UnityEngine;

namespace ComponentBind
{
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