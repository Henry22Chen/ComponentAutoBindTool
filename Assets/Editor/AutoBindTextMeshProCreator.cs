using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ComponentBind.Editor
{
    public class AutoBindTextMeshProCreator
    {
        private const string kUILayerName = "UI";

        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";

        private const string kBackgroundSpritePath = "UI/Skin/Background.psd";

        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";

        private const string kKnobPath = "UI/Skin/Knob.psd";

        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";

        private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";

        private const string kMaskPath = "UI/Skin/UIMask.psd";

        private static TMP_DefaultControls.Resources s_StandardResources;

        //[MenuItem("GameObject/3D Object/Text - TextMeshPro", false, 30)]
        private static void CreateTextMeshProObjectPerform(MenuCommand command)
        {
            //GameObject go = ObjectFactory.CreateGameObject("Text (TMP)");
            //StageUtility.PlaceGameObjectInCurrentStage(go);
            //TextMeshPro textComponent = ObjectFactory.AddComponent<TextMeshPro>(go);
            //var isWaitingOnResourceLoadField = textComponent.GetType().GetField("m_isWaitingOnResourceLoad",
            //    BindingFlags.Instance | BindingFlags.NonPublic);
            //if (!(bool)isWaitingOnResourceLoadField.GetValue(textComponent))
            //{
            //    Preset[] presets = Preset.GetDefaultPresetsForObject(textComponent);
            //    if (presets == null || presets.Length == 0)
            //    {
            //        textComponent.text = "Sample text";
            //        textComponent.alignment = TextAlignmentOptions.TopLeft;
            //    }
            //    else
            //    {
            //        textComponent.renderer.sortingLayerID = textComponent._SortingLayerID;
            //        textComponent.renderer.sortingOrder = textComponent._SortingOrder;
            //    }
            //    if (TMP_Settings.autoSizeTextContainer)
            //    {
            //        Vector2 size = textComponent.GetPreferredValues(32767f, 32767f);
            //        textComponent.rectTransform.sizeDelta = size;
            //    }
            //    else
            //    {
            //        textComponent.rectTransform.sizeDelta = TMP_Settings.defaultTextMeshProTextContainerSize;
            //    }
            //}
            //else
            //{
            //    textComponent.text = "Sample text";
            //    textComponent.alignment = TextAlignmentOptions.TopLeft;
            //}
            //Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            //GameObject contextObject = command.context as GameObject;
            //if (contextObject != null)
            //{
            //    GameObjectUtility.SetParentAndAlign(go, contextObject);
            //    Undo.SetTransformParent(go.transform, contextObject.transform, "Parent " + go.name);
            //}
            //Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/UI (Auto bind)/Text - TextMeshPro", false, 2001)]
        private static void CreateTextMeshProGuiObjectPerform(MenuCommand menuCommand)
        {
            GameObject gameObject = TMP_DefaultControls.CreateText(GetStandardResources());
            gameObject.name = $"{ComponentAutoBindTool.NamePrefixDict[typeof(TMP_Text)]}_";
            TextMeshProUGUI textComponent = gameObject.GetComponent<TextMeshProUGUI>();
            var isWaitingOnResourceLoadField = textComponent.GetType().GetField("m_isWaitingOnResourceLoad",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (!(bool)isWaitingOnResourceLoadField.GetValue(textComponent))
            {
                Preset[] presets = Preset.GetDefaultPresetsForObject(textComponent);
                if (presets == null || presets.Length == 0)
                {
                    textComponent.fontSize = TMP_Settings.defaultFontSize;
                    textComponent.color = Color.white;
                    textComponent.text = "New Text";
                }
                if (TMP_Settings.autoSizeTextContainer)
                {
                    Vector2 size = textComponent.GetPreferredValues(32767f, 32767f);
                    textComponent.rectTransform.sizeDelta = size;
                }
                else
                {
                    textComponent.rectTransform.sizeDelta = TMP_Settings.defaultTextMeshProUITextContainerSize;
                }
            }
            else
            {
                textComponent.fontSize = -99f;
                textComponent.color = Color.white;
                textComponent.text = "New Text";
            }
            PlaceUIElementRoot(gameObject, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Button - TextMeshPro", false, 2031)]
        public static void AddButton(MenuCommand menuCommand)
        {
            GameObject gameObject = TMP_DefaultControls.CreateButton(GetStandardResources());
            gameObject.name = $"{ComponentAutoBindTool.NamePrefixDict[typeof(Button)]}_";
            gameObject.GetComponentInChildren<TMP_Text>().fontSize = 24f;
            PlaceUIElementRoot(gameObject, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Input Field - TextMeshPro", false, 2037)]
        private static void AddTextMeshProInputField(MenuCommand menuCommand)
        {
            var gameObject = TMP_DefaultControls.CreateInputField(GetStandardResources());
            gameObject.name = $"{ComponentAutoBindTool.NamePrefixDict[typeof(TMP_InputField)]}_";
            PlaceUIElementRoot(gameObject, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Dropdown - TextMeshPro", false, 2036)]
        public static void AddDropdown(MenuCommand menuCommand)
        {
            var gameObject = TMP_DefaultControls.CreateDropdown(GetStandardResources());
            gameObject.name = $"{ComponentAutoBindTool.NamePrefixDict[typeof(TMP_Dropdown)]}_";
            PlaceUIElementRoot(gameObject, menuCommand);
        }

        private static TMP_DefaultControls.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            }
            return s_StandardResources;
        }

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
            {
                sceneView = SceneView.sceneViews[0] as SceneView;
            }
            if (!(sceneView == null) && !(sceneView.camera == null))
            {
                Camera camera = sceneView.camera;
                Vector3 position = Vector3.zero;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out var localPlanePosition))
                {
                    localPlanePosition.x += canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                    localPlanePosition.y += canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;
                    localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0f, canvasRTransform.sizeDelta.x);
                    localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0f, canvasRTransform.sizeDelta.y);
                    position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                    position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;
                    Vector3 minLocalPosition = default(Vector3);
                    minLocalPosition.x = canvasRTransform.sizeDelta.x * (0f - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                    minLocalPosition.y = canvasRTransform.sizeDelta.y * (0f - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;
                    Vector3 maxLocalPosition = default(Vector3);
                    maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1f - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                    maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1f - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;
                    position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                    position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
                }
                itemTransform.anchoredPosition = position;
                itemTransform.localRotation = Quaternion.identity;
                itemTransform.localScale = Vector3.one;
            }
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            bool explicitParentChoice = true;
            if (parent == null)
            {
                parent = GetOrCreateCanvasGameObject();
                explicitParentChoice = false;
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                {
                    parent = prefabStage.prefabContentsRoot;
                }
            }
            if (parent.GetComponentsInParent<Canvas>(includeInactive: true).Length == 0)
            {
                GameObject gameObject = CreateNewUI();
                Undo.SetTransformParent(gameObject.transform, parent.transform, "");
                parent = gameObject;
            }
            GameObjectUtility.EnsureUniqueNameForSibling(element);
            SetParentAndAlign(element, parent);
            if (!explicitParentChoice)
            {
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());
            }
            Undo.RegisterFullObjectHierarchyUndo((parent == null) ? element : parent, "");
            Undo.SetCurrentGroupName("Create " + element.name);
            Selection.activeGameObject = element;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (!(parent == null))
            {
                Undo.SetTransformParent(child.transform, parent.transform, "");
                RectTransform rectTransform = child.transform as RectTransform;
                if ((bool)rectTransform)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                    Vector3 localPosition = rectTransform.localPosition;
                    localPosition.z = 0f;
                    rectTransform.localPosition = localPosition;
                }
                else
                {
                    child.transform.localPosition = Vector3.zero;
                }
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
                SetLayerRecursively(child, parent.layer);
            }
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
            }
        }

        public static GameObject CreateNewUI()
        {
            GameObject root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer("UI");
            root.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            StageUtility.PlaceGameObjectInCurrentStage(root);
            bool customScene = false;
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                root.transform.SetParent(prefabStage.prefabContentsRoot.transform, worldPositionStays: false);
                customScene = true;
            }
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);
            if (!customScene)
            {
                CreateEventSystem(select: false);
            }
            return root;
        }

        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            EventSystem esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }
            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }

        public static GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;
            Canvas canvas = ((selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null);
            if (IsValidCanvas(canvas))
            {
                return canvas.gameObject;
            }
            Canvas[] canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
            for (int i = 0; i < canvasArray.Length; i++)
            {
                if (IsValidCanvas(canvasArray[i]))
                {
                    return canvasArray[i].gameObject;
                }
            }
            return CreateNewUI();
        }

        private static bool IsValidCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
            {
                return false;
            }
            if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
            {
                return false;
            }
            if (StageUtility.GetStageHandle(canvas.gameObject) != StageUtility.GetCurrentStageHandle())
            {
                return false;
            }
            return true;
        }
    }
}
