using System.Collections;
using System.Collections.Generic;
using APEX.Combat;
using APEX.Combat.Attacks;
using APEX.Core.Events;
using APEX.Core.Pooling;
using APEX.Enemies;
using APEX.Tags;
using UnityEngine;
using UnityEngine.InputSystem;

namespace APEX.Player
{
    /// <summary>
    /// Owns the player's four attack slots. Ticks cooldowns, routes input to manual fire
    /// and auto-fire toggle, and exposes shared helpers (pool, nearest-enemy, damage resolve).
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private List<AttackDefinition> _attackDefinitions = new();

        private static readonly Collider2D[] _overlapBuffer = new Collider2D[64];

        private PlayerController _controller;
        private PrefabPool _pool;
        private readonly List<IAttackInstance> _instances = new();

        private APEXControls _controls;

        public PlayerController Controller => _controller;
        public PrefabPool Pool => _pool;
        public IReadOnlyList<IAttackInstance> Instances => _instances;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _pool = new PrefabPool();

            _controls = new APEXControls();

            for (int i = 0; i < _attackDefinitions.Count && i < 4; i++)
            {
                var def = _attackDefinitions[i];
                if (def == null)
                {
                    _instances.Add(null);
                    continue;
                }
                _instances.Add(def.CreateInstance(this));
            }
        }

        private void OnEnable()
        {
            _controls.Combat.Enable();
            _controls.Combat.Attack1.performed += OnAttack1;
            _controls.Combat.Attack2.performed += OnAttack2;
            _controls.Combat.Attack3.performed += OnAttack3;
            _controls.Combat.Attack4.performed += OnAttack4;
            _controls.Combat.ToggleAuto1.performed += OnToggle1;
            _controls.Combat.ToggleAuto2.performed += OnToggle2;
            _controls.Combat.ToggleAuto3.performed += OnToggle3;
            _controls.Combat.ToggleAuto4.performed += OnToggle4;
        }

        private void OnDisable()
        {
            _controls.Combat.Attack1.performed -= OnAttack1;
            _controls.Combat.Attack2.performed -= OnAttack2;
            _controls.Combat.Attack3.performed -= OnAttack3;
            _controls.Combat.Attack4.performed -= OnAttack4;
            _controls.Combat.ToggleAuto1.performed -= OnToggle1;
            _controls.Combat.ToggleAuto2.performed -= OnToggle2;
            _controls.Combat.ToggleAuto3.performed -= OnToggle3;
            _controls.Combat.ToggleAuto4.performed -= OnToggle4;
            _controls.Combat.Disable();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _instances.Count; i++)
            {
                _instances[i]?.Cleanup();
            }
            _controls?.Dispose();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _instances.Count; i++)
            {
                _instances[i]?.Tick(dt);
            }
        }

        // --- Input handlers -------------------------------------------------

        private void OnAttack1(InputAction.CallbackContext ctx) => TryManualFire(0);
        private void OnAttack2(InputAction.CallbackContext ctx) => TryManualFire(1);
        private void OnAttack3(InputAction.CallbackContext ctx) => TryManualFire(2);
        private void OnAttack4(InputAction.CallbackContext ctx) => TryManualFire(3);

        private void OnToggle1(InputAction.CallbackContext ctx) => ToggleAutoFire(0);
        private void OnToggle2(InputAction.CallbackContext ctx) => ToggleAutoFire(1);
        private void OnToggle3(InputAction.CallbackContext ctx) => ToggleAutoFire(2);
        private void OnToggle4(InputAction.CallbackContext ctx) => ToggleAutoFire(3);

        private void TryManualFire(int slot)
        {
            // Shift+N is reserved for the toggle action — don't also fire the slot.
            var kb = Keyboard.current;
            if (kb != null && kb.shiftKey.isPressed) return;
            if (slot < 0 || slot >= _instances.Count) return;
            _instances[slot]?.Fire(true);
        }

        private void ToggleAutoFire(int slot)
        {
            if (slot < 0 || slot >= _instances.Count) return;
            var inst = _instances[slot];
            if (inst == null) return;
            inst.AutoFire = !inst.AutoFire;
            Debug.Log($"[Combat] Slot {slot + 1} ({inst.Definition.displayName}) auto={inst.AutoFire}");
        }

        // --- Shared helpers -------------------------------------------------

        /// <summary>Finds the nearest live enemy within radius, or null.</summary>
        public EnemyController FindNearestEnemy(Vector2 origin, float radius)
        {
            int count = Physics2D.OverlapCircleNonAlloc(origin, radius, _overlapBuffer);
            EnemyController nearest = null;
            float nearestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = _overlapBuffer[i];
                if (col == null) continue;
                if (!col.TryGetComponent(out EnemyController enemy)) continue;
                var hp = enemy.GetComponent<Health>();
                if (hp == null || !hp.IsAlive) continue;

                float sqr = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        /// <summary>Builds a Damage payload with stat multipliers applied.</summary>
        public Damage BuildDamage(float baseAmount, TagSet tags, object source)
        {
            float mult = _controller.Stats != null
                ? _controller.Stats.GetMultiplierFor(tags)
                : 1f;
            return new Damage(baseAmount * mult, tags, source);
        }

        /// <summary>Applies damage to a target and raises the player-hit event.</summary>
        public void DealDamage(IDamageable target, float baseAmount, TagSet tags, object source)
        {
            if (target == null) return;
            var dmg = BuildDamage(baseAmount, tags, source);
            target.TakeDamage(dmg);
            EventBus.RaisePlayerHitEnemy(target, dmg);
        }

        /// <summary>Attack instances can schedule work on the player MonoBehaviour.</summary>
        public Coroutine RunCoroutine(IEnumerator routine) => StartCoroutine(routine);

        /// <summary>Mouse in world space, or null if no mouse or camera.</summary>
        public Vector2? TryGetMouseWorldPosition()
        {
            var mouse = Mouse.current;
            var cam = Camera.main;
            if (mouse == null || cam == null) return null;
            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            return new Vector2(world.x, world.y);
        }
    }
}
