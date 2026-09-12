using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Common.SaveSystem.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.SaveSystem.External
{
    public class AutoSaveController : MonoBehaviour, IAutoSaveController
    {
        private readonly Dictionary<string, ISaveData> m_RegisteredData = new Dictionary<string, ISaveData>();
        private string m_SavePath;
        private float m_AutoSaveInterval = 5f;
        private Coroutine m_AutoSaveCoroutine;
        private bool m_IsInitialized = false;

        public event Action<string> OnDataSaved;
        public event Action<string> OnDataLoaded;

        public void Initialize()
        {
            m_SavePath = Path.Combine(Application.persistentDataPath, "Saves");

            if (!Directory.Exists(m_SavePath))
            {
                Directory.CreateDirectory(m_SavePath);
                Debug.Log($"AutoSaveController: Created save directory at {m_SavePath}");
            }

            m_IsInitialized = true;
            
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            StartAutoSave();
            
            Debug.Log("AutoSaveController: Initialized via Zenject");
        }

        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            Debug.Log($"AutoSaveController: Scene '{scene.name}' is being unloaded, saving all data...");
            SaveAll();
        }

        private void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            StopAutoSave();
            SaveAll();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && m_IsInitialized)
            {
                SaveAll();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && m_IsInitialized)
            {
                SaveAll();
            }
        }

        public void RegisterAutoSave<T>(T saveData) where T : ISaveData
        {
            if (!m_IsInitialized)
            {
                Debug.LogWarning("AutoSaveController: Trying to register data before initialization. Delaying registration.");
                StartCoroutine(DelayedRegister(saveData));
                return;
            }

            string key = saveData.GetSaveKey();
            
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("AutoSaveController: Save key cannot be null or empty");
                return;
            }
            
            m_RegisteredData[key] = saveData;

            Debug.Log($"AutoSaveController: Registered auto-save for '{key}'");
            
            LoadDataIntoRegistered(saveData);
        }

        private IEnumerator DelayedRegister<T>(T saveData) where T : ISaveData
        {
            yield return new WaitUntil(() => m_IsInitialized);
            RegisterAutoSave(saveData);
        }

        public void UnregisterAutoSave(string saveKey)
        {
            if (string.IsNullOrEmpty(saveKey))
            {
                Debug.LogError("AutoSaveController: Save key cannot be null or empty");
                return;
            }
            
            if (m_RegisteredData.ContainsKey(saveKey))
            {
                if (m_RegisteredData.TryGetValue(saveKey, out var saveData))
                {
                    SaveData(saveData);
                }
                
                m_RegisteredData.Remove(saveKey);
                Debug.Log($"AutoSaveController: Unregistered auto-save for '{saveKey}'");
            }
        }

        public void SaveAll()
        {
            if (!m_IsInitialized)
            {
                Debug.LogWarning("AutoSaveController: Cannot save - not initialized yet");
                return;
            }

            int savedCount = 0;
            foreach (var kvp in m_RegisteredData)
            {
                if (SaveData(kvp.Value))
                {
                    savedCount++;
                }
            }

            Debug.Log($"AutoSaveController: Saved {savedCount} of {m_RegisteredData.Count} registered data");
        }

        public void LoadAll()
        {
            if (!m_IsInitialized)
            {
                Debug.LogWarning("AutoSaveController: Cannot load - not initialized yet");
                return;
            }

            foreach (var kvp in m_RegisteredData)
            {
                LoadDataIntoRegistered(kvp.Value);
            }

            Debug.Log($"AutoSaveController: Loaded all {m_RegisteredData.Count} registered data");
        }

        public void SetAutoSaveInterval(float seconds)
        {
            m_AutoSaveInterval = seconds;

            if (m_AutoSaveCoroutine != null)
            {
                StopAutoSave();
                StartAutoSave();
            }
        }

        private void StartAutoSave()
        {
            if (m_AutoSaveCoroutine == null && m_IsInitialized)
            {
                m_AutoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
                Debug.Log($"AutoSaveController: Started auto-save with interval {m_AutoSaveInterval}s");
            }
        }

        private void StopAutoSave()
        {
            if (m_AutoSaveCoroutine != null)
            {
                StopCoroutine(m_AutoSaveCoroutine);
                m_AutoSaveCoroutine = null;
            }
        }

        private IEnumerator AutoSaveRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(m_AutoSaveInterval);

                if (m_RegisteredData.Count > 0 && m_IsInitialized)
                {
                    SaveAll();
                }
            }
        }

        private void LoadDataIntoRegistered<T>(T registeredInstance) where T : ISaveData
        {
            if (!m_IsInitialized)
            {
                Debug.LogWarning("AutoSaveController: Cannot load data - not initialized yet");
                return;
            }

            if (registeredInstance == null)
            {
                Debug.LogError("AutoSaveController: Cannot load data into null instance");
                return;
            }

            try
            {
                string saveKey = registeredInstance.GetSaveKey();
                
                if (string.IsNullOrEmpty(saveKey))
                {
                    Debug.LogError("AutoSaveController: Save key cannot be null or empty");
                    return;
                }
                
                string filePath = GetFilePath(saveKey);

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    
                    if (!string.IsNullOrEmpty(json))
                    {
                        JsonUtility.FromJsonOverwrite(json, registeredInstance);
                        OnDataLoaded?.Invoke(saveKey);
                        Debug.Log($"AutoSaveController: Loaded data into registered instance '{saveKey}'");
                    }
                    else
                    {
                        Debug.LogWarning($"AutoSaveController: Save file for '{saveKey}' is empty");
                    }
                }
                else
                {
                    Debug.Log($"AutoSaveController: No save file found for '{saveKey}', using default values");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoSaveController: Failed to load data for '{registeredInstance.GetSaveKey()}': {ex.Message}");
            }
        }
        
        public bool SaveData<T>(T saveData) where T : ISaveData
        {
            if (!m_IsInitialized)
            {
                Debug.LogWarning("AutoSaveController: Cannot save data - not initialized yet");
                return false;
            }

            if (saveData == null)
            {
                Debug.LogError("AutoSaveController: Cannot save null data");
                return false;
            }

            try
            {
                string saveKey = saveData.GetSaveKey();
                
                if (string.IsNullOrEmpty(saveKey))
                {
                    Debug.LogError("AutoSaveController: Save key cannot be null or empty");
                    return false;
                }
                
                string filePath = GetFilePath(saveKey);
                
                if (string.IsNullOrEmpty(filePath))
                {
                    Debug.LogError($"AutoSaveController: Failed to get file path for key '{saveKey}'");
                    return false;
                }

                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(filePath, json);
                OnDataSaved?.Invoke(saveKey);
                Debug.Log($"AutoSaveController: Saved data for key '{saveKey}'");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoSaveController: Failed to save data for key '{saveData.GetSaveKey()}': {ex.Message}");
                return false;
            }
        }
        
        void ISaveController.SaveData<T>(T saveData)
        {
            SaveData(saveData);
        }

        public T LoadData<T>(string saveKey) where T : ISaveData, new()
        {
            if (!m_IsInitialized)
            {
                Debug.LogWarning("AutoSaveController: Cannot load data - not initialized yet");
                return new T();
            }

            if (string.IsNullOrEmpty(saveKey))
            {
                Debug.LogError("AutoSaveController: Save key cannot be null or empty");
                return new T();
            }

            try
            {
                string filePath = GetFilePath(saveKey);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"AutoSaveController: Save file not found for key '{saveKey}', returning default");
                    return new T();
                }

                string json = File.ReadAllText(filePath);
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning($"AutoSaveController: Save file for key '{saveKey}' is empty, returning default");
                    return new T();
                }
                
                T loadedData = JsonUtility.FromJson<T>(json);

                OnDataLoaded?.Invoke(saveKey);
                Debug.Log($"AutoSaveController: Loaded data for key '{saveKey}'");
                return loadedData ?? new T();
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoSaveController: Failed to load data for key '{saveKey}': {ex.Message}");
                return new T();
            }
        }

        public bool HasSave(string saveKey)
        {
            if (!m_IsInitialized || string.IsNullOrEmpty(saveKey))
            {
                return false;
            }

            string filePath = GetFilePath(saveKey);
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }

        public void DeleteSave(string saveKey)
        {
            if (!m_IsInitialized)
            {
                Debug.LogWarning("AutoSaveController: Cannot delete save - not initialized yet");
                return;
            }

            if (string.IsNullOrEmpty(saveKey))
            {
                Debug.LogError("AutoSaveController: Save key cannot be null or empty");
                return;
            }

            try
            {
                string filePath = GetFilePath(saveKey);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"AutoSaveController: Deleted save for key '{saveKey}'");
                }
                else
                {
                    Debug.LogWarning($"AutoSaveController: Save file not found for key '{saveKey}'");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoSaveController: Failed to delete save for key '{saveKey}': {ex.Message}");
            }
        }

        public void DeleteAllSaves()
        {
            if (!m_IsInitialized)
            {
                Debug.LogWarning("AutoSaveController: Cannot delete saves - not initialized yet");
                return;
            }

            try
            {
                if (Directory.Exists(m_SavePath))
                {
                    Directory.Delete(m_SavePath, true);
                    Directory.CreateDirectory(m_SavePath);
                    Debug.Log("AutoSaveController: Deleted all saves");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoSaveController: Failed to delete all saves: {ex.Message}");
            }
        }

        private string GetFilePath(string saveKey)
        {
            if (string.IsNullOrEmpty(m_SavePath))
            {
                Debug.LogError("AutoSaveController: Save path is not initialized");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(saveKey))
            {
                Debug.LogError("AutoSaveController: Save key cannot be null or empty");
                return string.Empty;
            }

            return Path.Combine(m_SavePath, $"{saveKey}.json");
        }
    }
}