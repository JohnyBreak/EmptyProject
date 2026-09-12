using System.Collections.Generic;
using Common.AssetManagement.External;
using Common.ConfigSystem.Runtime;
using UnityEngine;

namespace Common.ConfigSystem.External
{
    public class ConfigLoader : IConfigLoader
    {
        private readonly IAssetLoader m_AssetLoader;
        private readonly IConfigReader m_ConfigReader;

        private readonly Dictionary<string, object> m_LoadedConfigs = new();

        public ConfigLoader(
            IAssetLoader assetLoader,
            IConfigReader configReader)
        {
            m_AssetLoader = assetLoader;
            m_ConfigReader = configReader;
        }

        public void Initialize()
        {
            Debug.Log("[ConfigLoader] Initialized");
        }

        public T GetConfig<T>(string configName) where T : class
        {
            if (string.IsNullOrEmpty(configName))
            {
                Debug.LogError("[ConfigLoader] Config name is null or empty");
                return null;
            }

            if (m_LoadedConfigs.TryGetValue(configName, out var cachedConfig))
            {
                if (cachedConfig is T typedConfig)
                {
                    Debug.Log($"[ConfigLoader] Returned cached config: {configName}");
                    return typedConfig;
                }
                
                Debug.LogError($"[ConfigLoader] Cached config {configName} has wrong type. Expected: {typeof(T).Name}");
                return null;
            }

            var config = LoadAndParseConfig<T>(configName);
            if (config != null)
            {
                m_LoadedConfigs[configName] = config;
                Debug.Log($"[ConfigLoader] Cached new config: {configName}");
            }

            return config;
        }

        public bool IsConfigLoaded(string configName)
        {
            return !string.IsNullOrEmpty(configName) && m_LoadedConfigs.ContainsKey(configName);
        }

        public void PreloadConfig(string configName)
        {
            if (string.IsNullOrEmpty(configName))
            {
                Debug.LogError("[ConfigLoader] Cannot preload: config name is null or empty");
                return;
            }

            if (IsConfigLoaded(configName))
            {
                Debug.Log($"[ConfigLoader] Config {configName} already loaded");
                return;
            }

            try
            {
                var jsonText = LoadConfigJson(configName);
                if (!string.IsNullOrEmpty(jsonText))
                {
                    m_LoadedConfigs[configName] = jsonText;
                    Debug.Log($"[ConfigLoader] Preloaded {configName}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConfigLoader] Failed to preload {configName}: {ex.Message}");
            }
        }

        private T LoadAndParseConfig<T>(string configName) where T : class
        {
            try
            {
                string jsonText;
                
                if (m_LoadedConfigs.TryGetValue(configName, out var preloaded) && preloaded is string preloadedJson)
                {
                    jsonText = preloadedJson;
                    Debug.Log($"[ConfigLoader] Using preloaded JSON for {configName}");
                }
                else
                {
                    jsonText = LoadConfigJson(configName);
                }

                if (string.IsNullOrEmpty(jsonText))
                {
                    Debug.LogError($"[ConfigLoader] Empty JSON for {configName}");
                    return null;
                }

                var result = m_ConfigReader.Deserialize<T>(jsonText);
                if (!result.IsExist)
                {
                    Debug.LogError($"[ConfigLoader] Failed to deserialize {configName} to {typeof(T).Name}");
                    return null;
                }

                Debug.Log($"[ConfigLoader] Successfully loaded {configName} as {typeof(T).Name}");
                return result.Object;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConfigLoader] Error loading config {configName}: {ex.Message}");
                return null;
            }
        }

        private string LoadConfigJson(string configName)
        {
            var textAsset = m_AssetLoader.LoadSync<TextAsset>(configName);
            if (textAsset == null)
            {
                Debug.LogError($"[ConfigLoader] Failed to load TextAsset: {configName}");
                return null;
            }

            Debug.Log($"[ConfigLoader] Loaded TextAsset: {configName}");
            return textAsset.text;
        }
    }
}