using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace WaterGrow.Battle
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private Slider hpBar;
        [SerializeField] private Image bodyImage;
        [SerializeField] private Text nameText;
        [SerializeField] private float reachDistance = 0.05f;
        [SerializeField] private float uiMoveSpeedMultiplier = 95f;
        [SerializeField] private float minHpScale = 0.62f;
        [SerializeField] private float maxHpScale = 1.08f;
        [SerializeField] private Color normalColor = new Color(1f, 0.35f, 0.08f);
        [SerializeField] private Color hitColor = new Color(1f, 0.92f, 0.25f);

        private EnemyData data;
        private Transform targetPoint;
        private int maxHp;
        private int currentHp;
        private bool isResolved;
        private Action<EnemyController> killedCallback;
        private Action<EnemyController> reachedBaseCallback;
        private Coroutine hitFlashRoutine;
        private CanvasGroup canvasGroup;
        private bool usesRectTransform;
        private Vector3 baseScale = Vector3.one;

        public bool IsDead => currentHp <= 0;
        public string EnemyId => data == null ? string.Empty : data.enemyId;
        public int RewardGold => data == null ? 0 : data.rewardGold;
        public int DamageToBase => data == null ? 0 : data.damageToBase;
        public float DistanceToTarget => targetPoint == null ? float.MaxValue : Vector3.Distance(transform.position, targetPoint.position);

        public void Initialize(EnemyData enemyData, Transform target, Action<EnemyController> onKilled, Action<EnemyController> onReachedBase)
        {
            data = enemyData;
            targetPoint = target;
            killedCallback = onKilled;
            reachedBaseCallback = onReachedBase;

            maxHp = Mathf.Max(1, data == null ? 1 : data.hp);
            currentHp = maxHp;
            isResolved = false;
            usesRectTransform = transform is RectTransform;
            baseScale = transform.localScale;
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && usesRectTransform)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (nameText != null)
            {
                nameText.text = data == null ? "Enemy" : data.enemyNameKo;
            }

            RefreshHpBar();
            ApplyHpScale();
        }

        public void ConfigurePreviewVisual(Image body, Slider hp, Text label)
        {
            bodyImage = body;
            hpBar = hp;
            nameText = label;

            if (bodyImage != null)
            {
                bodyImage.color = normalColor;
            }
        }

        private void Update()
        {
            if (isResolved || targetPoint == null || data == null)
            {
                return;
            }

            float speed = data.moveSpeed * (usesRectTransform ? uiMoveSpeedMultiplier : 1f);
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

            if (DistanceToTarget <= reachDistance)
            {
                isResolved = true;
                reachedBaseCallback?.Invoke(this);
                StartCoroutine(ResolveAndDestroy(false));
            }
        }

        public void TakeDamage(int damage)
        {
            if (isResolved)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, damage));
            RefreshHpBar();
            ApplyHpScale();
            PlayHitFeedback();

            if (IsDead)
            {
                isResolved = true;
                killedCallback?.Invoke(this);
                StartCoroutine(ResolveAndDestroy(true));
            }
        }

        private void PlayHitFeedback()
        {
            if (bodyImage == null || !gameObject.activeInHierarchy)
            {
                return;
            }

            if (hitFlashRoutine != null)
            {
                StopCoroutine(hitFlashRoutine);
            }

            hitFlashRoutine = StartCoroutine(HitFlash());
        }

        private void RefreshHpBar()
        {
            if (hpBar != null)
            {
                hpBar.value = (float)currentHp / maxHp;
            }
        }

        private void ApplyHpScale()
        {
            float hpRatio = maxHp <= 0 ? 0f : Mathf.Clamp01((float)currentHp / maxHp);
            float scale = Mathf.Lerp(minHpScale, maxHpScale, hpRatio);
            transform.localScale = baseScale * scale;
        }

        private IEnumerator HitFlash()
        {
            bodyImage.color = hitColor;
            Vector3 hpScale = transform.localScale;
            transform.localScale = hpScale * 1.12f;
            yield return new WaitForSeconds(0.08f);
            bodyImage.color = normalColor;
            ApplyHpScale();
            hitFlashRoutine = null;
        }

        private IEnumerator ResolveAndDestroy(bool killed)
        {
            float elapsed = 0f;
            const float duration = 0.22f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = killed ? Vector3.one * 0.82f : Vector3.one * 0.92f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
