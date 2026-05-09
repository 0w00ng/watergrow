using UnityEngine;
using System;

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
        public int RemainingEnemies { get; private set; }
        public bool IsStageRunning { get; private set; }
        public bool IsStageFinished { get; private set; }

        public void StartTestStage(int totalEnemyCount)
        {
            IsStageRunning = true;
            IsStageFinished = false;
            RemainingEnemies = Mathf.Max(0, totalEnemyCount);
            baseHp = Mathf.Max(1, maxBaseHp);

            StageChanged?.Invoke(currentStageId);
            RemainingEnemiesChanged?.Invoke(RemainingEnemies);
            BaseHpChanged?.Invoke(baseHp);
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

        public void MoveNextStage()
        {
            Debug.Log("Next stage flow will be implemented in MVP Alpha 0.3.");
        }

        private void CompleteStage()
        {
            IsStageRunning = false;
            IsStageFinished = true;
            StageCleared?.Invoke();
        }

        private void FailStage()
        {
            IsStageRunning = false;
            IsStageFinished = true;
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
