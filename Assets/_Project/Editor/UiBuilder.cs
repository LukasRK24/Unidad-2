#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Helpers para construir interfaz por código (Canvas con Canvas Scaler, paneles, botones, sliders).
public static class UiBuilder
{
    public static readonly Color Dark = new Color(0.08f, 0.1f, 0.16f, 0.92f);
    public static readonly Color Accent = new Color(0.25f, 0.65f, 0.95f, 1f);

    public static Sprite BoxSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

    public static Canvas CreateCanvas(string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static void CreateEventSystem()
    {
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        InputSystemUIInputModule module = go.AddComponent<InputSystemUIInputModule>();
        module.AssignDefaultActions();
    }

    public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return rt;
    }

    public static RectTransform Stretch(string name, Transform parent)
    {
        RectTransform rt = Rect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    public static Image Image(string name, Transform parent, Color color, Sprite sprite = null)
    {
        RectTransform rt = Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        image.sprite = sprite;
        if (sprite != null && sprite == BoxSprite) image.type = UnityEngine.UI.Image.Type.Sliced;
        return image;
    }

    public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, TextAlignmentOptions alignment, Color color)
    {
        RectTransform rt = Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 80f));
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    public static Button Button(string name, Transform parent, string label, Vector2 position, Vector2 size)
    {
        Image image = Image(name, parent, Accent, BoxSprite);
        RectTransform rt = image.rectTransform;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Button button = image.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.55f, 0.85f, 1f);
        colors.selectedColor = new Color(0.55f, 0.85f, 1f);
        colors.pressedColor = new Color(0.15f, 0.4f, 0.7f);
        button.colors = colors;
        button.targetGraphic = image;

        TextMeshProUGUI text = Text("Label", rt, label, 40f, TextAlignmentOptions.Center, Color.white);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    public static Slider Slider(string name, Transform parent, Vector2 position, Vector2 size)
    {
        RectTransform root = Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        Slider slider = root.gameObject.AddComponent<Slider>();

        Image background = Image("Background", root, new Color(0.2f, 0.25f, 0.35f, 1f), BoxSprite);
        Anchor(background.rectTransform, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));

        RectTransform fillArea = Rect("Fill Area", root, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        fillArea.offsetMin = new Vector2(5f, 0f);
        fillArea.offsetMax = new Vector2(-15f, 0f);
        Image fill = Image("Fill", fillArea, Accent, BoxSprite);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        fill.rectTransform.sizeDelta = new Vector2(10f, 0f);

        RectTransform handleArea = Rect("Handle Slide Area", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        handleArea.offsetMin = new Vector2(10f, 0f);
        handleArea.offsetMax = new Vector2(-10f, 0f);
        Image handle = Image("Handle", handleArea, Color.white, BoxSprite);
        handle.rectTransform.sizeDelta = new Vector2(30f, 0f);
        handle.rectTransform.anchorMin = new Vector2(0f, 0f);
        handle.rectTransform.anchorMax = new Vector2(0f, 1f);

        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;
        return slider;
    }

    public static void Anchor(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // Panel centrado con título y botones apilados. Devuelve el panel raíz (oculto por los scripts en Awake).
    public static GameObject Panel(string name, Transform parent, string title, out TextMeshProUGUI titleText, params string[] buttonLabels)
    {
        Image dim = Image(name, parent, new Color(0f, 0f, 0f, 0.6f));
        Anchor(dim.rectTransform, Vector2.zero, Vector2.one);

        Image box = Image("Box", dim.transform, Dark, BoxSprite);
        box.rectTransform.sizeDelta = new Vector2(760f, 220f + buttonLabels.Length * 120f);

        titleText = Text("Title", box.transform, title, 72f, TextAlignmentOptions.Center, Color.white);
        titleText.rectTransform.anchoredPosition = new Vector2(0f, box.rectTransform.sizeDelta.y / 2f - 90f);
        titleText.rectTransform.sizeDelta = new Vector2(700f, 100f);
        return dim.gameObject;
    }
}
#endif
