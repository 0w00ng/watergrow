using UnityEngine;

namespace WaterGrow.Core
{
    public class SaveManager : MonoBehaviour
    {
        private const string SaveKey = "WaterGrow_SaveData_v1";

        public SaveData Current { get; private set; }

        private void Awake()
        {
            Load();
        }

        public void Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            Current = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);

            if (Current == null)
            {
                Current = new SaveData();
            }
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(Current);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void ResetSave()
        {
            Current = new SaveData();
            PlayerPrefs.DeleteKey(SaveKey);
            Save();
        }
    }
}

