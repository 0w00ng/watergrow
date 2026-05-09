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
        [SerializeField] private Color emptyColor = new Color(0.88f, 0.98f, 1f);
        [SerializeField] private Color unitColor = new Color(0.82f, 0.96f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.68f, 0.95f, 0.78f);
        [SerializeField] private Color mergeableColor = new Color(0.80f, 1f, 0.88f);

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
                levelText.text = Unit == null ? $"{CellId}\n~~~~" : $"물방울\nLv.{Unit.Level}";
                levelText.color = Unit == null ? new Color(0.36f, 0.68f, 0.78f) : new Color(0.05f, 0.28f, 0.42f);
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
