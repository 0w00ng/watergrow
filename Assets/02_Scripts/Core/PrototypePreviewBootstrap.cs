using WaterGrow.Battle;
using WaterGrow.Board;
using WaterGrow.Reward;
using WaterGrow.Stage;
using WaterGrow.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WaterGrow.Core
{
    public class PrototypePreviewBootstrap : MonoBehaviour
    {
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080, 1920);

        private Font defaultFont;

        private void Awake()
        {
            RebuildPreviewScene();
        }

        public void RebuildPreviewScene()
        {
            ClearGeneratedPreviewObjects();
            defaultFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 24);
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            Camera mainCamera = CreateMainCamera();
            EnsureEventSystem();

            GameObject systemsRoot = new GameObject("Systems");
            DataManager dataManager = systemsRoot.AddComponent<DataManager>();
            SaveManager saveManager = systemsRoot.AddComponent<SaveManager>();
            BoardManager boardManager = systemsRoot.AddComponent<BoardManager>();
            UIManager uiManager = systemsRoot.AddComponent<UIManager>();
            StageManager stageManager = systemsRoot.AddComponent<StageManager>();
            RewardManager rewardManager = systemsRoot.AddComponent<RewardManager>();
            UpgradeManager upgradeManager = systemsRoot.AddComponent<UpgradeManager>();
            EnemySpawner enemySpawner = systemsRoot.AddComponent<EnemySpawner>();
            BattleManager battleManager = systemsRoot.AddComponent<BattleManager>();
            GameManager gameManager = systemsRoot.AddComponent<GameManager>();

            Canvas canvas = CreateCanvas(mainCamera);
            RectTransform phoneFrame = CreatePhoneFrame(canvas.transform);
            RectTransform safeArea = CreatePanel("SafeArea", phoneFrame, new Color(0.72f, 0.91f, 1f, 1f));
            Stretch(safeArea);
            BuildBackground(safeArea);

            RectTransform hudPanel = CreatePanel("HudPanel", safeArea, new Color(1f, 1f, 1f, 0f));
            SetAnchors(hudPanel, new Vector2(0.03f, 0.945f), new Vector2(0.97f, 0.99f));

            Text stageText = CreateHudCard(hudPanel, "StageCard", ">  Stage 1-1", new Vector2(0.00f, 0f), new Vector2(0.24f, 1f), new Color(0.06f, 0.26f, 0.42f));
            Text remainingText = CreateHudCard(hudPanel, "WaveCard", "~  Enemies 10", new Vector2(0.255f, 0f), new Vector2(0.47f, 1f), new Color(0.10f, 0.45f, 0.40f));
            Text goldText = CreateHudCard(hudPanel, "GoldCard", "Gold 100", new Vector2(0.485f, 0f), new Vector2(0.70f, 1f), new Color(0.78f, 0.48f, 0.05f));
            Text baseHpText = CreateHudCard(hudPanel, "BaseHpCard", "Base HP 5", new Vector2(0.715f, 0f), new Vector2(0.92f, 1f), new Color(0.05f, 0.36f, 0.72f));
            CreateHudCard(hudPanel, "PauseCard", "II", new Vector2(0.935f, 0f), new Vector2(1f, 1f), new Color(0.08f, 0.35f, 0.25f));

            RectTransform battleField = CreatePanel("BattleField", safeArea, new Color(0.86f, 0.97f, 1f, 0.86f));
            SetAnchors(battleField, new Vector2(0.035f, 0.66f), new Vector2(0.965f, 0.935f));
            AddShadow(battleField.gameObject, new Vector2(0f, -6f), new Color(0.04f, 0.20f, 0.32f, 0.18f));
            BuildBattleField(battleField);

            Text representativeText = CreateText("RepresentativeText", battleField, "대표 출전: 없음", new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.94f), TextAnchor.MiddleCenter, 28, new Color(0.08f, 0.30f, 0.26f));
            representativeText.fontStyle = FontStyle.Bold;

            Transform targetPoint = CreateMarker("BasePoint", battleField, new Vector2(0.24f, 0.34f), new Color(0.10f, 0.46f, 0.92f), new Vector2(76f, 96f), "WATER");
            Transform spawnPoint = CreateMarker("SpawnPoint", battleField, new Vector2(0.92f, 0.34f), new Color(0.78f, 0.69f, 0.52f), new Vector2(76f, 116f), "BASE");
            RectTransform waterUnitPreview = (RectTransform)CreateMarker("WaterUnitPreview", battleField, new Vector2(0.22f, 0.36f), new Color(0.72f, 0.94f, 1f), new Vector2(150f, 150f), "WATER");

            RectTransform enemyRoot = new GameObject("EnemyRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            enemyRoot.SetParent(battleField, false);
            Stretch(enemyRoot);
            enemySpawner.Configure(spawnPoint, targetPoint, enemyRoot);

            RectTransform wavePanel = CreatePanel("WaveStatusPanel", safeArea, new Color(1f, 1f, 1f, 0.82f));
            SetAnchors(wavePanel, new Vector2(0.035f, 0.605f), new Vector2(0.965f, 0.65f));
            AddShadow(wavePanel.gameObject, new Vector2(0f, -4f), new Color(0.10f, 0.24f, 0.18f, 0.18f));
            CreateText("WaveTimerText", wavePanel, "다음 웨이브  00:05        ----", new Vector2(0.06f, 0f), new Vector2(0.94f, 1f), TextAnchor.MiddleCenter, 30, new Color(0.12f, 0.22f, 0.20f));

            RectTransform boardPanel = CreatePanel("MergeBoardPanel", safeArea, new Color(0.94f, 1f, 0.97f, 0.90f));
            SetAnchors(boardPanel, new Vector2(0.035f, 0.15f), new Vector2(0.965f, 0.595f));
            AddShadow(boardPanel.gameObject, new Vector2(0f, -7f), new Color(0.10f, 0.24f, 0.18f, 0.22f));

            GridLayoutGroup grid = boardPanel.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.spacing = new Vector2(12f, 12f);
            grid.padding = new RectOffset(14, 14, 18, 18);
            grid.cellSize = new Vector2(150f, 188f);

            BoardCell cellPrefab = CreateBoardCellPrefab(canvas.transform);
            boardManager.ConfigureBoard(boardPanel, cellPrefab);

            Text guideText = CreateText("GuideText", safeArea, "같은 등급을 합치면 더 높은 등급의 물정령이 됩니다.", new Vector2(0.11f, 0.125f), new Vector2(0.89f, 0.145f), TextAnchor.MiddleCenter, 24, new Color(0.12f, 0.42f, 0.29f));

            RectTransform actionDock = CreatePanel("ActionDock", safeArea, new Color(1f, 1f, 1f, 0.86f));
            SetAnchors(actionDock, new Vector2(0.035f, 0.02f), new Vector2(0.965f, 0.118f));
            AddShadow(actionDock.gameObject, new Vector2(0f, -6f), new Color(0.08f, 0.22f, 0.16f, 0.28f));

            Button upgradeButton = CreateButton("UpgradeButton", actionDock, "강화", new Vector2(0.04f, 0.15f), new Vector2(0.22f, 0.85f), new Color(0.94f, 1f, 0.95f), 27);
            Text upgradeLabel = upgradeButton.GetComponentInChildren<Text>();
            if (upgradeLabel != null)
            {
                SetAnchors(upgradeLabel.GetComponent<RectTransform>(), new Vector2(0f, 0.40f), Vector2.one);
                upgradeLabel.color = new Color(0.10f, 0.43f, 0.25f);
            }
            Text upgradeText = CreateText("UpgradeText", upgradeButton.GetComponent<RectTransform>(), "ATK Lv.0", new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.42f), TextAnchor.MiddleCenter, 15, new Color(0.10f, 0.43f, 0.25f));
            Button summonButton = CreateButton("SummonButton", actionDock, "물방울 소환 100", new Vector2(0.27f, 0.10f), new Vector2(0.73f, 0.90f), new Color(0.18f, 0.76f, 0.74f), 38);
            Button restartButton = CreateButton("RestartButton", actionDock, "재도전", new Vector2(0.78f, 0.15f), new Vector2(0.89f, 0.85f), new Color(0.94f, 1f, 0.95f), 20);
            Button resetButton = CreateButton("ResetButton", actionDock, "초기화", new Vector2(0.90f, 0.15f), new Vector2(0.98f, 0.85f), new Color(0.96f, 0.93f, 0.90f), 18);
            Button saveButton = null;

            dataManager.Load();
            saveManager.Load();
            rewardManager.Configure(boardManager, saveManager);
            upgradeManager.Configure(boardManager, saveManager);
            uiManager.Configure(boardManager, gameManager, summonButton, saveButton, upgradeButton, restartButton, resetButton, goldText, guideText, stageText, remainingText, baseHpText, upgradeText, representativeText, waterUnitPreview, upgradeManager);
            battleManager.Configure(boardManager, enemySpawner, stageManager, uiManager, dataManager, rewardManager, upgradeManager, representativeText, battleField);
            boardManager.Initialize(saveManager.Current);
            uiManager.RefreshAll();
        }

        private void BuildPreviewScene()
        {
            RebuildPreviewScene();
        }

        private void ClearGeneratedPreviewObjects()
        {
            DeleteIfExists("Systems");
            DeleteIfExists("PrototypePreviewCanvas");
            DeleteIfExists("Main Camera");
            DeleteIfExists("EventSystem");
        }

        private Camera CreateMainCamera()
        {
            Camera existing = Camera.main;
            if (existing != null)
            {
                existing.orthographic = true;
                existing.orthographicSize = 5f;
                existing.clearFlags = CameraClearFlags.SolidColor;
                existing.backgroundColor = new Color(0.72f, 0.91f, 1f);
                existing.transform.position = new Vector3(0f, 0f, -10f);
                return existing;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.72f, 0.91f, 1f);
            camera.transform.position = new Vector3(0f, 0f, -10f);
            return camera;
        }

        private Canvas CreateCanvas(Camera mainCamera)
        {
            GameObject canvasObject = new GameObject("PrototypePreviewCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = 1f;
            return canvas;
        }

        private RectTransform CreatePhoneFrame(Transform canvasRoot)
        {
            RectTransform backdrop = CreatePanel("CanvasBackdrop", canvasRoot, new Color(0.05f, 0.20f, 0.29f, 1f));
            Stretch(backdrop);

            RectTransform frame = CreatePanel("PhoneFrame_9x16", backdrop, new Color(0.72f, 0.91f, 1f, 1f));
            Stretch(frame);

            AspectRatioFitter fitter = frame.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = referenceResolution.x / referenceResolution.y;

            AddShadow(frame.gameObject, new Vector2(0f, -10f), new Color(0f, 0.08f, 0.12f, 0.45f));
            return frame;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private void BuildBackground(RectTransform parent)
        {
            RectTransform topSky = CreatePanel("SkyWash", parent, new Color(0.82f, 0.96f, 1f, 1f));
            SetAnchors(topSky, new Vector2(0f, 0.56f), Vector2.one);

            RectTransform lowerGarden = CreatePanel("GardenWash", parent, new Color(0.91f, 0.98f, 0.82f, 1f));
            SetAnchors(lowerGarden, Vector2.zero, new Vector2(1f, 0.63f));

            RectTransform border = CreatePanel("SoftGreenBorder", parent, new Color(0.47f, 0.70f, 0.28f, 0.22f));
            SetAnchors(border, new Vector2(0.006f, 0.006f), new Vector2(0.994f, 0.994f));
            Outline borderOutline = border.gameObject.AddComponent<Outline>();
            borderOutline.effectColor = new Color(0.35f, 0.55f, 0.20f, 0.45f);
            borderOutline.effectDistance = new Vector2(4f, 4f);

            for (int i = 0; i < 5; i++)
            {
                RectTransform bubble = CreatePanel($"Bubble_{i + 1}", parent, new Color(1f, 1f, 1f, 0.18f));
                bubble.anchorMin = new Vector2(0.10f + i * 0.17f, 0.80f - (i % 2) * 0.06f);
                bubble.anchorMax = bubble.anchorMin;
                bubble.pivot = new Vector2(0.5f, 0.5f);
                bubble.sizeDelta = new Vector2(42f + i * 10f, 42f + i * 10f);
            }
        }

        private void BuildBattleField(RectTransform battleField)
        {
            RectTransform sky = CreatePanel("BattleSky", battleField, new Color(0.77f, 0.94f, 1f, 0.75f));
            SetAnchors(sky, new Vector2(0f, 0.46f), Vector2.one);

            RectTransform grass = CreatePanel("BattleGrass", battleField, new Color(0.78f, 0.91f, 0.58f, 0.78f));
            SetAnchors(grass, Vector2.zero, new Vector2(1f, 0.52f));

            RectTransform waterZone = CreatePanel("WaterZone", battleField, new Color(0.68f, 0.90f, 1f, 0.48f));
            SetAnchors(waterZone, new Vector2(0.03f, 0.12f), new Vector2(0.38f, 0.58f));

            RectTransform fireZone = CreatePanel("FireZone", battleField, new Color(1f, 0.58f, 0.22f, 0.30f));
            SetAnchors(fireZone, new Vector2(0.54f, 0.12f), new Vector2(0.96f, 0.58f));

            RectTransform lane = CreatePanel("BattleLane", battleField, new Color(0.96f, 0.86f, 0.55f, 0.50f));
            SetAnchors(lane, new Vector2(0.22f, 0.30f), new Vector2(0.88f, 0.39f));

            RectTransform laneHighlight = CreatePanel("BattleLaneHighlight", battleField, new Color(1f, 1f, 1f, 0.55f));
            SetAnchors(laneHighlight, new Vector2(0.24f, 0.37f), new Vector2(0.84f, 0.40f));

            CreateText("WaterSideLabel", battleField, "출전 중", new Vector2(0.10f, 0.57f), new Vector2(0.27f, 0.69f), TextAnchor.MiddleCenter, 23, new Color(0.05f, 0.36f, 0.30f)).fontStyle = FontStyle.Bold;
            CreateText("EnemySideLabel", battleField, "불꽃 병사", new Vector2(0.50f, 0.54f), new Vector2(0.72f, 0.66f), TextAnchor.MiddleCenter, 22, new Color(0.72f, 0.20f, 0.08f));
            CreateText("EnemySideLabel2", battleField, "불꽃 방패병", new Vector2(0.70f, 0.54f), new Vector2(0.92f, 0.66f), TextAnchor.MiddleCenter, 22, new Color(0.72f, 0.20f, 0.08f));
        }

        private RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel.GetComponent<RectTransform>();
        }

        private Text CreateHudCard(RectTransform parent, string name, string value, Vector2 anchorMin, Vector2 anchorMax, Color textColor)
        {
            RectTransform card = CreatePanel(name, parent, new Color(0.96f, 1f, 0.98f, 0.90f));
            SetAnchors(card, anchorMin, anchorMax);
            AddShadow(card.gameObject, new Vector2(0f, -3f), new Color(0.08f, 0.22f, 0.16f, 0.18f));

            Outline outline = card.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.33f, 0.70f, 0.62f, 0.55f);
            outline.effectDistance = new Vector2(2f, 2f);

            Text text = CreateText("Label", card, value, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f), TextAnchor.MiddleCenter, 25, textColor);
            text.fontStyle = FontStyle.Bold;
            return text;
        }

        private Text CreateText(string name, RectTransform parent, string value, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment, int fontSize, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = defaultFont;
            text.text = value;
            text.color = color;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = fontSize;

            RectTransform rect = textObject.GetComponent<RectTransform>();
            SetAnchors(rect, anchorMin, anchorMax);
            return text;
        }

        private Button CreateButton(string name, RectTransform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color color, int fontSize)
        {
            RectTransform rect = CreatePanel(name, parent, color);
            SetAnchors(rect, anchorMin, anchorMax);
            AddShadow(rect.gameObject, new Vector2(0f, -6f), new Color(0.03f, 0.13f, 0.18f, 0.35f));
            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.45f);
            outline.effectDistance = new Vector2(2f, 2f);

            Button button = rect.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.colors = CreateButtonColors(color);

            Text text = CreateText("Label", rect, label, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, fontSize, Color.white);
            text.fontStyle = FontStyle.Bold;
            return button;
        }

        private BoardCell CreateBoardCellPrefab(Transform parent)
        {
            RectTransform cellRect = CreatePanel("BoardCellPrefab", parent, new Color(0.18f, 0.55f, 0.78f, 1f));
            cellRect.gameObject.SetActive(false);
            AddShadow(cellRect.gameObject, new Vector2(0f, -3f), new Color(0f, 0.05f, 0.08f, 0.35f));
            Outline outline = cellRect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.28f);
            outline.effectDistance = new Vector2(2f, 2f);

            BoardCell cell = cellRect.gameObject.AddComponent<BoardCell>();
            Text levelText = CreateText("LevelText", cellRect, "B00", Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 30, Color.white);
            levelText.fontStyle = FontStyle.Bold;

            RectTransform marker = CreatePanel("RepresentativeIcon", cellRect, new Color(1f, 0.86f, 0.18f, 1f));
            SetAnchors(marker, new Vector2(0.70f, 0.68f), new Vector2(0.95f, 0.95f));
            CreateText("Crown", marker, "*", Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 24, new Color(0.36f, 0.21f, 0f));
            marker.gameObject.SetActive(false);

            cell.ConfigureVisuals(cellRect.GetComponent<Image>(), levelText, marker.gameObject);
            return cell;
        }

        private Transform CreateMarker(string name, RectTransform parent, Vector2 anchorPosition, Color color, Vector2 size, string label)
        {
            RectTransform glow = CreatePanel($"{name}Glow", parent, new Color(color.r, color.g, color.b, 0.22f));
            glow.anchorMin = anchorPosition;
            glow.anchorMax = anchorPosition;
            glow.pivot = new Vector2(0.5f, 0.5f);
            glow.sizeDelta = size * 1.55f;
            glow.anchoredPosition = Vector2.zero;

            RectTransform marker = CreatePanel(name, parent, color);
            marker.anchorMin = anchorPosition;
            marker.anchorMax = anchorPosition;
            marker.pivot = new Vector2(0.5f, 0.5f);
            marker.sizeDelta = size;
            marker.anchoredPosition = Vector2.zero;
            AddShadow(marker.gameObject, new Vector2(0f, -5f), new Color(0.02f, 0.08f, 0.12f, 0.30f));

            Text text = CreateText($"{name}Label", marker, label, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 20, Color.white);
            text.fontStyle = FontStyle.Bold;
            return marker;
        }

        private static ColorBlock CreateButtonColors(Color baseColor)
        {
            return new ColorBlock
            {
                normalColor = baseColor,
                highlightedColor = Color.Lerp(baseColor, Color.white, 0.16f),
                pressedColor = Color.Lerp(baseColor, Color.black, 0.12f),
                selectedColor = baseColor,
                disabledColor = new Color(0.40f, 0.48f, 0.52f, 0.55f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };
        }

        private static void AddShadow(GameObject target, Vector2 distance, Color color)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectDistance = distance;
            shadow.effectColor = color;
        }

        private static void DeleteIfExists(string objectName)
        {
            GameObject[] objects = FindObjectsOfType<GameObject>();
            foreach (GameObject target in objects)
            {
                if (target == null)
                {
                    continue;
                }

                if (target.name != objectName)
                {
                    continue;
                }

                DestroyImmediate(target);
            }
        }

        private static void Stretch(RectTransform rect)
        {
            SetAnchors(rect, Vector2.zero, Vector2.one);
        }

        private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
