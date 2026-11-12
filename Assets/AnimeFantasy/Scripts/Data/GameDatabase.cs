using System.Collections.Generic;
using UnityEngine;

namespace AnimeFantasy
{
    /// <summary>
    /// Alle Spieldaten an einem Ort: Helden, Gegner, Fähigkeiten und Kapitel.
    /// Neue Inhalte hier eintragen (oder später auf ScriptableObjects umstellen).
    /// </summary>
    public static class GameDatabase
    {
        public static readonly string[] HeroIds = { "akari", "ren", "yuki" };

        static readonly Dictionary<string, SkillData> skills = new Dictionary<string, SkillData>();
        static readonly Dictionary<string, CharacterData> characters = new Dictionary<string, CharacterData>();
        static readonly List<StageData> stages = new List<StageData>();
        static readonly List<ItemData> items = new List<ItemData>();
        static readonly Dictionary<int, StoryLine[]> intros = new Dictionary<int, StoryLine[]>();
        static readonly Dictionary<int, StoryLine[]> outros = new Dictionary<int, StoryLine[]>();

        public static IReadOnlyList<ItemData> Items => items;

        public static SkillData BasicAttack => skills["attack"];

        static GameDatabase()
        {
            // ---------- Fähigkeiten ----------
            AddSkill("attack", "Angriff", "Ein einfacher Angriff.", Element.Neutral, SkillKind.Physical, TargetType.SingleEnemy, 1.0f, 0);

            // Akari – Flammenklinge
            AddSkill("flammenhieb", "Flammenhieb", "Feuriger Schwerthieb gegen einen Gegner.", Element.Fire, SkillKind.Physical, TargetType.SingleEnemy, 1.6f, 6);
            AddSkill("kampfgeist", "Kampfgeist", "Erhöht den eigenen Angriff für 4 Züge.", Element.Neutral, SkillKind.Support, TargetType.Self, 0f, 8, StatusType.AttackUp, 4);
            AddSkill("phoenixtanz", "Phönixtanz", "BURST: Flammenwirbel gegen alle Gegner.", Element.Fire, SkillKind.Physical, TargetType.AllEnemies, 2.2f, 0, burst: true);

            // Ren – Sturmweiser
            AddSkill("windklinge", "Windklinge", "Scharfe Windmagie gegen einen Gegner.", Element.Wind, SkillKind.Magical, TargetType.SingleEnemy, 1.5f, 6);
            AddSkill("orkan", "Orkan", "Ein Sturm trifft alle Gegner.", Element.Wind, SkillKind.Magical, TargetType.AllEnemies, 1.0f, 14);
            AddSkill("sturmbarriere", "Sturmbarriere", "Erhöht die Verteidigung aller Verbündeten.", Element.Neutral, SkillKind.Support, TargetType.AllAllies, 0f, 12, StatusType.DefenseUp, 3);
            AddSkill("himmelsriss", "Himmelsriss", "BURST: Der Himmel zerreißt über allen Gegnern.", Element.Wind, SkillKind.Magical, TargetType.AllEnemies, 2.4f, 0, burst: true);

            // Yuki – Mondpriesterin
            AddSkill("heilung", "Heilung", "Heilt einen Verbündeten.", Element.Light, SkillKind.Heal, TargetType.SingleAlly, 1.8f, 8);
            AddSkill("lichtsegen", "Lichtsegen", "Heilt alle Verbündeten und gewährt Regeneration.", Element.Light, SkillKind.Heal, TargetType.AllAllies, 0.9f, 16, StatusType.Regen, 3);
            AddSkill("heiligerstrahl", "Heiliger Strahl", "Lichtstrahl gegen einen Gegner.", Element.Light, SkillKind.Magical, TargetType.SingleEnemy, 1.4f, 7);
            AddSkill("mondwiedergeburt", "Mondwiedergeburt", "BURST: Belebt und heilt alle Verbündeten.", Element.Light, SkillKind.Heal, TargetType.AllAllies, 1.4f, 0, StatusType.Regen, 3, burst: true, revives: true);

            // Gegner
            AddSkill("blubberschlag", "Blubberschlag", "", Element.Water, SkillKind.Physical, TargetType.SingleEnemy, 1.3f, 4);
            AddSkill("hinterhalt", "Hinterhalt", "", Element.Dark, SkillKind.Physical, TargetType.SingleEnemy, 1.5f, 5);
            AddSkill("erdbeben", "Erdbeben", "", Element.Earth, SkillKind.Physical, TargetType.AllEnemies, 0.8f, 8);
            AddSkill("fuchsfeuer", "Fuchsfeuer", "", Element.Fire, SkillKind.Magical, TargetType.SingleEnemy, 1.4f, 6, StatusType.Burn, 3, 0.5f);
            AddSkill("schattenwelle", "Schattenwelle", "", Element.Dark, SkillKind.Magical, TargetType.AllEnemies, 1.1f, 10);
            AddSkill("finsterklinge", "Finsterklinge", "", Element.Dark, SkillKind.Physical, TargetType.SingleEnemy, 1.8f, 8);
            AddSkill("dunkleaura", "Dunkle Aura", "", Element.Dark, SkillKind.Support, TargetType.Self, 0f, 12, StatusType.AttackUp, 4);

            // ---------- Helden ----------
            AddCharacter(new CharacterData
            {
                Id = "akari", Name = "Akari", Title = "Flammenklinge", Element = Element.Fire,
                Color = new Color(0.95f, 0.35f, 0.35f),
                Hp = 120, Mp = 30, Atk = 18, Def = 10, Mag = 8, Spd = 14,
                Skills = new[] { "flammenhieb", "kampfgeist" }, BurstSkill = "phoenixtanz"
            });
            AddCharacter(new CharacterData
            {
                Id = "ren", Name = "Ren", Title = "Sturmweiser", Element = Element.Wind,
                Color = new Color(0.35f, 0.85f, 0.75f),
                Hp = 85, Mp = 60, Atk = 8, Def = 7, Mag = 20, Spd = 16,
                Skills = new[] { "windklinge", "orkan", "sturmbarriere" }, BurstSkill = "himmelsriss"
            });
            AddCharacter(new CharacterData
            {
                Id = "yuki", Name = "Yuki", Title = "Mondpriesterin", Element = Element.Light,
                Color = new Color(0.85f, 0.80f, 1f),
                Hp = 95, Mp = 70, Atk = 7, Def = 9, Mag = 17, Spd = 12,
                Skills = new[] { "heilung", "lichtsegen", "heiligerstrahl" }, BurstSkill = "mondwiedergeburt"
            });

            // ---------- Gegner ----------
            AddEnemy("schleim", "Waldschleim", Element.Water, 60, 10, 10, 6, 5, 8, 12, "attack", "blubberschlag");
            AddEnemy("kobold", "Schattenkobold", Element.Dark, 70, 15, 13, 7, 6, 15, 16, "attack", "hinterhalt");
            AddEnemy("golem", "Steingolem", Element.Earth, 160, 20, 16, 18, 5, 5, 30, "attack", "erdbeben");
            AddEnemy("fuchs", "Irrlicht-Fuchs", Element.Fire, 90, 30, 9, 9, 15, 18, 22, "attack", "fuchsfeuer");
            AddEnemy("noctis", "Dunkelfürstin Noctis", Element.Dark, 430, 80, 18, 13, 16, 13, 200,
                "attack", "schattenwelle", "finsterklinge", "dunkleaura");
            characters["noctis"].IsBoss = true;

            // ---------- Kapitel ----------
            AddStage("Flüsterwald", 1, new Color(0.35f, 0.65f, 0.55f), new Color(0.05f, 0.15f, 0.12f), "schleim", "schleim");
            AddStage("Tiefer Flüsterwald", 2, new Color(0.25f, 0.45f, 0.40f), new Color(0.03f, 0.10f, 0.10f), "schleim", "kobold", "schleim");
            AddStage("Kristallhöhle", 3, new Color(0.35f, 0.45f, 0.80f), new Color(0.05f, 0.05f, 0.20f), "golem", "kobold");
            AddStage("Fuchsschrein", 4, new Color(0.95f, 0.55f, 0.45f), new Color(0.25f, 0.05f, 0.15f), "fuchs", "kobold", "fuchs");
            AddContent();
            AddStage("Turm der Finsternis", 5, new Color(0.40f, 0.15f, 0.55f), new Color(0.02f, 0.00f, 0.06f), "noctis");
        }

