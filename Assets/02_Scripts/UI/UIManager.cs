using WaterGrow.Board;
using WaterGrow.Core;
using System.Collections;
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
        [SerializeField] private Button restartButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Text goldText;
        [SerializeField] private Text guideText;
        [SerializeField] private Text stageText;
        [SerializeField] private Text remainingEnemiesText;
        [SerializeField] private Text baseHpText;
        [SerializeField] private Text representativeLevelText;
        [SerializeField] private RectTransform representativePreview;
        private bool boardEventsRegistered;
        private Coroutine attackPulseRoutine;

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
            RegisterBoardEvents();

            if (summonButton != null)
            {
                summonButton.onClick.AddListener(HandleSummonClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.AddListener(HandleSaveClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(HandleResetClicked);
            }
        }

        private void OnDisable()
        {
            UnregisterBoardEvents();

            if (summonButton != null)
            {
                summonButton.onClick.RemoveListener(HandleSummonClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(HandleSaveClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(HandleResetClicked);
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

        public void Configure(
            BoardManager board,
            GameManager game,
            Button summon,
            Button save,
            Button restart,
            Button reset,
            Text gold,
            Text guide,
            Text stage,
            Text remainingEnemies,
            Text baseHp,
            Text representativeLevel,
            RectTransform representativePreviewTarget = null)
        {
            UnregisterBoardEvents();
            boardManager = board;
            gameManager = game;
            summonButton = summon;
            saveButton = save;
            restartButton = restart;
            resetButton = reset;
            goldText = gold;
            guideText = guide;
            stageText = stage;
            remainingEnemiesText = remainingEnemies;
            baseHpText = baseHp;
            representativeLevelText = representativeLevel;
            representativePreview = representativePreviewTarget;

            RegisterBoardEvents();

            if (summonButton != null)
            {
                summonButton.onClick.RemoveListener(HandleSummonClicked);
                summonButton.onClick.AddListener(HandleSummonClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(HandleSaveClicked);
                saveButton.onClick.AddListener(HandleSaveClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
                restartButton.onClick.AddListener(HandleRestartClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(HandleResetClicked);
                resetButton.onClick.AddListener(HandleResetClicked);
            }
        }

        public void UpdateStage(string stageId)
        {
            if (stageText != null)
            {
                stageText.text = string.IsNullOrEmpty(stageId) ? "Stage -" : $"Stage {stageId}";
            }
        }

        public void UpdateRemainingEnemies(int remainingEnemies)
        {
            if (remainingEnemiesText != null)
            {
                remainingEnemiesText.text = $"남은 적 {remainingEnemies}";
            }
        }

        public void UpdateBaseHp(int baseHp)
        {
            if (baseHpText != null)
            {
                baseHpText.text = $"Base HP {baseHp}";
            }
        }

        public void UpdateRepresentativeUnit(int level)
        {
            if (representativeLevelText != null)
            {
                representativeLevelText.text = level <= 0 ? "대표 없음" : $"대표 Lv.{level}";
            }

            if (representativePreview == null)
            {
                return;
            }

            Image previewImage = representativePreview.GetComponent<Image>();
            if (previewImage != null)
            {
                previewImage.color = GetRepresentativeColor(level);
            }

            representativePreview.sizeDelta = level <= 0
                ? new Vector2(96f, 96f)
                : new Vector2(96f + level * 10f, 96f + level * 10f);

            Text previewLabel = representativePreview.GetComponentInChildren<Text>();
            if (previewLabel != null)
            {
                previewLabel.text = level <= 0 ? "대표\n없음" : $"Lv.{level}\n물정령";
            }
        }

        public void ShowGuideMessage(string message)
        {
            if (guideText != null)
            {
                guideText.text = message;
            }
        }

        public void ShowAttackPulse()
        {
            if (representativePreview == null || !gameObject.activeInHierarchy)
            {
                return;
            }

            if (attackPulseRoutine != null)
            {
                StopCoroutine(attackPulseRoutine);
            }

            attackPulseRoutine = StartCoroutine(AttackPulse());
        }

        public void UpdateBoardStateChanged()
        {
            HandleSummonAvailabilityChanged(boardManager != null && boardManager.CanSummon);
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

        private void HandleRestartClicked()
        {
            WaterGrow.Battle.BattleManager battleManager = FindObjectOfType<WaterGrow.Battle.BattleManager>();
            battleManager?.StartTestStage();
            ShowGuideMessage("테스트 스테이지 재시작");
        }

        private void HandleResetClicked()
        {
            SaveManager saveManager = gameManager == null ? FindObjectOfType<SaveManager>() : gameManager.SaveManager;
            saveManager?.ResetSave();

            if (boardManager != null && saveManager != null)
            {
                boardManager.Initialize(saveManager.Current);
            }

            ShowGuideMessage("저장 데이터 초기화");
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

        private void HandleBoardStateChanged()
        {
            UpdateBoardStateChanged();
        }

        private void RegisterBoardEvents()
        {
            if (boardManager == null || boardEventsRegistered)
            {
                return;
            }

            boardManager.GoldChanged += HandleGoldChanged;
            boardManager.SummonAvailabilityChanged += HandleSummonAvailabilityChanged;
            boardManager.RepresentativeChanged += HandleRepresentativeChanged;
            boardManager.BoardStateChanged += HandleBoardStateChanged;
            boardManager.BoardMessage += ShowGuideMessage;
            boardEventsRegistered = true;
        }

        private void UnregisterBoardEvents()
        {
            if (boardManager == null || !boardEventsRegistered)
            {
                return;
            }

            boardManager.GoldChanged -= HandleGoldChanged;
            boardManager.SummonAvailabilityChanged -= HandleSummonAvailabilityChanged;
            boardManager.RepresentativeChanged -= HandleRepresentativeChanged;
            boardManager.BoardStateChanged -= HandleBoardStateChanged;
            boardManager.BoardMessage -= ShowGuideMessage;
            boardEventsRegistered = false;
        }

        private void HandleRepresentativeChanged(MergeUnit unit)
        {
            UpdateRepresentativeUnit(unit == null ? 0 : unit.Level);

            if (guideText != null)
            {
                guideText.text = unit == null ? "소환 버튼으로 Lv.1 물방울을 만드세요." : $"Lv.{unit.Level} 물정령이 대표로 출전합니다.";
            }
        }

        private IEnumerator AttackPulse()
        {
            representativePreview.localScale = Vector3.one * 1.16f;
            yield return new WaitForSeconds(0.08f);
            representativePreview.localScale = Vector3.one;
            attackPulseRoutine = null;
        }

        private static Color GetRepresentativeColor(int level)
        {
            return level switch
            {
                1 => new Color(0.25f, 0.67f, 1f),
                2 => new Color(0.18f, 0.78f, 0.95f),
                3 => new Color(0.14f, 0.88f, 0.72f),
                4 => new Color(0.38f, 0.72f, 1f),
                5 => new Color(0.72f, 0.88f, 1f),
                _ => new Color(0.16f, 0.25f, 0.34f)
            };
        }
    }
}
