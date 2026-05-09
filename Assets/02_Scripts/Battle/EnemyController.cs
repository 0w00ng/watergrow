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

            if (nameText != null)
            {
                nameText.text = data == null ? "Enemy" : data.enemyNameKo;
            }

            RefreshHpBar();
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

            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, data.moveSpeed * Time.deltaTime);

            if (DistanceToTarget <= reachDistance)
            {
                isResolved = true;
                reachedBaseCallback?.Invoke(this);
                Destroy(gameObject);
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
            PlayHitFeedback();

            if (IsDead)
            {
                isResolved = true;
                killedCallback?.Invoke(this);
                Destroy(gameObject);
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

        private IEnumerator HitFlash()
        {
            bodyImage.color = hitColor;
            transform.localScale = Vector3.one * 1.12f;
            yield return new WaitForSeconds(0.08f);
            bodyImage.color = normalColor;
            transform.localScale = Vector3.one;
            hitFlashRoutine = null;
        }
    }
}
