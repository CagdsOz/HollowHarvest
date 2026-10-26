using System.Collections;
using UnityEngine;

namespace HollowHarvest
{
    public sealed class BuildingTransition : MonoBehaviour
    {
        [SerializeField] private PlayerMovement player;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.4f;
        private Rigidbody2D teleportedBody;
        private RigidbodyInterpolation2D savedInterpolation;
        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            fadeOverlay.alpha = 0;
            fadeOverlay.blocksRaycasts = false;
        }
        public bool TryTransition(Transform destination, GameObject destinationArea, GameObject departingArea)
        {
            if (IsTransitioning || player.IsMovementLocked || destination == null) return false;
            IsTransitioning = true;
            player.SetMovementLocked(this, true);
            fadeOverlay.blocksRaycasts = true;
            StartCoroutine(Transition(destination, destinationArea, departingArea));
            return true;
        }
        private IEnumerator Transition(Transform destination, GameObject destinationArea, GameObject departingArea)
        {
            yield return Fade(0, 1);
            if (destination != null)
            {
                if (destinationArea != null) destinationArea.SetActive(true);
                var body = player.GetComponent<Rigidbody2D>();
                body.linearVelocity = Vector2.zero;
                // Temporarily disable interpolation so a previous physics pose cannot flash on return.
                teleportedBody = body;
                savedInterpolation = body.interpolation;
                body.interpolation = RigidbodyInterpolation2D.None;
                body.position = destination.position;
                player.transform.position = new Vector3(destination.position.x, destination.position.y, player.transform.position.z);
                Physics2D.SyncTransforms();
                cameraFollow.SnapToTarget();
                if (departingArea != null && departingArea != destinationArea) departingArea.SetActive(false);
                yield return null;
                RestoreInterpolation();
            }
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Fade(1, 0);
            Release();
        }
        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0;
            while (elapsed < Mathf.Max(0.01f, fadeDuration))
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Lerp(from, to, elapsed / Mathf.Max(0.01f, fadeDuration));
                yield return null;
            }
            fadeOverlay.alpha = to;
        }
        private void Release()
        {
            RestoreInterpolation();
            IsTransitioning = false;
            if (player != null) player.SetMovementLocked(this, false);
            if (fadeOverlay != null)
            {
                fadeOverlay.alpha = 0;
                fadeOverlay.blocksRaycasts = false;
            }
        }
        private void RestoreInterpolation()
        {
            if (teleportedBody != null) teleportedBody.interpolation = savedInterpolation;
            teleportedBody = null;
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            Release();
        }
    }
}
