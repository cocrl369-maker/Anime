using UnityEngine;
using UnityEngine.UI;

namespace AnimeFantasy
{
    /// <summary>Weltkarte mit den Kapiteln als Stationen auf einem Pfad.</summary>
    public class MapView : MonoBehaviour
    {
        // Positionen der Stationen (relativ zur Kartenfläche), von unten nach oben
        static readonly Vector2[] NodePositions =
        {
            new Vector2(0.25f, 0.08f), new Vector2(0.70f, 0.27f), new Vector2(0.30f, 0.46f),
            new Vector2(0.72f, 0.66f), new Vector2(0.45f, 0.88f)
        };

        public void Build(GameManager gm)
        {
            var save = gm.Save;
            var root = (RectTransform)transform;
            gm.SetBackground(new Color(0.18f, 0.32f, 0.45f), new Color(0.05f, 0.10f, 0.12f));

            int cycle = save.stage / GameDatabase.StageCount;
            int current = save.stage % GameDatabase.StageCount;

            UIFactory.CreateText("Title", root, "Weltkarte von Luminara", 62, UIFactory.Gold, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Stretch(0.04f, 0.92f, 0.96f, 0.98f);
            UIFactory.CreateText("Info", root,
                (cycle > 0 ? "Durchlauf " + (cycle + 1) + " (Gegner +" + cycle * 5 + " Level)   " : "") + "Gold: " + save.gold,
                34, Color.white).rectTransform.Stretch(0.04f, 0.88f, 0.96f, 0.92f);

            var map = UIFactory.CreateRect("Map", root).Stretch(0.05f, 0.14f, 0.95f, 0.86f);

            // Pfad zwischen den Stationen
            for (int i = 0; i < NodePositions.Length - 1; i++)
                CreatePath(map, NodePositions[i], NodePositions[i + 1], i < current || cycle > 0);

            for (int i = 0; i < GameDatabase.StageCount && i < NodePositions.Length; i++)
            {
                bool isCurrent = i == current;
                bool cleared = i < current || (cycle > 0 && i > current);
                bool locked = !cleared && !isCurrent;
                // Bereits geschaffte Kapitel nach dem aktuellen stammen aus dem vorigen Durchlauf
                int index = (i > current ? cycle - 1 : cycle) * GameDatabase.StageCount + i;
                if (index < 0) index = i;
                CreateNode(gm, map, i, index, NodePositions[i], cleared, isCurrent, locked);
            }

            var back = UIFactory.CreateButton("Back", root, "Zurück", new Color(0.30f, 0.30f, 0.42f), gm.ShowMenu, 40);
            ((RectTransform)back.transform).Stretch(0.06f, 0.03f, 0.40f, 0.10f);
            var shop = UIFactory.CreateButton("Shop", root, "Ausrüstung", new Color(0.30f, 0.50f, 0.75f), gm.ShowShop, 40);
            ((RectTransform)shop.transform).Stretch(0.46f, 0.03f, 0.94f, 0.10f);
        }

        static void CreatePath(RectTransform map, Vector2 a, Vector2 b, bool done)
        {
            // Gepunktete Linie aus kleinen Kreisen (funktioniert unabhängig vom Seitenverhältnis)
            const int dots = 9;
            for (int d = 1; d < dots; d++)
            {
                var p = Vector2.Lerp(a, b, d / (float)dots);
                var dot = UIFactory.CreateImage("Dot", map, done ? new Color(1f, 0.85f, 0.4f, 0.9f) : new Color(1f, 1f, 1f, 0.3f), SpriteFactory.Circle);
                var rt = dot.rectTransform;
                rt.anchorMin = rt.anchorMax = p;
                rt.sizeDelta = new Vector2(16, 16);
            }
        }

        static void CreateNode(GameManager gm, RectTransform map, int localIndex, int stageIndex, Vector2 pos,
            bool cleared, bool isCurrent, bool locked)
        {
            int bonus;
            var stage = GameDatabase.GetStage(stageIndex, out bonus);

            var holder = UIFactory.CreateRect("Node" + localIndex, map);
            holder.anchorMin = holder.anchorMax = pos;
            holder.sizeDelta = new Vector2(220, 220);

            if (isCurrent)
            {
                var glow = UIFactory.CreateImage("Glow", holder, new Color(1f, 0.8f, 0.3f, 0.35f), SpriteFactory.Circle);
                glow.rectTransform.Stretch(-0.15f, -0.15f, 1.15f, 1.15f);
                glow.gameObject.AddComponent<Pulse>();
            }

            Color nodeColor = locked ? new Color(0.25f, 0.25f, 0.3f) : Color.Lerp(stage.SkyTop, Color.white, 0.2f);
            var circle = UIFactory.CreateImage("Circle", holder, nodeColor, SpriteFactory.Circle);
            circle.raycastTarget = true;
            var btn = circle.gameObject.AddComponent<Button>();
            btn.targetGraphic = circle;
            btn.interactable = !locked;

            // Gegner-Portrait des Kapitels als Vorschau
            var preview = UIFactory.LoadPortrait(stage.Enemies[0]);
            if (preview != null && !locked)
            {
                var img = UIFactory.CreateImage("Preview", circle.transform, Color.white, preview);
                img.rectTransform.Stretch(0.1f, 0.1f, 0.9f, 0.9f);
                img.preserveAspect = true;
            }
            else
            {
                UIFactory.CreateText("Lock", circle.transform, locked ? "?" : (localIndex + 1).ToString(), 90, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            }

            var label = UIFactory.CreateText("Label", holder, (localIndex + 1) + ". " + stage.Name + (cleared ? "  (geschafft)" : ""), 32,
                locked ? new Color(1, 1, 1, 0.5f) : Color.white, TextAnchor.UpperCenter, FontStyle.Bold);
            label.rectTransform.Stretch(-0.6f, -0.32f, 1.6f, -0.02f);

            btn.onClick.AddListener(() =>
            {
                string body = "Gegner-Level " + (stage.EnemyLevel + bonus) + "\n" +
                              string.Join(", ", System.Array.ConvertAll(stage.Enemies, id => GameDatabase.GetCharacter(id).Name));
                body += cleared ? "\n\n(Wiederholung: EP und Gold für Training)" : "";
                UIFactory.ShowModal(gm.Overlay,
                    stage.Name, body,
                    "Kämpfen", () => gm.EnterChapter(stageIndex, cleared),
                    "Abbrechen", null);
            });
        }
    }

    /// <summary>Lässt ein UI-Element sanft pulsieren.</summary>
    public class Pulse : MonoBehaviour
    {
        void Update()
        {
            transform.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(Time.unscaledTime * 3f));
        }
    }
}
