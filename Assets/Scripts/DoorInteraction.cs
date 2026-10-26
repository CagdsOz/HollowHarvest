using UnityEngine;

namespace HollowHarvest
{
    public sealed class DoorInteraction : FarmInteractable
    {
        [SerializeField] private Vector2 zoneSize = new Vector2(2, 2);
        [SerializeField] private bool isExit;
        [SerializeField] private Transform destination;
        [Tooltip("Indoor area to activate; leave empty when returning outdoors.")]
        [SerializeField] private GameObject destinationArea;
        [Tooltip("Indoor area to deactivate; leave empty on an outdoor door.")]
        [SerializeField] private GameObject departingArea;
        public override InteractionKind Kind => InteractionKind.Door;
        public override string Prompt => isExit ? "Press F to exit" : "Press F to enter";
        public override bool IsInRange(Vector2 position)
        {
            Vector3 local = transform.InverseTransformPoint(position);
            return destination != null && Mathf.Abs(local.x) <= zoneSize.x * 0.5f
                && Mathf.Abs(local.y) <= zoneSize.y * 0.5f;
        }
        public override void Interact(PlayerInteraction player)
            => player.Transitions.TryTransition(destination, destinationArea, departingArea);
        private void OnValidate() => zoneSize = Vector2.Max(zoneSize, Vector2.one * 0.1f);
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(zoneSize.x, zoneSize.y, 0));
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
