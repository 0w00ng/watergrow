using System;

namespace WaterGrow.Battle
{
    [Serializable]
    public class EnemyData
    {
        public string enemyId;
        public string enemyNameKo;
        public string enemyType;
        public int hp;
        public float moveSpeed;
        public int attackPower;
        public int rewardGold;
        public int rewardCrystal;
        public bool isBoss;
        public int damageToBase;

        public EnemyData(string enemyId, string enemyNameKo, int hp, float moveSpeed, int rewardGold, int damageToBase)
            : this(enemyId, enemyNameKo, "Basic", hp, moveSpeed, damageToBase, rewardGold, 0, false)
        {
        }

        public EnemyData(string enemyId, string enemyNameKo, string enemyType, int hp, float moveSpeed, int attackPower, int rewardGold, int rewardCrystal, bool isBoss)
        {
            this.enemyId = enemyId;
            this.enemyNameKo = enemyNameKo;
            this.enemyType = enemyType;
            this.hp = hp;
            this.moveSpeed = moveSpeed;
            this.attackPower = attackPower;
            this.rewardGold = rewardGold;
            this.rewardCrystal = rewardCrystal;
            this.isBoss = isBoss;
            this.damageToBase = Math.Max(1, attackPower);
        }
    }
}
