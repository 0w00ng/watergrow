using UnityEngine;

namespace WaterGrow.Battle
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float uiSpeed = 950f;
        private EnemyController target;
        private int damage;
        private bool usesRectTransform;

        public void Launch(EnemyController enemy, int projectileDamage)
        {
            target = enemy;
            damage = projectileDamage;
            usesRectTransform = transform is RectTransform;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            float moveSpeed = usesRectTransform ? uiSpeed : speed;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target.transform.position) <= 0.05f)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
