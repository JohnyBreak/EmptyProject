using System;
using Common.TimerSystem.External.Interfaces;
using Common.TimerSystem.External.Models;
using Common.TimerSystem.External.Providers;
using UniRx;

namespace Common.TimerSystem.External
{
    public class SimpleTimer : ISimpleTimer
    {
        private static int m_NextId = 0;

        private readonly int m_Id;
        private readonly SimpleTimerInfo m_TimerInfo;
        private readonly IDateTimeProvider m_DateTimeProvider;
        
        private readonly Subject<ISimpleTimer> m_OnCompletedSubject = new Subject<ISimpleTimer>();
        private readonly Subject<SimpleTimerInfo> m_OnTickSubject = new Subject<SimpleTimerInfo>();

        private bool m_IsRunning;
        private bool m_UseAbsoluteTime;
        private long m_EndTimestampSeconds;
        
        public SimpleTimer(float duration)
        {
            m_Id = m_NextId++;
            m_DateTimeProvider = DateTimeProviderRegistry.Current;
            m_UseAbsoluteTime = false;
            
            m_TimerInfo = new SimpleTimerInfo
            {
                Duration = duration,
                Time = 0f,
                TimeLeft = duration,
                Delta = 0f
            };
        }
        
        public SimpleTimer(long endTimestamp, float duration)
        {
            m_Id = m_NextId++;
            m_DateTimeProvider = DateTimeProviderRegistry.Current;
            m_UseAbsoluteTime = true;
            m_EndTimestampSeconds = endTimestamp;

            var now = m_DateTimeProvider.GetNowTimestamp();
            var timeLeft = Math.Max(0, endTimestamp - now);
            
            m_TimerInfo = new SimpleTimerInfo
            {
                Duration = duration,
                Time = Math.Max(0, duration - timeLeft),
                TimeLeft = timeLeft,
                Delta = 0f
            };
        }

        public IObservable<ISimpleTimer> OnCompleted 
        { 
            get { return m_OnCompletedSubject.AsObservable(); } 
        }
        
        public IObservable<SimpleTimerInfo> OnTick 
        { 
            get { return m_OnTickSubject.AsObservable(); } 
        }

        public int GetId() 
        { 
            return m_Id; 
        }
        
        public SimpleTimerInfo GetTimerInfo() 
        { 
            return m_TimerInfo; 
        }
        
        public bool IsRunning 
        { 
            get { return m_IsRunning; } 
        }

        public void Update(float deltaTime)
        {
            if (!m_IsRunning)
                return;

            if (m_UseAbsoluteTime)
            {
                var now = m_DateTimeProvider.GetNowTimestamp();
                var newTimeLeft = Math.Max(0, m_EndTimestampSeconds - now);
                m_TimerInfo.Delta = m_TimerInfo.TimeLeft - newTimeLeft;
                m_TimerInfo.TimeLeft = newTimeLeft;
                m_TimerInfo.Time = m_TimerInfo.Duration - m_TimerInfo.TimeLeft;
            }
            else
            {
                m_TimerInfo.Delta = deltaTime;
                m_TimerInfo.TimeLeft = Math.Max(0, m_TimerInfo.TimeLeft - deltaTime);
                m_TimerInfo.Time += deltaTime;
            }

            m_OnTickSubject.OnNext(m_TimerInfo);

            if (m_TimerInfo.TimeLeft <= 0f)
            {
                m_TimerInfo.TimeLeft = 0f;
                m_IsRunning = false;
                m_OnCompletedSubject.OnNext(this);
            }
        }

        public TimerSaveData GetSaveData()
        {
            var now = m_DateTimeProvider.GetNowTimestamp();
            long startTimestamp;
            long endTimestamp;
            
            if (m_UseAbsoluteTime)
            {
                endTimestamp = m_EndTimestampSeconds;
                startTimestamp = endTimestamp - (long)m_TimerInfo.Duration;
            }
            else
            {
                endTimestamp = now + (long)m_TimerInfo.TimeLeft;
                startTimestamp = now - (long)m_TimerInfo.Time;
            }

            return new TimerSaveData(startTimestamp, endTimestamp, m_TimerInfo.Duration);
        }

        public void Start() 
        { 
            m_IsRunning = true; 
        }
        
        public void Stop() 
        { 
            m_IsRunning = false; 
        }

        public void Reset()
        {
            if (m_UseAbsoluteTime)
            {
                var now = m_DateTimeProvider.GetNowTimestamp();
                m_TimerInfo.TimeLeft = Math.Max(0, m_EndTimestampSeconds - now);
                m_TimerInfo.Time = m_TimerInfo.Duration - m_TimerInfo.TimeLeft;
            }
            else
            {
                m_TimerInfo.TimeLeft = m_TimerInfo.Duration;
                m_TimerInfo.Time = 0f;
            }
            
            m_IsRunning = false;
        }

        public void Dispose()
        {
            if (m_OnCompletedSubject != null)
            {
                m_OnCompletedSubject.OnCompleted();
                m_OnCompletedSubject.Dispose();
            }
            
            if (m_OnTickSubject != null)
            {
                m_OnTickSubject.OnCompleted(); 
                m_OnTickSubject.Dispose();
            }
        }
    }
}