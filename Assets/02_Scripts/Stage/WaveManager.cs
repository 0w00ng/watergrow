using System;
using System.Collections;
using System.Collections.Generic;
using WaterGrow.Battle;
using WaterGrow.Core;
using UnityEngine;

namespace WaterGrow.Stage
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        private class WaveEntry
        {
            public string enemyId = "ENEMY_001";
            public int count = 10;
            public float spawnInterval = 1.5f;
        }

        [Serializable]
        private class WaveGroup
        {
            public string waveGroupId = "WAVE_1_01";
            public List<WaveEntry> waves = new List<WaveEntry>();
        }

        public event Action<EnemyController> EnemySpawned;
        public event Action WaveGroupCompleted;

        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private List<WaveGroup> waveGroups = new List<WaveGroup>();

        private Coroutine waveRoutine;
        private Action<EnemyController> enemyKilledCallback;
        private Action<EnemyController> enemyReachedBaseCallback;

        private void Awake()
        {
            enemySpawner ??= FindObjectOfType<EnemySpawner>();
            dataManager ??= FindObjectOfType<DataManager>();
            EnsureFallbackWaveGroups();
        }

        public void Configure(EnemySpawner spawner, DataManager data)
        {
            enemySpawner = spawner;
            dataManager = data;
            EnsureFallbackWaveGroups();
        }

        public void StartWaveGroup(string waveGroupId, Action<EnemyController> onEnemyKilled, Action<EnemyController> onEnemyReachedBase)
        {
            StopWaveGroup();
            enemyKilledCallback = onEnemyKilled;
            enemyReachedBaseCallback = onEnemyReachedBase;
            waveRoutine = StartCoroutine(RunWaveGroup(GetWaveGroup(waveGroupId)));
        }

        public void StopWaveGroup()
        {
            if (waveRoutine != null)
            {
                StopCoroutine(waveRoutine);
                waveRoutine = null;
            }
        }

        public int GetTotalEnemyCount(string waveGroupId)
        {
            WaveGroup group = GetWaveGroup(waveGroupId);
            int total = 0;

            foreach (WaveEntry wave in group.waves)
            {
                total += Mathf.Max(0, wave.count);
            }

            return total;
        }

        private IEnumerator RunWaveGroup(WaveGroup group)
        {
            foreach (WaveEntry wave in group.waves)
            {
                int count = Mathf.Max(0, wave.count);
                for (int i = 0; i < count; i++)
                {
                    EnemyData enemyData = dataManager == null ? null : dataManager.GetEnemyData(wave.enemyId);
                    enemyData ??= new EnemyData("ENEMY_001", "불꽃 병사", 30, 1.0f, 5, 1);

                    EnemyController enemy = enemySpawner == null
                        ? null
                        : enemySpawner.SpawnEnemy(enemyData, enemyKilledCallback, enemyReachedBaseCallback);

                    if (enemy != null)
                    {
                        EnemySpawned?.Invoke(enemy);
                    }

                    yield return new WaitForSeconds(Mathf.Max(0.1f, wave.spawnInterval));
                }
            }

            waveRoutine = null;
            WaveGroupCompleted?.Invoke();
        }

        private WaveGroup GetWaveGroup(string waveGroupId)
        {
            EnsureFallbackWaveGroups();
            WaveGroup group = waveGroups.Find(item => item.waveGroupId == waveGroupId);
            return group ?? waveGroups[0];
        }

        private void EnsureFallbackWaveGroups()
        {
            if (waveGroups.Count > 0)
            {
                return;
            }

            // MVP 임시 웨이브 테이블. 추후 JSON/ScriptableObject 로딩으로 교체하기 쉽게 한 곳에만 둔다.
            waveGroups.Add(new WaveGroup
            {
                waveGroupId = "WAVE_1_01",
                waves = new List<WaveEntry>
                {
                    new WaveEntry { enemyId = "ENEMY_001", count = 10, spawnInterval = 1.5f }
                }
            });

            waveGroups.Add(new WaveGroup
            {
                waveGroupId = "WAVE_1_02",
                waves = new List<WaveEntry>
                {
                    new WaveEntry { enemyId = "ENEMY_001", count = 7, spawnInterval = 1.35f },
                    new WaveEntry { enemyId = "ENEMY_001", count = 7, spawnInterval = 1.15f }
                }
            });

            waveGroups.Add(new WaveGroup
            {
                waveGroupId = "WAVE_1_03",
                waves = new List<WaveEntry>
                {
                    new WaveEntry { enemyId = "ENEMY_001", count = 9, spawnInterval = 1.2f },
                    new WaveEntry { enemyId = "ENEMY_001", count = 9, spawnInterval = 1.0f }
                }
            });
        }
    }
}
