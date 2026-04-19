using UnityEngine;

namespace APEX.Progression
{
    public enum Rarity
    {
        Common,
        Rare,
        Epic
    }

    public static class RarityColors
    {
        public static Color For(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => new Color(0.82f, 0.82f, 0.82f),
                Rarity.Rare => new Color(0.40f, 0.70f, 1.00f),
                Rarity.Epic => new Color(0.80f, 0.45f, 1.00f),
                _ => Color.white
            };
        }
    }
}