        static void AddContent()
        {
            // ---------- Ausrüstung ----------
            AddItem("bronzeklinge", "Bronzeklinge", EquipSlot.Weapon, 60, atk: 4);
            AddItem("eichenstab", "Eichenstab", EquipSlot.Weapon, 60, mag: 4, mp: 5);
            AddItem("drachenzahn", "Drachenzahn", EquipSlot.Weapon, 220, atk: 9, spd: 1);
            AddItem("sternenzepter", "Sternenzepter", EquipSlot.Weapon, 240, mag: 9, mp: 10);
            AddItem("lederweste", "Lederweste", EquipSlot.Armor, 50, def: 3, hp: 15);
            AddItem("seidenrobe", "Seidenrobe", EquipSlot.Armor, 70, def: 2, mp: 10, mag: 2);
            AddItem("mithrilmantel", "Mithrilmantel", EquipSlot.Armor, 210, def: 7, hp: 30);
            AddItem("federamulett", "Federamulett", EquipSlot.Accessory, 90, spd: 3);
            AddItem("glutherz", "Glutherz", EquipSlot.Accessory, 150, hp: 40, atk: 2);
            AddItem("mondtraene", "Mondträne", EquipSlot.Accessory, 160, mp: 15, mag: 3);

            // ---------- Geschichte ----------
            intros[0] = new[]
            {
                new StoryLine("", "Seit sieben Nächten verblassen die Sterne über dem Königreich Luminara ..."),
                new StoryLine("yuki", "Das Mondorakel hat gesprochen. Die Dunkelfürstin Noctis ist aus ihrem Schlaf erwacht."),
                new StoryLine("akari", "Dann holen wir uns das Sternenlicht eben zurück! Meine Klinge glüht schon."),
                new StoryLine("ren", "Langsam, Akari. Der Weg zum Turm führt durch den Flüsterwald. Und der ist voller Schleime."),
                new StoryLine("akari", "Umso besser. Ein kleines Aufwärmtraining!")
            };
            outros[0] = new[]
            {
                new StoryLine("ren", "Hm. Die Schleime waren unruhig. Etwas treibt sie aus dem Wald hinaus."),
                new StoryLine("yuki", "Ich spüre eine kalte Präsenz tiefer im Wald. Lasst uns vorsichtig sein.")
            };
            intros[1] = new[]
            {
                new StoryLine("akari", "Habt ihr das gesehen? Da hat sich gerade ein Schatten bewegt!"),
                new StoryLine("ren", "Schattenkobolde. Sie dienen Noctis als Späher."),
                new StoryLine("yuki", "Wenn sie uns entdeckt haben, weiß die Fürstin bald, dass wir kommen.")
            };
            outros[1] = new[]
            {
                new StoryLine("yuki", "Der Kobold trug einen Kristallsplitter bei sich. Er leuchtet ganz schwach."),
                new StoryLine("ren", "Kristall aus der Höhle im Norden. Dort muss ihr Nachschub herkommen.")
            };
            intros[2] = new[]
            {
                new StoryLine("", "Die Kristallhöhle glitzert in tausend Farben. Doch etwas Gewaltiges versperrt den Weg."),
                new StoryLine("akari", "Ist das ... ein Felsen mit Augen?"),
                new StoryLine("ren", "Ein Steingolem. Wind und Magie sind seine Schwäche, rohe Kraft prallt an ihm ab."),
                new StoryLine("akari", "Rohe Kraft ist aber meine Spezialität!")
            };
            outros[2] = new[]
            {
                new StoryLine("ren", "Der Golem wurde mit dunkler Magie gebunden. Noctis wird stärker."),
                new StoryLine("yuki", "Hinter der Höhle liegt der alte Fuchsschrein. Dort lebten einst die Hüter des Sternenlichts.")
            };
            intros[3] = new[]
            {
                new StoryLine("yuki", "Der Schrein ... er brennt! Die Irrlicht-Füchse waren immer friedlich!"),
                new StoryLine("ren", "Ihre Augen sind leer. Noctis hat ihren Willen gestohlen."),
                new StoryLine("akari", "Wir besiegen sie, ohne aufzugeben. Danach befreien wir sie!"),
                new StoryLine("yuki", "Vorsicht vor ihrem Fuchsfeuer, es hinterlässt brennende Wunden.")
            };
            outros[3] = new[]
            {
                new StoryLine("", "Als die Füchse fallen, löst sich ein dunkler Nebel von ihnen. Sie fliehen in den Wald, wieder frei."),
                new StoryLine("akari", "Seht! Am Horizont, der schwarze Turm. Dort wartet sie."),
                new StoryLine("yuki", "Wir sollten uns gut vorbereiten. Ausrüstung, Training ... das wird der härteste Kampf.")
            };
            intros[4] = new[]
            {
                new StoryLine("", "Die Spitze des Turms der Finsternis. Ein Thron aus schwarzem Kristall."),
                new StoryLine("noctis", "Wie niedlich. Drei kleine Sterne, die glauben, sie könnten die Nacht vertreiben."),
                new StoryLine("akari", "Gib Luminara das Sternenlicht zurück, Noctis!"),
                new StoryLine("noctis", "Das Licht? Ich habe es verschlungen. Und ihr werdet die Nächsten sein."),
                new StoryLine("ren", "Ihr Element ist Dunkelheit. Yukis Licht wird sie am härtesten treffen."),
                new StoryLine("yuki", "Ich bin bereit. Gemeinsam!")
            };
            outros[4] = new[]
            {
                new StoryLine("noctis", "Unmöglich ... dieses Licht ... es ist zu hell ..."),
                new StoryLine("", "Noctis zerfällt zu schwarzem Staub. Über Luminara entzünden sich die Sterne wieder, einer nach dem anderen."),
                new StoryLine("akari", "Wir haben es geschafft!"),
                new StoryLine("ren", "Spürt ihr das? Ihre Dunkelheit ist nicht ganz verschwunden. Sie wird zurückkehren, stärker als zuvor."),
                new StoryLine("yuki", "Dann werden wir auch stärker. Das Abenteuer geht weiter!")
            };
        }

