using System;

namespace WaterGrow.Stage
{
    [Serializable]
    public class StageData
    {
        public string stageId;
        public string chapterId;
        public int stageNumber;
        public string stageNameKo;
        public string waveGroupId;
        public int clearRewardGold;
        public int clearRewardCrystal;
        public bool isBossStage;

        public StageData(string stageId, string chapterId, int stageNumber, string stageNameKo, string waveGroupId, int clearRewardGold, int clearRewardCrystal, bool isBossStage)
        {
            this.stageId = stageId;
            this.chapterId = chapterId;
            this.stageNumber = stageNumber;
            this.stageNameKo = stageNameKo;
            this.waveGroupId = waveGroupId;
            this.clearRewardGold = clearRewardGold;
            this.clearRewardCrystal = clearRewardCrystal;
            this.isBossStage = isBossStage;
        }
    }
}
