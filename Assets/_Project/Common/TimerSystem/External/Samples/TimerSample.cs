using System;
using Common.TimerSystem.External.Interfaces;
using Common.TimerSystem.External.Models;
using UniRx;
using UnityEngine;

namespace Common.TimerSystem.External.Samples
{
    public class TimerSample : IDisposable
    {
        private readonly ITimerController m_TimerController;
        private readonly float m_DelayTime = 2f;
        private CompositeDisposable m_TimerDisposables;

        private SimpleTimerWrapper m_Timer;
        
        public TimerSample(ITimerController timerController)
        {
            m_TimerController = timerController;
        }
        
        public void StartTimer()
        {
            StopTimer();
            
            m_Timer = new SimpleTimerWrapper(m_TimerController, m_DelayTime);
            
            TimerSubscribes();
            
            m_Timer.StartTimer();
            
            Debug.Log("PickupTimer: Timer started");
        }

        private void TimerSubscribes()
        {
            m_TimerDisposables = new CompositeDisposable();
            
            m_Timer.OnTick
                .Sample(TimeSpan.FromSeconds(0.5f))
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
            
            Debug.Log("PickupTimer: Timer stopped");
        }
        
        private void OnTimerTick(SimpleTimerInfo timerInfo)
        {
            Debug.Log($"PickupTimer: Timer ticked - {timerInfo.TimeLeft:F1}s left");
        }

        private void OnTimerCompleted(ISimpleTimer timer)
        {
            Debug.Log("PickupTimer: Timer completed");
            
            StopTimer();
        }

        public void Dispose()
        {
            StopTimer();
        }
    }
}