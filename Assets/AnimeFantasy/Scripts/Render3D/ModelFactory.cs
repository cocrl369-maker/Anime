using UnityEngine;

namespace AnimeFantasy
{
    /// <summary>
    /// Liefert das 3D-Modell einer Figur:
    /// 1) ein eigenes Modell aus Resources/Models/&lt;id&gt; (z. B. VRoid/VRM-Figur, Asset-Store- oder Auftragsmodell)
    /// 2) sonst eine stilisierte Platzhalter-Figur aus Grundformen im Toon-Look.
    /// </summary>
    public static class ModelFactory
    {
        public static readonly Color OutlineColor = new Color(0.13f, 0.08f, 0.12f);

        /// <summary>Zielhöhe in Metern pro Figur (eigene Modelle werden darauf skaliert).</summary>
        public static float HeightOf(CharacterData d)
        {
            switch (d.Id)
            {
                case "schleim": return 1.0f;
                case "kobold": return 1.3f;
                case "golem": return 2.5f;
                case "fuchs": return 1.4f;
                case "noctis": return 2.0f;
                default: return 1.7f;
            }
        }

        public static GameObject Create(CharacterData data)
        {
            float height = HeightOf(data);
            var prefab = Resources.Load<GameObject>("Models/" + data.Id);
            if (prefab != null)
            {
                var model = Object.Instantiate(prefab);
                model.name = data.Id;
                foreach (var c in model.GetComponentsInChildren<Collider>()) Object.Destroy(c);
                Toon.ConvertModel(model, OutlineColor);
                FitHeight(model, height);
                return model;
            }
            return BuildPlaceholder(data);
        }

        static void FitHeight(GameObject model, float height)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            if (b.size.y < 0.01f) return;
            float s = height / b.size.y;
            model.transform.localScale *= s;
            // Füße auf den Boden setzen
            model.transform.position += Vector3.up * (-b.min.y * s);
        }

        // ------------------------------------------------------------------ Platzhalter

        class Builder
        {
            readonly Transform root;
            public Builder(Transform r) { root = r; }

            public Transform Empty(string name, float scale)
            {
                var t = new GameObject(name).transform;
                t.SetParent(root, false);
                t.localScale = Vector3.one * scale;
                return t;
            }

