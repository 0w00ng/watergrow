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
                string message = gold < summonCost ? "골드가 부족합니다." : "보드가 가득 찼습니다.";
                BoardMessage?.Invoke(message);
                return;
            }

            BoardCell target = cells.FirstOrDefault(cell => cell.IsEmpty);
            if (target == null)
            {
                return;
            }

            gold -= summonCost;
            target.SetUnit(new MergeUnit(1, ++createdSequence));
            BoardMessage?.Invoke("Lv.1 물방울 소환");
            selectedCell = target;
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

            if (selectedCell == null || selectedCell == clicked)
            {
                selectedCell = clicked;
                RefreshBoardState();
                return;
            }

            if (MergeSystem.CanMerge(selectedCell.Unit, clicked.Unit, maxUnitLevel))
            {
                clicked.SetUnit(MergeSystem.CreateMergedUnit(selectedCell.Unit, clicked.Unit, ++createdSequence, maxUnitLevel));
                selectedCell.SetUnit(null);
                selectedCell = clicked;
                BoardMessage?.Invoke($"Lv.{clicked.Unit.Level} 물정령 머지 성공");
            }
            else
            {
                selectedCell = clicked;
                BoardMessage?.Invoke("같은 Lv. 물방울을 선택하세요.");
            }

            RefreshBoardState();
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
                Destroy(child.gameObject);
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
                bool isSelected = cell == selectedCell;
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
