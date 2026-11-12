using UnityEngine;
using UnityEngine.UI;

namespace AnimeFantasy
{
    /// <summary>Kaufen und Anlegen von Ausrüstung. Jedes Item gibt es einmal und es kann einem Helden angelegt werden.</summary>
    public class ShopView : MonoBehaviour
    {
        GameManager gm;
        string selectedHero = GameDatabase.HeroIds[0];
        float scrollPos = 1f;

        public void Build(GameManager manager)
        {
            gm = manager;
            gm.SetBackground(new Color(0.35f, 0.22f, 0.15f), new Color(0.08f, 0.04f, 0.06f));
            Rebuild();
        }

        void Rebuild()
        {
            var oldScroll = GetComponentInChildren<ScrollRect>();
            if (oldScroll != null) scrollPos = oldScroll.verticalNormalizedPosition;
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);

            var root = (RectTransform)transform;
            var save = gm.Save;

            UIFactory.CreateText("Title", root, "Ausrüstung & Shop", 60, UIFactory.Gold, TextAnchor.MiddleLeft, FontStyle.Bold)
                .rectTransform.Stretch(0.05f, 0.92f, 0.70f, 0.98f);
            var goldPanel = UIFactory.Panel("Gold", root, new Color(0f, 0f, 0f, 0.45f));
            goldPanel.rectTransform.Stretch(0.70f, 0.925f, 0.96f, 0.975f);
            UIFactory.CreateText("GoldText", goldPanel.transform, save.gold + " G", 40, UIFactory.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);

            // Helden-Reiter
            var tabs = UIFactory.CreateRect("Tabs", root).Stretch(0.04f, 0.84f, 0.96f, 0.91f);
            var h = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 12;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = true;
            foreach (var id in GameDatabase.HeroIds)
            {
                var heroId = id;
                var data = GameDatabase.GetCharacter(id);
                bool selected = id == selectedHero;
                var tab = UIFactory.CreateButton(id, tabs, data.Name,
                    selected ? Color.Lerp(data.Color, Color.black, 0.2f) : new Color(0.22f, 0.20f, 0.30f),
                    () => { selectedHero = heroId; Rebuild(); }, 40);
                tab.GetComponentInChildren<Text>().rectTransform.Stretch(0.35f, 0.05f, 0.97f, 0.95f);
                var icon = UIFactory.CreateRect("Icon", tab.transform).Stretch(0.04f, 0.08f, 0.32f, 0.92f);
                Color c;
                UIFactory.CreatePortrait(icon, data, out c);
            }

            BuildHeroPanel(root, save);

            // Item-Liste
            var listArea = UIFactory.Panel("ListArea", root, new Color(0f, 0f, 0f, 0.35f));
            listArea.rectTransform.Stretch(0.04f, 0.12f, 0.96f, 0.56f);
            var listHolder = UIFactory.CreateRect("Holder", listArea.transform).Stretch(0.02f, 0.01f, 0.98f, 0.99f);
            var content = UIFactory.CreateScrollList(listHolder, 10);
            foreach (var item in GameDatabase.Items) CreateItemRow(content, item, save);

            var scroll = content.parent.GetComponent<ScrollRect>();
            if (scroll != null) StartCoroutine(RestoreScroll(scroll));

            var back = UIFactory.CreateButton("Back", root, "Zurück", new Color(0.30f, 0.30f, 0.42f), gm.ShowMenu, 40);
            ((RectTransform)back.transform).Stretch(0.06f, 0.025f, 0.46f, 0.095f);
            var map = UIFactory.CreateButton("Map", root, "Weltkarte", new Color(0.85f, 0.35f, 0.45f), gm.ShowMap, 40);
            ((RectTransform)map.transform).Stretch(0.54f, 0.025f, 0.94f, 0.095f);
        }

        System.Collections.IEnumerator RestoreScroll(ScrollRect scroll)
        {
            yield return null; // warten, bis das Layout berechnet ist
            if (scroll != null) scroll.verticalNormalizedPosition = scrollPos;
        }

