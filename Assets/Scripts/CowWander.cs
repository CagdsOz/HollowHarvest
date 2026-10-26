using UnityEngine;

namespace HollowHarvest
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public sealed class CowWander : MonoBehaviour
    {
        [Tooltip("World-aligned rectangle centered on this marker. Keep it outside obstacles.")]
        [SerializeField] private Transform grazingCenter;
        [SerializeField] private Vector2 grazingSize = new Vector2(10, 7);
        [SerializeField, Min(0)] private float moveSpeed = 1;
        [SerializeField, Min(0)] private float wanderingRadius = 3;
        [SerializeField, Min(0)] private float minimumIdleDuration = 1;
        [SerializeField, Min(0)] private float maximumIdleDuration = 4;
        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private Vector2 target;
        private float idleRemaining;
        private bool walking;
        public bool IsInspected { get; set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            sprite = GetComponent<SpriteRenderer>();
            if (grazingCenter == null)
            {
                Debug.LogError("CowWander needs a separate grazing center marker.", this);
                enabled = false;
                return;
            }
            body.position = ClampToPasture(body.position);
            BeginIdle();
        }

        private void FixedUpdate()
        {
            // Clamp every step, not just destinations, including after Inspector changes.
            Vector2 position = ClampToPasture(body.position);
            if (IsInspected)
            {
                body.MovePosition(position);
                return;
            }
            if (!walking)
            {
                idleRemaining -= Time.fixedDeltaTime;
                if (idleRemaining <= 0)
                {
                    target = ClampToPasture(position + Random.insideUnitCircle * wanderingRadius);
                    walking = true;
                }
            }
            else
            {
                target = ClampToPasture(target);
                position = Vector2.MoveTowards(position, target, Mathf.Max(0, moveSpeed) * Time.fixedDeltaTime);
                if (Vector2.SqrMagnitude(position - target) < 0.001f) BeginIdle();
            }
            body.MovePosition(ClampToPasture(position));
        }

        private void BeginIdle()
        {
            walking = false;
            idleRemaining = Random.Range(minimumIdleDuration, Mathf.Max(minimumIdleDuration, maximumIdleDuration));
        }

        private Vector2 ClampToPasture(Vector2 position)
        {
            Vector3 centerOffset = sprite.bounds.center - transform.position;
            Vector2 halfSprite = (Vector2)sprite.bounds.extents
                + new Vector2(Mathf.Abs(centerOffset.x), Mathf.Abs(centerOffset.y));
            // Inset by the sprite footprint, so the whole cow stays in the pasture.
            Vector2 halfArea = Vector2.Max(grazingSize * 0.5f - halfSprite, Vector2.zero);
            return ClampPosition(position, grazingCenter.position, halfArea);
        }

        public static Vector2 ClampPosition(Vector2 position, Vector2 center, Vector2 halfArea)
            => new Vector2(Mathf.Clamp(position.x, center.x - halfArea.x, center.x + halfArea.x),
                Mathf.Clamp(position.y, center.y - halfArea.y, center.y + halfArea.y));

        private void OnValidate()
        {
            var sr = GetComponent<SpriteRenderer>();
            Vector2 minimumSize = sr != null ? (Vector2)sr.bounds.size : Vector2.one;
            grazingSize = Vector2.Max(grazingSize, minimumSize + Vector2.one * 0.01f);
            maximumIdleDuration = Mathf.Max(minimumIdleDuration, maximumIdleDuration);
        }

        private void OnDrawGizmosSelected()
        {
            if (grazingCenter == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(grazingCenter.position, new Vector3(grazingSize.x, grazingSize.y, 0));
        }
    }
}
