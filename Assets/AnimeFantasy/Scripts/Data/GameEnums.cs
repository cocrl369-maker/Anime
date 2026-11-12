using UnityEngine;

namespace AnimeFantasy
{
    public enum Element { Neutral, Fire, Water, Wind, Earth, Light, Dark }
    public enum SkillKind { Physical, Magical, Heal, Support }
    public enum TargetType { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }
    public enum StatusType { None, AttackUp, DefenseUp, Burn, Regen }

    /// <summary>Elementnamen, Farben und das Stärke/Schwäche-System.</summary>
    public static class ElementInfo
    {
        public static string Name(Element e)
        {
            switch (e)
            {
                case Element.Fire: return "Feuer";
                case Element.Water: return "Wasser";
                case Element.Wind: return "Wind";
                case Element.Earth: return "Erde";
                case Element.Light: return "Licht";
                case Element.Dark: return "Dunkel";
                default: return "Neutral";
            }
        }

        public static Color GetColor(Element e)
        {
            switch (e)
            {
                case Element.Fire: return new Color(1f, 0.45f, 0.25f);
                case Element.Water: return new Color(0.30f, 0.60f, 1f);
                case Element.Wind: return new Color(0.40f, 0.95f, 0.60f);
                case Element.Earth: return new Color(0.75f, 0.55f, 0.30f);
                case Element.Light: return new Color(1f, 0.92f, 0.50f);
                case Element.Dark: return new Color(0.60f, 0.35f, 0.90f);
                default: return new Color(0.80f, 0.80f, 0.85f);
            }
        }

        /// <summary>
        /// Feuer > Wind > Erde > Wasser > Feuer. Licht und Dunkel sind gegenseitig stark.
        /// </summary>
        public static float Multiplier(Element attack, Element defend)
        {
            if (attack == Element.Neutral || defend == Element.Neutral) return 1f;
            if ((attack == Element.Light && defend == Element.Dark) ||
                (attack == Element.Dark && defend == Element.Light)) return 1.5f;
            if (Beats(attack, defend)) return 1.5f;
            if (Beats(defend, attack)) return 0.75f;
            return 1f;
        }

        static bool Beats(Element a, Element b)
        {
            return (a == Element.Fire && b == Element.Wind) ||
                   (a == Element.Wind && b == Element.Earth) ||
                   (a == Element.Earth && b == Element.Water) ||
                   (a == Element.Water && b == Element.Fire);
        }
    }

    public static class StatusInfo
    {
        public static string ShortName(StatusType s)
        {
            switch (s)
            {
                case StatusType.AttackUp: return "ATK+";
                case StatusType.DefenseUp: return "DEF+";
                case StatusType.Burn: return "Brand";
                case StatusType.Regen: return "Regen";
                default: return "";
            }
        }

        public static Color GetColor(StatusType s)
        {
            switch (s)
            {
                case StatusType.AttackUp: return new Color(1f, 0.55f, 0.45f);
                case StatusType.DefenseUp: return new Color(0.55f, 0.75f, 1f);
                case StatusType.Burn: return new Color(1f, 0.6f, 0.2f);
                case StatusType.Regen: return new Color(0.5f, 1f, 0.6f);
                default: return Color.white;
            }
        }
    }
}
