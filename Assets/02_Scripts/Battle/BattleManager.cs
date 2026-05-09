using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WaterGrow.Board;
using WaterGrow.Core;
using WaterGrow.Reward;
using WaterGrow.Stage;
using WaterGrow.UI;
using UnityEngine;
using UnityEngine.UI;

namespace WaterGrow.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private RewardManager rewardManager;
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private Text representativeText;
        [SerializeField] private RectTransform attackEffectRoot;
        [SerializeField] private Projectile projectilePrefab;

        [Header("Prototype Stages")]
        [SerializeField] private bool autoStartTestStage = true;
        [SerializeField] private int baseHpPerStage = 5;
        [SerializeField] private List<StageData> stageConfigs = new List<StageData>();

        private MergeUnit representative;
        private WaterUnitData representativeData;
        private readonly List<EnemyController> activeEnemies = new List<EnemyController>();
        private Coroutine spawnRoutine;
        private Coroutine autoAdvanceRoutine;
        private float attackTimer;
        private bool isSubscribed;
        private int currentStageIndex;

        private StageData CurrentStage => stageConfigs[Mathf.Clamp(currentStageIndex, 0, stageConfigs.Count - 1)];

        private void Awake()
        {
            boardManager ??= FindObjectOfType<BoardManager>();
            enemySpawner ??= FindObjectOfType<EnemySpawner>();
            stageManager ??= FindObjectOfType<StageManager>();
            waveManager ??= FindObjectOfType<WaveManager>();
            uiManager ??= FindObjectOfType<UIManager>();
            dataManager ??= FindObjectOfType<DataManager>();
            saveManager ??= FindObjectOfType<SaveManager>();
            rewardManager ??= FindObjectOfType<RewardManager>();
            upgradeManager ??= FindObjectOfType<UpgradeManager>();

            EnsureFallbackStages();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Start()
        {
            if (autoStartTestStage)
            {
                SelectSavedStage();
                StartCurrentStage();
            }
        }

        private void Update()
        {
            if (representativeData == null || activeEnemies.Count == 0 || stageManager == null || !stageManager.IsStageRunning)
            {
                return;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f)
            {
                return;
            }

            EnemyController target = FindFrontEnemy();
            if (target == null)
            {
                return;
            }

            attackTimer = Mathf.Max(0.1f, representativeData.attackInterval);
            uiManager?.ShowAttackPulse();
            FireProjectile(target, GetRepresentativeAttack());
        }

        public void Configure(BoardManager board, EnemySpawner spawner, StageManager stage, WaveManager wave, UIManager ui, DataManager data, SaveManager save, RewardManager reward, UpgradeManager upgrade, Text representativeLabel, RectTransform effectRoot)
        {
            UnsubscribeEvents();
            boardManager = board;
            enemySpawner = spawner;
            stageManager = stage;
            waveManager = wave;
            uiManager = ui;
            dataManager = data;
            saveManager = save;
            rewardManager = reward;
            upgradeManager = upgrade;
            representativeText = representativeLabel;
            attackEffectRoot = effectRoot;
            SubscribeEvents();
        }

        public void StartTestStage()
        {
            StartCurrentStage();
        }

        public void StartNextOrRetryStage()
        {
            if (stageManager != null && stageManager.IsStageFinished && stageManager.LastStageCleared && currentStageIndex < stageConfigs.Count - 1)
            {
                currentStageIndex++;
            }

            StartCurrentStage();
        }

        public int GetRepresentativeAttack()
        {
            return representativeData == null ? 0 : GetUpgradedAttackPower(representativeData.attackPower);
        }

        private void StartCurrentStage()
        {
            if (stageManager == null)
            {
                Debug.LogWarning("StageManager is not assigned.");
                return;
            }

            StageData config = CurrentStage;
            StopCurrentSpawnRoutine();
            StopAutoAdvanceRoutine();
            ClearActiveEnemies();
            attackTimer = 0f;

            int totalEnemyCount = Mathf.Max(1, waveManager == null ? 10 : waveManager.GetTotalEnemyCount(config.waveGroupId));
            stageManager.MoveToStage(config.stageId);
            stageManager.StartStage(config.stageId, totalEnemyCount, baseHpPerStage);
            SaveCurrentStage(config.stageId);
            uiManager?.UpdateStage($"{config.stageNumber}. {config.stageNameKo}");
            uiManager?.UpdateWaveProgress(1, waveManager == null ? 1 : waveManager.GetWaveCount(config.waveGroupId));
            uiManager?.UpdateRemainingEnemies(stageManager.RemainingEnemies);
            uiManager?.UpdateBaseHp(stageManager.BaseHp, stageManager.MaxBaseHp);
            uiManager?.SetRestartButtonLabel("RETRY");
            uiManager?.ShowGuideMessage($"{config.stageNameKo} 시작. 물정령을 키워 불꽃 적을 막으세요.");

            if (waveManager != null)
            {
                waveManager.StartWaveGroup(config.waveGroupId, HandleEnemyKilled, HandleEnemyReachedBase);
            }
            else
            {
                spawnRoutine = StartCoroutine(SpawnStageEnemies(config));
            }
        }

        private IEnumerator SpawnStageEnemies(StageData config)
        {
            int enemyCount = 10;
            for (int i = 0; i < enemyCount; i++)
            {
                if (stageManager == null || !stageManager.IsStageRunning)
                {
                    yield break;
                }

                SpawnEnemy(dataManager == null ? null : dataManager.GetEnemyData("ENEMY_001"));
                yield return new WaitForSeconds(1.5f);
            }

            spawnRoutine = null;
        }

        private void SpawnEnemy(EnemyData enemyData)
        {
            if (enemySpawner == null)
            {
                Debug.LogWarning("EnemySpawner is not assigned.");
                return;
            }

            enemyData ??= new EnemyData("ENEMY_001", "Fire Soldier", 30, 1.0f, 5, 1);

            EnemyController enemy = enemySpawner.SpawnEnemy(enemyData, HandleEnemyKilled, HandleEnemyReachedBase);
            if (enemy != null)
            {
                activeEnemies.Add(enemy);
            }
        }

        private void HandleRepresentativeChanged(MergeUnit unit)
        {
            representative = unit;
            representativeData = representative == null ? null : GetWaterUnitData(representative.Level);
            attackTimer = 0f;

            if (representativeText != null)
            {
                representativeText.text = representative == null ? "Representative: None" : $"Representative: Lv.{representative.Level}";
            }

            uiManager?.UpdateRepresentativeUnit(representative == null ? 0 : representative.Level);
        }

        private void HandleEnemyKilled(EnemyController enemy)
        {
            activeEnemies.Remove(enemy);
            rewardManager?.GrantEnemyReward(enemy == null ? 0 : enemy.RewardGold, enemy == null ? 0 : enemy.RewardCrystal);
            stageManager?.NotifyEnemyKilled();

            if (stageManager != null)
            {
                uiManager?.UpdateRemainingEnemies(stageManager.RemainingEnemies);
            }
        }

        private void HandleEnemyReachedBase(EnemyController enemy)
        {
            activeEnemies.Remove(enemy);
            stageManager?.NotifyEnemyReachedBase(enemy == null ? 0 : enemy.DamageToBase);

            if (stageManager != null)
            {
                uiManager?.UpdateRemainingEnemies(stageManager.RemainingEnemies);
                uiManager?.UpdateBaseHp(stageManager.BaseHp, stageManager.MaxBaseHp);
            }
        }

        private void HandleStageCleared()
        {
            StopCurrentSpawnRoutine();
            StageData config = CurrentStage;
            rewardManager?.GrantStageClearReward(config.stageId, config.clearRewardGold, config.clearRewardCrystal);

            if (currentStageIndex < stageConfigs.Count - 1)
            {
                uiManager?.SetRestartButtonLabel("AUTO");
                uiManager?.ShowGuideMessage($"{config.stageNameKo} 클리어. 보상 +{config.clearRewardGold} Gold, +{config.clearRewardCrystal} Crystal.");
                autoAdvanceRoutine = StartCoroutine(AutoAdvanceToNextStage());
            }
            else
            {
                uiManager?.SetRestartButtonLabel("RETRY");
                uiManager?.ShowGuideMessage($"챕터 1 클리어. 보상 +{config.clearRewardGold} Gold, +{config.clearRewardCrystal} Crystal.");
            }
        }

        private void HandleStageFailed()
        {
            StopCurrentSpawnRoutine();
            StopAutoAdvanceRoutine();
            uiManager?.SetRestartButtonLabel("RETRY");
            uiManager?.UpdateBaseHp(0, stageManager == null ? 1 : stageManager.MaxBaseHp);
            uiManager?.ShowGuideMessage("Base HP 0. Stage failed. Merge stronger units and retry.");
        }

        private IEnumerator AutoAdvanceToNextStage()
        {
            yield return new WaitForSeconds(1.25f);

            if (stageManager == null || !stageManager.IsStageFinished || !stageManager.LastStageCleared)
            {
                autoAdvanceRoutine = null;
                yield break;
            }

            if (currentStageIndex < stageConfigs.Count - 1)
            {
                currentStageIndex++;
                autoAdvanceRoutine = null;
                StartCurrentStage();
                yield break;
            }

            autoAdvanceRoutine = null;
        }

        private EnemyController FindFrontEnemy()
        {
            ClearDeadEnemyReferences();
            return activeEnemies
                .Where(enemy => enemy != null && !enemy.IsDead)
                .OrderBy(enemy => enemy.DistanceToTarget)
                .FirstOrDefault();
        }

        private WaterUnitData GetWaterUnitData(int level)
        {
            WaterUnitData data = dataManager == null ? null : dataManager.GetWaterUnitData(level);
            if (data != null)
            {
                return data;
            }

            return level switch
            {
                1 => new WaterUnitData(1, 10, 1.0f, 4.0f),
                2 => new WaterUnitData(2, 24, 1.1f, 4.2f),
                3 => new WaterUnitData(3, 55, 1.0f, 4.8f),
                4 => new WaterUnitData(4, 120, 0.9f, 5.0f),
                5 => new WaterUnitData(5, 260, 0.8f, 5.2f),
                _ => null
            };
        }

        private int GetUpgradedAttackPower(int baseAttack)
        {
            float multiplier = upgradeManager == null ? 1f : upgradeManager.AttackMultiplier;
            return Mathf.Max(1, Mathf.RoundToInt(baseAttack * multiplier));
        }

        private void ClearDeadEnemyReferences()
        {
            activeEnemies.RemoveAll(enemy => enemy == null || enemy.IsDead);
        }

        private void StopCurrentSpawnRoutine()
        {
            waveManager?.StopWaveGroup();

            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }

        private void StopAutoAdvanceRoutine()
        {
            if (autoAdvanceRoutine != null)
            {
                StopCoroutine(autoAdvanceRoutine);
                autoAdvanceRoutine = null;
            }
        }

        private void ClearActiveEnemies()
        {
            foreach (EnemyController enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            activeEnemies.Clear();
        }

        private IEnumerator PerformAttack(EnemyController target, int damage)
        {
            if (attackEffectRoot == null || target == null)
            {
                target?.TakeDamage(damage);
                yield break;
            }

            GameObject effect = new GameObject("WaterAttackEffect", typeof(RectTransform));
            effect.transform.SetParent(attackEffectRoot, false);
            Image image = effect.AddComponent<Image>();
            image.color = new Color(0.25f, 0.85f, 1f, 0.75f);

            RectTransform rect = effect.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(36f, 36f);
            rect.anchorMin = new Vector2(0.23f, 0.48f);
            rect.anchorMax = new Vector2(0.23f, 0.48f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            Vector3 start = rect.position;
            Vector3 end = target.transform.position;
            float elapsed = 0f;
            const float duration = 0.18f;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;
                end = target.transform.position;
                rect.position = Vector3.Lerp(start, end, elapsed / duration);
                yield return null;
            }

            Destroy(effect);

            if (target != null && !target.IsDead)
            {
                target.TakeDamage(damage);
            }
        }

        private void FireProjectile(EnemyController target, int damage)
        {
            if (target == null)
            {
                return;
            }

            Projectile projectile = CreateProjectile();
            if (projectile == null)
            {
                target.TakeDamage(damage);
                return;
            }

            projectile.Launch(target, damage);
        }

        private Projectile CreateProjectile()
        {
            if (projectilePrefab != null)
            {
                return Instantiate(projectilePrefab, GetProjectileStartPosition(), Quaternion.identity, attackEffectRoot == null ? transform : attackEffectRoot);
            }

            if (attackEffectRoot == null)
            {
                GameObject projectileObject = new GameObject("WaterProjectile");
                projectileObject.transform.position = GetProjectileStartPosition();
                return projectileObject.AddComponent<Projectile>();
            }

            GameObject effect = new GameObject("WaterProjectile", typeof(RectTransform));
            effect.transform.SetParent(attackEffectRoot, false);
            Image image = effect.AddComponent<Image>();
            image.color = new Color(0.25f, 0.85f, 1f, 0.75f);

            RectTransform rect = effect.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(34f, 34f);
            rect.anchorMin = new Vector2(0.23f, 0.48f);
            rect.anchorMax = new Vector2(0.23f, 0.48f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            return effect.AddComponent<Projectile>();
        }

        private Vector3 GetProjectileStartPosition()
        {
            return attackEffectRoot == null ? transform.position : attackEffectRoot.TransformPoint(new Vector3(0.23f, 0.48f, 0f));
        }

        private void SaveCurrentStage(string stageId)
        {
            if (saveManager?.Current == null)
            {
                return;
            }

            boardManager?.WriteBoardState(saveManager.Current);
            saveManager.Current.currentStageId = stageId;
            saveManager.Save();
        }

        private void SelectSavedStage()
        {
            if (saveManager?.Current == null || string.IsNullOrEmpty(saveManager.Current.currentStageId))
            {
                return;
            }

            int savedIndex = stageConfigs.FindIndex(stage => stage.stageId == saveManager.Current.currentStageId);
            if (savedIndex >= 0)
            {
                currentStageIndex = savedIndex;
            }
        }

        private void EnsureFallbackStages()
        {
            if (stageConfigs != null && stageConfigs.Count > 0)
            {
                return;
            }

            stageConfigs = new List<StageData>
            {
                new StageData("STAGE_1_01", "CHAPTER_1", 1, "물의 숲 입구", "WAVE_1_01", 50, 0, false),
                new StageData("STAGE_1_02", "CHAPTER_1", 2, "작은 불씨 길", "WAVE_1_02", 70, 0, false),
                new StageData("STAGE_1_03", "CHAPTER_1", 3, "불꽃 병사 무리", "WAVE_1_03", 90, 1, false),
                new StageData("STAGE_1_04", "CHAPTER_1", 4, "방패 든 불꽃", "WAVE_1_04", 110, 1, false),
                new StageData("STAGE_1_05", "CHAPTER_1", 5, "병사와 방패병", "WAVE_1_05", 135, 2, false),
                new StageData("STAGE_1_06", "CHAPTER_1", 6, "빠른 불꽃 기병", "WAVE_1_06", 160, 2, false),
                new StageData("STAGE_1_07", "CHAPTER_1", 7, "기병 돌파", "WAVE_1_07", 190, 3, false),
                new StageData("STAGE_1_08", "CHAPTER_1", 8, "방패와 기병", "WAVE_1_08", 220, 3, false),
                new StageData("STAGE_1_09", "CHAPTER_1", 9, "불꽃 혼합 부대", "WAVE_1_09", 260, 4, false),
                new StageData("STAGE_1_10", "CHAPTER_1", 10, "물의 숲 수호전", "WAVE_1_10", 320, 8, true)
            };
        }

        private void SubscribeEvents()
        {
            if (isSubscribed)
            {
                return;
            }

            if (boardManager != null)
            {
                boardManager.RepresentativeChanged += HandleRepresentativeChanged;
            }

            if (stageManager != null)
            {
                stageManager.StageCleared += HandleStageCleared;
                stageManager.StageFailed += HandleStageFailed;
            }

            if (waveManager != null)
            {
                waveManager.EnemySpawned += HandleEnemySpawned;
                waveManager.WaveProgressChanged += HandleWaveProgressChanged;
            }

            isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (boardManager != null)
            {
                boardManager.RepresentativeChanged -= HandleRepresentativeChanged;
            }

            if (stageManager != null)
            {
                stageManager.StageCleared -= HandleStageCleared;
                stageManager.StageFailed -= HandleStageFailed;
            }

            if (waveManager != null)
            {
                waveManager.EnemySpawned -= HandleEnemySpawned;
                waveManager.WaveProgressChanged -= HandleWaveProgressChanged;
            }

            isSubscribed = false;
        }

        private void HandleEnemySpawned(EnemyController enemy)
        {
            if (enemy != null)
            {
                activeEnemies.Add(enemy);
            }
        }

        private void HandleWaveProgressChanged(int currentWave, int totalWaves)
        {
            uiManager?.UpdateWaveProgress(currentWave, totalWaves);
        }
    }
}
