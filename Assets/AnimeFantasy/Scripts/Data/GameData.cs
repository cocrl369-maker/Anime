using UnityEngine;

namespace AnimeFantasy
{
    public class SkillData
    {
        public string Id;
        public string Name;
        public string Description;
        public Element Element;
        public SkillKind Kind;
        public TargetType Target;
        public float Power;
        public int MpCost;
        public bool IsBurst;
        public bool Revives;
        public StatusType Status = StatusType.None;
        public int StatusTurns;
        public float StatusChance = 1f;

        public bool IsSingleTarget => Target == TargetType.SingleEnemy || Target == TargetType.SingleAlly;
        public bool IsDamage => Kind == SkillKind.Physical || Kind == SkillKind.Magical;
    }

    public class CharacterData
    {
        public string Id;
        public string Name;
        public string Title;
        public Element Element;
        public Color Color;
        public int Hp, Mp, Atk, Def, Mag, Spd;
        public string[] Skills;
        public string BurstSkill;
        public int XpReward;
        public bool IsBoss;
    }

    public class StageData
    {
        public string Name;
        public string[] Enemies;
        public int EnemyLevel;
        public Color SkyTop;
        public Color SkyBottom;
    }
}

namespace AnimeFantasy
{
    public enum EquipSlot { Weapon, Armor, Accessory }

    /// <summary>Ausrüstung: jedes Item existiert einmal und kann von einem Helden getragen werden.</summary>
    public class ItemData
    {
        public string Id;
        public string Name;
        public EquipSlot Slot;
        public int Price;
        public int Hp, Mp, Atk, Def, Mag, Spd;

        public string StatText()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (Hp != 0) parts.Add("HP+" + Hp);
            if (Mp != 0) parts.Add("MP+" + Mp);
            if (Atk != 0) parts.Add("ATK+" + Atk);
            if (Def != 0) parts.Add("DEF+" + Def);
            if (Mag != 0) parts.Add("MAG+" + Mag);
            if (Spd != 0) parts.Add("TMP+" + Spd);
            return string.Join("  ", parts.ToArray());
        }

        public static string SlotName(EquipSlot s)
        {
            switch (s)
            {
                case EquipSlot.Weapon: return "Waffe";
                case EquipSlot.Armor: return "Rüstung";
                default: return "Accessoire";
            }
        }
    }

    public class StoryLine
    {
        public string Speaker; // Charakter-ID, leer = Erzähler
        public string Text;
        public StoryLine(string speaker, string text) { Speaker = speaker; Text = text; }
    }
}
