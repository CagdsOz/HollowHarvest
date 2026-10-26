using System.Collections.Generic;
using UnityEngine;

namespace HollowHarvest
{
    public enum InteractionKind { Inspect, Door }

    public abstract class FarmInteractable : MonoBehaviour
    {
        private static readonly HashSet<FarmInteractable> registered = new HashSet<FarmInteractable>();
        public static IEnumerable<FarmInteractable> Registered => registered;
        public abstract InteractionKind Kind { get; }
        public abstract string Prompt { get; }
        public abstract bool IsInRange(Vector2 position);
        public abstract void Interact(PlayerInteraction player);
        protected virtual void OnEnable() => registered.Add(this);
        protected virtual void OnDisable() => registered.Remove(this);
    }
}
