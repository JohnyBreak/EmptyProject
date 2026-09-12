using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.TimerSystem.External.Models
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class TimerSaveData
    {
        [SerializeField, JsonProperty("StartTimestamp")] private long m_StartTimestamp;
        [SerializeField, JsonProperty("EndTimestamp")] private long m_EndTimestamp;
        [SerializeField, JsonProperty("Duration")] private float m_Duration;
        
        public long StartTimestamp 
        { 
            get => m_StartTimestamp; 
            set => m_StartTimestamp = value; 
        }
        
        public long EndTimestamp 
        { 
            get => m_EndTimestamp; 
            set => m_EndTimestamp = value; 
        }
        
        public float Duration 
        { 
            get => m_Duration; 
            set => m_Duration = value; 
        }
        
        public TimerSaveData(long startTimestamp, long endTimestamp, float duration)
        {
            m_StartTimestamp = startTimestamp;
            m_EndTimestamp = endTimestamp;
            m_Duration = duration;
        }
    }
}