using System;

namespace WaterGrow.Board
{
    [Serializable]
    public class MergeUnit
    {
        public int Level;
        public int CreatedOrder;

        public MergeUnit(int level, int createdOrder)
        {
            Level = level;
            CreatedOrder = createdOrder;
        }
    }
}

