using System;
using Common.TimerSystem.External.Interfaces;
using Common.TimerSystem.External.Models;
using Common.TimerSystem.External.Providers;
using UniRx;

namespace Common.TimerSystem.External
{
    public class SimpleTimerWrapper : IDisposable
    {
        private readonly ITimerController m_TimerController;
        private readonly CompositeDisposable m_Disposables = new CompositeDisposable();
        
        private readonly Func<TimerSaveData> m_LoadDataFunc;
        private readonly Action<TimerSaveData> m_SaveDataAction;
        private readonly float m_Duration;
        
        private SimpleTimer m_Timer;
        private bool m_IsAddedToController;
        private TimerSaveData m_CurrentSaveData;
        
        public SimpleTimerWrapper(ITimerController timerController, 
            Func<TimerSaveData> loadDataFunc,
            Action<TimerSaveData> saveDataAction,
            float duration)
        {
            m_TimerController = timerController;
            m_LoadDataFunc = loadDataFunc;
            m_SaveDataAction = saveDataAction;
            m_Duration = duration;
            
            LoadTimerData();
            CreateTimer(m_CurrentSaveData);
            SetupSubscriptions();
        }
        
        public SimpleTimerWrapper(ITimerController timerController, float duration)
        {
            m_TimerController = timerController;
            m_Duration = duration;
            
            CreateTimer(null);
            SetupSubscriptions();
        }

        public SimpleTimerInfo GetTimerInfo() 
        { 
            return m_Timer?.GetTimerInfo(); 
        }
        
        public bool IsRunning 
        { 
            get { return m_Timer?.IsRunning ?? false; } 
        }
        
        public IObservable<ISimpleTimer> OnCompleted 
        { 
            get { return m_Timer?.OnCompleted ?? Observable.Empty<ISimpleTimer>(); } 
        }
        
        public IObservable<SimpleTimerInfo> OnTick 
        { 
            get { return m_Timer?.OnTick ?? Observable.Empty<SimpleTimerInfo>(); } 
        }

        public TimerSaveData GetSaveData() 
        { 
            return m_Timer?.GetSaveData(); 
        }

        public int RemovePeriods(TimerSaveData saveData)
        {
            if (saveData == null)
                return 0;

            var dateTimeProvider = DateTimeProviderRegistry.Current;
            var now = dateTimeProvider.GetNowTimestamp();
            var elapsedSeconds = now - saveData.StartTimestamp;
            
            if (elapsedSeconds <= 0)
                return 0;

            return (int)(elapsedSeconds / saveData.Duration);
        }

        public void StartTimer()
        {
            if (m_Timer == null)
                return;

            if (!m_IsAddedToController)
            {
                m_TimerController.AddTimer(m_Timer);
                m_IsAddedToController = true;
            }
            
            m_Timer.Start();
            
            SaveTimerData();
        }

        public void StopTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
                
                if (m_IsAddedToController)
                {
                    m_TimerController.RemoveTimer(m_Timer);
                    m_IsAddedToController = false;
                }
            }
        }

        public void ResetTimer() 
        { 
            m_Timer?.Reset();
        }

        private void CreateTimer(TimerSaveData saveData)
        {
            if (saveData != null)
            {
                m_Timer = new SimpleTimer(saveData.EndTimestamp, saveData.Duration);
            }
            else
            {
                m_Timer = new SimpleTimer(m_Duration);
            }
        }

        private void SetupSubscriptions()
        {
            if (m_Timer == null)
                return;

            m_Timer.OnCompleted
                .Subscribe(HandleTimerCompleted)
                .AddTo(m_Disposables);
        }

        private void HandleTimerCompleted(ISimpleTimer timer)
        {
            m_IsAddedToController = false;
        }

        private void LoadTimerData()
        {
            if (m_LoadDataFunc != null)
            {
                m_CurrentSaveData = m_LoadDataFunc.Invoke();
            }
        }

        private void SaveTimerData()
        {
            if (m_SaveDataAction != null && m_Timer != null)
            {
                var saveData = m_Timer.GetSaveData();
                m_SaveDataAction.Invoke(saveData);
            }
        }

        public void Dispose()
        {
            StopTimer();
            m_Disposables?.Dispose();
            m_Timer?.Dispose();
        }
    }
}