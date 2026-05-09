using System;

namespace WaterGrow.Stage
{
    [Serializable]
    public class WaveData
    {
        public string waveGroupId;
        public int spawnOrder;
        public string enemyId;
        public int count;
        public float spawnInterval;
        public float delayBeforeNextGroup;

        public WaveData(string waveGroupId, int spawnOrder, string enemyId, int count, float spawnInterval, float delayBeforeNextGroup)
        {
            this.waveGroupId = waveGroupId;
            this.spawnOrder = spawnOrder;
            this.enemyId = enemyId;
            this.count = count;
            this.spawnInterval = spawnInterval;
            this.delayBeforeNextGroup = delayBeforeNextGroup;
        }
    }
}
