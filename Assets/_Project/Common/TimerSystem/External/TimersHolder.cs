using System;
using System.Collections.Generic;
using Common.TimerSystem.External.Interfaces;
using UniRx;
using UnityEngine;

namespace Common.TimerSystem.External
{
    public class TimersHolder : IDisposable
    {
        private readonly Dictionary<int, List<ISimpleTimer>> m_TimerMap = new Dictionary<int, List<ISimpleTimer>>();
        private readonly List<ISimpleTimer> m_TimersToDelete = new List<ISimpleTimer>();
        private readonly List<ISimpleTimer> m_TimersToAdd = new List<ISimpleTimer>();
        private readonly CompositeDisposable m_Disposables = new CompositeDisposable();

        public void AddTimer(ISimpleTimer timer)
        {
            if (timer == null)
            {
                Debug.LogError("[TimersHolder] Cannot add null timer!");
                return;
            }
            m_TimersToAdd.Add(timer);
        }

        public void RemoveTimer(ISimpleTimer timer)
        {
            if (timer == null) return;
            m_TimersToDelete.Add(timer);
        }

        public void Update(float deltaTime)
        {
            ProcessTimersToAdd();
            UpdateActiveTimers(deltaTime);
            ProcessTimersToDelete();
        }

        private void ProcessTimersToAdd()
        {
            foreach (var timer in m_TimersToAdd)
                AddTimerInternal(timer);
            m_TimersToAdd.Clear();
        }

        private void UpdateActiveTimers(float deltaTime)
        {
            foreach (var timerList in m_TimerMap.Values)
            {
                foreach (var timer in timerList)
                    timer.Update(deltaTime);
            }
        }

        private void ProcessTimersToDelete()
        {
            foreach (var timer in m_TimersToDelete)
                RemoveTimerInternal(timer);
            m_TimersToDelete.Clear();
        }

        private void AddTimerInternal(ISimpleTimer timer)
        {
            var id = timer.GetId();
            
            if (!m_TimerMap.ContainsKey(id))
                m_TimerMap[id] = new List<ISimpleTimer>();

            if (m_TimerMap[id].Contains(timer))
            {
                Debug.LogWarning($"[TimersHolder] Timer already exists! ID: {id}");
                return;
            }

            m_TimerMap[id].Add(timer);
            timer.OnCompleted.Subscribe(HandleTimerCompleted).AddTo(m_Disposables);
        }

        private void HandleTimerCompleted(ISimpleTimer timer)
        {
            RemoveTimer(timer);
        }

        private void RemoveTimerInternal(ISimpleTimer timer)
        {
            var timerId = timer.GetId();
            if (!m_TimerMap.ContainsKey(timerId)) return;

            var timers = m_TimerMap[timerId];
            timers.Remove(timer);

            if (timers.Count == 0)
                m_TimerMap.Remove(timerId);
        }

        public int GetActiveTimersCount()
        {
            var count = 0;
            foreach (var timerList in m_TimerMap.Values)
                count += timerList.Count;
            return count;
        }

        public void Dispose() 
        { 
            m_Disposables?.Dispose(); 
        }
    }
}