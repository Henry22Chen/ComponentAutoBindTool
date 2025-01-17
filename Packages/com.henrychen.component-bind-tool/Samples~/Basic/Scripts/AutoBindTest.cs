using ComponentBind;
using UnityEngine;
using UnityEngine.UI;

namespace ComponentBindExample
{
    public partial class AutoBindTest : AutoBindBehaviour, IAutoBindPage
    {
        private int _count = 0;
        private Image currentImage;
        
        private Color _highlight = Color.green;
        
        private void Start()
        {
            _btnTest2.onClick.AddListener(OnBtnClick);
        }

        private void OnBtnClick()
        {
            _count++;
            _txtTest3.text = "点击了按钮" + _count + "次";
            _imgTest1.gameObject.SetActive(!_imgTest1.gameObject.activeInHierarchy);
            _testCompTest2.Print();

            var idx = _count % _imgImagesList.Count;
            if (currentImage != null)
            {
                currentImage.color = Color.white;
            }
            currentImage = _imgImagesList[idx];
            currentImage.color = _highlight;
        }
    }

}