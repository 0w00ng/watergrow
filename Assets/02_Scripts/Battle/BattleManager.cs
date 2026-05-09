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
        [Serializable]
        private class StageConfig
        {
            public string stageId = "STAGE_1_01";
            public int enemyCount = 10;
            public float spawnInterval = 1.5f;
            public int baseHp = 5;
            public string enemyId = "ENEMY_001";
            public int clearRewardGold = 30;
            public int clearRewardCrystal;
        }

        [SerializeField] private BoardManager boardManager;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private RewardManager rewardManager;
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private Text representativeText;
        [SerializeField] private RectTransform attackEffectRoot;

        [Header("Prototype Stages")]
        [SerializeField] private bool autoStartTestStage = true;
        [SerializeField]
        private List<StageConfig> stageConfigs = new List<StageConfig>
        {
            new StageConfig { stageId = "STAGE_1_01", enemyCount = 10, spawnInterval = 1.5f, baseHp = 5, enemyId = "ENEMY_001", clearRewardGold = 50, clearRewardCrystal = 0 },
            new StageConfig { stageId = "STAGE_1_02", enemyCount = 14, spawnInterval = 1.35f, baseHp = 5, enemyId = "ENEMY_001", clearRewardGold = 80, clearRewardCrystal = 1 },
            new StageConfig { stageId = "STAGE_1_03", enemyCount = 18, spawnInterval = 1.2f, baseHp = 5, enemyId = "ENEMY_001", clearRewardGold = 120, clearRewardCrystal = 2 }
        };

        private MergeUnit representative;
        private WaterUnitData representativeData;
        private readonly List<EnemyController> activeEnemies = new List<EnemyController>();
        private Coroutine spawnRoutine;
        private Coroutine autoAdvanceRoutine;
        private float attackTimer;
        private bool isSubscribed;
        private int currentStageIndex;

        private StageConfig CurrentStage => stageConfigs[Mathf.Clamp(currentStageIndex, 0, stageConfigs.Count - 1)];

        private void Awake()
        {
            boardManager ??= FindObjectOfType<BoardManager>();
            enemySpawner ??= FindObjectOfType<EnemySpawner>();
            stageManager ??= FindObjectOfType<StageManager>();
            uiManager ??= FindObjectOfType<UIManager>();
            dataManager ??= FindObjectOfType<DataManager>();
            rewardManager ??= FindObjectOfType<RewardManager>();
            upgradeManager ??= FindObjectOfType<UpgradeManager>();

            if (stageConfigs == null || stageConfigs.Count == 0)
            {
                stageConfigs = new List<StageConfig>
                {
                    new StageConfig()
                };
            }
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
            StartCoroutine(PerformAttack(target, GetUpgradedAttackPower(representativeData.attackPower)));
        }

        public void Configure(BoardManager board, EnemySpawner spawner, StageManager stage, UIManager ui, DataManager data, RewardManager reward, UpgradeManager upgrade, Text representativeLabel, RectTransform effectRoot)
        {
            UnsubscribeEvents();
            boardManager = board;
            enemySpawner = spawner;
            stageManager = stage;
            uiManager = ui;
            dataManager = data;
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

            StageConfig config = CurrentStage;
            StopCurrentSpawnRoutine();
            StopAutoAdvanceRoutine();
            ClearActiveEnemies();
            attackTimer = 0f;

            stageManager.StartStage(config.stageId, config.enemyCount, config.baseHp);
            uiManager?.UpdateStage(stageManager.CurrentStageId);
            uiManager?.UpdateRemainingEnemies(stageManager.RemainingEnemies);
            uiManager?.UpdateBaseHp(stageManager.BaseHp, stageManager.MaxBaseHp);
            uiManager?.SetRestartButtonLabel("RETRY");
            uiManager?.ShowGuideMessage($"{config.stageId} started. Build water units and defend the base.");

            spawnRoutine = StartCoroutine(SpawnStageEnemies(config));
        }

        private IEnumerator SpawnStageEnemies(StageConfig config)
        {
            for (int i = 0; i < config.enemyCount; i++)
            {
                if (stageManager == null || !stageManager.IsStageRunning)
                {
                    yield break;
                }

                SpawnEnemy(dataManager == null ? null : dataManager.GetEnemyData(config.enemyId));
                yield return new WaitForSeconds(Mathf.Max(0.1f, config.spawnInterval));
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
            boardManager?.AddGold(enemy == null ? 0 : enemy.RewardGold);
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
            StageConfig config = CurrentStage;
            rewardManager?.GrantStageClearReward(config.stageId, config.clearRewardGold, config.clearRewardCrystal);

            if (currentStageIndex < stageConfigs.Count - 1)
            {
                uiManager?.SetRestartButtonLabel("AUTO");
                uiManager?.ShowGuideMessage($"Stage cleared. Reward +{config.clearRewardGold} Gold, +{config.clearRewardCrystal} Crystal. Next stage soon.");
                autoAdvanceRoutine = StartCoroutine(AutoAdvanceToNextStage());
            }
            else
            {
                uiManager?.SetRestartButtonLabel("RETRY");
                uiManager?.ShowGuideMessage($"Prototype stages cleared. Reward +{config.clearRewardGold} Gold, +{config.clearRewardCrystal} Crystal.");
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
                StartCurrentStage();
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

            isSubscribed = false;
        }
    }
}
