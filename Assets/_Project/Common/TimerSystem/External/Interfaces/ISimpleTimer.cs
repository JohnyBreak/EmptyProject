using System;
using Common.TimerSystem.External.Models;

namespace Common.TimerSystem.External.Interfaces
{
    public interface ISimpleTimer : IDisposable
    {
        int GetId();
        void Update(float deltaTime);
        
        SimpleTimerInfo GetTimerInfo();
        bool IsRunning { get; }
        
        IObservable<ISimpleTimer> OnCompleted { get; }
        IObservable<SimpleTimerInfo> OnTick { get; }
        
        void Start();
        void Stop();
        void Reset();
    }
}