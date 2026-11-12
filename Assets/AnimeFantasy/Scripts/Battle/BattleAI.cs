using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnimeFantasy
{
    /// <summary>Einfache KI für Gegner und für den Auto-Kampf der Helden.</summary>
    public static class BattleAI
    {
        public static void Choose(BattleUnit u, List<BattleUnit> allies, List<BattleUnit> opponents,
            out SkillData skill, out List<BattleUnit> targets)
        {
            var usable = u.Data.Skills
                .Select(GameDatabase.GetSkill)
                .Where(s => s != null && s.MpCost <= u.Mp)
                .ToList();
            var aliveOpp = opponents.Where(o => o.IsAlive).ToList();

            // 1) Burst
            if (u.BurstReady)
            {
                var burst = GameDatabase.GetSkill(u.Data.BurstSkill);
                if (burst != null && BurstWorthIt(burst, allies))
                {
                    skill = burst;
                    targets = BattleLogic.Candidates(u, burst, allies, opponents);
                    return;
                }
            }

            // 2) Heilen
            var wounded = allies.Where(a => a.IsAlive && a.HpRatio < 0.5f).OrderBy(a => a.HpRatio).ToList();
            if (wounded.Count > 0)
            {
                var groupHeal = usable.FirstOrDefault(s => s.Kind == SkillKind.Heal && s.Target == TargetType.AllAllies);
                var singleHeal = usable.FirstOrDefault(s => s.Kind == SkillKind.Heal && s.Target == TargetType.SingleAlly);
                if (wounded.Count >= 2 && groupHeal != null)
                {
                    skill = groupHeal;
                    targets = BattleLogic.Candidates(u, groupHeal, allies, opponents);
                    return;
                }
                if (singleHeal != null)
                {
                    skill = singleHeal;
                    targets = new List<BattleUnit> { wounded[0] };
                    return;
                }
            }

            // 3) Unterstützung (nur wenn der Effekt noch nicht aktiv ist)
            var support = usable.Where(s => s.Kind == SkillKind.Support &&
                                            !(s.Target == TargetType.Self ? u.Has(s.Status) : allies.Any(a => a.IsAlive && a.Has(s.Status))))
                                .ToList();
            if (support.Count > 0 && Random.value < 0.3f)
            {
                skill = support[Random.Range(0, support.Count)];
                targets = BattleLogic.Candidates(u, skill, allies, opponents);
                return;
            }

            // 4) Angriff
            var attacks = usable.Where(s => s.IsDamage).ToList();
            var aoe = attacks.Where(s => s.Target == TargetType.AllEnemies).ToList();
            var single = attacks.Where(s => s.Target == TargetType.SingleEnemy && s.Id != "attack").ToList();

            if (aoe.Count > 0 && aliveOpp.Count >= 2 && Random.value < 0.5f)
            {
                skill = aoe[Random.Range(0, aoe.Count)];
                targets = aliveOpp;
                return;
            }

            skill = (single.Count > 0 && Random.value < 0.6f) ? single[Random.Range(0, single.Count)] : GameDatabase.BasicAttack;
            targets = new List<BattleUnit> { PickTarget(u, skill, aliveOpp) };
        }

        static bool BurstWorthIt(SkillData burst, List<BattleUnit> allies)
        {
            if (burst.Kind != SkillKind.Heal) return true;
            return allies.Any(a => !a.IsAlive || a.HpRatio < 0.6f);
        }

        static BattleUnit PickTarget(BattleUnit u, SkillData skill, List<BattleUnit> aliveOpp)
        {
            if (u.IsPlayer)
            {
                // Helden: Elementvorteil zuerst, dann der schwächste Gegner
                return aliveOpp
                    .OrderByDescending(o => ElementInfo.Multiplier(skill.Element, o.Data.Element))
                    .ThenBy(o => o.Hp)
                    .First();
            }
            // Gegner: teils gezielt auf den Schwächsten, teils zufällig
            if (Random.value < 0.4f) return aliveOpp.OrderBy(o => o.HpRatio).First();
            return aliveOpp[Random.Range(0, aliveOpp.Count)];
        }
    }
}
