using System;
using System.Collections.Generic;
using System.Linq;
using WaterGrow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace WaterGrow.Board
{
    public class BoardManager : MonoBehaviour
    {
        public event Action<int> GoldChanged;
        public event Action<MergeUnit> RepresentativeChanged;
        public event Action<bool> SummonAvailabilityChanged;
        public event Action BoardStateChanged;
        public event Action<string> BoardMessage;

        [Header("Board")]
        [SerializeField] private int columns = 6;
        [SerializeField] private int rows = 4;
        [SerializeField] private int maxUnitLevel = 5;
        [SerializeField] private Transform gridRoot;
        [SerializeField] private BoardCell cellPrefab;
        [SerializeField] private List<BoardCell> cells = new List<BoardCell>();

        [Header("Economy")]
        [SerializeField] private int summonCost = 10;

        private int gold;
        private int createdSequence;
        private BoardCell selectedCell;
        private BoardCell draggingCell;
        private MergeUnit currentRepresentative;

        public int Gold => gold;
        public int SummonCost => summonCost;
        public bool CanSummon => gold >= summonCost && cells.Any(cell => cell.IsEmpty);

        public void ConfigureBoard(Transform boardGridRoot, BoardCell boardCellPrefab)
        {
            gridRoot = boardGridRoot;
            cellPrefab = boardCellPrefab;
        }

        public void Initialize(SaveData saveData)
        {
            EnsureCells();

            gold = saveData.gold;
            createdSequence = 0;
            selectedCell = null;
            draggingCell = null;

            foreach (BoardCell cell in cells)
            {
                cell.SetUnit(null);
            }

            foreach (BoardSlotSaveData slot in saveData.boardState)
            {
                if (slot.cellIndex < 0 || slot.cellIndex >= cells.Count || slot.unitLevel <= 0)
                {
                    continue;
                }

                cells[slot.cellIndex].SetUnit(new MergeUnit(slot.unitLevel, slot.createdOrder));
                createdSequence = Math.Max(createdSequence, slot.createdOrder);
            }

            RefreshBoardState();
            GoldChanged?.Invoke(gold);
        }

        public void Summon()
        {
            if (!CanSummon)
            {
                BoardMessage?.Invoke(gold < summonCost ? "Not enough gold." : "Board is full.");
                return;
            }

            BoardCell target = cells.FirstOrDefault(cell => cell.IsEmpty);
            if (target == null)
            {
                return;
            }

            gold -= summonCost;
            target.SetUnit(new MergeUnit(1, ++createdSequence));
            selectedCell = target;
            BoardMessage?.Invoke("Summoned Lv.1 water unit.");
            RefreshBoardState();
            GoldChanged?.Invoke(gold);
        }

        public void HandleCellClicked(BoardCell clicked)
        {
            if (clicked == null || clicked.IsEmpty)
            {
                selectedCell = null;
                RefreshBoardState();
                return;
            }

            selectedCell = clicked;
            BoardMessage?.Invoke($"Drag Lv.{clicked.Unit.Level} onto another Lv.{clicked.Unit.Level} to merge.");
            RefreshBoardState();
        }

        public void HandleDragStarted(BoardCell source)
        {
            if (source == null || source.IsEmpty)
            {
                draggingCell = null;
                selectedCell = null;
                RefreshBoardState();
                return;
            }

            draggingCell = source;
            selectedCell = source;
            BoardMessage?.Invoke($"Dragging Lv.{source.Unit.Level}. Drop on same level to merge.");
            RefreshBoardState();
        }

        public void HandleCellDropped(BoardCell source, BoardCell target)
        {
            if (source == null || target == null || source == target)
            {
                ClearDragSelection();
                return;
            }

            if (source.IsEmpty || target.IsEmpty)
            {
                BoardMessage?.Invoke("Drop on a same-level unit to merge.");
                ClearDragSelection();
                return;
            }

            if (!MergeSystem.CanMerge(source.Unit, target.Unit, maxUnitLevel))
            {
                BoardMessage?.Invoke(source.Unit.Level >= maxUnitLevel
                    ? $"Lv.{maxUnitLevel} is the current max level."
                    : "Only same-level units can merge.");
                ClearDragSelection();
                return;
            }

            target.SetUnit(MergeSystem.CreateMergedUnit(source.Unit, target.Unit, ++createdSequence, maxUnitLevel));
            source.SetUnit(null);
            draggingCell = null;
            selectedCell = target;
            BoardMessage?.Invoke($"Merged into Lv.{target.Unit.Level} water unit.");
            RefreshBoardState();
        }

        public void HandleDragEnded(BoardCell source)
        {
            if (draggingCell == source)
            {
                ClearDragSelection();
            }
        }

        public void AddGold(int amount)
        {
            gold = Math.Max(0, gold + amount);
            RefreshBoardState();
            GoldChanged?.Invoke(gold);
        }

        public void WriteBoardState(SaveData saveData)
        {
            saveData.gold = gold;
            saveData.boardState.Clear();

            for (int i = 0; i < cells.Count; i++)
            {
                MergeUnit unit = cells[i].Unit;
                if (unit == null)
                {
                    continue;
                }

                saveData.boardState.Add(new BoardSlotSaveData
                {
                    cellIndex = i,
                    unitLevel = unit.Level,
                    createdOrder = unit.CreatedOrder
                });
            }
        }

        private void EnsureCells()
        {
            if (cells.Count == columns * rows)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    cells[i].Initialize(this, i);
                }

                return;
            }

            if (gridRoot == null || cellPrefab == null)
            {
                Debug.LogWarning("Board cells are not fully assigned. Add 24 BoardCell objects or set gridRoot and cellPrefab.");
                return;
            }

            foreach (Transform child in gridRoot)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            cells.Clear();
            for (int i = 0; i < columns * rows; i++)
            {
                BoardCell cell = Instantiate(cellPrefab, gridRoot);
                cell.gameObject.SetActive(true);
                cell.Initialize(this, i);
                cells.Add(cell);
            }
        }

        private void RefreshBoardState()
        {
            HashSet<int> mergeableLevels = MergeSystem.FindMergeableLevels(cells.Select(cell => cell.Unit));
            MergeUnit representative = MergeSystem.FindRepresentative(cells.Select(cell => cell.Unit));

            for (int i = 0; i < cells.Count; i++)
            {
                BoardCell cell = cells[i];
                bool isSelected = cell == selectedCell || cell == draggingCell;
                bool isMergeable = cell.Unit != null && mergeableLevels.Contains(cell.Unit.Level);
                bool isRepresentative = cell.Unit != null && representative != null && cell.Unit.CreatedOrder == representative.CreatedOrder;
                cell.RefreshVisual(isSelected, isMergeable, isRepresentative);
            }

            if (!IsSameRepresentative(currentRepresentative, representative))
            {
                currentRepresentative = representative;
                RepresentativeChanged?.Invoke(currentRepresentative);
            }

            SummonAvailabilityChanged?.Invoke(CanSummon);
            BoardStateChanged?.Invoke();
        }

        private void ClearDragSelection()
        {
            draggingCell = null;
            selectedCell = null;
            RefreshBoardState();
        }

        private static bool IsSameRepresentative(MergeUnit a, MergeUnit b)
        {
            if (a == null || b == null)
            {
                return a == b;
            }

            return a.Level == b.Level && a.CreatedOrder == b.CreatedOrder;
        }
    }
}
