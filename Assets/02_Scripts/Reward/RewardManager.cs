using WaterGrow.Board;
using UnityEngine;

namespace WaterGrow.Reward
{
    public class RewardManager : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }
        }

        public void GrantGold(int amount)
        {
            boardManager?.AddGold(amount);
        }
    }
}

