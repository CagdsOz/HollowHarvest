using UnityEngine;
using UnityEngine.UI;

namespace HollowHarvest
{
    public sealed class ClockHud : MonoBehaviour
    {
        [SerializeField] private GameClock clock;
        [SerializeField] private Text label;

        private void OnEnable()
        {
            if (clock != null) clock.TimeChanged += Refresh;
        }
        // Start runs after every Awake, so the initial text sees the initialized clock.
        private void Start() => Refresh();
        private void OnDisable()
        {
            if (clock != null) clock.TimeChanged -= Refresh;
        }
        private void Refresh()
        {
            if (clock == null || label == null) return;
            label.text = $"HOLLOW HARVEST\nDAY {clock.Day:00} / {GameClock.LastDay}    {clock.Hour:00}:{clock.Minute:00}"
                + (clock.IsComplete ? "\n30-DAY PROTOTYPE COMPLETE" : "\nWASD / ARROWS  -  MOVE");
        }
    }
}