            public Transform Part(PrimitiveType type, Material mat, Vector3 pos, Vector3 scale, Vector3 euler = default(Vector3), bool outline = true, Transform parent = null)
            {
                var go = GameObject.CreatePrimitive(type);
                var col = go.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);
                var t = go.transform;
                t.SetParent(parent != null ? parent : root, false);
                t.localPosition = pos;
                t.localScale = scale;
                t.localEulerAngles = euler;
                var r = go.GetComponent<Renderer>();
                r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                if (outline) Toon.AddOutline(r, Toon.Outline(OutlineColor));
                return t;
            }
        }

        enum HairStyle { Ponytail, Spiky, Long, Hood }

        class HumanStyle
        {
            public Color Hair, Outfit, Accent, Legs, Eyes;
            public HairStyle Style;
            public bool Skirt, LongDress, Cape;
            public float Scale = 1f;
            public float EyeGlow;
            public Color? SkinColor;
        }

        static GameObject BuildPlaceholder(CharacterData d)
        {
            var root = new GameObject(d.Id + "_Platzhalter");
            var b = new Builder(root.transform);
            switch (d.Id)
            {
                case "akari":
                    Human(b, new HumanStyle { Hair = new Color(0.86f, 0.18f, 0.24f), Outfit = new Color(0.55f, 0.12f, 0.18f), Accent = new Color(0.98f, 0.76f, 0.30f), Legs = new Color(0.20f, 0.12f, 0.14f), Eyes = new Color(1f, 0.62f, 0.15f), Style = HairStyle.Ponytail, Skirt = true });
                    // Schwert mit glühender Klinge
                    b.Part(PrimitiveType.Cube, Toon.Mat(new Color(1f, 0.55f, 0.25f), 0.9f, 0.6f), new Vector3(0.36f, 1.15f, 0.18f), new Vector3(0.06f, 0.95f, 0.02f), new Vector3(-25f, 0f, -8f));
                    b.Part(PrimitiveType.Cube, Toon.Mat(new Color(0.98f, 0.76f, 0.30f), 0f, 0.6f), new Vector3(0.34f, 0.72f, 0.0f), new Vector3(0.22f, 0.05f, 0.07f), new Vector3(-25f, 0f, -8f));
                    break;
                case "ren":
                    Human(b, new HumanStyle { Hair = new Color(0.18f, 0.66f, 0.58f), Outfit = new Color(0.12f, 0.15f, 0.36f), Accent = new Color(0.50f, 0.95f, 0.80f), Legs = new Color(0.10f, 0.10f, 0.20f), Eyes = new Color(0.35f, 0.95f, 0.55f), Style = HairStyle.Spiky, Cape = true });
                    b.Part(PrimitiveType.Cylinder, Toon.Mat(new Color(0.42f, 0.28f, 0.18f)), new Vector3(0.38f, 0.95f, 0.06f), new Vector3(0.04f, 0.85f, 0.04f));
                    b.Part(PrimitiveType.Sphere, Toon.Mat(new Color(0.45f, 1f, 0.85f), 1.2f, 0.8f), new Vector3(0.38f, 1.85f, 0.06f), Vector3.one * 0.16f, default(Vector3), false).gameObject.AddComponent<Spin>().BobHeight = 0.03f;
                    break;
                case "yuki":
                    Human(b, new HumanStyle { Hair = new Color(0.90f, 0.88f, 1f), Outfit = new Color(0.97f, 0.96f, 1f), Accent = new Color(0.55f, 0.72f, 1f), Legs = new Color(0.92f, 0.92f, 0.98f), Eyes = new Color(0.45f, 0.45f, 1f), Style = HairStyle.Long, Skirt = true, LongDress = true });
                    b.Part(PrimitiveType.Cylinder, Toon.Mat(new Color(0.98f, 0.85f, 0.45f), 0.1f, 0.6f), new Vector3(-0.38f, 0.95f, 0.06f), new Vector3(0.035f, 0.85f, 0.035f));
                    b.Part(PrimitiveType.Sphere, Toon.Mat(new Color(1f, 0.95f, 0.65f), 1.3f), new Vector3(-0.38f, 1.85f, 0.06f), new Vector3(0.24f, 0.24f, 0.06f), default(Vector3), false);
                    b.Part(PrimitiveType.Sphere, Toon.Mat(new Color(1f, 0.85f, 0.4f), 0.6f), new Vector3(-0.17f, 1.84f, 0.05f), new Vector3(0.1f, 0.1f, 0.04f));
                    break;
                case "noctis":
                    Human(b, new HumanStyle { Hair = new Color(0.16f, 0.09f, 0.26f), Outfit = new Color(0.12f, 0.06f, 0.20f), Accent = new Color(0.72f, 0.45f, 1f), Legs = new Color(0.1f, 0.05f, 0.15f), Eyes = new Color(1f, 0.15f, 0.25f), Style = HairStyle.Long, Skirt = true, LongDress = true, Cape = true, Scale = 1.18f, EyeGlow = 1f, SkinColor = new Color(0.95f, 0.88f, 0.93f) });
                    var horn = Toon.Mat(new Color(0.15f, 0.1f, 0.2f), 0f, 0.5f);
                    b.Part(PrimitiveType.Capsule, horn, new Vector3(0.16f, 2.24f, -0.02f) , new Vector3(0.07f, 0.17f, 0.07f), new Vector3(0f, 0f, -28f));
                    b.Part(PrimitiveType.Capsule, horn, new Vector3(-0.16f, 2.24f, -0.02f), new Vector3(0.07f, 0.17f, 0.07f), new Vector3(0f, 0f, 28f));
                    b.Part(PrimitiveType.Cylinder, Toon.Mat(new Color(0.75f, 0.5f, 1f), 0.8f, 0.8f), new Vector3(0f, 2.2f, 0.02f), new Vector3(0.26f, 0.035f, 0.26f));
                    break;
                case "kobold":
                    Human(b, new HumanStyle { Hair = new Color(0.17f, 0.10f, 0.24f), Outfit = new Color(0.20f, 0.12f, 0.28f), Accent = new Color(0.55f, 0.35f, 0.8f), Legs = new Color(0.15f, 0.1f, 0.18f), Eyes = new Color(1f, 0.2f, 0.25f), Style = HairStyle.Hood, Scale = 0.78f, EyeGlow = 1f, SkinColor = new Color(0.50f, 0.66f, 0.40f) });
                    var ear = Toon.Mat(new Color(0.50f, 0.66f, 0.40f));
                    b.Part(PrimitiveType.Capsule, ear, new Vector3(0.27f, 1.32f, 0f), new Vector3(0.06f, 0.16f, 0.06f), new Vector3(0f, 0f, -70f));
                    b.Part(PrimitiveType.Capsule, ear, new Vector3(-0.27f, 1.32f, 0f), new Vector3(0.06f, 0.16f, 0.06f), new Vector3(0f, 0f, 70f));
                    b.Part(PrimitiveType.Cube, Toon.Mat(new Color(0.75f, 0.75f, 0.85f), 0f, 0.8f), new Vector3(0.3f, 0.7f, 0.2f), new Vector3(0.04f, 0.4f, 0.015f), new Vector3(-40f, 0f, 0f));
                    break;
                case "schleim": Slime(b); break;
                case "golem": Golem(b); break;
                case "fuchs": Fox(b); break;
                default:
                    b.Part(PrimitiveType.Capsule, Toon.Mat(d.Color), new Vector3(0f, 0.85f, 0f), new Vector3(0.6f, 0.85f, 0.6f));
                    break;
            }
            return root;
        }

        static void Human(Builder b, HumanStyle s)
        {
            var skin = s.SkinColor.HasValue ? Toon.Mat(s.SkinColor.Value, 0.05f, 0f, 0.25f, new Color(0.95f, 0.78f, 0.80f)) : Toon.Skin;
            var hair = Toon.Mat(s.Hair, 0.04f, 0.55f, 0.4f);
            var outfit = Toon.Mat(s.Outfit, 0f, 0.15f);
            var accent = Toon.Mat(s.Accent, 0.15f, 0.6f);
            var legs = Toon.Mat(s.Legs);
            // Eigene Wurzel, damit der ganze Körper skaliert werden kann
            Transform p = b.Empty("Body", s.Scale);

            // Beine und Schuhe
            for (int side = -1; side <= 1; side += 2)
            {
                b.Part(PrimitiveType.Capsule, legs, new Vector3(0.1f * side, 0.4f, 0f), new Vector3(0.16f, 0.38f, 0.16f), default(Vector3), true, p);
                b.Part(PrimitiveType.Sphere, accent, new Vector3(0.1f * side, 0.05f, 0.04f), new Vector3(0.15f, 0.1f, 0.24f), default(Vector3), true, p);
            }
            if (s.LongDress)
                b.Part(PrimitiveType.Cylinder, outfit, new Vector3(0f, 0.5f, 0f), new Vector3(0.58f, 0.42f, 0.5f), default(Vector3), true, p);
            else if (s.Skirt)
                b.Part(PrimitiveType.Cylinder, outfit, new Vector3(0f, 0.8f, 0f), new Vector3(0.5f, 0.13f, 0.42f), default(Vector3), true, p);

            // Oberkörper
            b.Part(PrimitiveType.Capsule, outfit, new Vector3(0f, 1.08f, 0f), new Vector3(0.38f, 0.34f, 0.27f), default(Vector3), true, p);
            b.Part(PrimitiveType.Cylinder, accent, new Vector3(0f, 0.88f, 0f), new Vector3(0.4f, 0.03f, 0.3f), default(Vector3), false, p);
            b.Part(PrimitiveType.Sphere, accent, new Vector3(0f, 1.34f, 0.03f), new Vector3(0.32f, 0.11f, 0.25f), default(Vector3), true, p);
            if (s.Cape)
                b.Part(PrimitiveType.Cube, Toon.Mat(Color.Lerp(s.Outfit, Color.black, 0.3f)), new Vector3(0f, 0.95f, -0.17f), new Vector3(0.52f, 0.85f, 0.04f), new Vector3(8f, 0f, 0f), true, p);

            // Arme und Hände
            for (int side = -1; side <= 1; side += 2)
            {
                b.Part(PrimitiveType.Capsule, outfit, new Vector3(0.27f * side, 1.1f, 0f), new Vector3(0.12f, 0.27f, 0.12f), new Vector3(0f, 0f, 12f * side), true, p);
                b.Part(PrimitiveType.Sphere, skin, new Vector3(0.33f * side, 0.84f, 0.02f), Vector3.one * 0.1f, default(Vector3), true, p);
            }

            // Kopf (im Anime-Stil etwas größer)
            b.Part(PrimitiveType.Cylinder, skin, new Vector3(0f, 1.43f, 0f), new Vector3(0.1f, 0.06f, 0.1f), default(Vector3), false, p);
            b.Part(PrimitiveType.Sphere, skin, new Vector3(0f, 1.63f, 0f), new Vector3(0.4f, 0.42f, 0.38f), default(Vector3), true, p);
            var eyeMat = Toon.Mat(s.Eyes, s.EyeGlow, 0.3f);
            var white = Toon.Mat(Color.white, 0.6f);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Part(PrimitiveType.Sphere, eyeMat, new Vector3(0.085f * side, 1.6f, 0.172f), new Vector3(0.075f, 0.1f, 0.04f), default(Vector3), false, p);
                b.Part(PrimitiveType.Sphere, white, new Vector3(0.07f * side, 1.625f, 0.19f), new Vector3(0.025f, 0.025f, 0.02f), default(Vector3), false, p);
            }

            // Haare
            switch (s.Style)
            {
                case HairStyle.Hood:
                    b.Part(PrimitiveType.Sphere, hair, new Vector3(0f, 1.68f, -0.04f), new Vector3(0.5f, 0.5f, 0.5f), default(Vector3), true, p);
                    b.Part(PrimitiveType.Capsule, hair, new Vector3(0f, 1.95f, -0.15f), new Vector3(0.12f, 0.15f, 0.12f), new Vector3(-50f, 0f, 0f), true, p);
                    break;
                default:
                    b.Part(PrimitiveType.Sphere, hair, new Vector3(0f, 1.69f, -0.03f), new Vector3(0.46f, 0.45f, 0.46f), default(Vector3), true, p);
                    b.Part(PrimitiveType.Sphere, hair, new Vector3(0f, 1.77f, 0.1f), new Vector3(0.42f, 0.18f, 0.28f), default(Vector3), true, p);
                    for (int side = -1; side <= 1; side += 2)
                        b.Part(PrimitiveType.Capsule, hair, new Vector3(0.19f * side, 1.5f, 0.07f), new Vector3(0.08f, 0.17f, 0.08f), default(Vector3), true, p);
                    break;
            }
            switch (s.Style)
            {
                case HairStyle.Ponytail:
                    b.Part(PrimitiveType.Capsule, hair, new Vector3(0f, 1.42f, -0.3f), new Vector3(0.17f, 0.34f, 0.17f), new Vector3(28f, 0f, 0f), true, p);
                    b.Part(PrimitiveType.Sphere, accent, new Vector3(0f, 1.74f, -0.24f), Vector3.one * 0.1f, default(Vector3), true, p);
                    break;
                case HairStyle.Spiky:
                    for (int i = 0; i < 5; i++)
                    {
                        float a = -40f + i * 20f;
                        b.Part(PrimitiveType.Capsule, hair, new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 0.18f, 1.86f, -0.04f), new Vector3(0.1f, 0.14f, 0.1f), new Vector3(-20f, 0f, -a), true, p);
                    }
                    break;
                case HairStyle.Long:
                    b.Part(PrimitiveType.Capsule, hair, new Vector3(0f, 1.25f, -0.15f), new Vector3(0.46f, 0.45f, 0.18f), default(Vector3), true, p);
                    break;
            }
        }

        static void Slime(Builder b)
        {
            var body = Toon.Mat(new Color(0.30f, 0.62f, 1f), 0.15f, 0.9f, 0.7f, new Color(0.6f, 0.7f, 0.95f));
            b.Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.42f, 0f), new Vector3(1.0f, 0.82f, 1.0f));
            b.Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.88f, -0.05f), new Vector3(0.28f, 0.3f, 0.28f));
            var eye = Toon.Mat(new Color(0.08f, 0.12f, 0.25f));
            var white = Toon.Mat(Color.white, 0.8f);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Part(PrimitiveType.Sphere, eye, new Vector3(0.17f * side, 0.52f, 0.44f), new Vector3(0.13f, 0.17f, 0.05f), default(Vector3), false);
                b.Part(PrimitiveType.Sphere, white, new Vector3(0.15f * side, 0.56f, 0.465f), Vector3.one * 0.045f, default(Vector3), false);
                b.Part(PrimitiveType.Sphere, Toon.Mat(new Color(1f, 0.55f, 0.7f), 0.3f), new Vector3(0.3f * side, 0.42f, 0.38f), new Vector3(0.12f, 0.05f, 0.03f), default(Vector3), false);
            }
        }

        static void Golem(Builder b)
        {
            var stone = Toon.Mat(new Color(0.58f, 0.50f, 0.42f), 0f, 0.05f, 0.25f);
            var dark = Toon.Mat(new Color(0.42f, 0.36f, 0.30f));
            var glow = Toon.Mat(new Color(1f, 0.65f, 0.25f), 1.4f);
            var crystal = Toon.Mat(new Color(0.45f, 0.85f, 1f), 0.8f, 0.9f, 0.6f);
            b.Part(PrimitiveType.Cube, dark, new Vector3(-0.3f, 0.35f, 0f), new Vector3(0.38f, 0.7f, 0.42f));
            b.Part(PrimitiveType.Cube, dark, new Vector3(0.3f, 0.35f, 0f), new Vector3(0.38f, 0.7f, 0.42f));
            b.Part(PrimitiveType.Cube, stone, new Vector3(0f, 1.25f, 0f), new Vector3(1.05f, 0.95f, 0.7f));
            b.Part(PrimitiveType.Cube, stone, new Vector3(0f, 2.0f, 0.05f), new Vector3(0.55f, 0.45f, 0.5f));
            for (int side = -1; side <= 1; side += 2)
            {
                b.Part(PrimitiveType.Cube, dark, new Vector3(0.78f * side, 1.1f, 0f), new Vector3(0.4f, 0.95f, 0.42f), new Vector3(0f, 0f, 6f * side));
                b.Part(PrimitiveType.Cube, glow, new Vector3(0.13f * side, 2.03f, 0.3f), new Vector3(0.14f, 0.06f, 0.02f), default(Vector3), false);
                b.Part(PrimitiveType.Cube, crystal, new Vector3(0.55f * side, 1.85f, -0.1f), new Vector3(0.14f, 0.38f, 0.14f), new Vector3(15f, 0f, -25f * side));
            }
            b.Part(PrimitiveType.Cube, crystal, new Vector3(0f, 2.4f, 0f), new Vector3(0.14f, 0.32f, 0.14f), new Vector3(0f, 45f, 0f));
            b.Part(PrimitiveType.Cube, glow, new Vector3(0f, 1.3f, 0.36f), new Vector3(0.2f, 0.2f, 0.02f), new Vector3(0f, 0f, 45f), false);
        }

        static void Fox(Builder b)
        {
            var fur = Toon.Mat(new Color(1f, 0.55f, 0.22f), 0.05f, 0.2f, 0.45f, new Color(0.9f, 0.62f, 0.7f));
            var white = Toon.Mat(new Color(1f, 0.96f, 0.92f));
            var dark = Toon.Mat(new Color(0.18f, 0.08f, 0.05f));
            var flame = Toon.Mat(new Color(1f, 0.6f, 0.2f), 1.3f);
            var flameTip = Toon.Mat(new Color(1f, 0.92f, 0.5f), 1.5f);
            b.Part(PrimitiveType.Capsule, fur, new Vector3(0f, 0.6f, 0f), new Vector3(0.45f, 0.42f, 0.45f), new Vector3(90f, 0f, 0f));
            b.Part(PrimitiveType.Sphere, white, new Vector3(0f, 0.68f, 0.3f), new Vector3(0.36f, 0.4f, 0.3f));
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    b.Part(PrimitiveType.Capsule, fur, new Vector3(0.14f * sx, 0.22f, 0.25f * sz), new Vector3(0.1f, 0.22f, 0.1f));
            b.Part(PrimitiveType.Sphere, fur, new Vector3(0f, 1.0f, 0.45f), new Vector3(0.44f, 0.4f, 0.42f));
            b.Part(PrimitiveType.Sphere, white, new Vector3(0f, 0.93f, 0.66f), new Vector3(0.2f, 0.15f, 0.24f));
            b.Part(PrimitiveType.Sphere, dark, new Vector3(0f, 0.96f, 0.78f), Vector3.one * 0.06f, default(Vector3), false);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Part(PrimitiveType.Capsule, fur, new Vector3(0.14f * side, 1.24f, 0.42f), new Vector3(0.1f, 0.16f, 0.06f), new Vector3(0f, 0f, -15f * side));
                b.Part(PrimitiveType.Sphere, Toon.Mat(new Color(1f, 0.85f, 0.3f), 1f), new Vector3(0.1f * side, 1.04f, 0.65f), new Vector3(0.07f, 0.04f, 0.03f), default(Vector3), false);
            }
            b.Part(PrimitiveType.Capsule, flame, new Vector3(0f, 1.0f, -0.55f), new Vector3(0.26f, 0.45f, 0.26f), new Vector3(-35f, 0f, 0f));
            b.Part(PrimitiveType.Sphere, flameTip, new Vector3(0f, 1.42f, -0.82f), Vector3.one * 0.24f, default(Vector3), false);
        }
    }
}
