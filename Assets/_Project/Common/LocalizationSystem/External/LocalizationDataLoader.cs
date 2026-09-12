using System;
using System.Collections.Generic;
using System.Linq;
using Common.AssetManagement.External;
using UnityEngine;

namespace Common.LocalizationSystem.External
{
    public class LocalizationDataLoader : IDisposable
    {
        private const string LOCALIZATION_CSV_ADDRESS = "localization_data";
        
        private readonly IAssetLoader m_AssetLoader;

        public LocalizationDataLoader(IAssetLoader assetLoader)
        {
            m_AssetLoader = assetLoader;
        }

        public Dictionary<string, Dictionary<string, string>> LoadLocalizationData()
        {
            try
            {
                var csvAsset = m_AssetLoader.LoadSync<TextAsset>(LOCALIZATION_CSV_ADDRESS);
                if (csvAsset == null)
                {
                    Debug.LogError($"[LocalizationDataLoader] Failed to load CSV file: {LOCALIZATION_CSV_ADDRESS}");
                    return new Dictionary<string, Dictionary<string, string>>();
                }

                return ParseCSVData(csvAsset.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalizationDataLoader] Error loading localization data: {ex.Message}");
                return new Dictionary<string, Dictionary<string, string>>();
            }
        }

        private Dictionary<string, Dictionary<string, string>> ParseCSVData(string csvText)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            
            string[] lines = csvText.Split('\n');
            if (lines.Length < 2)
            {
                Debug.LogError("[LocalizationDataLoader] CSV file is empty or has no header");
                return result;
            }
            
            string[] headers = lines[0].Split(',');
            if (headers.Length < 2)
            {
                Debug.LogError("[LocalizationDataLoader] CSV header must have at least Key and one language column");
                return result;
            }
            
            for (int i = 1; i < headers.Length; i++)
            {
                string language = headers[i].Trim().ToUpper();
                result[language] = new Dictionary<string, string>();
            }
            
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;
                
                string[] values = line.Split(',');
                if (values.Length < 2)
                    continue;

                string key = values[0].Trim().Trim('"');
                if (string.IsNullOrEmpty(key))
                    continue;
                
                for (int i = 1; i < values.Length && i < headers.Length; i++)
                {
                    string language = headers[i].Trim().ToUpper();
                    if (!result.ContainsKey(language))
                        continue;
                        
                    string value = values[i].Trim().Trim('"');
                    
                    value = value.Replace("\\n", "\n")
                                 .Replace("\\t", "\t")
                                 .Replace("\"\"", "\"");
                    
                    result[language][key] = value;
                }
            }

            Debug.Log($"[LocalizationDataLoader] Parsed {result.Values.FirstOrDefault()?.Count ?? 0} localization keys for {result.Count} languages");
            return result;
        }

        public void Dispose()
        {
            m_AssetLoader?.Release(LOCALIZATION_CSV_ADDRESS);
        }
    }
}