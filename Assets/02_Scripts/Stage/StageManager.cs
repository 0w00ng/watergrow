using System;
using UnityEngine;

namespace WaterGrow.Stage
{
    public class StageManager : MonoBehaviour
    {
        public event Action<string> StageChanged;
        public event Action<int> RemainingEnemiesChanged;
        public event Action<int> BaseHpChanged;
        public event Action StageCleared;
        public event Action StageFailed;

        [SerializeField] private string currentStageId = "STAGE_1_01";
        [SerializeField] private int maxBaseHp = 5;
        [SerializeField] private int baseHp = 5;

        public string CurrentStageId => currentStageId;
        public int BaseHp => baseHp;
        public int MaxBaseHp => maxBaseHp;
        public int RemainingEnemies { get; private set; }
        public bool IsStageRunning { get; private set; }
        public bool IsStageFinished { get; private set; }
        public bool LastStageCleared { get; private set; }

        public void StartStage(string stageId, int totalEnemyCount, int baseHpValue)
        {
            currentStageId = string.IsNullOrEmpty(stageId) ? currentStageId : stageId;
            maxBaseHp = Mathf.Max(1, baseHpValue);
            baseHp = maxBaseHp;
            RemainingEnemies = Mathf.Max(0, totalEnemyCount);
            IsStageRunning = true;
            IsStageFinished = false;
            LastStageCleared = false;

            StageChanged?.Invoke(currentStageId);
            RemainingEnemiesChanged?.Invoke(RemainingEnemies);
            BaseHpChanged?.Invoke(baseHp);
        }

        public void StartTestStage(int totalEnemyCount)
        {
            StartStage(currentStageId, totalEnemyCount, maxBaseHp);
        }

        public void NotifyEnemyKilled()
        {
            if (!IsStageRunning || IsStageFinished)
            {
                return;
            }

            ResolveEnemy();
        }

        public void NotifyEnemyReachedBase(int damage)
        {
            if (!IsStageRunning || IsStageFinished)
            {
                return;
            }

            baseHp = Mathf.Max(0, baseHp - Mathf.Max(0, damage));
            BaseHpChanged?.Invoke(baseHp);
            ResolveEnemy();

            if (baseHp <= 0)
            {
                FailStage();
            }
        }

        private void CompleteStage()
        {
            IsStageRunning = false;
            IsStageFinished = true;
            LastStageCleared = true;
            StageCleared?.Invoke();
        }

        private void FailStage()
        {
            IsStageRunning = false;
            IsStageFinished = true;
            LastStageCleared = false;
            StageFailed?.Invoke();
        }

        private void ResolveEnemy()
        {
            RemainingEnemies = Mathf.Max(0, RemainingEnemies - 1);
            RemainingEnemiesChanged?.Invoke(RemainingEnemies);

            if (RemainingEnemies <= 0 && baseHp > 0)
            {
                CompleteStage();
            }
        }
    }
}
