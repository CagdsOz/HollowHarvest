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
        private AnimalStats animal;
        private CowWander wander;
        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            closeButton.onClick.AddListener(Close);
            panelRoot.SetActive(false);
        }
        public void Open(AnimalStats selectedAnimal, CowWander selectedWander)
        {
            if (selectedAnimal == null) return;
            Close();
            animal = selectedAnimal;
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
        }
        private void Refresh()
        {
            details.text = $"{animal.AnimalName}\n\nHealth: {animal.Health:0.0} / 100\n"
                + $"Hunger: {animal.Hunger:0.0} / 100 (fully fed = 100)\n"
                + $"Stress: {animal.Stress:0.0} / 100\nBehavior: {animal.Behavior}";
        }
        private void OnDisable() => Close();
        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }
    }
}
