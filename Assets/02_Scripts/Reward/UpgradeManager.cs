using UnityEngine;

namespace WaterGrow.Reward
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private int attackUpgradeLevel;

        public int AttackUpgradeLevel => attackUpgradeLevel;
        public float AttackMultiplier => 1f + attackUpgradeLevel * 0.1f;

        public void UpgradeAttack()
        {
            attackUpgradeLevel++;
        }
    }
}

