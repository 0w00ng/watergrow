using WaterGrow.Board;
using UnityEngine;
using UnityEngine.UI;

namespace WaterGrow.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Text representativeText;

        private MergeUnit representative;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }
        }

        private void OnEnable()
        {
            if (boardManager != null)
            {
                boardManager.RepresentativeChanged += HandleRepresentativeChanged;
            }
        }

        private void OnDisable()
        {
            if (boardManager != null)
            {
                boardManager.RepresentativeChanged -= HandleRepresentativeChanged;
            }
        }

        private void HandleRepresentativeChanged(MergeUnit unit)
        {
            representative = unit;
            if (representativeText != null)
            {
                representativeText.text = representative == null ? "대표 물정령 없음" : $"대표 출전: Lv.{representative.Level}";
            }
        }

        public int GetRepresentativeAttack()
        {
            if (representative == null)
            {
                return 0;
            }

            return representative.Level switch
            {
                1 => 10,
                2 => 24,
                3 => 55,
                4 => 120,
                5 => 260,
                _ => 10
            };
        }
    }
}

