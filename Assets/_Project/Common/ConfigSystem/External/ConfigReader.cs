using Common.ConfigSystem.Runtime;
using Newtonsoft.Json;
using UnityEngine;
using Utils.Result;

namespace Common.ConfigSystem.External
{
    public class ConfigReader : IConfigReader
    {
        private readonly JsonSerializerSettings m_JsonSettings;

        public ConfigReader()
        {
            m_JsonSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include
            };
        }

        public Result<T> Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[ConfigReader] JSON string is null or empty");
                return Result<T>.Fail();
            }

            try
            {
                var result = JsonConvert.DeserializeObject<T>(json, m_JsonSettings);
                
                if (result == null)
                {
                    Debug.LogError($"[ConfigReader] Deserialization returned null for type {typeof(T).Name}");
                    return Result<T>.Fail();
                }

                Debug.Log($"[ConfigReader] Successfully deserialized {typeof(T).Name}");
                return Result<T>.Success(result);
            }
            catch (JsonException jsonEx)
            {
                Debug.LogError($"[ConfigReader] JSON parsing error for {typeof(T).Name}: {jsonEx.Message}");
                return Result<T>.Fail();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConfigReader] Failed to deserialize {typeof(T).Name}: {ex.Message}");
                return Result<T>.Fail();
            }
        }
    }
}