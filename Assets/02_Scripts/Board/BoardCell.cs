using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace WaterGrow.Board
{
    public class BoardCell : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Text levelText;
        [SerializeField] private GameObject representativeIcon;
        [SerializeField] private Color emptyColor = new Color(0.13f, 0.38f, 0.52f);
        [SerializeField] private Color unitColor = new Color(0.10f, 0.66f, 0.98f);
        [SerializeField] private Color selectedColor = new Color(1f, 0.78f, 0.20f);
        [SerializeField] private Color mergeableColor = new Color(0.18f, 0.86f, 0.64f);

        private BoardManager owner;
        private Coroutine feedbackRoutine;

        public int Index { get; private set; }
        public string CellId => $"B{Index + 1:00}";
        public MergeUnit Unit { get; private set; }
        public bool IsEmpty => Unit == null;

        public void ConfigureVisuals(Image backgroundImage, Text unitLevelText, GameObject representativeMarker)
        {
            background = backgroundImage;
            levelText = unitLevelText;
            representativeIcon = representativeMarker;
            RefreshVisual(false, false, Unit != null && representativeIcon != null && representativeIcon.activeSelf);
        }

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

            if (Unit != null && gameObject.activeInHierarchy)
            {
                PlaySpawnFeedback();
            }
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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsEmpty)
            {
                eventData.pointerDrag = null;
                return;
            }

            owner?.HandleDragStarted(this);
            transform.localScale = Vector3.one * 1.08f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // The prototype keeps units locked to board cells; drag is used as an input gesture only.
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
            owner?.HandleDragEnded(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            BoardCell source = eventData.pointerDrag == null ? null : eventData.pointerDrag.GetComponent<BoardCell>();
            owner?.HandleCellDropped(source, this);
        }

        private void PlaySpawnFeedback()
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(SpawnFeedback());
        }

        private IEnumerator SpawnFeedback()
        {
            transform.localScale = Vector3.one * 1.08f;
            yield return new WaitForSeconds(0.08f);
            transform.localScale = Vector3.one;
            feedbackRoutine = null;
        }
    }
}
