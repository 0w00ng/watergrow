using System;

namespace WaterGrow.Battle
{
    [Serializable]
    public class EnemyData
    {
        public string enemyId;
        public string enemyNameKo;
        public int hp;
        public float moveSpeed;
        public int rewardGold;
        public int damageToBase;

        public EnemyData(string enemyId, string enemyNameKo, int hp, float moveSpeed, int rewardGold, int damageToBase)
        {
            this.enemyId = enemyId;
            this.enemyNameKo = enemyNameKo;
            this.hp = hp;
            this.moveSpeed = moveSpeed;
            this.rewardGold = rewardGold;
            this.damageToBase = damageToBase;
        }
    }
}

