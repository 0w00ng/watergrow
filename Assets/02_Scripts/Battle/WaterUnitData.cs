using System;

namespace WaterGrow.Battle
{
    [Serializable]
    public class WaterUnitData
    {
        public int level;
        public int attackPower;
        public float attackInterval;
        public float attackRange;

        public WaterUnitData(int level, int attackPower, float attackInterval, float attackRange)
        {
            this.level = level;
            this.attackPower = attackPower;
            this.attackInterval = attackInterval;
            this.attackRange = attackRange;
        }
    }
}

