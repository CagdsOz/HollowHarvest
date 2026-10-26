using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HollowHarvest
{
    public sealed class CowStatusPanel : MonoBehaviour
    {
        [SerializeField] private PlayerMovement player;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text details;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button feedButton;
        [SerializeField] private Button petButton;
        [SerializeField] private Button milkButton;
        [SerializeField] private Text feedback;
        [SerializeField, Min(0.1f)] private float feedbackSeconds = 4;
        private CowCare care;
        private float feedbackUntil;
        private AnimalStats animal;
        private CowWander wander;
        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            closeButton.onClick.AddListener(Close);
            if (feedButton != null) feedButton.onClick.AddListener(Feed);
            if (petButton != null) petButton.onClick.AddListener(Pet);
            if (milkButton != null) milkButton.onClick.AddListener(Milk);
            panelRoot.SetActive(false);
        }
        public void Open(AnimalStats selectedAnimal, CowWander selectedWander)
        {
            if (selectedAnimal == null) return;
            Close();
            animal = selectedAnimal;
            care = animal.GetComponent<CowCare>();
            if (feedback != null) feedback.text = "";
            wander = selectedWander;
            if (wander != null) wander.IsInspected = true;
            player.SetMovementLocked(this, true);
            panelRoot.SetActive(true);
            Refresh();
            closeButton.Select();
        }
        public void Close()
        {
            if (wander != null) wander.IsInspected = false;
            wander = null;
            animal = null;
            care = null;
            if (panelRoot != null) panelRoot.SetActive(false);
            if (player != null) player.SetMovementLocked(this, false);
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == closeButton?.gameObject)
                EventSystem.current.SetSelectedGameObject(null);
        }
        private void LateUpdate()
        {
            if (!IsOpen) return;
            if (animal == null) Close();
            else Refresh();
            if (feedback != null && Time.unscaledTime >= feedbackUntil) feedback.text = "";
        }
        private void Refresh()
        {
            details.text = $"{animal.AnimalName}\n\nHealth: {animal.Health:0.0} / 100\n"
                + $"Hunger: {animal.Hunger:0.0} / 100 (fully fed = 100)\n"
                + $"Stress: {animal.Stress:0.0} / 100\nBehavior: {animal.Behavior}";
            if (care != null)
                details.text += $"\nFeed ready in: {System.Math.Ceiling(care.FeedRemaining):0} game min"
                    + $"  |  Pet: {System.Math.Ceiling(care.PetRemaining):0} game min"
                    + (care.MilkedToday ? "\nMilking: already milked today" : "\nMilking: ready today");
            // Keep cooldown/milk buttons clickable so failed attempts explain why.
            if (feedButton != null) feedButton.interactable = care != null;
            if (petButton != null) petButton.interactable = care != null;
            if (milkButton != null) milkButton.interactable = care != null;
        }
        private void Feed() { if (care != null) ShowResult(care.Feed()); }
        private void Pet() { if (care != null) ShowResult(care.Pet()); }
        private void Milk() { if (care != null) ShowResult(care.Milk()); }
        private void ShowResult(CareResult result)
        {
            string message = result switch
            {
                CareResult.Fed => "Fed! Satiety increased.",
                CareResult.Petted => "Petted! Stress reduced and health improved.",
                CareResult.Milked => "Milked! +1 milk collected.",
                CareResult.FeedCooldown => $"Feed again in {System.Math.Ceiling(care.FeedRemaining):0} game minutes.",
                CareResult.PetCooldown => $"Pet again in {System.Math.Ceiling(care.PetRemaining):0} game minutes.",
                CareResult.AlreadyMilked => "Already milked today. Try again tomorrow.",
                CareResult.FullyFed => "Already fully fed; no feeding needed.",
                CareResult.NoPetNeeded => "Already calm and healthy!",
                _ => "Care is unavailable. Check the clock and milk counter references."
            };
            if (feedback != null) feedback.text = message;
            feedbackUntil = Time.unscaledTime + feedbackSeconds;
            Refresh();
        }
        private void OnDisable() => Close();
        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (feedButton != null) feedButton.onClick.RemoveListener(Feed);
            if (petButton != null) petButton.onClick.RemoveListener(Pet);
            if (milkButton != null) milkButton.onClick.RemoveListener(Milk);
        }
    }
}
