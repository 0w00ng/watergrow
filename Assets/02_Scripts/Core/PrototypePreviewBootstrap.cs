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
            if (FindObjectOfType<BoardManager>() != null)
            {
                return;
            }

            BuildPreviewScene();
        }

        public void RebuildPreviewScene()
        {
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
            RectTransform safeArea = CreatePanel("SafeArea", canvas.transform, new Color(0.06f, 0.08f, 0.11f, 1f));
            Stretch(safeArea);

            Text stageText = CreateHudText("StageText", safeArea, "Stage STAGE_1_01", new Vector2(0.04f, 0.96f), new Vector2(0.46f, 0.995f), TextAnchor.MiddleLeft);
            Text remainingText = CreateHudText("RemainingText", safeArea, "Enemies 10", new Vector2(0.52f, 0.96f), new Vector2(0.96f, 0.995f), TextAnchor.MiddleRight);
            Text baseHpText = CreateHudText("BaseHpText", safeArea, "Base HP 5", new Vector2(0.04f, 0.925f), new Vector2(0.46f, 0.955f), TextAnchor.MiddleLeft);
            Text goldText = CreateHudText("GoldText", safeArea, "Gold 100", new Vector2(0.52f, 0.925f), new Vector2(0.96f, 0.955f), TextAnchor.MiddleRight);

            RectTransform battleField = CreatePanel("BattleField", safeArea, new Color(0.11f, 0.18f, 0.20f, 1f));
            SetAnchors(battleField, new Vector2(0.04f, 0.46f), new Vector2(0.96f, 0.90f));
            CreateBattleLabels(battleField);

            Text representativeText = CreateHudText("RepresentativeText", battleField, "Representative: None", new Vector2(0.04f, 0.78f), new Vector2(0.96f, 0.94f), TextAnchor.MiddleCenter);
            Text guideText = CreateHudText("GuideText", battleField, "Tap Summon to create Lv.1 water units. Tap two same-level units to merge.", new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.15f), TextAnchor.MiddleCenter);

            Transform targetPoint = CreateUiMarker("BasePoint", battleField, new Vector2(0.12f, 0.48f), new Color(0.2f, 0.55f, 1f), new Vector2(72f, 72f));
            Transform spawnPoint = CreateUiMarker("SpawnPoint", battleField, new Vector2(0.88f, 0.48f), new Color(1f, 0.35f, 0.08f), new Vector2(72f, 72f));
            RectTransform waterUnitPreview = (RectTransform)CreateUiMarker("WaterUnitPreview", battleField, new Vector2(0.23f, 0.48f), new Color(0.25f, 0.67f, 1f), new Vector2(96f, 96f));
            Text waterLabel = CreateHudText("WaterUnitLabel", waterUnitPreview, "Water", Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
            waterLabel.fontSize = 22;

            RectTransform enemyRoot = new GameObject("EnemyRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            enemyRoot.SetParent(battleField, false);
            Stretch(enemyRoot);
            enemySpawner.Configure(spawnPoint, targetPoint, enemyRoot);

            RectTransform boardPanel = CreatePanel("MergeBoardPanel", safeArea, new Color(0.09f, 0.12f, 0.16f, 1f));
            SetAnchors(boardPanel, new Vector2(0.04f, 0.13f), new Vector2(0.96f, 0.43f));
            GridLayoutGroup grid = boardPanel.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.spacing = new Vector2(8f, 8f);
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.cellSize = new Vector2(150f, 120f);

            BoardCell cellPrefab = CreateBoardCellPrefab(canvas.transform);
            boardManager.ConfigureBoard(boardPanel, cellPrefab);

            Button summonButton = CreateButton("SummonButton", safeArea, "Summon", new Vector2(0.04f, 0.04f), new Vector2(0.46f, 0.105f), new Color(0.16f, 0.47f, 0.88f));
            Button restartButton = CreateButton("RestartButton", safeArea, "Restart", new Vector2(0.49f, 0.04f), new Vector2(0.68f, 0.105f), new Color(0.18f, 0.42f, 0.36f));
            Button resetButton = CreateButton("ResetButton", safeArea, "Reset", new Vector2(0.71f, 0.04f), new Vector2(0.96f, 0.105f), new Color(0.42f, 0.18f, 0.20f));
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

        private Camera CreateMainCamera()
        {
            Camera existing = Camera.main;
            if (existing != null)
            {
                existing.orthographic = true;
                existing.orthographicSize = 5f;
                existing.transform.position = new Vector3(0f, 0f, -10f);
                return existing;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.11f);
            camera.transform.position = new Vector3(0f, 0f, -10f);
            return camera;
        }

        private Canvas CreateCanvas(Camera mainCamera)
        {
            GameObject canvasObject = new GameObject("PrototypePreviewCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCamera;
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

        private RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel.GetComponent<RectTransform>();
        }

        private Text CreateHudText(string name, RectTransform parent, string value, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = defaultFont;
            text.text = value;
            text.color = Color.white;
            text.fontSize = 34;
            text.alignment = alignment;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 18;
            text.resizeTextMaxSize = 36;

            RectTransform rect = textObject.GetComponent<RectTransform>();
            SetAnchors(rect, anchorMin, anchorMax);
            return text;
        }

        private Button CreateButton(string name, RectTransform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            RectTransform rect = CreatePanel(name, parent, color);
            SetAnchors(rect, anchorMin, anchorMax);
            Button button = rect.gameObject.AddComponent<Button>();

            Text text = CreateHudText("Label", rect, label, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
            text.fontSize = 38;
            return button;
        }

        private BoardCell CreateBoardCellPrefab(Transform parent)
        {
            RectTransform cellRect = CreatePanel("BoardCellPrefab", parent, new Color(0.16f, 0.22f, 0.28f, 1f));
            cellRect.gameObject.SetActive(false);
            BoardCell cell = cellRect.gameObject.AddComponent<BoardCell>();

            Text levelText = CreateHudText("LevelText", cellRect, "B00", Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
            levelText.fontSize = 32;

            RectTransform marker = CreatePanel("RepresentativeIcon", cellRect, new Color(0.1f, 0.72f, 1f, 1f));
            SetAnchors(marker, new Vector2(0.72f, 0.72f), new Vector2(0.94f, 0.94f));
            marker.gameObject.SetActive(false);

            cell.ConfigureVisuals(cellRect.GetComponent<Image>(), levelText, marker.gameObject);
            return cell;
        }

        private void CreateBattleLabels(RectTransform battleField)
        {
            CreateHudText("WaterSideLabel", battleField, "Water Spirit", new Vector2(0.05f, 0.52f), new Vector2(0.30f, 0.68f), TextAnchor.MiddleCenter).color = new Color(0.45f, 0.78f, 1f);
            CreateHudText("EnemySideLabel", battleField, "Fire Soldier", new Vector2(0.70f, 0.52f), new Vector2(0.95f, 0.68f), TextAnchor.MiddleCenter).color = new Color(1f, 0.55f, 0.20f);
        }

        private Transform CreateUiMarker(string name, RectTransform parent, Vector2 anchorPosition, Color color, Vector2 size)
        {
            RectTransform marker = CreatePanel(name, parent, color);
            marker.anchorMin = anchorPosition;
            marker.anchorMax = anchorPosition;
            marker.pivot = new Vector2(0.5f, 0.5f);
            marker.sizeDelta = size;
            marker.anchoredPosition = Vector2.zero;
            return marker;
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
