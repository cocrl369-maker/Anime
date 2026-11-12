using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnimeFantasy
{
    /// <summary>
    /// Eine Figur auf der 3D-Kampfbühne. Nutzt Animator-Zustände eines eigenen Modells, falls vorhanden
    /// ("Idle", "Attack", "Cast", "Hit", "Die", "Victory"), und ergänzt prozedurale Bewegungen,
    /// die mit jedem Modell funktionieren.
    /// </summary>
    public class CharacterActor : MonoBehaviour, IPointerClickHandler
    {
        public BattleUnit Unit { get; private set; }
        public float Height { get; private set; }
        public Action<BattleUnit> Clicked;

        Transform pivot;
        Animator animator;
        Renderer[] renderers;
        MaterialPropertyBlock block;
        float flash;
        Color flashColor = Color.white;
        bool dead;
        float phase;
        Vector3 home;
        Transform ring;
        Material ringMat;
        Color ringColor;
        float ringAlpha;
        bool busy;

        public Vector3 HeadPoint => transform.position + Vector3.up * (Height + 0.35f);
        public Vector3 CenterPoint => transform.position + Vector3.up * (Height * 0.55f);
        public Vector3 Home => home;

        public void Init(BattleUnit unit, GameObject model, float height)
        {
            Unit = unit;
            Height = height;
            pivot = new GameObject("Pivot").transform;
            pivot.SetParent(transform, false);
            model.transform.SetParent(pivot, false);
            renderers = model.GetComponentsInChildren<Renderer>();
            animator = model.GetComponentInChildren<Animator>();
            block = new MaterialPropertyBlock();
            home = transform.position;
            phase = UnityEngine.Random.value * 10f;

            // Klickfläche für die Zielauswahl
            var col = gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, height * 0.5f, 0f);
            col.height = height;
            col.radius = Mathf.Max(0.45f, height * 0.25f);

            // Leuchtring am Boden (aktive Figur / Zielauswahl)
            ringMat = Toon.Additive(Vfx.RingTexture, Color.clear);
            var q = Vfx.CreateQuad("Ring", ringMat);
            ring = q.transform;
            ring.SetParent(transform, false);
            ring.localPosition = new Vector3(0f, 0.04f, 0f);
            ring.localRotation = Quaternion.Euler(90f, 0f, 0f);
            float rs = Mathf.Max(1.3f, height * 0.85f);
            ring.localScale = new Vector3(rs, rs, rs);

            TryPlay("Idle");
        }

        public void SetRing(Color color, float alpha)
        {
            ringColor = color;
            ringAlpha = alpha;
        }

        void Update()
        {
            float t = Time.time;
            if (!dead)
            {
                float s = Mathf.Sin(t * 2.2f + phase);
                pivot.localPosition = new Vector3(0f, s * 0.02f, 0f);
                pivot.localScale = new Vector3(1f - s * 0.006f, 1f + s * 0.012f, 1f - s * 0.006f);
            }

            if (flash > 0f)
            {
                flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 3.5f);
                ApplyFlash();
            }

            if (ringMat != null)
            {
                var c = ringColor;
                c.a = ringAlpha * (0.75f + 0.25f * Mathf.Sin(t * 5f));
                ringMat.SetColor("_Color", c);
                ring.Rotate(0f, 0f, 25f * Time.deltaTime, Space.Self);
            }
        }

        void ApplyFlash()
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetFloat("_FlashAmount", flash);
                block.SetColor("_FlashColor", flashColor);
                r.SetPropertyBlock(block);
            }
        }

        public void Flash(Color c, float amount)
        {
            flashColor = c;
            flash = amount;
            ApplyFlash();
        }

        bool TryPlay(string state)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;
            int hash = Animator.StringToHash(state);
            if (!animator.HasState(0, hash)) return false;
            animator.CrossFadeInFixedTime(hash, 0.12f);
            return true;
        }

        static float Ease(float k) => k * k * (3f - 2f * k);

        /// <summary>Nahkampf: zum Ziel sprinten, zuschlagen, zurück.</summary>
        public IEnumerator Lunge(Vector3 target)
        {
            busy = true;
            TryPlay("Attack");
            var dir = target - home;
            dir.y = 0f;
            var end = target - dir.normalized * 1.2f;
            for (float t = 0f; t < 0.22f; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(home, end, Ease(t / 0.22f)) + Vector3.up * Mathf.Sin(t / 0.22f * Mathf.PI) * 0.35f;
                yield return null;
            }
            transform.position = end;
            yield return new WaitForSeconds(0.12f);
            StartCoroutine(Return(end));
        }

        IEnumerator Return(Vector3 from)
        {
            yield return new WaitForSeconds(0.15f);
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(from, home, Ease(t / 0.3f));
                yield return null;
            }
            transform.position = home;
            busy = false;
            if (!dead) TryPlay("Idle");
        }

        /// <summary>Zauber: kurz abheben und in der Elementfarbe aufleuchten.</summary>
        public IEnumerator Cast(Color color)
        {
            TryPlay("Cast");
            Flash(color, 0.45f);
            Vfx.Rise(transform.position, color, 24);
            for (float t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                transform.position = home + Vector3.up * Mathf.Sin(t / 0.4f * Mathf.PI) * 0.25f;
                yield return null;
            }
            transform.position = home;
            if (!dead) TryPlay("Idle");
        }

        public void Hit(bool strong)
        {
            TryPlay("Hit");
            Flash(Color.white, 0.9f);
            if (!busy) StartCoroutine(Shake(strong ? 0.16f : 0.09f));
        }

        IEnumerator Shake(float amount)
        {
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                var r = UnityEngine.Random.insideUnitCircle * amount * (1f - t / 0.3f);
                transform.position = home + new Vector3(r.x, 0f, r.y);
                yield return null;
            }
            transform.position = home;
        }

        public void SetAlive(bool alive)
        {
            if (alive == !dead) return;
            dead = !alive;
            StartCoroutine(alive ? Revive() : Die());
        }

        IEnumerator Die()
        {
            bool animated = TryPlay("Die");
            SetRing(Color.clear, 0f);
            if (animated) yield break;
            var start = pivot.localRotation;
            var end = Quaternion.Euler(-80f, 0f, 0f);
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                float k = Ease(t / 0.6f);
                pivot.localRotation = Quaternion.Slerp(start, end, k);
                pivot.localPosition = new Vector3(0f, -0.15f * k, 0f);
                yield return null;
            }
            Flash(new Color(0.2f, 0.15f, 0.3f), 0.55f);
            flash = 0.55f;
        }

        IEnumerator Revive()
        {
            TryPlay("Idle");
            var start = pivot.localRotation;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                pivot.localRotation = Quaternion.Slerp(start, Quaternion.identity, Ease(t / 0.5f));
                yield return null;
            }
            pivot.localRotation = Quaternion.identity;
            Flash(new Color(1f, 1f, 0.8f), 1f);
        }

        public void Victory()
        {
            if (dead) return;
            if (!TryPlay("Victory")) StartCoroutine(Jump());
        }

        IEnumerator Jump()
        {
            for (int i = 0; i < 2; i++)
                for (float t = 0f; t < 0.35f; t += Time.deltaTime)
                {
                    transform.position = home + Vector3.up * Mathf.Sin(t / 0.35f * Mathf.PI) * 0.4f;
                    yield return null;
                }
            transform.position = home;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Clicked != null) Clicked(Unit);
        }

        void OnDestroy()
        {
            if (ringMat != null) Destroy(ringMat);
        }
    }
}
