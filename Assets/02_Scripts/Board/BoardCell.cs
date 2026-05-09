using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WaterGrow.Board
{
    public class BoardCell : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Text levelText;
        [SerializeField] private GameObject representativeIcon;
        [SerializeField] private Color emptyColor = new Color(0.16f, 0.22f, 0.28f);
        [SerializeField] private Color unitColor = new Color(0.22f, 0.58f, 0.92f);
        [SerializeField] private Color selectedColor = new Color(0.96f, 0.78f, 0.26f);
        [SerializeField] private Color mergeableColor = new Color(0.27f, 0.76f, 0.53f);

        private BoardManager owner;

        public int Index { get; private set; }
        public string CellId => $"B{Index + 1:00}";
        public MergeUnit Unit { get; private set; }
        public bool IsEmpty => Unit == null;

        public void Initialize(BoardManager boardManager, int index)
        {
            owner = boardManager;
            Index = index;
            SetUnit(null);
        }

        public void SetUnit(MergeUnit unit)
        {
            Unit = unit;
            RefreshVisual(false, false, false);
        }

        public void RefreshVisual(bool isSelected, bool isMergeable, bool isRepresentative)
        {
            if (levelText != null)
            {
                levelText.text = Unit == null ? CellId : $"Lv.{Unit.Level}";
            }

            if (background != null)
            {
                background.color = isSelected ? selectedColor : isMergeable ? mergeableColor : Unit == null ? emptyColor : unitColor;
            }

            if (representativeIcon != null)
            {
                representativeIcon.SetActive(isRepresentative);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            owner?.HandleCellClicked(this);
        }
    }
}

