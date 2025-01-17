using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ComponentBind.Editor
{
    public class AutoBindWidgetCreator
    {
        private enum MenuOptionsPriorityOrder
        {
            Image = 1,
            RawImage = 2,
            Panel = 8,
            Toggle = 31,
            Slider = 34,
            Scrollbar = 35,
            ScrollView = 36,
            Canvas = 60,
            EventSystem = 61,
            Text = 80,
            Button = 81,
            Dropdown = 82,
            InputField = 83
        }


        private class DefaultEditorFactory : DefaultControls.IFactoryControls
        {
            public static readonly DefaultEditorFactory Default = new DefaultEditorFactory();

            public GameObject CreateGameObject(string name, params Type[] components)
            {
                return ObjectFactory.CreateGameObject(name, components);
            }
        }

        private class FactorySwapToEditor : IDisposable
        {
            private DefaultControls.IFactoryControls factory;

            public FactorySwapToEditor()
            {
                factory = DefaultControls.factory;
                DefaultControls.factory = DefaultEditorFactory.Default;
            }

            public void Dispose()
            {
                DefaultControls.factory = factory;
            }
        }

        private const string kUILayerName = "UI";

        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";

        private const string kBackgroundSpritePath = "UI/Skin/Background.psd";

        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";

        private const string kKnobPath = "UI/Skin/Knob.psd";

        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";

        private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";

        private const string kMaskPath = "UI/Skin/UIMask.psd";

        private static DefaultControls.Resources s_StandardResources;

        private static DefaultControls.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                s_StandardResources.background =
                    AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                s_StandardResources.inputField =
                    AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
                s_StandardResources.dropdown =
                    AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            }

            return s_StandardResources;
        }

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (!(sceneView == null) && !(sceneView.camera == null))
            {
                Camera camera = sceneView.camera;
                Vector3 position = Vector3.zero;
                Vector2 localPlanePosition;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform,
                        new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
                {
                    localPlanePosition.x += canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                    localPlanePosition.y += canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;
                    localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0f, canvasRTransform.sizeDelta.x);
                    localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0f, canvasRTransform.sizeDelta.y);
                    position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                    position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;
                    Vector3 minLocalPosition = default(Vector3);
                    minLocalPosition.x = canvasRTransform.sizeDelta.x * (0f - canvasRTransform.pivot.x) +
                                         itemTransform.sizeDelta.x * itemTransform.pivot.x;
                    minLocalPosition.y = canvasRTransform.sizeDelta.y * (0f - canvasRTransform.pivot.y) +
                                         itemTransform.sizeDelta.y * itemTransform.pivot.y;
                    Vector3 maxLocalPosition = default(Vector3);
                    maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1f - canvasRTransform.pivot.x) -
                                         itemTransform.sizeDelta.x * itemTransform.pivot.x;
                    maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1f - canvasRTransform.pivot.y) -
                                         itemTransform.sizeDelta.y * itemTransform.pivot.y;
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

            if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
            {
                GameObject canvas = CreateNewUI();
                Undo.SetTransformParent(canvas.transform, parent.transform, "");
                parent = canvas;
            }

            GameObjectUtility.EnsureUniqueNameForSibling(element);
            SetParentAndAlign(element, parent);
            if (!explicitParentChoice)
            {
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(),
                    element.GetComponent<RectTransform>());
            }

            Undo.RegisterFullObjectHierarchyUndo((parent == null) ? element : parent, "");
            Undo.SetCurrentGroupName($"Create {element.name}");
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

        [MenuItem("GameObject/UI (Auto bind)/Image", false, (int)MenuOptionsPriorityOrder.Image)]
        public static void AddImage(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateImage(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(Image)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Raw Image", false, (int)MenuOptionsPriorityOrder.RawImage)]
        public static void AddRawImage(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateRawImage(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(RawImage)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Toggle", false, (int)MenuOptionsPriorityOrder.Toggle)]
        public static void AddToggle(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateToggle(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(Toggle)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Slider", false, (int)MenuOptionsPriorityOrder.Slider)]
        public static void AddSlider(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateSlider(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(Slider)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Scrollbar", false, (int)MenuOptionsPriorityOrder.Scrollbar)]
        public static void AddScrollbar(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateScrollbar(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(Scrollbar)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Scroll View", false, (int)MenuOptionsPriorityOrder.ScrollView)]
        public static void AddScrollView(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateScrollView(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(ScrollRect)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Canvas", false, (int)MenuOptionsPriorityOrder.Canvas)]
        public static void AddCanvas(MenuCommand menuCommand)
        {
            GameObject go = CreateNewUI();
            SetParentAndAlign(go, menuCommand.context as GameObject);
            if ((bool)(go.transform.parent as RectTransform))
            {
                RectTransform rect = go.transform as RectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }

            go.name =
                $"{ComponentAutoBindTool.NamePrefixDict[typeof(Canvas)]}_";
            Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/UI (Auto bind)/Text", false, (int)MenuOptionsPriorityOrder.Text)]
        public static void AddText(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateText(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(Text)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Button", false, (int)MenuOptionsPriorityOrder.Button)]
        public static void AddButton(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateButton(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(Button)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Dropdown", false, (int)MenuOptionsPriorityOrder.Dropdown)]
        public static void AddDropdown(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateDropdown(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(Dropdown)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI (Auto bind)/Input Field", false, (int)MenuOptionsPriorityOrder.InputField)]
        public static void AddInputField(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreateInputField(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(InputField)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        public static GameObject CreateNewUI()
        {
            GameObject root = ObjectFactory.CreateGameObject("Canvas", typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            root.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            StageUtility.PlaceGameObjectInCurrentStage(root);
            bool customScene = false;
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                Undo.SetTransformParent(root.transform, prefabStage.prefabContentsRoot.transform, "");
                customScene = true;
            }

            Undo.SetCurrentGroupName($"Create {root.name}");
            if (!customScene)
            {
                CreateEventSystem(false);
            }

            return root;
        }

        [MenuItem("GameObject/UI (Auto bind)/Event System", false, (int)MenuOptionsPriorityOrder.EventSystem)]
        public static void CreateEventSystem(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            CreateEventSystem(true, parent);
        }


        [MenuItem("GameObject/UI (Auto bind)/Panel", false, (int)MenuOptionsPriorityOrder.Panel)]
        public static void AddPanel(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
            {
                go = DefaultControls.CreatePanel(GetStandardResources());
                go.name =
                    $"{ComponentAutoBindTool.NamePrefixDict[typeof(RectTransform)]}_";
            }

            PlaceUIElementRoot(go, menuCommand);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }


        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            EventSystem esys =
                ((parent == null) ? StageUtility.GetCurrentStageHandle() : StageUtility.GetStageHandle(parent))
                .FindComponentOfType<EventSystem>();
            if (esys == null)
            {
                GameObject eventSystem = ObjectFactory.CreateGameObject("EventSystem");
                if (parent == null)
                {
                    StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
                }
                else
                {
                    SetParentAndAlign(eventSystem, parent);
                }

                esys = ObjectFactory.AddComponent<EventSystem>(eventSystem);
                ObjectFactory.AddComponent<StandaloneInputModule>(eventSystem);
                Undo.RegisterCreatedObjectUndo(eventSystem, $"Create {eventSystem.name}");
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

            return StageUtility.GetStageHandle(canvas.gameObject) == StageUtility.GetCurrentStageHandle();
        }


        private static readonly Dictionary<Object, bool> AutoBindCache = new();

        public static bool IsChildOfAutoBind(Object target)
        {
            if (!AutoBindCache.ContainsKey(target))
            {
                var comp = target as Component;
                var autoBindTool =
                    comp.GetComponentInParent<ComponentAutoBindTool>();
                var flag = autoBindTool != null && autoBindTool.gameObject != comp.gameObject;
                AutoBindCache.Add(target, flag);
            }

            return AutoBindCache[target];
        }

        public static void AutoBind(Object target, Type targetType = default)
        {
            targetType ??= target.GetType();
            if (ComponentAutoBindTool.NamePrefixDict.TryGetValue(targetType,
                    out var prefix) && GUILayout.Button("绑定控件"))
            {
                var originName = target.name;
                var nameParts = originName.Split('_');
                if (nameParts.Length <= 1 || !nameParts.Any(p => p.Equals(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    target.name = $"{prefix}_{originName}";
                }
            }
        }
    }
}