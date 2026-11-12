using System.Collections.Generic;
using UnityEngine;

namespace AnimeFantasy
{
    /// <summary>Materialien und globale Beleuchtung für den Anime-Cel-Shading-Look.</summary>
    public static class Toon
    {
        static Shader toonShader, outlineShader, skyShader, additiveShader, shadowShader;
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        public static Shader ToonShader => toonShader != null ? toonShader : (toonShader = Load("AFToon", "AnimeFantasy/Toon"));
        public static Shader OutlineShader => outlineShader != null ? outlineShader : (outlineShader = Load("AFOutline", "AnimeFantasy/Outline"));
        public static Shader SkyShader => skyShader != null ? skyShader : (skyShader = Load("AFSky", "AnimeFantasy/Sky"));
        public static Shader AdditiveShader => additiveShader != null ? additiveShader : (additiveShader = Load("AFAdditive", "AnimeFantasy/Additive"));

        public static Shader ShadowShader => shadowShader != null ? shadowShader : (shadowShader = Load("AFShadow", "AnimeFantasy/Shadow"));

        static Shader Load(string file, string name)
        {
            var s = Resources.Load<Shader>("Shaders/" + file);
            if (s == null) s = Shader.Find(name);
            if (s == null)
            {
                Debug.LogWarning("Shader fehlt: " + name + " – verwende Ersatz.");
                s = Shader.Find("Unlit/Color");
            }
            return s;
        }

        /// <summary>Setzt Licht für alle Toon-Materialien. dirToLight zeigt zur Lichtquelle.</summary>
        public static void SetLighting(Vector3 dirToLight, Color light, Color ambient)
        {
            Shader.SetGlobalVector("_AF_LightDir", dirToLight.normalized);
            Shader.SetGlobalColor("_AF_LightColor", light);
            Shader.SetGlobalColor("_AF_Ambient", ambient);
        }

        /// <summary>Gecachtes Toon-Material. spec = Glanzstärke (Haare, Metall), rim = Randlicht.</summary>
        public static Material Mat(Color color, float emission = 0f, float spec = 0f, float rim = 0.3f, Color? shade = null)
        {
            var sh = shade ?? new Color(0.78f, 0.70f, 0.86f);
            string key = "t" + color + emission + spec + rim + sh;
            Material m;
            if (cache.TryGetValue(key, out m) && m != null) return m;
            m = new Material(ToonShader);
            m.SetColor("_Color", color);
            m.SetColor("_ShadeColor", sh);
            m.SetColor("_EmissionColor", color * emission);
            m.SetColor("_SpecColor2", new Color(1f, 1f, 1f, spec));
            m.SetColor("_RimColor", new Color(1f, 0.97f, 0.92f, rim));
            m.color = color;
            cache[key] = m;
            return m;
        }

        public static Material Skin => Mat(new Color(1f, 0.87f, 0.80f), 0.05f, 0f, 0.25f, new Color(0.96f, 0.76f, 0.76f));

        public static Material Outline(Color color, float width = 0.0035f)
        {
            string key = "o" + color + width;
            Material m;
            if (cache.TryGetValue(key, out m) && m != null) return m;
            m = new Material(OutlineShader);
            m.SetColor("_OutlineColor", color);
            m.SetFloat("_OutlineWidth", width);
            cache[key] = m;
            return m;
        }

        public static Material Additive(Texture tex, Color color)
        {
            var m = new Material(AdditiveShader);
            m.mainTexture = tex;
            m.SetColor("_Color", color);
            return m;
        }

        /// <summary>Hängt eine Konturlinie als zweites Material an (funktioniert bei Meshes mit einem Submesh am besten).</summary>
        public static void AddOutline(Renderer r, Material outline)
        {
            var mats = new List<Material>(r.sharedMaterials);
            if (mats.Contains(outline)) return;
            mats.Add(outline);
            r.sharedMaterials = mats.ToArray();
        }

        /// <summary>
        /// Wandelt ein importiertes Modell (z. B. Standard-Shader) in den Toon-Look um.
        /// Modelle mit eigenem Anime-Shader (z. B. VRoid/VRM mit MToon) werden nicht verändert.
        /// </summary>
        public static void ConvertModel(GameObject model, Color outlineColor)
        {
            foreach (var r in model.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var src = mats[i];
                    if (src == null || src.shader == null) continue;
                    var sn = src.shader.name;
                    if (sn.Contains("MToon") || sn.StartsWith("AnimeFantasy")) continue;
                    var m = new Material(ToonShader);
                    if (src.HasProperty("_MainTex")) m.mainTexture = src.mainTexture;
                    else if (src.HasProperty("_BaseMap")) m.mainTexture = src.GetTexture("_BaseMap");
                    Color c = Color.white;
                    if (src.HasProperty("_Color")) c = src.GetColor("_Color");
                    else if (src.HasProperty("_BaseColor")) c = src.GetColor("_BaseColor");
                    m.SetColor("_Color", c);
                    mats[i] = m;
                    changed = true;
                }
                if (changed)
                {
                    r.sharedMaterials = mats;
                    AddOutline(r, Outline(outlineColor));
                }
            }
        }
    }
}
