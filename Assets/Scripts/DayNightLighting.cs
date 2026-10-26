using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HollowHarvest
{
    [RequireComponent(typeof(Light2D))]
    public sealed class DayNightLighting : MonoBehaviour
    {
        [SerializeField] private GameClock clock;
        [SerializeField] private Gradient lightColors = new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(new Color(0.35f, 0.43f, 0.7f), 0f),
                new GradientColorKey(new Color(0.35f, 0.43f, 0.7f), 0.17f),
                new GradientColorKey(new Color(1f, 0.72f, 0.5f), 0.25f),
                new GradientColorKey(Color.white, 0.34f),
                new GradientColorKey(Color.white, 0.67f),
                new GradientColorKey(new Color(1f, 0.55f, 0.35f), 0.77f),
                new GradientColorKey(new Color(0.35f, 0.43f, 0.7f), 0.87f),
                new GradientColorKey(new Color(0.35f, 0.43f, 0.7f), 1f)
            },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        };
        [SerializeField] private AnimationCurve intensity = new AnimationCurve(
            new Keyframe(0f, 0.25f), new Keyframe(0.17f, 0.25f),
            new Keyframe(0.34f, 1f), new Keyframe(0.67f, 1f),
            new Keyframe(0.87f, 0.25f), new Keyframe(1f, 0.25f));
        private Light2D globalLight;

        private void Awake() => globalLight = GetComponent<Light2D>();
        private void LateUpdate()
        {
            if (clock == null) return;
            globalLight.color = lightColors.Evaluate(clock.NormalizedTime);
            globalLight.intensity = Mathf.Max(0f, intensity.Evaluate(clock.NormalizedTime));
        }
    }
}
