using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnimeFantasy
{
    public class PlayerCommand
    {
        public SkillData Skill;
        public bool Defend;
    }

    public class UnitCard
    {
        public BattleUnit Unit;
        public RectTransform Root;
        public Button Button;
        public CanvasGroup Group;
        public Image Highlight;
        public Image Portrait;
        public Color PortraitColor;
        public Text NameText;
        public Text StatusText;
        public UIBar Hp, Mp, Burst;
    }

    /// <summary>Die komplette Kampf-Oberfläche (Hochformat, für Touch optimiert).</summary>
    public class BattleView : MonoBehaviour
    {
        static readonly Color HpColor = new Color(0.35f, 0.85f, 0.45f);
        static readonly Color EnemyHpColor = new Color(0.90f, 0.30f, 0.35f);
        static readonly Color MpColor = new Color(0.35f, 0.55f, 1f);
        static readonly Color BurstColor = new Color(1f, 0.80f, 0.25f);

        RectTransform root, commandArea, popupLayer;
        Text roundText, logText;
        Button autoButton, speedButton;
        BattleController controller;
        BattleStage stage;
        Action<BattleUnit> currentPick;
        BattleUnit activeUnit;
        readonly Dictionary<BattleUnit, UnitCard> cards = new Dictionary<BattleUnit, UnitCard>();
        readonly List<UnitCard> targetCards = new List<UnitCard>();

        // ------------------------------------------------------------------ Aufbau

        public void Build(BattleController c, string title, List<BattleUnit> party, List<BattleUnit> enemies, BattleStage battleStage)
        {
            controller = c;
            stage = battleStage;
            root = (RectTransform)transform;
            if (stage != null) stage.UnitClicked = OnUnitTapped;

            // Kopfzeile
            var header = UIFactory.Panel("Header", root, new Color(0.04f, 0.03f, 0.08f, 0.7f)).rectTransform.Stretch(0.02f, 0.935f, 0.98f, 0.995f);
            AddGoldEdge(header);
            UIFactory.CreateText("Title", header, title, 38, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold)
                .rectTransform.Stretch(0.03f, 0.48f, 0.62f, 1f);
            roundText = UIFactory.CreateText("Round", header, "Runde 1", 30, new Color(0.8f, 0.8f, 0.9f), TextAnchor.MiddleLeft);
            roundText.rectTransform.Stretch(0.03f, 0f, 0.62f, 0.5f);
            autoButton = UIFactory.CreateButton("Auto", header, "AUTO: AUS", new Color(0.30f, 0.30f, 0.45f), ToggleAuto, 30);
            ((RectTransform)autoButton.transform).Stretch(0.63f, 0.12f, 0.83f, 0.88f);
            speedButton = UIFactory.CreateButton("Speed", header, "x1", new Color(0.30f, 0.30f, 0.45f), ToggleSpeed, 34);
            ((RectTransform)speedButton.transform).Stretch(0.85f, 0.12f, 0.98f, 0.88f);

            // Gegner-Leiste (kompakt, die Figuren selbst stehen in der 3D-Szene)
            var enemyRow = UIFactory.CreateRect("Enemies", root).Stretch(0.03f, 0.868f, 0.97f, 0.93f);
            var eh = SetupRow(enemyRow, 12);
            if (enemies.Count == 1) eh.padding = new RectOffset(180, 180, 0, 0);
            foreach (var e in enemies) cards[e] = CreateCard(enemyRow, e);

            // Kampflog
            var logPanel = UIFactory.Panel("Log", root, new Color(0.03f, 0.02f, 0.07f, 0.55f)).rectTransform.Stretch(0.10f, 0.362f, 0.90f, 0.402f);
            logText = UIFactory.CreateText("LogText", logPanel, "", 34, Color.white);
            logText.rectTransform.Stretch(0.03f, 0.05f, 0.97f, 0.95f);

            // Party
            var partyRow = UIFactory.CreateRect("Party", root).Stretch(0.03f, 0.213f, 0.97f, 0.355f);
            SetupRow(partyRow, 12);
            foreach (var p in party) cards[p] = CreateCard(partyRow, p);

            // Befehle
            var cmdPanel = UIFactory.Panel("Commands", root, UIFactory.PanelColor);
            commandArea = cmdPanel.rectTransform;
            AddGoldEdge(commandArea);
            SetCommandHeight(false);

            // Ebene für schwebende Schadenszahlen und Banner
            popupLayer = UIFactory.CreateRect("Popups", root).Stretch(0f, 0f, 1f, 1f);

            RefreshAll(true);
        }

        static void AddGoldEdge(RectTransform rt)
        {
            var o = rt.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(UIFactory.Gold.r, UIFactory.Gold.g, UIFactory.Gold.b, 0.55f);
            o.effectDistance = new Vector2(2f, -2f);
        }

        /// <summary>Das Befehlsfeld wird für die Fähigkeitenliste nach oben erweitert.</summary>
        void SetCommandHeight(bool tall)
        {
            commandArea.Stretch(0.03f, 0.012f, 0.97f, tall ? 0.355f : 0.205f);
            commandArea.SetAsLastSibling();
            if (popupLayer != null) popupLayer.SetAsLastSibling();
        }

        static HorizontalLayoutGroup SetupRow(RectTransform row, float spacing)
        {
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = true;
            return h;
        }

        UnitCard CreateCard(RectTransform parent, BattleUnit u)
        {
            var card = new UnitCard { Unit = u };
            var frame = UIFactory.Panel(u.Name, parent, new Color(0.05f, 0.04f, 0.11f, 0.82f));
            frame.raycastTarget = true;
            card.Root = frame.rectTransform;
            AddGoldEdge(card.Root);
            card.Group = frame.gameObject.AddComponent<CanvasGroup>();
            card.Button = frame.gameObject.AddComponent<Button>();
            card.Button.transition = Selectable.Transition.None;
            card.Button.interactable = false;
            var nav = card.Button.navigation;
            nav.mode = Navigation.Mode.None;
            card.Button.navigation = nav;

            card.Highlight = UIFactory.Panel("Highlight", frame.transform, new Color(UIFactory.Gold.r, UIFactory.Gold.g, UIFactory.Gold.b, 0f));

            if (u.IsPlayer)
            {
                var portraitHolder = UIFactory.CreateRect("PortraitHolder", frame.transform).Stretch(0.02f, 0.10f, 0.33f, 0.90f);
                card.Portrait = UIFactory.CreatePortrait(portraitHolder, u.Data, out card.PortraitColor);
                card.NameText = UIFactory.CreateText("Name", frame.transform, "", 30, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
                card.NameText.rectTransform.Stretch(0.36f, 0.68f, 0.98f, 0.95f);
                card.Hp = new UIBar(frame.transform, "HP", HpColor, 22);
                card.Hp.Root.Stretch(0.36f, 0.43f, 0.96f, 0.63f);
                card.Mp = new UIBar(frame.transform, "MP", MpColor, 20);
                card.Mp.Root.Stretch(0.36f, 0.22f, 0.96f, 0.39f);
                card.Burst = new UIBar(frame.transform, "Burst", BurstColor, 0);
                card.Burst.Root.Stretch(0.36f, 0.07f, 0.96f, 0.16f);
                card.StatusText = UIFactory.CreateText("Status", frame.transform, "", 20, Color.white, TextAnchor.UpperLeft, FontStyle.Bold);
                card.StatusText.rectTransform.Stretch(0.03f, 0.80f, 0.60f, 0.99f);
            }
            else
            {
                var portraitHolder = UIFactory.CreateRect("PortraitHolder", frame.transform).Stretch(0.01f, 0.06f, 0.22f, 0.94f);
                card.Portrait = UIFactory.CreatePortrait(portraitHolder, u.Data, out card.PortraitColor);
                card.NameText = UIFactory.CreateText("Name", frame.transform, "", 26, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
                card.NameText.rectTransform.Stretch(0.25f, 0.52f, 0.98f, 0.97f);
                card.Hp = new UIBar(frame.transform, "HP", EnemyHpColor, 20);
                card.Hp.Root.Stretch(0.25f, 0.10f, 0.97f, 0.46f);
                card.StatusText = UIFactory.CreateText("Status", frame.transform, "", 18, Color.white, TextAnchor.UpperRight, FontStyle.Bold);
                card.StatusText.rectTransform.Stretch(0.5f, 0.55f, 0.98f, 0.98f);
            }
            return card;
        }

        // ------------------------------------------------------------------ Aktualisierung

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            foreach (var c in cards.Values)
            {
                c.Hp.Tick(dt);
                if (c.Mp != null) c.Mp.Tick(dt);
                if (c.Burst != null) c.Burst.Tick(dt);

                float alpha = 0f;
                if (targetCards.Contains(c)) alpha = 0.25f + 0.2f * Mathf.Sin(Time.unscaledTime * 6f);
                else if (c.Unit == activeUnit) alpha = 0.3f;
                var h = c.Highlight.color;
                h.a = alpha;
                c.Highlight.color = h;
            }
        }

        public void RefreshAll(bool instant = false)
        {
            foreach (var u in cards.Keys) RefreshUnit(u, instant);
        }

        public void RefreshUnit(BattleUnit u, bool instant = false)
        {
            UnitCard c;
            if (!cards.TryGetValue(u, out c)) return;

            c.NameText.text = u.Name + "  <size=24>Lv " + u.Level + "</size>";
            c.Hp.Set(u.HpRatio, u.Hp + " / " + u.MaxHp, instant);
            if (c.Mp != null) c.Mp.Set(u.MaxMp > 0 ? (float)u.Mp / u.MaxMp : 0f, "MP " + u.Mp, instant);
            if (c.Burst != null) c.Burst.Set(u.Burst / 100f, null, instant);

            var parts = u.Statuses.Select(s => "<color=#" + ColorUtility.ToHtmlStringRGB(StatusInfo.GetColor(s.Type)) + ">" +
                                               StatusInfo.ShortName(s.Type) + "</color>").ToList();
            if (u.Defending) parts.Insert(0, "<color=#99CCFF>VERT</color>");
            if (u.BurstReady) parts.Insert(0, "<color=#FFD040>BURST!</color>");
            c.StatusText.text = string.Join(" ", parts.ToArray());

            c.Group.alpha = u.IsAlive ? 1f : 0.35f;
            if (stage != null) stage.SetAlive(u, u.IsAlive);
            c.Portrait.color = u.IsAlive ? c.PortraitColor : Color.gray;
        }

        public void SetRound(int round) { roundText.text = "Runde " + round; }

        public void SetActive(BattleUnit u)
        {
            activeUnit = u;
            if (stage != null) stage.SetActive(u);
        }

        public void Victory(List<BattleUnit> party)
        {
            if (stage != null) stage.Victory(party);
        }

        public void Log(string message) { logText.text = message; }

        // ------------------------------------------------------------------ Befehle

        public void ClearCommands()
        {
            for (int i = commandArea.childCount - 1; i >= 0; i--)
                Destroy(commandArea.GetChild(i).gameObject);
        }

        void CommandHeader(string text)
        {
            var t = UIFactory.CreateText("Header", commandArea, text, 36, UIFactory.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.rectTransform.Stretch(0.03f, 0.83f, 0.97f, 0.98f);
        }

        Button CommandButton(string label, Color color, Action onClick, float x0, float y0, float x1, float y1, int size = 44)
        {
            var b = UIFactory.CreateButton(label, commandArea, label, color, onClick, size);
            ((RectTransform)b.transform).Stretch(x0, y0, x1, y1);
            return b;
        }

        static Action<PlayerCommand> Once(Action<PlayerCommand> callback)
        {
            bool done = false;
            return c =>
            {
                if (done) return;
                done = true;
                callback(c);
            };
        }

        public void ShowCommandMenu(BattleUnit u, Action<PlayerCommand> onChosen)
        {
            ClearCommands();
            SetCommandHeight(false);
            var choose = Once(onChosen);
            CommandHeader(u.Name + " ist am Zug");

            CommandButton("Angriff", new Color(0.85f, 0.35f, 0.40f),
                () => choose(new PlayerCommand { Skill = GameDatabase.BasicAttack }), 0.03f, 0.44f, 0.49f, 0.80f);
            CommandButton("Fähigkeiten", new Color(0.35f, 0.45f, 0.90f),
                () => ShowSkillMenu(u, onChosen), 0.51f, 0.44f, 0.97f, 0.80f);
            CommandButton("Verteidigen", new Color(0.35f, 0.40f, 0.55f),
                () => choose(new PlayerCommand { Defend = true }), 0.03f, 0.05f, 0.49f, 0.41f);

            var burst = GameDatabase.GetSkill(u.Data.BurstSkill);
            string label = u.BurstReady && burst != null ? "BURST\n<size=30>" + burst.Name + "</size>" : "Burst " + u.Burst + "%";
            var b = CommandButton("Burst", new Color(0.95f, 0.70f, 0.20f),
                () => choose(new PlayerCommand { Skill = burst }), 0.51f, 0.05f, 0.97f, 0.41f);
            b.SetLabel(label);
            b.interactable = u.BurstReady && burst != null;
        }

        void ShowSkillMenu(BattleUnit u, Action<PlayerCommand> onChosen)
        {
            ClearCommands();
            SetCommandHeight(true);
            var choose = Once(onChosen);
            CommandHeader("Fähigkeiten  –  MP " + u.Mp + " / " + u.MaxMp);

            var list = UIFactory.CreateRect("SkillList", commandArea).Stretch(0.03f, 0.25f, 0.97f, 0.82f);
            var v = list.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 10;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = true;

            foreach (var s in u.Data.Skills.Select(GameDatabase.GetSkill).Where(s => s != null))
            {
                var skill = s;
                var color = Color.Lerp(ElementInfo.GetColor(skill.Element), new Color(0.2f, 0.2f, 0.3f), 0.45f);
                var b = UIFactory.CreateButton(skill.Name, list, skill.Name + "  <color=#9FC3FF>(" + skill.MpCost + " MP)</color>\n<size=24>" + skill.Description + "</size>",
                    color, () => choose(new PlayerCommand { Skill = skill }), 36);
                b.interactable = u.Mp >= skill.MpCost;
            }

            CommandButton("Zurück", new Color(0.35f, 0.35f, 0.45f), () => ShowCommandMenu(u, onChosen), 0.03f, 0.04f, 0.97f, 0.21f, 38);
        }

        public void ShowTargetPrompt(SkillData skill, Action onCancel)
        {
            ClearCommands();
            SetCommandHeight(false);
            CommandHeader(skill.Name);
            var info = UIFactory.CreateText("Info", commandArea,
                skill.Target == TargetType.SingleAlly ? "Tippe auf einen Verbündeten" : "Tippe auf einen Gegner (Figur oder Leiste)", 44, Color.white);
            info.rectTransform.Stretch(0.05f, 0.40f, 0.95f, 0.80f);
            bool done = false;
            CommandButton("Abbrechen", new Color(0.35f, 0.35f, 0.45f), () =>
            {
                if (done) return;
                done = true;
                onCancel();
            }, 0.20f, 0.06f, 0.80f, 0.34f, 40);
        }

        public void BeginTargetSelect(List<BattleUnit> candidates, Action<BattleUnit> onPick)
        {
            EndTargetSelect();
            bool done = false;
            currentPick = u =>
            {
                if (done) return;
                done = true;
                onPick(u);
            };
            if (stage != null) stage.SetTargets(candidates);
            foreach (var u in candidates)
            {
                UnitCard c;
                if (!cards.TryGetValue(u, out c)) continue;
                var captured = u;
                c.Button.onClick.RemoveAllListeners();
                c.Button.onClick.AddListener(() =>
                {
                    if (done) return;
                    done = true;
                    onPick(captured);
                });
                c.Button.interactable = true;
                targetCards.Add(c);
            }
        }

        void OnUnitTapped(BattleUnit u)
        {
            UnitCard c;
            if (currentPick != null && cards.TryGetValue(u, out c) && targetCards.Contains(c)) currentPick(u);
        }

        public void EndTargetSelect()
        {
            currentPick = null;
            if (stage != null) stage.SetTargets(null);
            foreach (var c in targetCards)
            {
                c.Button.onClick.RemoveAllListeners();
                c.Button.interactable = false;
            }
            targetCards.Clear();
        }

        void ToggleAuto()
        {
            controller.AutoMode = !controller.AutoMode;
            autoButton.SetLabel(controller.AutoMode ? "AUTO: AN" : "AUTO: AUS");
            autoButton.targetGraphic.color = controller.AutoMode ? new Color(0.85f, 0.55f, 0.20f) : new Color(0.30f, 0.30f, 0.45f);
            if (controller.AutoMode) EndTargetSelect();
        }

        void ToggleSpeed()
        {
            Time.timeScale = Time.timeScale > 1.5f ? 1f : 2f;
            speedButton.SetLabel(Time.timeScale > 1.5f ? "x2" : "x1");
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
            if (stage != null) Destroy(stage.gameObject);
        }

        // ------------------------------------------------------------------ Effekte

        public void ShowResult(ActionResult r)
        {
            UnitCard c;
            if (!cards.TryGetValue(r.Target, out c)) return;
            if (stage != null) stage.ShowResult(r);

            if (r.IsHeal)
            {
                Popup(r.Target, r.Revived ? "Wiederbelebt!\n+" + r.Amount : "+" + r.Amount, new Color(0.45f, 1f, 0.55f), 60);
                StartCoroutine(Flash(c, new Color(0.6f, 1f, 0.6f)));
            }
            else if (r.IsDamage)
            {
                var color = r.Effectiveness > 1f ? new Color(1f, 0.6f, 0.2f)
                          : r.Effectiveness < 1f ? new Color(0.7f, 0.75f, 0.9f)
                          : Color.white;
                string text = r.Amount.ToString();
                if (r.Crit) text = "<size=34>KRITISCH!</size>\n" + text;
                Popup(r.Target, text, color, r.Crit ? 76 : 64);
                StartCoroutine(Shake(c));
                StartCoroutine(Flash(c, new Color(1f, 0.3f, 0.3f)));
            }

            if (r.StatusApplied != StatusType.None)
                Popup(r.Target, StatusInfo.ShortName(r.StatusApplied), StatusInfo.GetColor(r.StatusApplied), 40, -90f);

            RefreshUnit(r.Target);
        }

        public void Popup(BattleUnit u, string text, Color color, int size, float yOffset = 0f)
        {
            UnitCard c;
            if (!cards.TryGetValue(u, out c)) return;
            var t = UIFactory.CreateText("Popup", popupLayer, text, size, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(420f, 200f);
            Vector2 screen;
            Vector2 local;
            if (stage != null && stage.TryGetScreenPoint(u, out screen) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(popupLayer, screen, null, out local))
                rt.anchoredPosition = local + new Vector2(UnityEngine.Random.Range(-30f, 30f), yOffset);
            else
            {
                rt.position = c.Portrait.rectTransform.position;
                rt.anchoredPosition += new Vector2(0f, yOffset);
            }
            var outline = t.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(3f, -3f);
            StartCoroutine(FloatAndFade(t));
        }

        IEnumerator FloatAndFade(Text t)
        {
            var rt = t.rectTransform;
            var start = rt.anchoredPosition;
            var baseColor = t.color;
            float dur = 1.0f;
            for (float time = 0f; time < dur; time += Time.deltaTime)
            {
                float k = time / dur;
                rt.anchoredPosition = start + new Vector2(0f, 140f * Mathf.Sqrt(k));
                rt.localScale = Vector3.one * (k < 0.15f ? Mathf.Lerp(0.6f, 1.15f, k / 0.15f) : Mathf.Lerp(1.15f, 1f, (k - 0.15f) * 3f));
                var col = baseColor;
                col.a = k < 0.7f ? 1f : 1f - (k - 0.7f) / 0.3f;
                t.color = col;
                yield return null;
            }
            Destroy(t.gameObject);
        }

        IEnumerator Shake(UnitCard c)
        {
            var rt = c.Portrait.rectTransform;
            var parent = (RectTransform)rt.parent;
            var basePos = parent.anchoredPosition;
            for (float time = 0f; time < 0.3f; time += Time.deltaTime)
            {
                parent.anchoredPosition = basePos + UnityEngine.Random.insideUnitCircle * 14f;
                yield return null;
            }
            parent.anchoredPosition = basePos;
        }

        IEnumerator Flash(UnitCard c, Color flash)
        {
            for (float time = 0f; time < 0.35f; time += Time.deltaTime)
            {
                c.Portrait.color = Color.Lerp(flash, c.Unit.IsAlive ? c.PortraitColor : Color.gray, time / 0.35f);
                yield return null;
            }
            c.Portrait.color = c.Unit.IsAlive ? c.PortraitColor : Color.gray;
        }

        /// <summary>Angriffs-/Zauberanimation auf der 3D-Bühne (Fallback: Portrait-Animation).</summary>
        public IEnumerator AnimateAttack(BattleUnit u, SkillData skill, List<BattleUnit> targets)
        {
            if (stage != null)
            {
                yield return stage.AnimateAction(u, skill, targets);
                yield break;
            }
            UnitCard c;
            if (!cards.TryGetValue(u, out c)) yield break;
            var holder = (RectTransform)c.Portrait.rectTransform.parent;
            const float dur = 0.3f;
            for (float time = 0f; time < dur; time += Time.deltaTime)
            {
                holder.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(time / dur * Mathf.PI));
                yield return null;
            }
            holder.localScale = Vector3.one;
        }

        public void ShowBurstBanner(BattleUnit u, SkillData skill)
        {
            if (stage != null) stage.Burst(u, skill);
            var strip = UIFactory.CreateImage("BurstBanner", popupLayer, new Color(0.08f, 0.04f, 0.15f, 0.92f));
            strip.rectTransform.Stretch(0f, 0.44f, 1f, 0.56f);
            var glow = UIFactory.CreateImage("Glow", strip.transform, new Color(u.Data.Color.r, u.Data.Color.g, u.Data.Color.b, 0.35f));
            glow.rectTransform.Stretch(0f, 0.08f, 1f, 0.92f);
            var t = UIFactory.CreateText("Text", strip.transform, u.Name.ToUpper() + "\n<size=86>" + skill.Name.ToUpper() + "</size>",
                44, UIFactory.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.resizeTextForBestFit = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            StartCoroutine(BannerRoutine(strip, t.rectTransform));
        }

        IEnumerator BannerRoutine(Image strip, RectTransform text)
        {
            var group = strip.gameObject.AddComponent<CanvasGroup>();
            const float dur = 1.3f;
            for (float time = 0f; time < dur; time += Time.deltaTime)
            {
                float k = time / dur;
                float x = k < 0.2f ? Mathf.Lerp(500f, 0f, Mathf.SmoothStep(0f, 1f, k / 0.2f))
                        : k > 0.8f ? Mathf.Lerp(0f, -500f, (k - 0.8f) / 0.2f)
                        : 0f;
                text.anchoredPosition = new Vector2(x, 0f);
                group.alpha = k < 0.15f ? k / 0.15f : (k > 0.8f ? (1f - k) / 0.2f : 1f);
                yield return null;
            }
            Destroy(strip.gameObject);
        }
    }
}
