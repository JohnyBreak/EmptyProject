using System;
using Common.TimerSystem.External.Interfaces;
using Common.TimerSystem.External.Models;
using UniRx;
using UnityEngine;

namespace Common.TimerSystem.External.Samples
{
     public class TimerWithSaveSample : IDisposable
    {
        private readonly ITimerController m_TimerController;
        private readonly float m_DelayTime = 100f;
        private CompositeDisposable m_TimerDisposables;
        
        private SimpleTimerWrapper m_Timer;
        
        private readonly Func<TimerSaveData> m_LoadFunc;
        private readonly Action<TimerSaveData> m_SaveAction;
        
        public TimerWithSaveSample(ITimerController timerController, Func<TimerSaveData> loadFunc,
            Action<TimerSaveData> saveFunc)
        {
            m_TimerController = timerController;
            m_LoadFunc = loadFunc;
            m_SaveAction = saveFunc;
        }
        
        public void StartTimer()
        {
            StopTimer();
            
            m_Timer = new SimpleTimerWrapper(
                timerController: m_TimerController,
                loadDataFunc: LoadTimerData,
                saveDataAction: SaveTimerData,
                duration: m_DelayTime
            );
            
            TimerSubscribes();
            
            m_Timer.StartTimer();
            
            Debug.Log("TimerWithSave: Timer started");
        }

        private void TimerSubscribes()
        {
            m_TimerDisposables = new CompositeDisposable();
            
            m_Timer.OnTick
                .Sample(TimeSpan.FromSeconds(1f))
                .Subscribe(OnTimerTick)
                .AddTo(m_TimerDisposables);
            
            m_Timer.OnCompleted
                .Subscribe(OnTimerCompleted)
                .AddTo(m_TimerDisposables);
        }
        
        public void StopTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.Dispose();
                m_Timer = null;
            }
            
            if (m_TimerDisposables != null)
            {
                m_TimerDisposables.Dispose();
                m_TimerDisposables = null;
            }
            
            Debug.Log("TimerWithSave: Timer stopped");
        }
        
        private void OnTimerTick(SimpleTimerInfo timerInfo)
        {
            Debug.Log($"TimerWithSave: Timer ticked - {timerInfo.TimeLeft:F1}s left (Progress: {timerInfo.ProgressPercent:F0}%)");
        }

        private void OnTimerCompleted(ISimpleTimer timer)
        {
            Debug.Log("TimerWithSave: Timer completed!");

            SaveTimerData(null);
            
            StopTimer();
        }
        
        private TimerSaveData LoadTimerData()
        {
            return m_LoadFunc();
        }

        private void SaveTimerData(TimerSaveData data)
        {
            m_SaveAction(data);
        }

        public void Dispose()
        {
            StopTimer();
        }
    }
}