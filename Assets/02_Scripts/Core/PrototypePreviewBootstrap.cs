using WaterGrow.Battle;
using WaterGrow.Board;
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
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Camera mainCamera = CreateMainCamera();
            EnsureEventSystem();

            GameObject systemsRoot = new GameObject("Systems");
            DataManager dataManager = systemsRoot.AddComponent<DataManager>();
            SaveManager saveManager = systemsRoot.AddComponent<SaveManager>();
            BoardManager boardManager = systemsRoot.AddComponent<BoardManager>();
            UIManager uiManager = systemsRoot.AddComponent<UIManager>();
            StageManager stageManager = systemsRoot.AddComponent<StageManager>();
            EnemySpawner enemySpawner = systemsRoot.AddComponent<EnemySpawner>();
            BattleManager battleManager = systemsRoot.AddComponent<BattleManager>();
            GameManager gameManager = systemsRoot.AddComponent<GameManager>();

            Canvas canvas = CreateCanvas(mainCamera);
            RectTransform safeArea = CreatePanel("SafeArea", canvas.transform, new Color(0.72f, 0.91f, 1f, 1f));
            Stretch(safeArea);
            BuildBackground(safeArea);

            Text titleText = CreateText("TitleText", safeArea, "WATER GROW", new Vector2(0.06f, 0.945f), new Vector2(0.94f, 0.99f), TextAnchor.MiddleCenter, 48, new Color(0.05f, 0.28f, 0.48f));
            titleText.fontStyle = FontStyle.Bold;

            RectTransform hudPanel = CreatePanel("HudPanel", safeArea, new Color(1f, 1f, 1f, 0.72f));
            SetAnchors(hudPanel, new Vector2(0.04f, 0.895f), new Vector2(0.96f, 0.94f));
            AddShadow(hudPanel.gameObject, new Vector2(0f, -4f), new Color(0.06f, 0.23f, 0.36f, 0.22f));

            Text stageText = CreateText("StageText", hudPanel, "Stage 1-1", new Vector2(0.03f, 0.08f), new Vector2(0.27f, 0.92f), TextAnchor.MiddleCenter, 25, new Color(0.06f, 0.26f, 0.42f));
            Text remainingText = CreateText("RemainingText", hudPanel, "Enemies 10", new Vector2(0.28f, 0.08f), new Vector2(0.53f, 0.92f), TextAnchor.MiddleCenter, 25, new Color(0.72f, 0.20f, 0.08f));
            Text baseHpText = CreateText("BaseHpText", hudPanel, "Base HP 5", new Vector2(0.54f, 0.08f), new Vector2(0.76f, 0.92f), TextAnchor.MiddleCenter, 25, new Color(0.05f, 0.36f, 0.72f));
            Text goldText = CreateText("GoldText", hudPanel, "Gold 100", new Vector2(0.77f, 0.08f), new Vector2(0.97f, 0.92f), TextAnchor.MiddleCenter, 25, new Color(0.78f, 0.48f, 0.05f));

            RectTransform battleField = CreatePanel("BattleField", safeArea, new Color(0.86f, 0.96f, 1f, 1f));
            SetAnchors(battleField, new Vector2(0.04f, 0.47f), new Vector2(0.96f, 0.875f));
            AddShadow(battleField.gameObject, new Vector2(0f, -8f), new Color(0.04f, 0.20f, 0.32f, 0.20f));
            BuildBattleField(battleField);

            Text representativeText = CreateText("RepresentativeText", battleField, "Representative: None", new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.96f), TextAnchor.MiddleCenter, 32, new Color(0.05f, 0.30f, 0.52f));
            representativeText.fontStyle = FontStyle.Bold;
            Text guideText = CreateText("GuideText", battleField, "Summon water drops, then drag one unit onto the same level to merge.", new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.13f), TextAnchor.MiddleCenter, 22, new Color(0.18f, 0.34f, 0.42f));

            Transform targetPoint = CreateMarker("BasePoint", battleField, new Vector2(0.12f, 0.46f), new Color(0.10f, 0.46f, 0.92f), new Vector2(88f, 88f), "BASE");
            Transform spawnPoint = CreateMarker("SpawnPoint", battleField, new Vector2(0.88f, 0.46f), new Color(1f, 0.35f, 0.10f), new Vector2(88f, 88f), "FIRE");
            RectTransform waterUnitPreview = (RectTransform)CreateMarker("WaterUnitPreview", battleField, new Vector2(0.25f, 0.46f), new Color(0.10f, 0.62f, 1f), new Vector2(124f, 124f), "WATER");

            RectTransform enemyRoot = new GameObject("EnemyRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            enemyRoot.SetParent(battleField, false);
            Stretch(enemyRoot);
            enemySpawner.Configure(spawnPoint, targetPoint, enemyRoot);

            RectTransform boardPanel = CreatePanel("MergeBoardPanel", safeArea, new Color(0.05f, 0.25f, 0.37f, 0.92f));
            SetAnchors(boardPanel, new Vector2(0.04f, 0.145f), new Vector2(0.96f, 0.435f));
            AddShadow(boardPanel.gameObject, new Vector2(0f, -8f), new Color(0.02f, 0.11f, 0.17f, 0.35f));
            CreateText("BoardTitle", boardPanel, "MERGE BOARD", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 0.99f), TextAnchor.MiddleCenter, 24, new Color(0.75f, 0.95f, 1f));

            GridLayoutGroup grid = boardPanel.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.spacing = new Vector2(10f, 10f);
            grid.padding = new RectOffset(14, 14, 42, 14);
            grid.cellSize = new Vector2(144f, 104f);

            BoardCell cellPrefab = CreateBoardCellPrefab(canvas.transform);
            boardManager.ConfigureBoard(boardPanel, cellPrefab);

            Button summonButton = CreateButton("SummonButton", safeArea, "SUMMON", new Vector2(0.04f, 0.04f), new Vector2(0.52f, 0.115f), new Color(0.05f, 0.50f, 0.92f), 38);
            Button restartButton = CreateButton("RestartButton", safeArea, "RETRY", new Vector2(0.55f, 0.04f), new Vector2(0.74f, 0.115f), new Color(0.16f, 0.62f, 0.48f), 28);
            Button resetButton = CreateButton("ResetButton", safeArea, "RESET", new Vector2(0.77f, 0.04f), new Vector2(0.96f, 0.115f), new Color(0.74f, 0.24f, 0.28f), 28);
            Button saveButton = null;

            dataManager.Load();
            saveManager.Load();
            uiManager.Configure(boardManager, gameManager, summonButton, saveButton, restartButton, resetButton, goldText, guideText, stageText, remainingText, baseHpText, representativeText, waterUnitPreview);
            battleManager.Configure(boardManager, enemySpawner, stageManager, uiManager, dataManager, representativeText, battleField);
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
            RectTransform topSky = CreatePanel("SkyWash", parent, new Color(0.80f, 0.95f, 1f, 1f));
            SetAnchors(topSky, new Vector2(0f, 0.48f), Vector2.one);

            RectTransform lowerSea = CreatePanel("SeaWash", parent, new Color(0.22f, 0.67f, 0.86f, 1f));
            SetAnchors(lowerSea, Vector2.zero, new Vector2(1f, 0.50f));

            for (int i = 0; i < 5; i++)
            {
                RectTransform bubble = CreatePanel($"Bubble_{i + 1}", parent, new Color(1f, 1f, 1f, 0.18f));
                bubble.anchorMin = new Vector2(0.08f + i * 0.18f, 0.78f - (i % 2) * 0.08f);
                bubble.anchorMax = bubble.anchorMin;
                bubble.pivot = new Vector2(0.5f, 0.5f);
                bubble.sizeDelta = new Vector2(42f + i * 10f, 42f + i * 10f);
            }
        }

        private void BuildBattleField(RectTransform battleField)
        {
            RectTransform waterZone = CreatePanel("WaterZone", battleField, new Color(0.55f, 0.86f, 1f, 0.45f));
            SetAnchors(waterZone, new Vector2(0.02f, 0.18f), new Vector2(0.45f, 0.78f));

            RectTransform fireZone = CreatePanel("FireZone", battleField, new Color(1f, 0.58f, 0.22f, 0.30f));
            SetAnchors(fireZone, new Vector2(0.55f, 0.18f), new Vector2(0.98f, 0.78f));

            RectTransform lane = CreatePanel("BattleLane", battleField, new Color(0.96f, 0.86f, 0.55f, 0.75f));
            SetAnchors(lane, new Vector2(0.12f, 0.38f), new Vector2(0.88f, 0.54f));

            RectTransform laneHighlight = CreatePanel("BattleLaneHighlight", battleField, new Color(1f, 1f, 1f, 0.40f));
            SetAnchors(laneHighlight, new Vector2(0.14f, 0.48f), new Vector2(0.86f, 0.52f));

            CreateText("WaterSideLabel", battleField, "WATER SIDE", new Vector2(0.05f, 0.62f), new Vector2(0.32f, 0.75f), TextAnchor.MiddleCenter, 24, new Color(0.05f, 0.36f, 0.68f));
            CreateText("EnemySideLabel", battleField, "FIRE SIDE", new Vector2(0.68f, 0.62f), new Vector2(0.95f, 0.75f), TextAnchor.MiddleCenter, 24, new Color(0.72f, 0.20f, 0.08f));
        }

        private RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel.GetComponent<RectTransform>();
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
