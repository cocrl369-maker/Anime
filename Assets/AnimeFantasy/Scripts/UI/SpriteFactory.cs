using UnityEngine;

namespace AnimeFantasy
{
    /// <summary>Erzeugt einfache Sprites zur Laufzeit, damit das Projekt ohne Grafik-Assets startet.</summary>
    public static class SpriteFactory
    {
        static Sprite rounded;
        static Sprite circle;

        public static Sprite RoundedRect
        {
            get
            {
                if (rounded == null) rounded = CreateRounded(64, 20);
                return rounded;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle == null) circle = CreateCircle(256);
                return circle;
            }
        }

        static Texture2D NewTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }

        static Sprite CreateRounded(int size, int radius)
        {
            var tex = NewTexture(size, size);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float fx = x + 0.5f, fy = y + 0.5f;
                    float cx = Mathf.Clamp(fx, radius, size - radius);
                    float cy = Mathf.Clamp(fy, radius, size - radius);
                    float d = Vector2.Distance(new Vector2(fx, fy), new Vector2(cx, cy));
                    byte a = (byte)(Mathf.Clamp01(radius - d + 0.5f) * 255);
                    px[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        static Sprite CreateCircle(int size)
        {
            var tex = NewTexture(size, size);
            var px = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    float a = Mathf.Clamp01(r - d);
                    // leichter Glanz oben links für einen weicheren "Anime"-Look
                    float shine = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(x, y), new Vector2(size * 0.35f, size * 0.7f)) / (size * 0.6f));
                    byte c = (byte)(Mathf.Lerp(200, 255, shine) );
                    px[y * size + x] = new Color32(c, c, c, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite VerticalGradient(Color top, Color bottom)
        {
            const int h = 256;
            var tex = NewTexture(4, h);
            var px = new Color[4 * h];
            for (int y = 0; y < h; y++)
            {
                var c = Color.Lerp(bottom, top, y / (float)(h - 1));
                for (int x = 0; x < 4; x++) px[y * 4 + x] = c;
            }
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, h), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
