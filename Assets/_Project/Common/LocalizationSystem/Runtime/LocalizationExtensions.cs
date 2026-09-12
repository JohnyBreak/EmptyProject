namespace Common.LocalizationSystem.Runtime
{
    public static class LocalizationExtensions
    {
        private static ILocalizationService s_LocalizationService;

        public static void Initialize(ILocalizationService service)
        {
            s_LocalizationService = service;
        }

        public static string Localize(this string key)
        {
            return s_LocalizationService?.GetLocalizedText(key) ?? key;
        }

        public static string Localize(this string key, params object[] args)
        {
            return s_LocalizationService?.GetLocalizedText(key, args) ?? key;
        }
    }
}