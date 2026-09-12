namespace Common.TimerSystem.External.Interfaces
{
    public interface ITimerController
    {
        void AddTimer(ISimpleTimer timer);
        void RemoveTimer(ISimpleTimer timer);
        int GetActiveTimersCount();
    }
}