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
            AddStage("Turm der Finsternis", 5, new Color(0.40f, 0.15f, 0.55f), new Color(0.02f, 0.00f, 0.06f), "noctis");
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
