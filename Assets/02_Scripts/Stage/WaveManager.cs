using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WaterGrow.Battle;
using WaterGrow.Core;
using UnityEngine;

namespace WaterGrow.Stage
{
    public class WaveManager : MonoBehaviour
    {
        public event Action<EnemyController> EnemySpawned;
        public event Action<int, int> WaveProgressChanged;
        public event Action WaveGroupCompleted;

        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private List<WaveData> waves = new List<WaveData>();

        private Coroutine waveRoutine;
        private Action<EnemyController> enemyKilledCallback;
        private Action<EnemyController> enemyReachedBaseCallback;

        private void Awake()
        {
            enemySpawner ??= FindObjectOfType<EnemySpawner>();
            dataManager ??= FindObjectOfType<DataManager>();
            EnsureFallbackWaves();
        }

        public void Configure(EnemySpawner spawner, DataManager data)
        {
            enemySpawner = spawner;
            dataManager = data;
            EnsureFallbackWaves();
        }

        public void StartWaveGroup(string waveGroupId, Action<EnemyController> onEnemyKilled, Action<EnemyController> onEnemyReachedBase)
        {
            StopWaveGroup();
            enemyKilledCallback = onEnemyKilled;
            enemyReachedBaseCallback = onEnemyReachedBase;
            waveRoutine = StartCoroutine(RunWaveGroup(GetWaves(waveGroupId)));
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
            return GetWaves(waveGroupId).Sum(wave => Mathf.Max(0, wave.count));
        }

        public int GetWaveCount(string waveGroupId)
        {
            return GetWaves(waveGroupId).Count;
        }

        private IEnumerator RunWaveGroup(List<WaveData> waveGroup)
        {
            int totalWaves = Mathf.Max(1, waveGroup.Count);

            for (int waveIndex = 0; waveIndex < waveGroup.Count; waveIndex++)
            {
                WaveData wave = waveGroup[waveIndex];
                WaveProgressChanged?.Invoke(waveIndex + 1, totalWaves);

                int count = Mathf.Max(0, wave.count);
                for (int i = 0; i < count; i++)
                {
                    EnemyData enemyData = dataManager == null ? null : dataManager.GetEnemyData(wave.enemyId);
                    enemyData ??= new EnemyData("ENEMY_001", "불꽃 병사", "Basic", 35, 1.0f, 1, 5, 0, false);

                    EnemyController enemy = enemySpawner == null
                        ? null
                        : enemySpawner.SpawnEnemy(enemyData, enemyKilledCallback, enemyReachedBaseCallback);

                    if (enemy != null)
                    {
                        EnemySpawned?.Invoke(enemy);
                    }

                    yield return new WaitForSeconds(Mathf.Max(0.1f, wave.spawnInterval));
                }

                if (wave.delayBeforeNextGroup > 0f)
                {
                    yield return new WaitForSeconds(wave.delayBeforeNextGroup);
                }
            }

            waveRoutine = null;
            WaveGroupCompleted?.Invoke();
        }

        private List<WaveData> GetWaves(string waveGroupId)
        {
            EnsureFallbackWaves();
            List<WaveData> group = waves
                .Where(wave => wave.waveGroupId == waveGroupId)
                .OrderBy(wave => wave.spawnOrder)
                .ToList();

            return group.Count > 0
                ? group
                : waves.Where(wave => wave.waveGroupId == "WAVE_1_01").OrderBy(wave => wave.spawnOrder).ToList();
        }

        private void EnsureFallbackWaves()
        {
            if (waves.Count > 0)
            {
                return;
            }

            // MVP Alpha 0.3 임시 웨이브 테이블. 추후 JSON/ScriptableObject 로딩으로 교체한다.
            waves.Add(new WaveData("WAVE_1_01", 1, "ENEMY_001", 10, 1.45f, 0f));

            waves.Add(new WaveData("WAVE_1_02", 1, "ENEMY_001", 14, 1.35f, 0f));

            waves.Add(new WaveData("WAVE_1_03", 1, "ENEMY_001", 9, 1.25f, 0.75f));
            waves.Add(new WaveData("WAVE_1_03", 2, "ENEMY_001", 9, 1.10f, 0f));

            waves.Add(new WaveData("WAVE_1_04", 1, "ENEMY_001", 8, 1.20f, 0.75f));
            waves.Add(new WaveData("WAVE_1_04", 2, "ENEMY_002", 4, 1.55f, 0f));

            waves.Add(new WaveData("WAVE_1_05", 1, "ENEMY_001", 10, 1.10f, 0.65f));
            waves.Add(new WaveData("WAVE_1_05", 2, "ENEMY_002", 6, 1.40f, 0f));

            waves.Add(new WaveData("WAVE_1_06", 1, "ENEMY_001", 8, 1.10f, 0.65f));
            waves.Add(new WaveData("WAVE_1_06", 2, "ENEMY_003", 6, 1.05f, 0f));

            waves.Add(new WaveData("WAVE_1_07", 1, "ENEMY_001", 10, 1.00f, 0.55f));
            waves.Add(new WaveData("WAVE_1_07", 2, "ENEMY_003", 9, 0.95f, 0f));

            waves.Add(new WaveData("WAVE_1_08", 1, "ENEMY_002", 7, 1.30f, 0.55f));
            waves.Add(new WaveData("WAVE_1_08", 2, "ENEMY_003", 9, 0.90f, 0f));

            waves.Add(new WaveData("WAVE_1_09", 1, "ENEMY_001", 8, 0.95f, 0.45f));
            waves.Add(new WaveData("WAVE_1_09", 2, "ENEMY_002", 6, 1.20f, 0.45f));
            waves.Add(new WaveData("WAVE_1_09", 3, "ENEMY_003", 8, 0.85f, 0f));

            waves.Add(new WaveData("WAVE_1_10", 1, "ENEMY_001", 8, 0.95f, 0.45f));
            waves.Add(new WaveData("WAVE_1_10", 2, "ENEMY_002", 5, 1.20f, 0.45f));
            waves.Add(new WaveData("WAVE_1_10", 3, "ENEMY_003", 5, 0.85f, 0.70f));
            waves.Add(new WaveData("WAVE_1_10", 4, "BOSS_001", 1, 0.10f, 0f));
        }
    }
}
