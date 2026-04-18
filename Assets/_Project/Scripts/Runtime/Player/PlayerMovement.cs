using UnityEngine;
using UnityEngine.InputSystem;

namespace APEX.Player
{
    /// <summary>
    /// Reads the Gameplay/Move action and translates the player's Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;

        private APEXControls _controls;
        private InputAction _moveAction;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _controls = new APEXControls();
            _moveAction = _controls.Gameplay.Move;
        }

        private void OnEnable() => _controls.Gameplay.Enable();
        private void OnDisable() => _controls.Gameplay.Disable();
        private void OnDestroy() => _controls.Dispose();

        private void FixedUpdate()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();

            if (input.sqrMagnitude > 1f)
                input.Normalize();

            _rb.linearVelocity = input * moveSpeed;
        }
    }
}
