using System;
using Common.TimerSystem.External.Interfaces;

namespace Common.TimerSystem.External.Providers
{
    public class LocalDateTimeProvider : IDateTimeProvider
    {
        public DateTime GetNow()
        {
            return DateTime.UtcNow;
        }

        public long GetNowTimestamp()
        {
            return ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
        }
    }
}