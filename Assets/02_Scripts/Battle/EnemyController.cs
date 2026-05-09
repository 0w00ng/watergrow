using UnityEngine;
using UnityEngine.UI;

namespace WaterGrow.Battle
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private Slider hpBar;

        private int maxHp;
        private int currentHp;

        public bool IsDead => currentHp <= 0;

        public void Initialize(int hp)
        {
            maxHp = Mathf.Max(1, hp);
            currentHp = maxHp;
            RefreshHpBar();
        }

        public void TakeDamage(int damage)
        {
            currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, damage));
            RefreshHpBar();

            if (IsDead)
            {
                Destroy(gameObject);
            }
        }

        private void RefreshHpBar()
        {
            if (hpBar != null)
            {
                hpBar.value = (float)currentHp / maxHp;
            }
        }
    }
}

