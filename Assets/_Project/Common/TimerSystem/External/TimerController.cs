using Common.TimerSystem.External.Interfaces;
using UnityEngine;

namespace Common.TimerSystem.External
{
    public class TimerController : MonoBehaviour, ITimerController
    {
        private readonly TimersHolder m_TimersHolder = new TimersHolder();

        public void AddTimer(ISimpleTimer timer) 
        { 
            m_TimersHolder.AddTimer(timer); 
        }
        
        public void RemoveTimer(ISimpleTimer timer) 
        { 
            m_TimersHolder.RemoveTimer(timer); 
        }
        
        public int GetActiveTimersCount() 
        { 
            return m_TimersHolder.GetActiveTimersCount(); 
        }

        private void Update() 
        { 
            m_TimersHolder.Update(Time.unscaledDeltaTime); 
        }
        
        private void OnDestroy() 
        { 
            m_TimersHolder?.Dispose(); 
        }
    }
}