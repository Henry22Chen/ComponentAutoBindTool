//----------------------
// <auto-generated>
//     Generated by 
//     Date:2025/1/17 15:57:25
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//----------------------
using ComponentBind;
using UnityEngine;

namespace ComponentBindExample
{

	public partial class AutoBindTest
	{

		/// <summary>
		/// AutoBindTest/@Img_Test1
		/// </summary>
		private UnityEngine.UI.Image _imgTest1;
		/// <summary>
		/// AutoBindTest/@TestComp_Btn_Test2
		/// </summary>
		private ComponentBindExample.TestComponent _testCompTest2;
		/// <summary>
		/// AutoBindTest/@TestComp_Btn_Test2
		/// </summary>
		private UnityEngine.UI.Button _btnTest2;
		/// <summary>
		/// AutoBindTest/@Txt_Test3
		/// </summary>
		private UnityEngine.UI.Text _txtTest3;
		/// <summary>
		/// AutoBindTest/@Drop_Img_Test4
		/// </summary>
		private UnityEngine.UI.Dropdown _dropTest4;
		/// <summary>
		/// AutoBindTest/@Drop_Img_Test4
		/// </summary>
		private UnityEngine.UI.Image _imgTest4;
		/// <summary>
		/// AutoBindTest/@List`Img_Images
		/// </summary>
		private System.Collections.Generic.List<UnityEngine.UI.Image> _imgImagesList;

		protected override void BindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			_imgTest1 = autoBindTool.GetBindComponent<UnityEngine.UI.Image>("imgTest1");
			_testCompTest2 = autoBindTool.GetBindComponent<ComponentBindExample.TestComponent>("testCompTest2");
			_btnTest2 = autoBindTool.GetBindComponent<UnityEngine.UI.Button>("btnTest2");
			_txtTest3 = autoBindTool.GetBindComponent<UnityEngine.UI.Text>("txtTest3");
			_dropTest4 = autoBindTool.GetBindComponent<UnityEngine.UI.Dropdown>("dropTest4");
			_imgTest4 = autoBindTool.GetBindComponent<UnityEngine.UI.Image>("imgTest4");
			_imgImagesList = new System.Collections.Generic.List<UnityEngine.UI.Image>
			{
				autoBindTool.GetBindComponent<UnityEngine.UI.Image>("imgImagesList_0"),
				autoBindTool.GetBindComponent<UnityEngine.UI.Image>("imgImagesList_1"),
				autoBindTool.GetBindComponent<UnityEngine.UI.Image>("imgImagesList_2"),
				autoBindTool.GetBindComponent<UnityEngine.UI.Image>("imgImagesList_3"),
			};
		}
	}
}