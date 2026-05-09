using WaterGrow.Board;
using WaterGrow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace WaterGrow.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Button summonButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Text goldText;
        [SerializeField] private Text guideText;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        private void OnEnable()
        {
            if (boardManager != null)
            {
                boardManager.GoldChanged += HandleGoldChanged;
                boardManager.SummonAvailabilityChanged += HandleSummonAvailabilityChanged;
                boardManager.RepresentativeChanged += HandleRepresentativeChanged;
            }

            if (summonButton != null)
            {
                summonButton.onClick.AddListener(HandleSummonClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.AddListener(HandleSaveClicked);
            }
        }

        private void OnDisable()
        {
            if (boardManager != null)
            {
                boardManager.GoldChanged -= HandleGoldChanged;
                boardManager.SummonAvailabilityChanged -= HandleSummonAvailabilityChanged;
                boardManager.RepresentativeChanged -= HandleRepresentativeChanged;
            }

            if (summonButton != null)
            {
                summonButton.onClick.RemoveListener(HandleSummonClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(HandleSaveClicked);
            }
        }

        public void RefreshAll()
        {
            if (boardManager == null)
            {
                return;
            }

            HandleGoldChanged(boardManager.Gold);
            HandleSummonAvailabilityChanged(boardManager.CanSummon);
        }

        private void HandleSummonClicked()
        {
            boardManager?.Summon();
        }

        private void HandleSaveClicked()
        {
            gameManager?.SaveGame();
            if (guideText != null)
            {
                guideText.text = "저장 완료";
            }
        }

        private void HandleGoldChanged(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold {gold}";
            }
        }

        private void HandleSummonAvailabilityChanged(bool canSummon)
        {
            if (summonButton != null)
            {
                summonButton.interactable = canSummon;
            }
        }

        private void HandleRepresentativeChanged(MergeUnit unit)
        {
            if (guideText == null)
            {
                return;
            }

            guideText.text = unit == null ? "소환 버튼으로 Lv.1 물방울을 만드세요." : $"Lv.{unit.Level} 물정령이 대표로 출전합니다.";
        }
    }
}

