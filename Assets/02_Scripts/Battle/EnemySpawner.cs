using UnityEngine;
using System;
using UnityEngine.UI;

namespace WaterGrow.Battle
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private Transform enemyRoot;
        [SerializeField] private Color placeholderColor = new Color(1f, 0.35f, 0.08f);

        public void Configure(Transform spawn, Transform target, Transform root)
        {
            spawnPoint = spawn;
            targetPoint = target;
            enemyRoot = root;
        }

        public EnemyController SpawnEnemy(EnemyData enemyData, Action<EnemyController> onKilled, Action<EnemyController> onReachedBase)
        {
            EnemyController enemy;

            if (enemyPrefab != null)
            {
                enemy = Instantiate(enemyPrefab, GetSpawnPosition(), Quaternion.identity, enemyRoot == null ? transform : enemyRoot);
            }
            else
            {
                bool useUiPlaceholder = enemyRoot is RectTransform;
                GameObject placeholder = useUiPlaceholder
                    ? new GameObject(enemyData == null ? "Enemy" : enemyData.enemyNameKo, typeof(RectTransform))
                    : new GameObject(enemyData == null ? "Enemy" : enemyData.enemyNameKo);
                placeholder.transform.SetParent(enemyRoot == null ? transform : enemyRoot);
                placeholder.transform.position = GetSpawnPosition();

                if (useUiPlaceholder)
                {
                    RectTransform rect = placeholder.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(86f, 86f);
                    CreateEnemyGlow(rect);
                    Image image = placeholder.AddComponent<Image>();
                    image.color = placeholderColor;
                    Outline outline = placeholder.AddComponent<Outline>();
                    outline.effectColor = new Color(1f, 0.92f, 0.45f, 0.55f);
                    outline.effectDistance = new Vector2(3f, 3f);

                    Text label = CreateEnemyLabel(rect, enemyData == null ? "Enemy" : enemyData.enemyNameKo);
                    Slider hpBar = CreateHpBar(rect);
                    enemy = placeholder.AddComponent<EnemyController>();
                    enemy.ConfigurePreviewVisual(image, hpBar, label);
                }
                else
                {
                    SpriteRenderer renderer = placeholder.AddComponent<SpriteRenderer>();
                    renderer.sprite = CreatePlaceholderSprite();
                    renderer.color = placeholderColor;
                    placeholder.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
                    enemy = placeholder.AddComponent<EnemyController>();
                }

            }

            enemy.Initialize(enemyData, targetPoint, onKilled, onReachedBase);
            return enemy;
        }

        public Vector3 GetSpawnPosition()
        {
            return spawnPoint == null ? transform.position : spawnPoint.position;
        }

        public Transform TargetPoint => targetPoint;

        private static Sprite CreatePlaceholderSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        private static Slider CreateHpBar(RectTransform parent)
        {
            GameObject sliderObject = new GameObject("HPBar", typeof(RectTransform));
            sliderObject.transform.SetParent(parent, false);

            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.05f, 1.05f);
            sliderRect.anchorMax = new Vector2(0.95f, 1.20f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.transition = Selectable.Transition.None;

            GameObject background = new GameObject("Background", typeof(RectTransform));
            background.transform.SetParent(sliderRect, false);
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.24f, 0.05f, 0.04f, 1f);
            Stretch(background.GetComponent<RectTransform>());

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderRect, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillAreaRect, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.24f, 0.95f, 0.42f, 1f);
            Stretch(fill.GetComponent<RectTransform>());

            slider.fillRect = fill.GetComponent<RectTransform>();
            return slider;
        }

        private static Text CreateEnemyLabel(RectTransform parent, string labelText)
        {
            GameObject labelObject = new GameObject("NameLabel", typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(-0.5f, -0.38f);
            labelRect.anchorMax = new Vector2(1.5f, -0.05f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = labelText;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 0.86f, 0.62f, 1f);
            label.fontSize = 18;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = 18;
            return label;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateEnemyGlow(RectTransform parent)
        {
            GameObject glowObject = new GameObject("FlameGlow", typeof(RectTransform));
            glowObject.transform.SetParent(parent, false);
            RectTransform glowRect = glowObject.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(-0.20f, -0.20f);
            glowRect.anchorMax = new Vector2(1.20f, 1.20f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;

            Image glow = glowObject.AddComponent<Image>();
            glow.color = new Color(1f, 0.70f, 0.12f, 0.24f);
        }
    }
}
