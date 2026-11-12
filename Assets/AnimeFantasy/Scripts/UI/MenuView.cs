using UnityEngine;
using UnityEngine.UI;

namespace AnimeFantasy
{
    /// <summary>Titelbildschirm mit Party-Übersicht.</summary>
    public class MenuView : MonoBehaviour
    {
        public void Build(GameManager gm)
        {
            var root = (RectTransform)transform;
            var save = gm.Save;
            gm.SetBackground(new Color(0.22f, 0.10f, 0.38f), new Color(0.03f, 0.03f, 0.10f));

            UIFactory.CreateText("Title", root, "STERNENKLINGEN", 120, UIFactory.Gold, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Stretch(0.04f, 0.83f, 0.96f, 0.95f);
            UIFactory.CreateText("Subtitle", root, "Ein Anime-Fantasy-Rundenkampf-RPG", 44, new Color(0.85f, 0.80f, 1f))
                .rectTransform.Stretch(0.05f, 0.78f, 0.95f, 0.83f);

            // Party-Übersicht
            var partyRow = UIFactory.CreateRect("Party", root).Stretch(0.04f, 0.42f, 0.96f, 0.76f);
            var h = partyRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 18;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = true;

            foreach (var id in GameDatabase.HeroIds)
            {
                var data = GameDatabase.GetCharacter(id);
                var progress = save.GetHero(id);
                var unit = new BattleUnit(data, progress.level, true);

                var card = UIFactory.Panel(data.Name, partyRow, UIFactory.PanelColor);
                var holder = UIFactory.CreateRect("PortraitHolder", card.transform).Stretch(0.1f, 0.50f, 0.9f, 0.97f);
                Color baseColor;
                UIFactory.CreatePortrait(holder, data, out baseColor);

                UIFactory.CreateText("Name", card.transform, data.Name + "  <size=28>Lv " + progress.level + "</size>", 42, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold)
                    .rectTransform.Stretch(0.03f, 0.40f, 0.97f, 0.50f);
                UIFactory.CreateText("Title", card.transform, data.Title, 28, ElementInfo.GetColor(data.Element))
                    .rectTransform.Stretch(0.03f, 0.33f, 0.97f, 0.40f);

                var xpBar = new UIBar(card.transform, "XP", new Color(0.65f, 0.5f, 1f), 22);
                xpBar.Root.Stretch(0.08f, 0.26f, 0.92f, 0.32f);
                int need = Progression.XpToNext(progress.level);
                xpBar.Set((float)progress.xp / need, "EP " + progress.xp + "/" + need, true);

                UIFactory.CreateText("Stats", card.transform,
                    "HP " + unit.MaxHp + "   MP " + unit.MaxMp + "\nATK " + unit.Atk + "   MAG " + unit.Mag + "\nDEF " + unit.Def + "   TMP " + unit.Spd,
                    28, new Color(0.85f, 0.85f, 0.95f))
                    .rectTransform.Stretch(0.04f, 0.03f, 0.96f, 0.25f);
            }

            // Buttons
            int levelBonus;
            var next = GameDatabase.GetStage(save.stage, out levelBonus);
            string continueLabel = save.stage == 0
                ? "Abenteuer beginnen"
                : "Weiter: Kapitel " + (save.stage + 1) + "\n<size=30>" + next.Name + (levelBonus > 0 ? " (+" + levelBonus + ")" : "") + "</size>";

            var cont = UIFactory.CreateButton("Continue", root, continueLabel, new Color(0.85f, 0.35f, 0.45f),
                () => gm.StartBattle(save.stage, false), 50);
            ((RectTransform)cont.transform).Stretch(0.10f, 0.27f, 0.90f, 0.37f);

            int trainingStage = Mathf.Max(0, save.stage - 1);
            var training = UIFactory.CreateButton("Training", root,
                "Training\n<size=28>" + GameDatabase.GetStage(trainingStage, out levelBonus).Name + " wiederholen</size>",
                new Color(0.30f, 0.50f, 0.75f), () => gm.StartBattle(trainingStage, true), 42);
            ((RectTransform)training.transform).Stretch(0.10f, 0.165f, 0.90f, 0.25f);
            training.interactable = save.stage > 0;

            var reset = UIFactory.CreateButton("NewGame", root, "Neues Spiel", new Color(0.30f, 0.30f, 0.42f),
                () => UIFactory.ShowModal(root, "Neues Spiel?", "Der aktuelle Fortschritt wird gelöscht.",
                    "Löschen", () => gm.NewGame(), "Abbrechen", null), 38);
            ((RectTransform)reset.transform).Stretch(0.25f, 0.07f, 0.75f, 0.14f);

            UIFactory.CreateText("Version", root, "Prototyp v0.1", 24, new Color(1f, 1f, 1f, 0.4f))
                .rectTransform.Stretch(0f, 0.01f, 1f, 0.05f);
        }
    }
}
