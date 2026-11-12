using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnimeFantasy
{
    public enum StageTheme { Forest, Cave, Shrine, Tower }

    /// <summary>
    /// Die 3D-Kampfbühne: Umgebung je Kapitel, Licht, Kamera und alle Figuren.
    /// Die Kamera steht hinter der Gruppe (wie in modernen rundenbasierten Anime-RPGs), die Gegner blicken zur Kamera.
    /// </summary>
    public class BattleStage : MonoBehaviour
    {
        public Action<BattleUnit> UnitClicked;

        readonly Dictionary<BattleUnit, CharacterActor> actors = new Dictionary<BattleUnit, CharacterActor>();
        Camera cam;
        CameraState savedCamera;
        BattleUnit activeUnit;
        HashSet<BattleUnit> targets = new HashSet<BattleUnit>();
        float baseFov = 50f;
        float fovKick;
        Vector3 shake;

        // Kamera-Komposition
        const float Pitch = 26f;
        const float SceneHalfWidth = 3.7f;
        static readonly Vector3 Focus = new Vector3(0f, 0.9f, 0.4f);
        /// <summary>Verschiebt das Bild nach oben, damit die Bühne im freien Bereich zwischen den UI-Leisten liegt.</summary>
        const float VerticalShift = 0.12f;

        struct CameraState
        {
            public Vector3 Pos;
            public Quaternion Rot;
            public float Fov, Far, Near;
            public CameraClearFlags Clear;
            public Color Bg;
        }

        public static BattleStage Create(StageData data, List<BattleUnit> party, List<BattleUnit> enemies)
        {
            var go = new GameObject("BattleStage");
            var stage = go.AddComponent<BattleStage>();
            stage.Setup(data, party, enemies);
            return stage;
        }

        void Setup(StageData data, List<BattleUnit> party, List<BattleUnit> enemies)
        {
            SetupCamera();
            BuildEnvironment(data);

            // Helden vorne (zur Kamera), Gegner hinten
            for (int i = 0; i < party.Count; i++)
                Place(party[i], new Vector3((i - (party.Count - 1) * 0.5f) * 1.75f, 0f, -2.3f - Mathf.Abs(i - 1) * 0.25f), 0f);
            float spacing = enemies.Count > 2 ? 2.1f : 2.6f;
            for (int i = 0; i < enemies.Count; i++)
                Place(enemies[i], new Vector3((i - (enemies.Count - 1) * 0.5f) * spacing, 0f, 2.6f + Mathf.Abs(i - (enemies.Count - 1) * 0.5f) * 0.3f), 180f);
        }

        void Place(BattleUnit unit, Vector3 pos, float yaw)
        {
            var holder = new GameObject(unit.Name);
            holder.transform.SetParent(transform, false);
            holder.transform.position = pos;
            holder.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            var model = ModelFactory.Create(unit.Data);
            var actor = holder.AddComponent<CharacterActor>();
            actor.Init(unit, model, ModelFactory.HeightOf(unit.Data));
            actor.Clicked = u => { if (UnitClicked != null) UnitClicked(u); };
            actors[unit] = actor;

            // weicher Schatten unter der Figur
            var shadow = Vfx.CreateQuad("Shadow", ShadowMaterial);
            shadow.transform.SetParent(holder.transform, false);
            shadow.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            float s = Mathf.Max(1.1f, actor.Height * 0.65f);
            shadow.transform.localScale = new Vector3(s, s * 0.8f, s);
        }

        static Material shadowMaterial;
        static Material ShadowMaterial
        {
            get
            {
                if (shadowMaterial == null)
                {
                    shadowMaterial = new Material(Toon.ShadowShader);
                    shadowMaterial.mainTexture = Vfx.SoftDot;
                    shadowMaterial.SetColor("_Color", new Color(0f, 0f, 0.05f, 0.45f));
                }
                return shadowMaterial;
            }
        }

        public CharacterActor Get(BattleUnit u)
        {
            CharacterActor a;
            return actors.TryGetValue(u, out a) ? a : null;
        }

        // ------------------------------------------------------------------ Kamera

        void SetupCamera()
        {
            cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
            }
            savedCamera = new CameraState
            {
                Pos = cam.transform.position, Rot = cam.transform.rotation, Fov = cam.fieldOfView,
                Far = cam.farClipPlane, Near = cam.nearClipPlane, Clear = cam.clearFlags, Bg = cam.backgroundColor
            };
            cam.orthographic = false;
            cam.fieldOfView = baseFov;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 600f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            if (cam.GetComponent<PhysicsRaycaster>() == null) cam.gameObject.AddComponent<PhysicsRaycaster>();
            UpdateCamera();
        }

        void LateUpdate()
        {
            fovKick = Mathf.MoveTowards(fovKick, 0f, Time.deltaTime * 6f);
            shake = Vector3.MoveTowards(shake, Vector3.zero, Time.deltaTime * 2f);
            UpdateCamera();
        }

        void UpdateCamera()
        {
            if (cam == null) return;
            float fov = baseFov - fovKick;
            cam.fieldOfView = fov;
            float aspect = Mathf.Max(0.3f, cam.aspect);
            float halfH = Mathf.Atan(Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * aspect);
            float dist = Mathf.Clamp(SceneHalfWidth / Mathf.Tan(halfH), 7f, 22f);

            var rot = Quaternion.Euler(Pitch, 0f, 0f);
            var jitter = shake.sqrMagnitude > 0f ? UnityEngine.Random.insideUnitSphere * shake.magnitude : Vector3.zero;
            cam.transform.rotation = rot;
            cam.transform.position = Focus - rot * Vector3.forward * dist + jitter;

            // Bild nach oben verschieben (Lens Shift), damit die Bühne über dem Befehlsmenü liegt
            cam.ResetProjectionMatrix();
            var m = cam.projectionMatrix;
            m.m12 = -VerticalShift * 2f;
            cam.projectionMatrix = m;
        }

        /// <summary>Kurzer Zoom und Kamerawackeln, z. B. beim Burst.</summary>
        public void Impact(float zoom, float shakeAmount)
        {
            fovKick = Mathf.Max(fovKick, zoom);
            shake = Vector3.one * Mathf.Max(shake.magnitude, shakeAmount);
        }

        public bool TryGetScreenPoint(BattleUnit u, out Vector2 screen)
        {
            screen = Vector2.zero;
            var a = Get(u);
            if (a == null || cam == null) return false;
            var p = cam.WorldToScreenPoint(a.HeadPoint);
            if (p.z < 0f) return false;
            screen = p;
            return true;
        }

        // ------------------------------------------------------------------ Markierungen

        public void SetActive(BattleUnit u) { activeUnit = u; RefreshRings(); }

        public void SetTargets(IEnumerable<BattleUnit> list)
        {
            targets = list != null ? new HashSet<BattleUnit>(list) : new HashSet<BattleUnit>();
            RefreshRings();
        }

        void RefreshRings()
        {
            foreach (var kv in actors)
            {
                if (targets.Contains(kv.Key)) kv.Value.SetRing(kv.Key.IsPlayer ? new Color(0.4f, 1f, 0.6f) : new Color(1f, 0.3f, 0.3f), 1f);
                else if (kv.Key == activeUnit) kv.Value.SetRing(new Color(1f, 0.82f, 0.35f), 0.9f);
                else kv.Value.SetRing(Color.clear, 0f);
            }
        }

        // ------------------------------------------------------------------ Aktionen

        public IEnumerator AnimateAction(BattleUnit user, SkillData skill, List<BattleUnit> targetUnits)
        {
            var actor = Get(user);
            if (actor == null) yield break;
            var color = ElementInfo.GetColor(skill.Element);
            if (skill.Element == Element.Neutral) color = new Color(1f, 0.95f, 0.85f);

            bool melee = skill.Kind == SkillKind.Physical && skill.Target == TargetType.SingleEnemy && targetUnits.Count > 0;
            if (melee)
            {
                var t = Get(targetUnits[0]);
                yield return actor.Lunge(t != null ? t.Home : actor.Home + actor.transform.forward * 3f);
            }
            else
            {
                yield return actor.Cast(color);
                if (skill.IsDamage)
                {
                    foreach (var u in targetUnits)
                    {
                        var ta = Get(u);
                        if (ta != null) Vfx.Shockwave(ta.transform.position, color, 2.5f, 0.45f);
                    }
                    if (skill.Target == TargetType.AllEnemies)
                    {
                        var center = targetUnits.Select(u => Get(u)).Where(a => a != null).Aggregate(Vector3.zero, (s, a) => s + a.transform.position);
                        if (targetUnits.Count > 0) Vfx.Shockwave(center / targetUnits.Count, color, 7f, 0.7f);
                    }
                    yield return new WaitForSeconds(0.15f);
                }
            }
        }

        public void ShowResult(ActionResult r)
        {
            var a = Get(r.Target);
            if (a == null) return;
            var color = ElementInfo.GetColor(r.Element);
            if (r.Element == Element.Neutral) color = new Color(1f, 0.9f, 0.7f);

            if (r.IsDamage)
            {
                a.Hit(r.Crit);
                Vfx.Burst(a.CenterPoint, color, r.Crit ? 40 : 24, r.Crit ? 5f : 3.5f, 0.25f, 0.55f, r.Crit);
                Impact(r.Crit ? 2.5f : 0.8f, r.Crit ? 0.12f : 0.04f);
            }
            else if (r.IsHeal)
            {
                a.Flash(new Color(0.6f, 1f, 0.7f), 0.6f);
                Vfx.Rise(a.transform.position, new Color(0.5f, 1f, 0.6f), 30);
            }
            if (r.StatusApplied != StatusType.None)
                Vfx.Rise(a.transform.position, StatusInfo.GetColor(r.StatusApplied), 16);
            SetAlive(r.Target, r.Target.IsAlive);
        }

        public void SetAlive(BattleUnit u, bool alive)
        {
            var a = Get(u);
            if (a != null) a.SetAlive(alive);
        }

        public void Burst(BattleUnit u, SkillData skill)
        {
            Impact(8f, 0.15f);
            var a = Get(u);
            if (a != null)
            {
                var c = ElementInfo.GetColor(skill.Element);
                Vfx.Shockwave(a.transform.position, c, 4f, 0.8f);
                Vfx.Burst(a.CenterPoint, c, 50, 5f, 0.3f, 0.8f, true, -0.2f);
            }
        }

        public void Victory(List<BattleUnit> party)
        {
            foreach (var u in party)
            {
                var a = Get(u);
                if (a != null && u.IsAlive) a.Victory();
            }
        }

        // ------------------------------------------------------------------ Umgebung

        struct ThemeLook
        {
            public Color SkyTop, Horizon, SkyBottom, Sun, Light, Ambient, Ground, GroundInner, Particles;
            public Vector3 LightDir;
            public float ParticleGravity;
        }

        static ThemeLook Look(StageTheme theme)
        {
            switch (theme)
            {
                case StageTheme.Cave:
                    return new ThemeLook
                    {
                        SkyTop = new Color(0.04f, 0.05f, 0.14f), Horizon = new Color(0.18f, 0.22f, 0.45f), SkyBottom = new Color(0.03f, 0.03f, 0.08f),
                        Sun = new Color(0.4f, 0.6f, 1f), Light = new Color(0.75f, 0.85f, 1.1f), Ambient = new Color(0.22f, 0.24f, 0.38f),
                        Ground = new Color(0.25f, 0.27f, 0.38f), GroundInner = new Color(0.32f, 0.35f, 0.5f), Particles = new Color(0.45f, 0.75f, 1f),
                        LightDir = new Vector3(-0.4f, 0.8f, -0.3f), ParticleGravity = -0.02f
                    };
                case StageTheme.Shrine:
                    return new ThemeLook
                    {
                        SkyTop = new Color(0.35f, 0.30f, 0.65f), Horizon = new Color(1f, 0.62f, 0.50f), SkyBottom = new Color(0.25f, 0.15f, 0.2f),
                        Sun = new Color(1f, 0.75f, 0.5f), Light = new Color(1.1f, 0.88f, 0.78f), Ambient = new Color(0.35f, 0.25f, 0.35f),
                        Ground = new Color(0.55f, 0.48f, 0.42f), GroundInner = new Color(0.72f, 0.64f, 0.56f), Particles = new Color(1f, 0.65f, 0.8f),
                        LightDir = new Vector3(0.6f, 0.45f, 0.6f), ParticleGravity = 0.02f
                    };
                case StageTheme.Tower:
                    return new ThemeLook
                    {
                        SkyTop = new Color(0.08f, 0.02f, 0.12f), Horizon = new Color(0.55f, 0.12f, 0.30f), SkyBottom = new Color(0.03f, 0f, 0.05f),
                        Sun = new Color(1f, 0.2f, 0.35f), Light = new Color(0.95f, 0.70f, 1f), Ambient = new Color(0.28f, 0.15f, 0.32f),
                        Ground = new Color(0.16f, 0.12f, 0.2f), GroundInner = new Color(0.24f, 0.18f, 0.3f), Particles = new Color(0.8f, 0.4f, 1f),
                        LightDir = new Vector3(0.3f, 0.7f, 0.8f), ParticleGravity = -0.03f
                    };
                default:
                    return new ThemeLook
                    {
                        SkyTop = new Color(0.30f, 0.55f, 0.95f), Horizon = new Color(0.85f, 0.92f, 0.95f), SkyBottom = new Color(0.25f, 0.35f, 0.3f),
                        Sun = new Color(1f, 0.95f, 0.8f), Light = new Color(1.15f, 1.05f, 0.92f), Ambient = new Color(0.30f, 0.36f, 0.42f),
                        Ground = new Color(0.36f, 0.62f, 0.32f), GroundInner = new Color(0.62f, 0.56f, 0.42f), Particles = new Color(0.9f, 1f, 0.5f),
                        LightDir = new Vector3(-0.5f, 0.75f, -0.35f), ParticleGravity = -0.01f
                    };
            }
        }

        void BuildEnvironment(StageData data)
        {
            var look = Look(data.Theme);
            Toon.SetLighting(look.LightDir, look.Light, look.Ambient);
            cam.backgroundColor = look.Horizon;

            // Himmel
            var sky = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sky.name = "Sky";
            Destroy(sky.GetComponent<Collider>());
            sky.transform.SetParent(transform, false);
            sky.transform.localScale = Vector3.one * 800f;
            var skyMat = new Material(Toon.SkyShader);
            skyMat.SetColor("_Top", look.SkyTop);
            skyMat.SetColor("_Horizon", look.Horizon);
            skyMat.SetColor("_Bottom", look.SkyBottom);
            skyMat.SetColor("_SunColor", look.Sun);
            skyMat.SetVector("_SunDir", new Vector4(look.LightDir.x, Mathf.Max(0.08f, look.LightDir.y * 0.4f), Mathf.Abs(look.LightDir.z) + 0.5f, 0f));
            sky.GetComponent<Renderer>().sharedMaterial = skyMat;

            // Boden mit Kampfarena
            Prop(PrimitiveType.Cylinder, Toon.Mat(look.Ground, 0f, 0f, 0.1f), new Vector3(0f, -0.05f, 6f), new Vector3(70f, 0.05f, 70f), Vector3.zero, false);
            Prop(PrimitiveType.Cylinder, Toon.Mat(look.GroundInner, 0f, 0f, 0.1f), new Vector3(0f, -0.02f, 0.2f), new Vector3(10.5f, 0.03f, 9f), Vector3.zero, false);
            var circle = Vfx.CreateQuad("MagicCircle", Toon.Additive(Vfx.RingTexture, new Color(look.Particles.r, look.Particles.g, look.Particles.b, 0.35f)));
            circle.transform.SetParent(transform, false);
            circle.transform.position = new Vector3(0f, 0.03f, 0.2f);
            circle.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            circle.transform.localScale = Vector3.one * 9f;
            circle.AddComponent<Spin>().Speed = new Vector3(0f, 0f, 6f);
            circle.GetComponent<Spin>().BobHeight = 0f;

            var rng = new System.Random(data.Name.GetHashCode());
            switch (data.Theme)
            {
                case StageTheme.Forest: Forest(rng); break;
                case StageTheme.Cave: Cave(rng); break;
                case StageTheme.Shrine: Shrine(rng); break;
                case StageTheme.Tower: Tower(rng); break;
            }

            Vfx.Ambient(transform, look.Particles, new Vector3(16f, 5f, 12f), 14f, look.ParticleGravity);
        }

        Transform Prop(PrimitiveType type, Material mat, Vector3 pos, Vector3 scale, Vector3 euler, bool outline = true)
        {
            var go = GameObject.CreatePrimitive(type);
            Destroy(go.GetComponent<Collider>());
            var t = go.transform;
            t.SetParent(transform, false);
            t.position = pos;
            t.localScale = scale;
            t.eulerAngles = euler;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (outline) Toon.AddOutline(r, Toon.Outline(ModelFactory.OutlineColor, 0.0025f));
            return t;
        }

        static float R(System.Random rng, float a, float b) => a + (float)rng.NextDouble() * (b - a);

        /// <summary>Position im Halbkreis hinter/neben der Arena (nicht zwischen Kamera und Figuren).</summary>
        static Vector3 Around(System.Random rng, float minR, float maxR)
        {
            float ang = R(rng, -100f, 100f) * Mathf.Deg2Rad;
            float r = R(rng, minR, maxR);
            return new Vector3(Mathf.Sin(ang) * r * 1.2f, 0f, Mathf.Cos(ang) * r + 1f);
        }

        void Forest(System.Random rng)
        {
            var trunk = Toon.Mat(new Color(0.45f, 0.30f, 0.22f));
            var leaves = new[] { Toon.Mat(new Color(0.30f, 0.62f, 0.30f), 0f, 0.1f, 0.35f), Toon.Mat(new Color(0.40f, 0.72f, 0.32f), 0f, 0.1f, 0.35f), Toon.Mat(new Color(0.22f, 0.52f, 0.34f), 0f, 0.1f, 0.35f) };
            for (int i = 0; i < 22; i++)
            {
                var p = Around(rng, 7f, 20f);
                float h = R(rng, 2.5f, 4.5f);
                Prop(PrimitiveType.Cylinder, trunk, p + Vector3.up * h * 0.5f, new Vector3(0.35f, h * 0.5f, 0.35f), Vector3.zero);
                var leaf = leaves[rng.Next(leaves.Length)];
                for (int k = 0; k < 3; k++)
                {
                    float s = R(rng, 1.8f, 2.8f) * (1f - k * 0.2f);
                    Prop(PrimitiveType.Sphere, leaf, p + new Vector3(R(rng, -0.5f, 0.5f), h + k * 0.9f, R(rng, -0.5f, 0.5f)), new Vector3(s, s * 0.8f, s), Vector3.zero);
                }
            }
            var bush = Toon.Mat(new Color(0.28f, 0.55f, 0.28f), 0f, 0.1f, 0.3f);
            var flower = new[] { Toon.Mat(new Color(1f, 0.85f, 0.4f), 0.3f), Toon.Mat(new Color(1f, 0.55f, 0.7f), 0.3f), Toon.Mat(Color.white, 0.3f) };
            for (int i = 0; i < 26; i++)
            {
                var p = Around(rng, 5.5f, 12f);
                float s = R(rng, 0.6f, 1.3f);
                Prop(PrimitiveType.Sphere, bush, p + Vector3.up * s * 0.3f, new Vector3(s * 1.3f, s * 0.8f, s * 1.3f), Vector3.zero);
                Prop(PrimitiveType.Sphere, flower[rng.Next(flower.Length)], p + new Vector3(R(rng, -0.4f, 0.4f), s * 0.6f, R(rng, -0.4f, 0.4f)), Vector3.one * 0.15f, Vector3.zero, false);
            }
        }

        void Cave(System.Random rng)
        {
            var rock = Toon.Mat(new Color(0.32f, 0.33f, 0.42f), 0f, 0.05f, 0.2f);
            var crystals = new[] { Toon.Mat(new Color(0.45f, 0.85f, 1f), 0.7f, 0.9f, 0.6f), Toon.Mat(new Color(0.75f, 0.5f, 1f), 0.7f, 0.9f, 0.6f) };
            for (int i = 0; i < 18; i++)
            {
                var p = Around(rng, 7f, 18f);
                float s = R(rng, 1.5f, 4f);
                Prop(PrimitiveType.Sphere, rock, p, new Vector3(s * 1.4f, s, s * 1.2f), new Vector3(0f, R(rng, 0f, 360f), 0f));
            }
            for (int i = 0; i < 24; i++)
            {
                var p = Around(rng, 5.5f, 15f);
                float h = R(rng, 0.8f, 2.6f);
                var mat = crystals[rng.Next(crystals.Length)];
                Prop(PrimitiveType.Cube, mat, p + Vector3.up * h * 0.4f, new Vector3(h * 0.3f, h, h * 0.3f), new Vector3(R(rng, -25f, 25f), R(rng, 0f, 90f), R(rng, -25f, 25f)));
            }
            // Stalaktiten-Decke als dunkle Formen am oberen Bildrand
            for (int i = 0; i < 10; i++)
            {
                var p = Around(rng, 6f, 14f) + Vector3.up * R(rng, 7f, 9f);
                float h = R(rng, 1.5f, 3.5f);
                Prop(PrimitiveType.Capsule, rock, p, new Vector3(0.8f, h, 0.8f), Vector3.zero);
            }
        }

        void Shrine(System.Random rng)
        {
            var red = Toon.Mat(new Color(0.85f, 0.18f, 0.16f), 0.05f, 0.2f);
            var black = Toon.Mat(new Color(0.12f, 0.1f, 0.12f));
            var stone = Toon.Mat(new Color(0.7f, 0.68f, 0.64f));
            var lamp = Toon.Mat(new Color(1f, 0.8f, 0.45f), 1.2f);
            var blossom = new[] { Toon.Mat(new Color(1f, 0.72f, 0.82f), 0.1f, 0.1f, 0.4f), Toon.Mat(new Color(1f, 0.82f, 0.9f), 0.1f, 0.1f, 0.4f) };
            var trunk = Toon.Mat(new Color(0.35f, 0.22f, 0.2f));

            // Großes Tor hinter den Gegnern
            float gz = 8f, gw = 3.2f, gh = 5f;
            for (int side = -1; side <= 1; side += 2)
                Prop(PrimitiveType.Cylinder, red, new Vector3(gw * side, gh * 0.5f, gz), new Vector3(0.45f, gh * 0.5f, 0.45f), Vector3.zero);
            Prop(PrimitiveType.Cube, red, new Vector3(0f, gh * 0.82f, gz), new Vector3(gw * 2.4f, 0.35f, 0.4f), Vector3.zero);
            Prop(PrimitiveType.Cube, black, new Vector3(0f, gh + 0.1f, gz), new Vector3(gw * 2.9f, 0.4f, 0.6f), Vector3.zero);

            // Steinlaternen
            for (int i = 0; i < 6; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * (4.5f + (i / 2) * 0.6f);
                float z = 2f + (i / 2) * 3f;
                Prop(PrimitiveType.Cylinder, stone, new Vector3(x, 0.6f, z), new Vector3(0.25f, 0.6f, 0.25f), Vector3.zero);
                Prop(PrimitiveType.Cube, lamp, new Vector3(x, 1.4f, z), new Vector3(0.45f, 0.4f, 0.45f), Vector3.zero);
                Prop(PrimitiveType.Cube, stone, new Vector3(x, 1.7f, z), new Vector3(0.7f, 0.15f, 0.7f), new Vector3(0f, 45f, 0f));
            }

            // Kirschbäume
            for (int i = 0; i < 14; i++)
            {
                var p = Around(rng, 8f, 18f);
                float h = R(rng, 2.5f, 4f);
                Prop(PrimitiveType.Cylinder, trunk, p + Vector3.up * h * 0.5f, new Vector3(0.3f, h * 0.5f, 0.3f), new Vector3(0f, 0f, R(rng, -8f, 8f)));
                for (int k = 0; k < 4; k++)
                {
                    float s = R(rng, 1.4f, 2.4f);
                    Prop(PrimitiveType.Sphere, blossom[rng.Next(blossom.Length)], p + new Vector3(R(rng, -1.2f, 1.2f), h + R(rng, -0.2f, 0.9f), R(rng, -1f, 1f)), new Vector3(s, s * 0.75f, s), Vector3.zero);
                }
            }
        }

        void Tower(System.Random rng)
        {
            var pillar = Toon.Mat(new Color(0.18f, 0.14f, 0.24f), 0f, 0.2f, 0.25f);
            var trim = Toon.Mat(new Color(0.65f, 0.4f, 0.95f), 0.6f, 0.6f);
            var crystal = Toon.Mat(new Color(0.85f, 0.25f, 0.55f), 0.9f, 0.9f, 0.6f);

            for (int i = 0; i < 10; i++)
            {
                float ang = (-80f + i * (160f / 9f)) * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Sin(ang) * 9f, 0f, Mathf.Cos(ang) * 7f + 1.5f);
                float h = R(rng, 6f, 9f);
                Prop(PrimitiveType.Cylinder, pillar, p + Vector3.up * h * 0.5f, new Vector3(0.9f, h * 0.5f, 0.9f), Vector3.zero);
                Prop(PrimitiveType.Cylinder, trim, p + Vector3.up * 0.4f, new Vector3(1.15f, 0.12f, 1.15f), Vector3.zero, false);
                Prop(PrimitiveType.Cylinder, trim, p + Vector3.up * h, new Vector3(1.15f, 0.12f, 1.15f), Vector3.zero, false);
            }
            for (int i = 0; i < 9; i++)
            {
                var p = Around(rng, 5f, 12f) + Vector3.up * R(rng, 2.5f, 6f);
                var c = Prop(PrimitiveType.Cube, crystal, p, new Vector3(0.4f, 1.1f, 0.4f), new Vector3(0f, 0f, 45f));
                var spin = c.gameObject.AddComponent<Spin>();
                spin.Speed = new Vector3(0f, R(rng, 20f, 50f), 0f);
                spin.BobHeight = 0.3f;
            }
            // Thron hinter der Fürstin
            Prop(PrimitiveType.Cube, pillar, new Vector3(0f, 1.8f, 5.2f), new Vector3(2.2f, 3.6f, 0.6f), Vector3.zero);
            Prop(PrimitiveType.Cube, trim, new Vector3(0f, 3.75f, 5.2f), new Vector3(2.6f, 0.3f, 0.7f), Vector3.zero);
            Prop(PrimitiveType.Cube, crystal, new Vector3(0f, 4.4f, 5.2f), new Vector3(0.6f, 0.6f, 0.6f), new Vector3(0f, 0f, 45f));
        }

        void OnDestroy()
        {
            if (cam != null)
            {
                cam.ResetProjectionMatrix();
                cam.transform.position = savedCamera.Pos;
                cam.transform.rotation = savedCamera.Rot;
                cam.fieldOfView = savedCamera.Fov;
                cam.farClipPlane = savedCamera.Far;
                cam.nearClipPlane = savedCamera.Near;
                cam.clearFlags = savedCamera.Clear;
                cam.backgroundColor = savedCamera.Bg;
            }
        }
    }
}
