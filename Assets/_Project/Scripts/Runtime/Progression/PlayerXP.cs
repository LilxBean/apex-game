using APEX.Core.Events;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Player's XP and level state. Fires EventBus.OnXPGained and OnLevelUp.
    /// Apex Hunger and similar XP multipliers apply here.
    /// </summary>
    public class PlayerXP : MonoBehaviour
    {
        [SerializeField] private XPCurve _curve;

        private int _level = 1;
        private int _xpIntoLevel;
        private float _xpMultiplier = 1f;

        public int Level => _level;
        public int XPIntoLevel => _xpIntoLevel;
        public int XPToNextLevel => _curve != null ? _curve.GetXPForLevel(_level) : int.MaxValue;
        public float LevelFraction
        {
            get
            {
                int need = XPToNextLevel;
                return need <= 0 ? 0f : Mathf.Clamp01((float)_xpIntoLevel / need);
            }
        }

        public XPCurve Curve
        {
            get => _curve;
            set => _curve = value;
        }

        public void AddXPMultiplier(float mult) => _xpMultiplier *= mult;

        public void GainXP(int rawAmount)
        {
            if (rawAmount <= 0 || _curve == null) return;
            int amount = Mathf.Max(1, Mathf.RoundToInt(rawAmount * _xpMultiplier));
            _xpIntoLevel += amount;
            EventBus.RaiseXPGained(amount, _xpIntoLevel, XPToNextLevel, LevelFraction);

            // Cascade level-ups if a big drop pushed us past multiple thresholds.
            while (_level < _curve.maxLevel)
            {
                int need = _curve.GetXPForLevel(_level);
                if (_xpIntoLevel < need) break;
                _xpIntoLevel -= need;
                _level++;
                EventBus.RaiseLevelUp(_level);
            }
        }
    }
}
