using UnityEngine;

namespace HollowHarvest
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
        private Vector3 velocity;

        private void Start()
        {
            SnapToTarget();
        }

        public void SnapToTarget()
        {
            velocity = Vector3.zero;
            if (target != null) transform.position = target.position + offset;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.SmoothDamp(transform.position,
                target.position + offset, ref velocity, smoothTime);
        }
    }
}
