using System;
using System.Collections.Generic;
using WaterGrow.Battle;
using UnityEngine;

namespace WaterGrow.Core
{
    public class DataManager : MonoBehaviour
    {
        [SerializeField] private TextAsset unitTableJson;
        [SerializeField] private TextAsset enemyTableJson;

        private readonly Dictionary<int, WaterUnitData> unitsByLevel = new Dictionary<int, WaterUnitData>();
        private readonly Dictionary<string, EnemyData> enemiesById = new Dictionary<string, EnemyData>();

        private void Awake()
        {
            Load();
        }

        public void Load()
        {
            unitsByLevel.Clear();
            enemiesById.Clear();
            LoadFallbackData();
        }

        public WaterUnitData GetWaterUnitData(int level)
        {
            return unitsByLevel.TryGetValue(level, out WaterUnitData data) ? data : null;
        }

        public EnemyData GetEnemyData(string enemyId)
        {
            return enemiesById.TryGetValue(enemyId, out EnemyData data) ? data : null;
        }

        private void LoadFallbackData()
        {
            LoadUnitTable();
            LoadEnemyTable();

            // TextAsset이 아직 연결되지 않은 Preview Scene용 fallback. 값은 현재 JSON 테이블과 맞춘다.
            if (unitsByLevel.Count == 0)
            {
                AddUnit(new WaterUnitData(1, 10, 1.0f, 4.0f));
                AddUnit(new WaterUnitData(2, 24, 1.1f, 4.2f));
                AddUnit(new WaterUnitData(3, 55, 1.0f, 4.8f));
                AddUnit(new WaterUnitData(4, 120, 0.9f, 5.0f));
                AddUnit(new WaterUnitData(5, 260, 0.8f, 5.2f));
            }

            if (enemiesById.Count == 0)
            {
                AddEnemy(new EnemyData("ENEMY_001", "불꽃 병사", 30, 1.0f, 5, 1));
            }
        }

        private void AddUnit(WaterUnitData data)
        {
            unitsByLevel[data.level] = data;
        }

        private void AddEnemy(EnemyData data)
        {
            enemiesById[data.enemyId] = data;
        }

        private void LoadUnitTable()
        {
            if (unitTableJson == null)
            {
                unitTableJson = Resources.Load<TextAsset>("Data/UnitTable");
            }

            if (unitTableJson == null)
            {
                return;
            }

            UnitTable table = UnityEngine.JsonUtility.FromJson<UnitTable>(unitTableJson.text);
            if (table?.units == null)
            {
                return;
            }

            foreach (UnitRow row in table.units)
            {
                AddUnit(new WaterUnitData(row.unitLevel, row.baseAttack, row.attackSpeed, row.range));
            }
        }

        private void LoadEnemyTable()
        {
            if (enemyTableJson == null)
            {
                enemyTableJson = Resources.Load<TextAsset>("Data/EnemyTable");
            }

            if (enemyTableJson == null)
            {
                return;
            }

            EnemyTable table = UnityEngine.JsonUtility.FromJson<EnemyTable>(enemyTableJson.text);
            if (table?.enemies == null)
            {
                return;
            }

            foreach (EnemyRow row in table.enemies)
            {
                AddEnemy(new EnemyData(row.enemyId, row.enemyNameKo, row.hp, row.moveSpeed, row.rewardGold, row.attackPower));
            }
        }

        [Serializable]
        private class UnitTable
        {
            public UnitRow[] units;
        }

        [Serializable]
        private class UnitRow
        {
            public int unitLevel;
            public int baseAttack;
            public float attackSpeed;
            public float range;
        }

        [Serializable]
        private class EnemyTable
        {
            public EnemyRow[] enemies;
        }

        [Serializable]
        private class EnemyRow
        {
            public string enemyId;
            public string enemyNameKo;
            public int hp;
            public float moveSpeed;
            public int attackPower;
            public int rewardGold;
        }
    }
}
