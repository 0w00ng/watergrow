using WaterGrow.Board;
using WaterGrow.Core;
using WaterGrow.Stage;
using WaterGrow.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WaterGrow.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private Text representativeText;
        [SerializeField] private RectTransform attackEffectRoot;

        [Header("Prototype 0.2 Test Stage")]
        [SerializeField] private bool autoStartTestStage = true;
        [SerializeField] private int testStageEnemyCount = 10;
        [SerializeField] private float testStageSpawnInterval = 1.5f;
        [SerializeField] private string testEnemyId = "ENEMY_001";

        private MergeUnit representative;
        private WaterUnitData representativeData;
        private readonly List<EnemyController> activeEnemies = new List<EnemyController>();
        private Coroutine spawnRoutine;
        private float attackTimer;
        private bool isSubscribed;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<EnemySpawner>();
            }

            if (stageManager == null)
            {
                stageManager = FindObjectOfType<StageManager>();
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }

            if (dataManager == null)
            {
                dataManager = FindObjectOfType<DataManager>();
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
                StartTestStage();
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
            StartCoroutine(PerformAttack(target, representativeData.attackPower));
        }

        public void StartTestStage()
        {
            if (stageManager == null)
            {
                Debug.LogWarning("StageManager is not assigned.");
                return;
            }

            StopCurrentSpawnRoutine();
            ClearDeadEnemyReferences();
            activeEnemies.Clear();
            attackTimer = 0f;

            stageManager.StartTestStage(testStageEnemyCount);
            uiManager?.UpdateStage(stageManager.CurrentStageId);
            uiManager?.UpdateRemainingEnemies(stageManager.RemainingEnemies);
            uiManager?.UpdateBaseHp(stageManager.BaseHp);

            spawnRoutine = StartCoroutine(SpawnTestStageEnemies());
        }

        public void Configure(BoardManager board, EnemySpawner spawner, StageManager stage, UIManager ui, DataManager data, Text representativeLabel, RectTransform effectRoot)
        {
            UnsubscribeEvents();
            boardManager = board;
            enemySpawner = spawner;
            stageManager = stage;
            uiManager = ui;
            dataManager = data;
            representativeText = representativeLabel;
            attackEffectRoot = effectRoot;
            SubscribeEvents();
        }

        private void HandleRepresentativeChanged(MergeUnit unit)
        {
            representative = unit;
            representativeData = representative == null ? null : GetWaterUnitData(representative.Level);
            attackTimer = 0f;

            if (representativeText != null)
            {
                representativeText.text = representative == null ? "대표 물정령 없음" : $"대표 출전: Lv.{representative.Level}";
            }

            uiManager?.UpdateRepresentativeUnit(representative == null ? 0 : representative.Level);
        }

        public int GetRepresentativeAttack()
        {
            if (representativeData == null)
            {
                return 0;
            }

            return representativeData.attackPower;
        }

        private IEnumerator SpawnTestStageEnemies()
        {
            for (int i = 0; i < testStageEnemyCount; i++)
            {
                if (stageManager == null || !stageManager.IsStageRunning)
                {
                    yield break;
                }

                SpawnEnemy(dataManager == null ? null : dataManager.GetEnemyData(testEnemyId));
                yield return new WaitForSeconds(Mathf.Max(0.1f, testStageSpawnInterval));
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

            if (enemyData == null)
            {
                enemyData = new EnemyData("ENEMY_001", "불꽃 병사", 30, 1.0f, 5, 1);
            }

            EnemyController enemy = enemySpawner.SpawnEnemy(enemyData, HandleEnemyKilled, HandleEnemyReachedBase);
            if (enemy != null)
            {
                activeEnemies.Add(enemy);
            }
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
                uiManager?.UpdateBaseHp(stageManager.BaseHp);
            }
        }

        private void HandleStageCleared()
        {
            StopCurrentSpawnRoutine();
            uiManager?.ShowGuideMessage("스테이지 클리어");
        }

        private void HandleStageFailed()
        {
            StopCurrentSpawnRoutine();
            uiManager?.ShowGuideMessage("스테이지 실패");
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

            // DataManager가 없는 수동 Scene에서도 공격 루프가 죽지 않도록 유지하는 MVP 테스트용 fallback.
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
