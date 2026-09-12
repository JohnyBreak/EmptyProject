using System;

namespace Common.TimerSystem.External.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime GetNow();
        long GetNowTimestamp();
    }
}