        static void AddItem(string id, string name, EquipSlot slot, int price,
            int hp = 0, int mp = 0, int atk = 0, int def = 0, int mag = 0, int spd = 0)
        {
            items.Add(new ItemData { Id = id, Name = name, Slot = slot, Price = price, Hp = hp, Mp = mp, Atk = atk, Def = def, Mag = mag, Spd = spd });
        }

        public static ItemData GetItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return items.Find(i => i.Id == id);
        }

        /// <summary>Story vor dem ersten Kampf eines Kapitels (nur im ersten Durchlauf).</summary>
        public static StoryLine[] GetIntro(int stageIndex)
        {
            StoryLine[] s;
            return stageIndex < stages.Count && intros.TryGetValue(stageIndex, out s) ? s : null;
        }

        public static StoryLine[] GetOutro(int stageIndex)
        {
            StoryLine[] s;
            return stageIndex < stages.Count && outros.TryGetValue(stageIndex, out s) ? s : null;
        }

        public static int GoldFor(BattleUnit enemy) => Mathf.RoundToInt(XpFor(enemy) * 1.3f);

        /// <summary>Erstellt einen Helden mit Level und angelegter Ausrüstung aus dem Spielstand.</summary>
        public static BattleUnit CreateHero(SaveData save, string id)
        {
            var p = save.GetHero(id);
            var unit = new BattleUnit(GetCharacter(id), p.level, true);
            foreach (var itemId in new[] { p.weapon, p.armor, p.accessory })
            {
                var item = GetItem(itemId);
                if (item != null) unit.ApplyBonus(item);
            }
            return unit;
        }

