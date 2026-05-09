using UnityEngine;

namespace WaterGrow.Battle
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private int testEnemyHp = 50;

        public EnemyController SpawnTestEnemy()
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("Enemy prefab is not assigned.");
                return null;
            }

            EnemyController enemy = Instantiate(enemyPrefab, spawnRoot == null ? transform : spawnRoot);
            enemy.Initialize(testEnemyHp);
            return enemy;
        }
    }
}

