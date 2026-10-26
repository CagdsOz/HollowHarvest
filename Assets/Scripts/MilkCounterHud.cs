using UnityEngine;
using UnityEngine.UI;

namespace HollowHarvest
{
    public sealed class MilkCounterHud : MonoBehaviour
    {
        [SerializeField] private MilkCounter counter;
        [SerializeField] private Text label;
        private int displayed = -1;
        private void LateUpdate()
        {
            if (counter == null || label == null || displayed == counter.TotalMilk) return;
            displayed = counter.TotalMilk;
            label.text = $"MILK: {displayed}";
        }
    }
}
