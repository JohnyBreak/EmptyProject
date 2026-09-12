using Common.TimerSystem.External.Interfaces;

namespace Common.TimerSystem.External.Providers
{
    public static class DateTimeProviderRegistry
    {
        private static IDateTimeProvider s_Provider = new LocalDateTimeProvider();

        public static IDateTimeProvider Current 
        { 
            get { return s_Provider; } 
        }

        public static void SetProvider(IDateTimeProvider provider)
        {
            s_Provider = provider ?? new LocalDateTimeProvider();
        }
    }
}