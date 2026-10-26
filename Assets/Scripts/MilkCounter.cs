using UnityEngine;

namespace HollowHarvest
{
    // Session total shared by every cow. No inventory or economy dependency.
    public sealed class MilkCounter : MonoBehaviour
    {
        [SerializeField, Min(0)] private int totalMilk;
        public int TotalMilk => totalMilk;
        public bool CanAddMilk => totalMilk < int.MaxValue;
        public void AddMilk()
        {
            if (CanAddMilk) totalMilk++;
        }
    }
}
