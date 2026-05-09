using System;
using System.Collections.Generic;

namespace WaterGrow.Core
{
    [Serializable]
    public class SaveData
    {
        public int gold = 100;
        public int waterCrystal = 0;
        public string currentStageId = "STAGE_1_01";
        public string highestClearedStageId = "";
        public List<BoardSlotSaveData> boardState = new List<BoardSlotSaveData>();
        public List<UpgradeSaveData> unitUpgradeLevels = new List<UpgradeSaveData>();
        public List<string> seenTutorials = new List<string>();
        public List<string> ownedUnitIds = new List<string>();
    }

    [Serializable]
    public class BoardSlotSaveData
    {
        public int cellIndex;
        public int unitLevel;
        public int createdOrder;
    }

    [Serializable]
    public class UpgradeSaveData
    {
        public string upgradeId;
        public int level;
    }
}

