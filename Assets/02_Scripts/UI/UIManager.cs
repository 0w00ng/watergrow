using System.Collections;
using WaterGrow.Board;
using WaterGrow.Core;
using WaterGrow.Reward;
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
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Text goldText;
        [SerializeField] private Text guideText;
        [SerializeField] private Text stageText;
        [SerializeField] private Text remainingEnemiesText;
        [SerializeField] private Text baseHpText;
        [SerializeField] private Text upgradeText;
        [SerializeField] private Text representativeLevelText;
        [SerializeField] private RectTransform representativePreview;
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private float minBaseHpScale = 0.68f;
        [SerializeField] private float maxBaseHpScale = 1f;

        private bool boardEventsRegistered;
        private Coroutine attackPulseRoutine;
        private float currentBaseHpScale = 1f;

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

            if (upgradeManager == null)
            {
                upgradeManager = FindObjectOfType<UpgradeManager>();
            }
        }

        private void OnEnable()
        {
            RegisterBoardEvents();
            RegisterButtonEvents();
            RegisterUpgradeEvents();
            RefreshUpgrade();
        }

        private void OnDisable()
        {
            UnregisterBoardEvents();
            UnregisterButtonEvents();
            UnregisterUpgradeEvents();
        }

        public void Configure(
            BoardManager board,
            GameManager game,
            Button summon,
            Button save,
            Button upgrade,
            Button restart,
            Button reset,
            Text gold,
            Text guide,
            Text stage,
            Text remainingEnemies,
            Text baseHp,
            Text upgradeInfo,
            Text representativeLevel,
            RectTransform representativePreviewTarget = null,
            UpgradeManager upgradeMgr = null)
        {
            UnregisterBoardEvents();
            UnregisterButtonEvents();

            boardManager = board;
            gameManager = game;
            summonButton = summon;
            saveButton = save;
            upgradeButton = upgrade;
            restartButton = restart;
            resetButton = reset;
            goldText = gold;
            guideText = guide;
            stageText = stage;
            remainingEnemiesText = remainingEnemies;
            baseHpText = baseHp;
            upgradeText = upgradeInfo;
            representativeLevelText = representativeLevel;
            representativePreview = representativePreviewTarget;
            upgradeManager = upgradeMgr;

            RegisterBoardEvents();
            RegisterButtonEvents();
            RegisterUpgradeEvents();
            RefreshUpgrade();
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

        public void UpdateStage(string stageId)
        {
            if (stageText != null)
            {
                stageText.text = string.IsNullOrEmpty(stageId) ? ">  Stage -" : $">  Stage {stageId}";
            }
        }

        public void UpdateRemainingEnemies(int remainingEnemies)
        {
            if (remainingEnemiesText != null)
            {
                remainingEnemiesText.text = $"~  Enemies {remainingEnemies}";
            }
        }

        public void UpdateBaseHp(int baseHp)
        {
            UpdateBaseHp(baseHp, baseHp);
        }

        public void UpdateBaseHp(int baseHp, int maxBaseHp)
        {
            if (baseHpText != null)
            {
                baseHpText.text = $"Base HP {baseHp}";
            }

            if (representativePreview == null)
            {
                return;
            }

            float hpRatio = Mathf.Clamp01((float)Mathf.Max(0, baseHp) / Mathf.Max(1, maxBaseHp));
            currentBaseHpScale = Mathf.Lerp(minBaseHpScale, maxBaseHpScale, hpRatio);
            representativePreview.localScale = Vector3.one * currentBaseHpScale;
        }

        public void UpdateRepresentativeUnit(int level)
        {
            if (representativeLevelText != null)
            {
                representativeLevelText.text = level <= 0 ? "대표 출전: 없음" : $"대표 출전 물정령 Lv.{level}";
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
                ? new Vector2(124f, 124f)
                : new Vector2(124f + level * 8f, 124f + level * 8f);

            Text previewLabel = representativePreview.GetComponentInChildren<Text>();
            if (previewLabel != null)
            {
                previewLabel.text = level <= 0 ? "물" : $"물정령\nLv.{level}";
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

        public void SetRestartButtonLabel(string label)
        {
            if (restartButton == null)
            {
                return;
            }

            Text buttonText = restartButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = label;
            }
        }

        public void RefreshUpgrade()
        {
            if (upgradeText != null && upgradeManager != null)
            {
                upgradeText.text = $"ATK Lv.{upgradeManager.AttackUpgradeLevel}\n{upgradeManager.AttackUpgradeCost} Gold";
            }
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
            ShowGuideMessage("Saved.");
        }

        private void HandleRestartClicked()
        {
            Battle.BattleManager battleManager = FindObjectOfType<Battle.BattleManager>();
            battleManager?.StartNextOrRetryStage();
        }

        private void HandleUpgradeClicked()
        {
            if (upgradeManager == null)
            {
                return;
            }

            bool upgraded = upgradeManager.TryUpgradeAttack();
            RefreshUpgrade();
            ShowGuideMessage(upgraded
                ? $"공격력 강화 Lv.{upgradeManager.AttackUpgradeLevel} 완료."
                : $"공격력 강화에 Gold {upgradeManager.AttackUpgradeCost} 필요.");
        }

        private void HandleResetClicked()
        {
            SaveManager saveManager = gameManager == null ? FindObjectOfType<SaveManager>() : gameManager.SaveManager;
            saveManager?.ResetSave();

            if (boardManager != null && saveManager != null)
            {
                boardManager.Initialize(saveManager.Current);
            }

            upgradeManager?.ReloadFromSave();
            RefreshUpgrade();
            ShowGuideMessage("Save data reset.");
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

        private void HandleRepresentativeChanged(MergeUnit unit)
        {
            UpdateRepresentativeUnit(unit == null ? 0 : unit.Level);
            ShowGuideMessage(unit == null ? "Tap SUMMON to create a Lv.1 water unit." : $"Lv.{unit.Level} water unit is now fighting.");
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

        private void RegisterButtonEvents()
        {
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

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(HandleUpgradeClicked);
                upgradeButton.onClick.AddListener(HandleUpgradeClicked);
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

        private void UnregisterButtonEvents()
        {
            if (summonButton != null)
            {
                summonButton.onClick.RemoveListener(HandleSummonClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(HandleSaveClicked);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(HandleUpgradeClicked);
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

        private IEnumerator AttackPulse()
        {
            Vector3 baseScale = Vector3.one * currentBaseHpScale;
            representativePreview.localScale = baseScale * 1.16f;
            yield return new WaitForSeconds(0.08f);
            representativePreview.localScale = baseScale;
            attackPulseRoutine = null;
        }

        private void RegisterUpgradeEvents()
        {
            if (upgradeManager != null)
            {
                upgradeManager.UpgradeChanged -= RefreshUpgrade;
                upgradeManager.UpgradeChanged += RefreshUpgrade;
            }
        }

        private void UnregisterUpgradeEvents()
        {
            if (upgradeManager != null)
            {
                upgradeManager.UpgradeChanged -= RefreshUpgrade;
            }
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
                _ => new Color(0.10f, 0.62f, 1f)
            };
        }
    }
}
