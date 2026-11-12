using UnityEngine;

namespace AnimeFantasy
{
    /// <summary>Zauber- und Treffereffekte aus Partikeln und Leuchtringen, ohne externe Assets.</summary>
    public static class Vfx
    {
        static Texture2D softDot, ringTex, starTex;
        static Material particleMat, starMat;

        public static Texture2D SoftDot => softDot != null ? softDot : (softDot = MakeRadial(64, (d) => Mathf.Pow(Mathf.Clamp01(1f - d), 2f)));
        public static Texture2D RingTexture => ringTex != null ? ringTex : (ringTex = MakeRadial(128, (d) =>
            Mathf.Clamp01(1f - Mathf.Abs(d - 0.82f) / 0.1f) + Mathf.Clamp01(1f - Mathf.Abs(d - 0.62f) / 0.03f) * 0.6f));
        public static Texture2D StarTexture => starTex != null ? starTex : (starTex = MakeStar(64));

        static Texture2D MakeRadial(int size, System.Func<float, float> alpha)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            var px = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                    byte a = (byte)(Mathf.Clamp01(d > 1f ? 0f : alpha(d)) * 255);
                    px[y * size + x] = new Color32(255, 255, 255, a);
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        static Texture2D MakeStar(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            var px = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - r) / r, dy = Mathf.Abs(y + 0.5f - r) / r;
                    float cross = Mathf.Clamp01(1f - dx * 6f) * Mathf.Clamp01(1f - dy) + Mathf.Clamp01(1f - dy * 6f) * Mathf.Clamp01(1f - dx);
                    float core = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy) * 2.5f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(cross + core) * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        static Material ParticleMat => particleMat != null ? particleMat : (particleMat = Toon.Additive(SoftDot, Color.white));
        static Material StarMat => starMat != null ? starMat : (starMat = Toon.Additive(StarTexture, Color.white));

        static ParticleSystem NewSystem(string name, Vector3 pos, bool stars)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = stars ? StarMat : ParticleMat;
            return ps;
        }

        /// <summary>Funkenexplosion, z. B. bei Treffern.</summary>
        public static void Burst(Vector3 pos, Color color, int count = 26, float speed = 3.5f, float size = 0.22f, float life = 0.55f, bool stars = false, float gravity = 0.4f)
        {
            var ps = NewSystem("Burst", pos, stars);
            var main = ps.main;
            main.loop = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.6f, life);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.6f));
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = count * 2;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = g;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.1f));

            ps.Play();
            Object.Destroy(ps.gameObject, 2f);
        }

        /// <summary>Aufsteigende Lichtpartikel, z. B. für Heilung oder Buffs.</summary>
        public static void Rise(Vector3 pos, Color color, int count = 30)
        {
            var ps = NewSystem("Rise", pos, true);
            var main = ps.main;
            main.loop = false;
            main.duration = 0.6f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
            main.gravityModifier = -0.35f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = count / 0.6f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.6f;
            shape.rotation = new Vector3(90f, 0f, 0f);
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            ps.Play();
            Object.Destroy(ps.gameObject, 3f);
        }

        /// <summary>Dauerhafte Umgebungspartikel (Glühwürmchen, Blüten, Funken).</summary>
        public static ParticleSystem Ambient(Transform parent, Color color, Vector3 area, float rate, float gravity)
        {
            var ps = NewSystem("Ambient", parent.position + Vector3.up * area.y * 0.5f, false);
            ps.transform.SetParent(parent, true);
            var main = ps.main;
            main.loop = true;
            main.duration = 5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.5f));
            main.gravityModifier = gravity;
            main.maxParticles = 200;
            main.prewarm = true;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = area;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.3f;
            noise.frequency = 0.4f;
            ps.Play();
            return ps;
        }

        /// <summary>Flacher Leuchtring am Boden, der sich ausdehnt (Zauber-Einschlag, Burst).</summary>
        public static void Shockwave(Vector3 pos, Color color, float size = 3f, float duration = 0.5f)
        {
            var quad = CreateQuad("Shockwave", Toon.Additive(RingTexture, color));
            quad.transform.position = pos + Vector3.up * 0.05f;
            quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var fx = quad.AddComponent<ShockwaveFx>();
            fx.MaxSize = size;
            fx.Duration = duration;
            fx.Color = color;
        }

        public static GameObject CreateQuad(string name, Material mat)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            var col = quad.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            var r = quad.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return quad;
        }
    }

    public class ShockwaveFx : MonoBehaviour
    {
        public float MaxSize = 3f;
        public float Duration = 0.5f;
        public Color Color = Color.white;
        float t;
        Material mat;

        void Start() { mat = GetComponent<Renderer>().sharedMaterial; }

        void Update()
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Duration);
            float s = Mathf.Lerp(0.2f, MaxSize, 1f - (1f - k) * (1f - k));
            transform.localScale = new Vector3(s, s, s);
            var c = Color;
            c.a = 1f - k;
            if (mat != null) mat.SetColor("_Color", c);
            if (k >= 1f) Destroy(gameObject);
        }

        void OnDestroy() { if (mat != null) Destroy(mat); }
    }

    /// <summary>Langsames Drehen und Schweben (Kristalle, Deko).</summary>
    public class Spin : MonoBehaviour
    {
        public Vector3 Speed = new Vector3(0f, 30f, 0f);
        public float BobHeight = 0.2f;
        Vector3 basePos;
        float phase;

        void Start() { basePos = transform.localPosition; phase = Random.value * 10f; }

        void Update()
        {
            transform.Rotate(Speed * Time.deltaTime, Space.Self);
            transform.localPosition = basePos + Vector3.up * Mathf.Sin(Time.time + phase) * BobHeight;
        }
    }
}
