using UnityEngine;

namespace WaterGrow.Battle
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        private EnemyController target;
        private int damage;

        public void Launch(EnemyController enemy, int projectileDamage)
        {
            target = enemy;
            damage = projectileDamage;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target.transform.position) <= 0.05f)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}

