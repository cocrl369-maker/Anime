using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnimeFantasy
{
    public class ActionResult
    {
        public BattleUnit Target;
        public int Amount;
        public bool IsHeal;
        public bool IsDamage;
        public bool Crit;
        public float Effectiveness = 1f;
        public StatusType StatusApplied = StatusType.None;
        public bool Revived;
        public bool Killed;
    }

    /// <summary>Reine Kampfregeln ohne UI: Schaden, Heilung, Ziele.</summary>
    public static class BattleLogic
    {
        public const float CritChance = 0.1f;

        public static List<BattleUnit> Candidates(BattleUnit user, SkillData skill, List<BattleUnit> allies, List<BattleUnit> opponents)
        {
            switch (skill.Target)
            {
                case TargetType.SingleEnemy:
                case TargetType.AllEnemies:
                    return opponents.Where(o => o.IsAlive).ToList();
                case TargetType.SingleAlly:
                case TargetType.AllAllies:
                    return allies.Where(a => a.IsAlive || skill.Revives).ToList();
                default:
                    return new List<BattleUnit> { user };
            }
        }

        public static List<ActionResult> Execute(BattleUnit user, SkillData skill, List<BattleUnit> targets)
        {
            var results = new List<ActionResult>();
            user.Mp = Mathf.Max(0, user.Mp - skill.MpCost);
            if (skill.IsBurst) user.Burst = 0;

            foreach (var t in targets)
            {
                if (!t.IsAlive && !skill.Revives) continue;
                var r = new ActionResult { Target = t };

                switch (skill.Kind)
                {
                    case SkillKind.Heal:
                    {
                        if (!t.IsAlive) r.Revived = true;
                        int heal = Mathf.RoundToInt(skill.Power * user.EffectiveMag * 1.5f * Random.Range(0.95f, 1.05f));
                        heal = Mathf.Min(Mathf.Max(1, heal), t.MaxHp - t.Hp);
                        t.Hp += heal;
                        r.Amount = heal;
                        r.IsHeal = true;
                        user.GainBurst(6);
                        break;
                    }
                    case SkillKind.Support:
                        break;
                    default:
                    {
                        bool crit;
                        float eff;
                        int dmg = CalculateDamage(user, skill, t, out crit, out eff);
                        t.Hp = Mathf.Max(0, t.Hp - dmg);
                        r.Amount = dmg;
                        r.IsDamage = true;
                        r.Crit = crit;
                        r.Effectiveness = eff;
                        r.Killed = !t.IsAlive;
                        if (!skill.IsBurst) user.GainBurst(8);
                        t.GainBurst(12);
                        if (r.Killed) t.Statuses.Clear();
                        break;
                    }
                }

                if (skill.Status != StatusType.None && t.IsAlive && Random.value <= skill.StatusChance)
                {
                    t.AddStatus(skill.Status, skill.StatusTurns);
                    r.StatusApplied = skill.Status;
                }
                results.Add(r);
            }
            return results;
        }

        public static int CalculateDamage(BattleUnit user, SkillData skill, BattleUnit target, out bool crit, out float effectiveness)
        {
            bool magical = skill.Kind == SkillKind.Magical;
            float stat = magical ? user.EffectiveMag : user.EffectiveAtk;
            float def = target.EffectiveDef * (magical ? 0.7f : 1f);

            float dmg = skill.Power * stat * 2.2f;
            dmg *= 40f / (40f + def);
            effectiveness = ElementInfo.Multiplier(skill.Element, target.Data.Element);
            dmg *= effectiveness;
            dmg *= Random.Range(0.9f, 1.1f);

            crit = Random.value < CritChance;
            if (crit) dmg *= 1.5f;
            if (target.Defending) dmg *= 0.5f;

            return Mathf.Max(1, Mathf.RoundToInt(dmg));
        }
    }
}
