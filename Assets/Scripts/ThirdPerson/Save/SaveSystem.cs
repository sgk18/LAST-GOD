using System;
using UnityEngine;

namespace LastGod.ThirdPerson.Save
{
    [Serializable]
    public class GameSaveData
    {
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
        private const string SaveKey = "THE_LAST_GOD_SAVE_ACT1";
        private static GameSaveData _currentData;

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
            string json = JsonUtility.ToJson(Data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                _currentData = JsonUtility.FromJson<GameSaveData>(json);
            }
            else
            {
                _currentData = new GameSaveData();
            }
        }

        public static void ResetSave()
        {
            _currentData = new GameSaveData();
            Save();
        }
    }
}
