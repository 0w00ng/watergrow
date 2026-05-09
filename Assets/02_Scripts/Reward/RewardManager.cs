using WaterGrow.Board;
using WaterGrow.Core;
using UnityEngine;

namespace WaterGrow.Reward
{
    public class RewardManager : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private SaveManager saveManager;

        private void Awake()
        {
            boardManager ??= FindObjectOfType<BoardManager>();
            saveManager ??= FindObjectOfType<SaveManager>();
        }

        public void Configure(BoardManager board, SaveManager save)
        {
            boardManager = board;
            saveManager = save;
        }

        public void GrantGold(int amount)
        {
            boardManager?.AddGold(amount);
        }

        public void GrantStageClearReward(string stageId, int gold, int waterCrystal)
        {
            GrantGold(gold);

            if (saveManager?.Current != null)
            {
                boardManager?.WriteBoardState(saveManager.Current);
                saveManager.Current.waterCrystal += Mathf.Max(0, waterCrystal);
                saveManager.Current.highestClearedStageId = stageId;
                saveManager.Save();
            }
        }
    }
}