        static void AddSkill(string id, string name, string desc, Element el, SkillKind kind, TargetType target,
            float power, int mp, StatusType status = StatusType.None, int turns = 0, float chance = 1f,
            bool burst = false, bool revives = false)
        {
            skills[id] = new SkillData
            {
                Id = id, Name = name, Description = desc, Element = el, Kind = kind, Target = target,
                Power = power, MpCost = mp, Status = status, StatusTurns = turns, StatusChance = chance,
                IsBurst = burst, Revives = revives
            };
        }

        static void AddCharacter(CharacterData c) { characters[c.Id] = c; }

        static void AddEnemy(string id, string name, Element el, int hp, int mp, int atk, int def, int mag, int spd,
            int xp, params string[] skillIds)
        {
            AddCharacter(new CharacterData
            {
                Id = id, Name = name, Title = "", Element = el, Color = ElementInfo.GetColor(el),
                Hp = hp, Mp = mp, Atk = atk, Def = def, Mag = mag, Spd = spd, XpReward = xp, Skills = skillIds
            });
        }

        static void AddStage(string name, int level, Color top, Color bottom, params string[] enemies)
        {
            stages.Add(new StageData { Name = name, EnemyLevel = level, SkyTop = top, SkyBottom = bottom, Enemies = enemies });
        }

        public static SkillData GetSkill(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            SkillData s;
            return skills.TryGetValue(id, out s) ? s : null;
        }

        public static CharacterData GetCharacter(string id)
        {
            CharacterData c;
            return characters.TryGetValue(id, out c) ? c : null;
        }

        public static int StageCount => stages.Count;

        /// <summary>Nach dem letzten Kapitel beginnt ein schwierigerer Durchlauf (+5 Gegnerlevel pro Runde).</summary>
        public static StageData GetStage(int index, out int levelBonus)
        {
            index = Mathf.Max(0, index);
            levelBonus = (index / stages.Count) * 5;
            return stages[index % stages.Count];
        }

        public static int XpFor(BattleUnit enemy)
        {
            return Mathf.RoundToInt(enemy.Data.XpReward * (1f + 0.25f * (enemy.Level - 1)));
        }
    }
}
