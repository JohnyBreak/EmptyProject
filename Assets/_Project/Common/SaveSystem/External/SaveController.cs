using System;
using System.IO;
using Common.SaveSystem.Runtime;
using UnityEngine;

namespace Common.SaveSystem.External
{
    public class SaveController : ISaveController
    {
        private readonly string m_SavePath;

        public SaveController()
        {
            m_SavePath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(m_SavePath))
            {
                Directory.CreateDirectory(m_SavePath);
                Debug.Log($"SaveController: Created save directory at {m_SavePath}");
            }
        }

        public void SaveData<T>(T saveData) where T : ISaveData
        {
            try
            {
                string json = JsonUtility.ToJson(saveData, true);
                string filePath = GetFilePath(saveData.GetSaveKey());
                
                File.WriteAllText(filePath, json);
                Debug.Log($"SaveController: Saved data for key '{saveData.GetSaveKey()}'");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SaveController: Failed to save data for key '{saveData.GetSaveKey()}': {ex.Message}");
            }
        }

        public T LoadData<T>(string saveKey) where T : ISaveData, new()
        {
            try
            {
                string filePath = GetFilePath(saveKey);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"SaveController: Save file not found for key '{saveKey}', returning default");
                    return new T();
                }

                string json = File.ReadAllText(filePath);
                T loadedData = JsonUtility.FromJson<T>(json);
                
                Debug.Log($"SaveController: Loaded data for key '{saveKey}'");
                return loadedData ?? new T();
            }
            catch (Exception ex)
            {
                Debug.LogError($"SaveController: Failed to load data for key '{saveKey}': {ex.Message}");
                return new T();
            }
        }

        public bool HasSave(string saveKey)
        {
            string filePath = GetFilePath(saveKey);
            return File.Exists(filePath);
        }

        public void DeleteSave(string saveKey)
        {
            try
            {
                string filePath = GetFilePath(saveKey);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"SaveController: Deleted save for key '{saveKey}'");
                }
                else
                {
                    Debug.LogWarning($"SaveController: Save file not found for key '{saveKey}'");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"SaveController: Failed to delete save for key '{saveKey}': {ex.Message}");
            }
        }

        public void DeleteAllSaves()
        {
            try
            {
                if (Directory.Exists(m_SavePath))
                {
                    Directory.Delete(m_SavePath, true);
                    Directory.CreateDirectory(m_SavePath);
                    Debug.Log("SaveController: Deleted all saves");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"SaveController: Failed to delete all saves: {ex.Message}");
            }
        }

        private string GetFilePath(string saveKey)
        {
            return Path.Combine(m_SavePath, $"{saveKey}.json");
        }
    }
}