        void BuildHeroPanel(RectTransform root, SaveData save)
        {
            var data = GameDatabase.GetCharacter(selectedHero);
            var progress = save.GetHero(selectedHero);
            var unit = GameDatabase.CreateHero(save, selectedHero);

            var panel = UIFactory.Panel("Hero", root, UIFactory.PanelColor);
            panel.rectTransform.Stretch(0.04f, 0.58f, 0.96f, 0.825f);

            var holder = UIFactory.CreateRect("PortraitHolder", panel.transform).Stretch(0.02f, 0.06f, 0.34f, 0.94f);
            Color c;
            UIFactory.CreatePortrait(holder, data, out c);

            UIFactory.CreateText("Name", panel.transform, data.Name + "  <size=30>Lv " + progress.level + " · " + data.Title + "</size>",
                44, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.Stretch(0.37f, 0.78f, 0.98f, 0.96f);

            UIFactory.CreateText("Stats", panel.transform,
                "HP " + unit.MaxHp + "    MP " + unit.MaxMp + "    TMP " + unit.Spd + "\n" +
                "ATK " + unit.Atk + "    DEF " + unit.Def + "    MAG " + unit.Mag,
                32, new Color(0.85f, 0.88f, 1f), TextAnchor.MiddleLeft).rectTransform.Stretch(0.37f, 0.52f, 0.98f, 0.78f);

            string equip = "";
            foreach (EquipSlot slot in System.Enum.GetValues(typeof(EquipSlot)))
            {
                var item = GameDatabase.GetItem(progress.Get(slot));
                equip += ItemData.SlotName(slot) + ": " + (item != null ? "<color=#FFD35A>" + item.Name + "</color>" : "<color=#888888>leer</color>") + "\n";
            }
            UIFactory.CreateText("Equip", panel.transform, equip.TrimEnd('\n'), 30, Color.white, TextAnchor.UpperLeft)
                .rectTransform.Stretch(0.37f, 0.04f, 0.98f, 0.50f);
        }

        void CreateItemRow(RectTransform content, ItemData item, SaveData save)
        {
            var row = UIFactory.Panel(item.Id, content, new Color(0.12f, 0.10f, 0.20f, 0.95f));
            UIFactory.SetPreferredHeight(row, 140);

            UIFactory.CreateText("Name", row.transform, item.Name + "  <size=26><color=#AFC3FF>" + ItemData.SlotName(item.Slot) + "</color></size>",
                40, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.Stretch(0.04f, 0.50f, 0.66f, 0.95f);
            UIFactory.CreateText("Stats", row.transform, item.StatText(), 30, new Color(0.6f, 1f, 0.7f), TextAnchor.MiddleLeft)
                .rectTransform.Stretch(0.04f, 0.08f, 0.66f, 0.50f);

            var wearer = save.WornBy(item);
            string label;
            Color color;
            System.Action action;
            bool enabled = true;

            if (!save.Owns(item.Id))
            {
                label = "Kaufen\n<size=30>" + item.Price + " G</size>";
                color = new Color(0.85f, 0.65f, 0.20f);
                enabled = save.gold >= item.Price;
                action = () =>
                {
                    save.gold -= item.Price;
                    save.ownedItems.Add(item.Id);
                    save.Equip(selectedHero, item); // direkt anlegen ist auf dem Handy am bequemsten
                    SaveSystem.Save(save);
                    Rebuild();
                };
            }
            else if (wearer != null && wearer.id == selectedHero)
            {
                label = "Ablegen";
                color = new Color(0.40f, 0.40f, 0.50f);
                action = () =>
                {
                    wearer.Set(item.Slot, null);
                    SaveSystem.Save(save);
                    Rebuild();
                };
            }
            else
            {
                label = wearer != null ? "Anlegen\n<size=24>(trägt " + GameDatabase.GetCharacter(wearer.id).Name + ")</size>" : "Anlegen";
                color = new Color(0.35f, 0.60f, 0.85f);
                action = () =>
                {
                    save.Equip(selectedHero, item);
                    SaveSystem.Save(save);
                    Rebuild();
                };
            }

            var btn = UIFactory.CreateButton("Action", row.transform, label, color, action, 36);
            ((RectTransform)btn.transform).Stretch(0.68f, 0.12f, 0.97f, 0.88f);
            btn.interactable = enabled;
        }
    }
}
