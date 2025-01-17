using ComponentBind;
using UnityEngine;

namespace ComponentBindExample
{
    [AutoBindable("TestComp")]
    public class TestComponent : MonoBehaviour
    {
        public void Print()
        {
            Debug.Log("TestComponent Print");
        }
    }
}