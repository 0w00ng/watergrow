using System.Collections.Generic;
using System.Linq;

namespace WaterGrow.Board
{
    public static class MergeSystem
    {
        public static bool CanMerge(MergeUnit a, MergeUnit b, int maxLevel)
        {
            return a != null && b != null && a.Level == b.Level && a.Level < maxLevel;
        }

        public static MergeUnit CreateMergedUnit(MergeUnit a, MergeUnit b, int createdOrder, int maxLevel)
        {
            int nextLevel = System.Math.Min(a.Level + 1, maxLevel);
            return new MergeUnit(nextLevel, createdOrder);
        }

        public static MergeUnit FindRepresentative(IEnumerable<MergeUnit> units)
        {
            return units
                .Where(unit => unit != null)
                .OrderByDescending(unit => unit.Level)
                .ThenBy(unit => unit.CreatedOrder)
                .FirstOrDefault();
        }

        public static HashSet<int> FindMergeableLevels(IEnumerable<MergeUnit> units)
        {
            return units
                .Where(unit => unit != null)
                .GroupBy(unit => unit.Level)
                .Where(group => group.Count() >= 2)
                .Select(group => group.Key)
                .ToHashSet();
        }
    }
}

