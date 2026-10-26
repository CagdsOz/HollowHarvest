using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowHarvest
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        private Rigidbody2D body;
        private InputAction move;
        private InputActionAsset ownedActions;
        private Vector2 direction;
        private readonly HashSet<Object> movementLocks = new HashSet<Object>();
        public bool IsMovementLocked => movementLocks.Count > 0;

        public void SetMovementLocked(Object owner, bool locked)
        {
            if (locked) movementLocks.Add(owner);
            else movementLocks.Remove(owner);
            if (IsMovementLocked)
            {
                direction = Vector2.zero;
                if (body != null) body.linearVelocity = Vector2.zero;
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            // Own the actions so disabling this player cannot disable another consumer.
            if (inputActions == null)
            {
                Debug.LogError("Assign the project's InputSystem_Actions asset.", this);
                enabled = false;
                return;
            }
            ownedActions = Instantiate(inputActions);
            move = ownedActions.FindAction("Player/Move", true);
        }

        private void OnEnable() => move?.Enable();
        private void Update() => direction = IsMovementLocked ? Vector2.zero
            : Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);
        private void FixedUpdate() => body.linearVelocity = IsMovementLocked ? Vector2.zero : direction * moveSpeed;

        private void OnDisable()
        {
            move?.Disable();
            direction = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (ownedActions != null) Destroy(ownedActions);
        }
    }
}
