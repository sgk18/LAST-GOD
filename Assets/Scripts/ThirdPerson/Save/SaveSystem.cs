using System;
using UnityEngine;

namespace LastGod.ThirdPerson.Save
{
    [Serializable]
    public class GameSaveData
    {
        public int slotIndex = 1;
        public string timestamp = "";
        public string sectorName = "SECTOR B-03 CATWALK";
        public string playTime = "00:14:22";
        public int currentCheckpoint = 1;
        public float playerHealth = 100f;
        public bool surgeUnlocked = true;
        public float masterVolume = 1.0f;
        public float sfxVolume = 1.0f;
        public float musicVolume = 1.0f;
        public float mouseSensitivity = 2.2f;
        public bool invertY = false;
        public int qualityLevel = 2;
    }

    public static class SaveSystem
    {
        private const string SaveKeyPrefix = "THE_LAST_GOD_SAVE_SLOT_";
        private const string DefaultSaveKey = "THE_LAST_GOD_SAVE_ACT1";
        private static GameSaveData _currentData;
        public static int ActiveSlot { get; set; } = 1;

        public static GameSaveData Data
        {
            get
            {
                if (_currentData == null) Load();
                return _currentData;
            }
        }

        public static void SaveCheckpoint(int checkpointNumber)
        {
            Data.currentCheckpoint = checkpointNumber;
            Save();
            Debug.Log($"[SaveSystem] Checkpoint {checkpointNumber} Saved.");
        }

        public static void Save()
        {
            SaveSlot(ActiveSlot);
        }

        public static void SaveSlot(int slot)
        {
            ActiveSlot = slot;
            Data.slotIndex = slot;
            Data.timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

            string json = JsonUtility.ToJson(Data);
            PlayerPrefs.SetString(SaveKeyPrefix + slot, json);
            PlayerPrefs.SetString(DefaultSaveKey, json);
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] Archive Slot {slot} Saved successfully.");
        }

        public static void Load()
        {
            LoadSlot(ActiveSlot);
        }

        public static GameSaveData LoadSlot(int slot)
        {
            ActiveSlot = slot;
            string key = SaveKeyPrefix + slot;
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                _currentData = JsonUtility.FromJson<GameSaveData>(json);
            }
            else if (slot == 1 && PlayerPrefs.HasKey(DefaultSaveKey))
            {
                string json = PlayerPrefs.GetString(DefaultSaveKey);
                _currentData = JsonUtility.FromJson<GameSaveData>(json);
            }
            else
            {
                _currentData = new GameSaveData { slotIndex = slot };
            }
            return _currentData;
        }

        public static bool HasSlot(int slot)
        {
            string key = SaveKeyPrefix + slot;
            if (PlayerPrefs.HasKey(key)) return true;
            if (slot == 1 && PlayerPrefs.HasKey(DefaultSaveKey)) return true;
            return false;
        }

        public static GameSaveData GetSlotData(int slot)
        {
            string key = SaveKeyPrefix + slot;
            if (PlayerPrefs.HasKey(key))
            {
                return JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(key));
            }
            if (slot == 1 && PlayerPrefs.HasKey(DefaultSaveKey))
            {
                return JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(DefaultSaveKey));
            }
            return null;
        }

        public static void DeleteSlot(int slot)
        {
            string key = SaveKeyPrefix + slot;
            if (PlayerPrefs.HasKey(key)) PlayerPrefs.DeleteKey(key);
            if (slot == 1 && PlayerPrefs.HasKey(DefaultSaveKey)) PlayerPrefs.DeleteKey(DefaultSaveKey);
            PlayerPrefs.Save();
            if (ActiveSlot == slot) _currentData = new GameSaveData { slotIndex = slot };
            Debug.Log($"[SaveSystem] Archive Slot {slot} purged.");
        }

        public static void ResetSave()
        {
            _currentData = new GameSaveData();
            Save();
        }
    }
}
