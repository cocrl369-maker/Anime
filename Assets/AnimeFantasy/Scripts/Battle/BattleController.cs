using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnimeFantasy
{
    /// <summary>
    /// Steuert den Ablauf eines Kampfes: Zugreihenfolge nach Tempo, Spielereingaben,
    /// Gegner-KI, Statuseffekte und Sieg/Niederlage.
    /// </summary>
    public class BattleController : MonoBehaviour
    {
        public bool AutoMode;

        BattleView view;
        List<BattleUnit> party;
        List<BattleUnit> enemies;
        Action<bool, int> onFinished;
        bool over;
        bool victory;
        int round;

        public void Begin(BattleView battleView, BattleStage stage3d, string title,
            List<BattleUnit> heroes, List<BattleUnit> foes, Action<bool, int> finished)
        {
            view = battleView;
            party = heroes;
            enemies = foes;
            onFinished = finished;
            view.Build(this, title, party, enemies, stage3d);
            StartCoroutine(Run());
        }

        static WaitForSeconds Wait(float s) => new WaitForSeconds(s);

        IEnumerator Run()
        {
            var names = string.Join(", ", enemies.Select(e => e.Name).Distinct().ToArray());
            view.Log(names + " erscheint!");
            yield return Wait(1.0f);

            while (!over)
            {
                round++;
                view.SetRound(round);

                var order = party.Concat(enemies)
                    .Where(u => u.IsAlive)
                    .OrderByDescending(u => u.Spd + UnityEngine.Random.Range(0f, 4f))
                    .ToList();

                foreach (var u in order)
                {
                    if (over) break;
                    if (!u.IsAlive) continue;

                    u.Defending = false;
                    yield return TickStatuses(u);
                    if (CheckEnd()) break;
                    if (!u.IsAlive) continue;

                    view.SetActive(u);
                    if (u.IsPlayer && !AutoMode) yield return PlayerTurn(u);
                    else yield return AiTurn(u);
                    view.SetActive(null);
                    view.RefreshAll();

                    if (CheckEnd()) break;
                    yield return Wait(0.2f);
                }
            }

            view.ClearCommands();
            view.Log(victory ? "Sieg!" : "Die Gruppe ist gefallen ...");
            if (victory) view.Victory(party);
            yield return Wait(1.0f);

            int xp = victory ? enemies.Sum(e => GameDatabase.XpFor(e)) : 0;
            if (onFinished != null) onFinished(victory, xp);
        }

        bool CheckEnd()
        {
            if (enemies.All(e => !e.IsAlive)) { over = true; victory = true; }
            else if (party.All(p => !p.IsAlive)) { over = true; victory = false; }
            return over;
        }

        List<BattleUnit> AlliesOf(BattleUnit u) => u.IsPlayer ? party : enemies;
        List<BattleUnit> OpponentsOf(BattleUnit u) => u.IsPlayer ? enemies : party;

        IEnumerator TickStatuses(BattleUnit u)
        {
            bool anything = false;
            foreach (var s in u.Statuses.ToList())
            {
                if (s.Type == StatusType.Burn)
                {
                    int dmg = Mathf.Max(1, Mathf.RoundToInt(u.MaxHp * 0.06f));
                    u.Hp = Mathf.Max(0, u.Hp - dmg);
                    view.Popup(u, "-" + dmg, StatusInfo.GetColor(StatusType.Burn), 52);
                    view.Log(u.Name + " erleidet Brandschaden.");
                    anything = true;
                }
                else if (s.Type == StatusType.Regen && u.Hp < u.MaxHp)
                {
                    int heal = Mathf.Min(u.MaxHp - u.Hp, Mathf.Max(1, Mathf.RoundToInt(u.MaxHp * 0.08f)));
                    u.Hp += heal;
                    view.Popup(u, "+" + heal, StatusInfo.GetColor(StatusType.Regen), 52);
                    anything = true;
                }

                s.Turns--;
                if (s.Turns <= 0) u.Statuses.Remove(s);
            }
            if (!u.IsAlive) u.Statuses.Clear();
            view.RefreshUnit(u);
            if (anything) yield return Wait(0.6f);
        }

        IEnumerator PlayerTurn(BattleUnit u)
        {
            while (true)
            {
                PlayerCommand cmd = null;
                view.ShowCommandMenu(u, c => cmd = c);
                yield return new WaitUntil(() => cmd != null || AutoMode);

                if (cmd == null)
                {
                    // Spieler hat währenddessen Auto eingeschaltet
                    view.ClearCommands();
                    yield return AiTurn(u);
                    yield break;
                }

                if (cmd.Defend)
                {
                    view.ClearCommands();
                    u.Defending = true;
                    u.GainBurst(10);
                    view.Log(u.Name + " geht in Verteidigungshaltung.");
                    view.Popup(u, "Verteidigung", new Color(0.6f, 0.8f, 1f), 44);
                    view.RefreshUnit(u);
                    yield return Wait(0.6f);
                    yield break;
                }

                var skill = cmd.Skill;
                List<BattleUnit> targets;
                var candidates = BattleLogic.Candidates(u, skill, AlliesOf(u), OpponentsOf(u));

                if (skill.IsSingleTarget)
                {
                    BattleUnit picked = null;
                    bool cancelled = false;
                    view.ShowTargetPrompt(skill, () => cancelled = true);
                    view.BeginTargetSelect(candidates, t => picked = t);
                    yield return new WaitUntil(() => picked != null || cancelled || AutoMode);
                    view.EndTargetSelect();

                    if (picked == null)
                    {
                        if (AutoMode)
                        {
                            view.ClearCommands();
                            yield return AiTurn(u);
                            yield break;
                        }
                        continue; // zurück zum Befehlsmenü
                    }
                    targets = new List<BattleUnit> { picked };
                }
                else
                {
                    targets = candidates;
                }

                view.ClearCommands();
                yield return PerformAction(u, skill, targets);
                yield break;
            }
        }

        IEnumerator AiTurn(BattleUnit u)
        {
            yield return Wait(0.3f);
            SkillData skill;
            List<BattleUnit> targets;
            BattleAI.Choose(u, AlliesOf(u), OpponentsOf(u), out skill, out targets);
            yield return PerformAction(u, skill, targets);
        }

        IEnumerator PerformAction(BattleUnit user, SkillData skill, List<BattleUnit> targets)
        {
            view.Log(skill.Id == "attack" ? user.Name + " greift an!" : user.Name + " setzt " + skill.Name + " ein!");

            if (skill.IsBurst)
            {
                view.ShowBurstBanner(user, skill);
                yield return Wait(1.1f);
            }

            yield return view.AnimateAttack(user, skill, targets);

            var results = BattleLogic.Execute(user, skill, targets);
            foreach (var r in results)
            {
                view.ShowResult(r);
                yield return Wait(results.Count > 1 ? 0.15f : 0.3f);
            }
            view.RefreshUnit(user);

            if (results.Any(r => r.IsDamage && r.Effectiveness > 1f)) view.Log("Sehr effektiv!");
            else if (results.Any(r => r.IsDamage && r.Effectiveness < 1f)) view.Log("Nicht sehr effektiv ...");
            foreach (var r in results.Where(r => r.Killed))
                view.Log(r.Target.Name + " wurde besiegt!");

            yield return Wait(0.5f);
        }
    }
}
