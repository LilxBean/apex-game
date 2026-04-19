using System.Collections.Generic;
using APEX.Combat.Attacks;
using APEX.Player;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Tracks which passives and hammers the player has picked this run and applies effects.
    /// Also exposes dedup queries for the pick screen.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(PlayerXP))]
    public class PlayerBuild : MonoBehaviour
    {
        private PlayerController _controller;
        private PlayerCombat _combat;
        private PlayerXP _xp;

        private readonly Dictionary<PassiveDefinition, int> _passiveStacks = new();
        private readonly HashSet<HammerDefinition> _appliedHammers = new();

        public PlayerController Controller => _controller;
        public PlayerCombat Combat => _combat;
        public PlayerXP XP => _xp;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _combat = GetComponent<PlayerCombat>();
            _xp = GetComponent<PlayerXP>();
        }

        public bool CanPick(PassiveDefinition passive)
        {
            if (passive == null) return false;
            int current = _passiveStacks.TryGetValue(passive, out var s) ? s : 0;
            return current < Mathf.Max(1, passive.maxStacks);
        }

        public bool CanPick(HammerDefinition hammer)
        {
            if (hammer == null) return false;
            return !_appliedHammers.Contains(hammer);
        }

        public int GetStacks(PassiveDefinition passive)
        {
            return _passiveStacks.TryGetValue(passive, out var s) ? s : 0;
        }

        public IAttackInstance FindInstanceFor(AttackDefinition def)
        {
            if (def == null) return null;
            var list = _combat.Instances;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].Definition == def) return list[i];
            }
            return null;
        }

        public void ApplyPick(PassiveDefinition passive)
        {
            if (passive == null) return;
            int current = _passiveStacks.TryGetValue(passive, out var s) ? s : 0;
            _passiveStacks[passive] = current + 1;
            PassiveApplier.Apply(passive, this);
        }

        public void ApplyPick(HammerDefinition hammer)
        {
            if (hammer == null) return;
            if (!_appliedHammers.Add(hammer)) return;
            var target = FindInstanceFor(hammer.targetAttack);
            if (target == null)
            {
                Debug.LogWarning($"[PlayerBuild] Hammer '{hammer.displayName}' targets {hammer.targetAttack?.displayName}, but no matching attack instance found.");
                return;
            }
            HammerApplier.Apply(hammer, target, this);
        }
    }
}
