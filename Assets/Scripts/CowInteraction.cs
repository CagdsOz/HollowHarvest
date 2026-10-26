using UnityEngine;

namespace HollowHarvest
{
    [RequireComponent(typeof(AnimalStats))]
    public sealed class CowInteraction : FarmInteractable
    {
        [SerializeField, Min(0.1f)] private float interactionRadius = 2.5f;
        public override InteractionKind Kind => InteractionKind.Inspect;
        public override string Prompt => "Press E to inspect";
        public override bool IsInRange(Vector2 position)
            => Vector2.Distance(position, transform.position) <= interactionRadius;
        public override void Interact(PlayerInteraction player)
            => player.CowPanel.Open(GetComponent<AnimalStats>(), GetComponent<CowWander>());
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
