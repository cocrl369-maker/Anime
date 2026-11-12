using System.Collections.Generic;
using UnityEngine;

namespace AnimeFantasy
{
    public class StatusEffect
    {
        public StatusType Type;
        public int Turns;
        public StatusEffect(StatusType type, int turns) { Type = type; Turns = turns; }
    }

    /// <summary>Laufzeit-Zustand eines Kämpfers (Held oder Gegner) in einem Kampf.</summary>
    public class BattleUnit
    {
        public readonly CharacterData Data;
        public readonly int Level;
        public readonly bool IsPlayer;

        public int MaxHp, Hp, MaxMp, Mp, Atk, Def, Mag, Spd;
        public int Burst; // 0..100, nur Helden
        public bool Defending;
        public readonly List<StatusEffect> Statuses = new List<StatusEffect>();

        public BattleUnit(CharacterData data, int level, bool isPlayer)
        {
            Data = data;
            Level = Mathf.Max(1, level);
            IsPlayer = isPlayer;
            float m = 1f + 0.12f * (Level - 1);
            MaxHp = Hp = Mathf.RoundToInt(data.Hp * m);
            MaxMp = Mp = Mathf.RoundToInt(data.Mp * m);
            Atk = Mathf.RoundToInt(data.Atk * m);
            Def = Mathf.RoundToInt(data.Def * m);
            Mag = Mathf.RoundToInt(data.Mag * m);
            Spd = Mathf.RoundToInt(data.Spd * m);
        }

        public string Name => Data.Name;
        public bool IsAlive => Hp > 0;
        public bool BurstReady => IsPlayer && IsAlive && Burst >= 100 && !string.IsNullOrEmpty(Data.BurstSkill);
        public float HpRatio => MaxHp > 0 ? (float)Hp / MaxHp : 0f;

        public bool Has(StatusType t) => Statuses.Exists(s => s.Type == t);

        public float EffectiveAtk => Atk * (Has(StatusType.AttackUp) ? 1.3f : 1f);
        public float EffectiveMag => Mag * (Has(StatusType.AttackUp) ? 1.3f : 1f);
        public float EffectiveDef => Def * (Has(StatusType.DefenseUp) ? 1.4f : 1f);

        public void AddStatus(StatusType type, int turns)
        {
            var s = Statuses.Find(x => x.Type == type);
            if (s != null) s.Turns = Mathf.Max(s.Turns, turns);
            else Statuses.Add(new StatusEffect(type, turns));
        }

        public void GainBurst(int amount)
        {
            if (!IsPlayer || !IsAlive) return;
            Burst = Mathf.Clamp(Burst + amount, 0, 100);
        }
    }
}
