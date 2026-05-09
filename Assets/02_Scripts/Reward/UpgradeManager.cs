using System;
using System.Linq;
using WaterGrow.Board;
using WaterGrow.Core;
using UnityEngine;

namespace WaterGrow.Reward
{
    public class UpgradeManager : MonoBehaviour
    {
        public event Action UpgradeChanged;

        private const string AttackUpgradeId = "attack_power";

        [SerializeField] private BoardManager boardManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private int baseAttackUpgradeCost = 50;
        [SerializeField] private int attackUpgradeCostStep = 25;
        [SerializeField] private float attackBonusPerLevel = 0.15f;

        public int AttackUpgradeLevel { get; private set; }
        public int AttackUpgradeCost => baseAttackUpgradeCost + AttackUpgradeLevel * attackUpgradeCostStep;
        public float AttackMultiplier => 1f + AttackUpgradeLevel * attackBonusPerLevel;

        private void Awake()
        {
            boardManager ??= FindObjectOfType<BoardManager>();
            saveManager ??= FindObjectOfType<SaveManager>();
            LoadFromSave();
        }

        public void Configure(BoardManager board, SaveManager save)
        {
            boardManager = board;
            saveManager = save;
            LoadFromSave();
        }

        public void ReloadFromSave()
        {
            LoadFromSave();
        }

        public bool TryUpgradeAttack()
        {
            if (boardManager == null)
            {
                return false;
            }

            int cost = AttackUpgradeCost;
            if (!boardManager.TrySpendGold(cost))
            {
                return false;
            }

            AttackUpgradeLevel++;
            WriteToSave();
            saveManager?.Save();
            UpgradeChanged?.Invoke();
            return true;
        }

        private void LoadFromSave()
        {
            AttackUpgradeLevel = 0;
            UpgradeSaveData saveData = saveManager?.Current?.unitUpgradeLevels?.FirstOrDefault(item => item.upgradeId == AttackUpgradeId);
            if (saveData != null)
            {
                AttackUpgradeLevel = Mathf.Max(0, saveData.level);
            }

            UpgradeChanged?.Invoke();
        }

        private void WriteToSave()
        {
            if (saveManager?.Current == null)
            {
                return;
            }

            UpgradeSaveData saveData = saveManager.Current.unitUpgradeLevels.FirstOrDefault(item => item.upgradeId == AttackUpgradeId);
            if (saveData == null)
            {
                saveData = new UpgradeSaveData { upgradeId = AttackUpgradeId };
                saveManager.Current.unitUpgradeLevels.Add(saveData);
            }

            saveData.level = AttackUpgradeLevel;
        }
    }
}
