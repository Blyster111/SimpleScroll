using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleScrollUnity
{
    [CustomEditor(typeof(SimpleScroll))]
    class ScrollEditorGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var style = new GUIStyle(GUI.skin.button);
            style.fontSize = 10;
            style.alignment = TextAnchor.MiddleCenter;

            if (GUILayout.Button("Create Container & Scrollbar & Item Template", style))
            {
                SimpleScroll s = (SimpleScroll)target;

                if (s.ItemTemplate != null) DestroyImmediate(s.ItemTemplate);
                if (s.Container != null) DestroyImmediate(s.Container);
                if (s.ScrollingBar != null) DestroyImmediate(s.ScrollingBar.gameObject);

                if (s.IsHorizontal)
                {
                    CreateHorizontalHelpers(s);
                }
                else
                {
                    CreateVerticalHelpers(s);
                }

                SkinElements(s);
            }
        }

        private void CustomizeRectTransform(GameObject o, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPosition, Vector2 offsetMin, Vector2 offsetMax, Vector2 localScale)
        {
            var rTransform = o.GetComponent<RectTransform>();
            if (rTransform == null) rTransform = o.AddComponent<RectTransform>();

            rTransform.pivot = pivot;
            rTransform.anchorMin = anchorMin;
            rTransform.anchorMax = anchorMax;
            rTransform.anchoredPosition = anchoredPosition;
            rTransform.offsetMin = offsetMin;
            rTransform.offsetMax = offsetMax;
            rTransform.localScale = localScale;
        }

        private void SkinElements(SimpleScroll scroll)
        {
            var roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

            var mainImage = scroll.ScrollingBar.GetComponent<Image>();
            var handleImage = scroll.ScrollingBar.handleRect.GetComponent<Image>();

            mainImage.sprite = handleImage.sprite = roundedSprite;
            mainImage.type = handleImage.type = Image.Type.Sliced;
            mainImage.pixelsPerUnitMultiplier = handleImage.pixelsPerUnitMultiplier = 0.24f;
            mainImage.color = new Color(0, 0, 0, 0.1f);

            ColorBlock block = new ColorBlock();
            block.normalColor = new Color(0, 0, 0, 0.05f);
            block.selectedColor = new Color(0, 0, 0, 0.15f);
            block.pressedColor = new Color(0, 0, 0, 0.35f);
            block.highlightedColor = new Color(0, 0, 0, 0.15f);
            block.colorMultiplier = 1;
            scroll.ScrollingBar.colors = block;

            var itemImage = scroll.ItemTemplate.GetComponent<Image>();
            itemImage.sprite = handleImage.sprite = roundedSprite;
            itemImage.type = handleImage.type = Image.Type.Sliced;
            itemImage.pixelsPerUnitMultiplier = handleImage.pixelsPerUnitMultiplier = 0.24f;
            itemImage.color = new Color(0, 0, 0, 0.1f);
        }

        private void CreateVerticalHelpers(SimpleScroll scroll)
        {
            GameObject s = new GameObject();
            s.name = "Scrolling Bar";
            s.transform.SetParent(scroll.transform);
            s.AddComponent<Image>();

            //Scrollbar
            var sScroll = s.AddComponent<Scrollbar>();
            sScroll.direction = Scrollbar.Direction.TopToBottom;
            CustomizeRectTransform(s, new Vector2(0.5f, 0.5f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(-38.8f, 0),
                new Vector2(-58.2f, 20.0f), new Vector2(-19.5f, -20.0f), new Vector3(1, 1));

            //Sliding Area
            GameObject sArea = new GameObject();
            sArea.name = "Sliding Area";
            sArea.transform.SetParent(sScroll.transform);
            CustomizeRectTransform(sArea, new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0),
                new Vector2(10, 10), new Vector2(-10, -10), new Vector3(1, 1));

            //Handle
            GameObject sHandle = new GameObject();
            sHandle.name = "Handle";
            sHandle.transform.SetParent(sArea.transform);
            var sHandleImage = sHandle.AddComponent<Image>();
            CustomizeRectTransform(sHandle, new Vector2(0.5f, 0.5f), new Vector2(0f, 0.8f), new Vector2(1, 1), new Vector2(0, 0),
                new Vector2(-10, -10), new Vector2(10, 10), new Vector3(1, 1));

            sScroll.targetGraphic = sHandleImage;
            sScroll.handleRect = (RectTransform)sHandle.transform;

            //Container
            var container = new GameObject();
            container.name = "Container";
            container.AddComponent<Image>();
            var containerMask = container.AddComponent<Mask>();
            containerMask.showMaskGraphic = false;

            container.transform.SetParent(scroll.transform);

            CustomizeRectTransform(container, new Vector2(0.5f, 0.5f), new Vector2(0f, 0.0f), new Vector2(1, 1), new Vector2(-25, 0),
                new Vector2(20, 20), new Vector2(-70, -20), new Vector3(1, 1));

            var containerItem = new GameObject();
            containerItem.AddComponent<Image>();
            containerItem.name = "ItemTemplate";
            containerItem.transform.SetParent(container.transform);
            CustomizeRectTransform(containerItem, new Vector2(0.5f, 1.0f), new Vector2(0f, 1.0f), new Vector2(1, 1), new Vector2(0, 0),
                new Vector2(0, -117), new Vector2(0, 0), new Vector3(1, 1));

            scroll.ItemTemplate = containerItem;
            scroll.Container = container;
            scroll.ScrollingBar = sScroll;
        }

        private void CreateHorizontalHelpers(SimpleScroll scroll)
        {
            GameObject s = new GameObject();
            s.name = "Scrolling Bar";
            s.transform.SetParent(scroll.transform);

            s.AddComponent<Image>();

            //Scrollbar
            var sScroll = s.AddComponent<Scrollbar>();
            sScroll.direction = Scrollbar.Direction.LeftToRight;
            var sRect = (RectTransform)s.transform;
            CustomizeRectTransform(s, new Vector2(0.5f, 0.5f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(0f, 20),
                new Vector2(20f, 20.0f), new Vector2(-20, 58.7f), new Vector3(1, 1));

            //Sliding Area
            GameObject sArea = new GameObject();
            sArea.name = "Sliding Area";
            sArea.transform.SetParent(sScroll.transform);
            CustomizeRectTransform(sArea, new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0),
                new Vector2(10, 10), new Vector2(-10, -10), new Vector3(1, 1));

            //Handle
            GameObject sHandle = new GameObject();
            sHandle.name = "Handle";
            sHandle.transform.SetParent(sArea.transform);
            var sHandleImage = sHandle.AddComponent<Image>();
            CustomizeRectTransform(sHandle, new Vector2(0.5f, 0.5f), new Vector2(0f, 0.0f), new Vector2(0.2f, 1), new Vector2(0, 0),
                new Vector2(-10, -10), new Vector2(10, 10), new Vector3(1, 1));

            sScroll.targetGraphic = sHandleImage;
            sScroll.handleRect = (RectTransform)sHandle.transform;

            //Container
            var container = new GameObject();
            container.name = "Container";
            container.AddComponent<Image>();
            var containerMask = container.AddComponent<Mask>();
            containerMask.showMaskGraphic = false;
            container.transform.SetParent(scroll.transform);
            CustomizeRectTransform(container, new Vector2(0.5f, 0.5f), new Vector2(0f, 0.0f), new Vector2(1, 1), new Vector2(0, 30),
                new Vector2(20, 80), new Vector2(-20, -20), new Vector3(1, 1));

            var containerItem = new GameObject();
            containerItem.AddComponent<Image>();
            containerItem.name = "ItemTemplate";
            containerItem.transform.SetParent(container.transform);
            CustomizeRectTransform(containerItem, new Vector2(0.5f, 0.5f), new Vector2(0f, 0.0f), new Vector2(0, 1), new Vector2(0, 0),
               new Vector2(0, 0), new Vector2(117, 0), new Vector3(1, 1));

            scroll.ItemTemplate = containerItem;
            scroll.Container = container;
            scroll.ScrollingBar = sScroll;
        }
    }
}