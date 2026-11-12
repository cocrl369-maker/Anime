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
