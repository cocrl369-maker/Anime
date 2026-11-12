using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AnimeFantasy
{
    /// <summary>Hilfsfunktionen, um die komplette uGUI-Oberfläche im Code aufzubauen.</summary>
    public static class UIFactory
    {
        public static readonly Color PanelColor = new Color(0.07f, 0.05f, 0.14f, 0.85f);
        public static readonly Color Gold = new Color(1f, 0.82f, 0.35f);

        static Font font;

        public static Font Font
        {
            get
            {
                if (font == null)
                {
                    // Unity 2022.2+ / Unity 6 heißt die eingebaute Schrift "LegacyRuntime.ttf", ältere Versionen "Arial.ttf".
                    try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
                    if (font == null)
                    {
                        try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
                    }
                }
                return font;
            }
        }

        public static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
            // Neues Input System bevorzugen, falls installiert – sonst das klassische Modul.
            var inputSystemModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModule != null) es.AddComponent(inputSystemModule);
            else es.AddComponent<StandaloneInputModule>();
            Object.DontDestroyOnLoad(es);
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Setzt Anker relativ zum Elternobjekt (0..1) und entfernt Offsets.</summary>
        public static RectTransform Stretch(this RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image CreateImage(string name, Transform parent, Color color, Sprite sprite = null, bool sliced = false)
        {
            var rt = CreateRect(name, parent);
            rt.Stretch(0, 0, 1, 1);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            if (sprite != null && sliced) img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            return img;
        }

        public static Image Panel(string name, Transform parent, Color color)
        {
            return CreateImage(name, parent, color, SpriteFactory.RoundedRect, true);
        }

        public static Text CreateText(string name, Transform parent, string content, int size, Color color,
            TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var rt = CreateRect(name, parent);
            rt.Stretch(0, 0, 1, 1);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.raycastTarget = false;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = Mathf.Max(10, size / 2);
            t.resizeTextMaxSize = size;
            var shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        public static Button CreateButton(string name, Transform parent, string label, Color color, Action onClick, int fontSize = 44)
        {
            var img = Panel(name, parent, color);
            img.raycastTarget = true;
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.45f, 0.45f, 0.5f, 0.6f);
            btn.colors = colors;
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            var text = CreateText("Label", img.transform, label, fontSize, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            text.rectTransform.Stretch(0.04f, 0.06f, 0.96f, 0.94f);

            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        public static void SetLabel(this Button b, string label)
        {
            var t = b.GetComponentInChildren<Text>();
            if (t != null) t.text = label;
        }

        /// <summary>Erstellt ein rundes Portrait. Liegt eine Grafik unter Resources/Portraits/&lt;id&gt; vor, wird sie verwendet.</summary>
        public static Image CreatePortrait(Transform parent, CharacterData data, out Color baseColor)
        {
            var img = CreateImage("Portrait", parent, Color.white);
            var fitter = img.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;

            var art = Resources.Load<Sprite>("Portraits/" + data.Id);
            if (art != null)
            {
                img.sprite = art;
                img.preserveAspect = true;
                baseColor = Color.white;
            }
            else
            {
                img.sprite = SpriteFactory.Circle;
                baseColor = data.Color;
                var inner = CreateImage("Inner", img.transform, new Color(0f, 0f, 0f, 0.25f), SpriteFactory.Circle);
                inner.rectTransform.Stretch(0.12f, 0.12f, 0.88f, 0.88f);
                var initial = CreateText("Initial", img.transform, data.Name.Substring(0, 1), 140, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
                initial.rectTransform.Stretch(0.1f, 0.15f, 0.9f, 0.9f);
                var element = CreateText("Element", img.transform, ElementInfo.Name(data.Element), 26, Color.white);
                element.rectTransform.Stretch(0f, 0.02f, 1f, 0.22f);
            }
            img.color = baseColor;
            return img;
        }

        /// <summary>Modales Fenster mit Titel, Text und Buttons. Blockiert Eingaben dahinter.</summary>
        public static GameObject ShowModal(Transform parent, string title, string body,
            string label1, Action action1, string label2 = null, Action action2 = null)
        {
            var dim = CreateImage("Modal", parent, new Color(0f, 0f, 0f, 0.65f));
            dim.raycastTarget = true;
            dim.transform.SetAsLastSibling();

            var panel = Panel("Window", dim.transform, new Color(0.10f, 0.07f, 0.20f, 0.97f));
            panel.rectTransform.Stretch(0.07f, 0.30f, 0.93f, 0.70f);
            var border = Panel("Border", panel.transform, new Color(Gold.r, Gold.g, Gold.b, 0.18f));
            border.rectTransform.Stretch(0f, 0f, 1f, 1f);

            var t = CreateText("Title", panel.transform, title, 80, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.rectTransform.Stretch(0.05f, 0.78f, 0.95f, 0.97f);
            var b = CreateText("Body", panel.transform, body, 42, Color.white);
            b.rectTransform.Stretch(0.06f, 0.30f, 0.94f, 0.77f);

            var go = dim.gameObject;
            var labels = label2 == null ? new[] { label1 } : new[] { label1, label2 };
            var actions = new[] { action1, action2 };
            int n = labels.Length;
            for (int i = 0; i < n; i++)
            {
                var action = actions[i];
                float w = 0.9f / n;
                var btn = CreateButton("Button" + i, panel.transform, labels[i],
                    i == 0 ? new Color(0.85f, 0.35f, 0.45f) : new Color(0.30f, 0.30f, 0.45f),
                    () => { Object.Destroy(go); if (action != null) action(); });
                ((RectTransform)btn.transform).Stretch(0.05f + i * w + 0.01f, 0.05f, 0.05f + (i + 1) * w - 0.01f, 0.24f);
            }
            return go;
        }
    }

    /// <summary>Animierter Balken (HP, MP, Burst, EP).</summary>
    public class UIBar
    {
        public readonly RectTransform Root;
        readonly RectTransform fill;
        readonly Text label;
        float current = 1f;
        float target = 1f;

        public UIBar(Transform parent, string name, Color fillColor, int fontSize)
        {
            var bg = UIFactory.CreateImage(name, parent, new Color(0f, 0f, 0f, 0.6f));
            Root = bg.rectTransform;
            var f = UIFactory.CreateImage("Fill", Root, fillColor);
            fill = f.rectTransform;
            if (fontSize > 0)
                label = UIFactory.CreateText("Label", Root, "", fontSize, Color.white);
        }

        public void Set(float ratio, string text, bool instant = false)
        {
            target = Mathf.Clamp01(ratio);
            if (instant) current = target;
            if (label != null) label.text = text;
            Apply();
        }

        public void Tick(float dt)
        {
            if (Mathf.Approximately(current, target)) return;
            current = Mathf.MoveTowards(current, target, dt * 1.5f);
            Apply();
        }

        void Apply()
        {
            fill.anchorMax = new Vector2(current, 1f);
            fill.offsetMax = Vector2.zero;
        }
    }
}
