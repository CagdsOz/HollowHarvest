using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HollowHarvest
{
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private CowStatusPanel cowPanel;
        [SerializeField] private BuildingTransition transitions;
        [SerializeField] private Text prompt;
        private InputActionAsset ownedActions;
        private InputAction inspect, door, close;
        private PlayerMovement movement;
        public CowStatusPanel CowPanel => cowPanel;
        public BuildingTransition Transitions => transitions;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            if (inputActions == null || cowPanel == null || transitions == null || prompt == null)
            {
                Debug.LogError("Run HollowHarvest > Add Cow and Building Interactions to connect references.", this);
                enabled = false;
                return;
            }
            ownedActions = Instantiate(inputActions);
            inspect = ownedActions.FindAction("FarmInteraction/Inspect", true);
            door = ownedActions.FindAction("FarmInteraction/Door", true);
            close = ownedActions.FindAction("FarmInteraction/Close", true);
        }
        private void OnEnable()
        {
            inspect?.Enable();
            door?.Enable();
            close?.Enable();
        }
        private void OnDisable()
        {
            ownedActions?.Disable();
            if (prompt != null) prompt.text = "";
            if (cowPanel != null) cowPanel.Close();
        }
        private void OnDestroy()
        {
            if (ownedActions != null) Destroy(ownedActions);
        }
        private void Update()
        {
            prompt.text = "";
            if (cowPanel.IsOpen)
            {
                if (close.WasPressedThisFrame()) cowPanel.Close();
                return;
            }
            if (movement.IsMovementLocked || transitions.IsTransitioning) return;
            FarmInteractable nearest = null;
            float distance = float.PositiveInfinity;
            foreach (var candidate in FarmInteractable.Registered)
            {
                if (candidate == null || !candidate.isActiveAndEnabled || !candidate.IsInRange(transform.position)) continue;
                float d = ((Vector2)(candidate.transform.position - transform.position)).sqrMagnitude;
                if (d >= distance) continue;
                distance = d;
                nearest = candidate;
            }
            if (nearest == null) return;
            prompt.text = nearest.Prompt;
            InputAction action = nearest.Kind == InteractionKind.Inspect ? inspect : door;
            if (action.WasPressedThisFrame())
            {
                nearest.Interact(this);
                prompt.text = "";
            }
        }
    }
}
