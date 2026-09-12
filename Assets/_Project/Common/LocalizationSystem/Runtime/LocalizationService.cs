using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Common.LocalizationSystem.Runtime
{
    public class LocalizationService : ILocalizationService, IDisposable
    {
        private const string DEFAULT_LANGUAGE = "EN";
        
        private readonly ReactiveProperty<string> m_CurrentLanguage = new(DEFAULT_LANGUAGE);
        private readonly Subject<Unit> m_OnLocalizationChanged = new();
        private readonly CompositeDisposable m_Disposables = new();
        
        private Dictionary<string, Dictionary<string, string>> m_LocalizationData = new();
        private List<string> m_AvailableLanguages = new();
        private bool m_IsInitialized = false;

        public IReadOnlyReactiveProperty<string> CurrentLanguage => m_CurrentLanguage.ToReadOnlyReactiveProperty();
        public IObservable<Unit> OnLocalizationChanged => m_OnLocalizationChanged.AsObservable();

        public void Initialize(Dictionary<string, Dictionary<string, string>> localizationData, string initialLanguage = null)
        {
            if (m_IsInitialized)
                return;

            m_LocalizationData = localizationData ?? new Dictionary<string, Dictionary<string, string>>();
            m_AvailableLanguages = m_LocalizationData.Keys.ToList();
            
            string targetLanguage = initialLanguage ?? DEFAULT_LANGUAGE;
            if (m_AvailableLanguages.Contains(targetLanguage))
            {
                m_CurrentLanguage.Value = targetLanguage;
            }
            else if (m_AvailableLanguages.Count > 0)
            {
                m_CurrentLanguage.Value = m_AvailableLanguages[0];
            }

            m_CurrentLanguage
                .Subscribe(_ => m_OnLocalizationChanged.OnNext(Unit.Default))
                .AddTo(m_Disposables);

            m_IsInitialized = true;
        }

        public void SetLanguage(string languageCode)
        {
            if (!m_IsInitialized || string.IsNullOrEmpty(languageCode))
                return;

            string upperLanguage = languageCode.ToUpper();
            
            if (!m_AvailableLanguages.Contains(upperLanguage))
                return;

            if (m_CurrentLanguage.Value != upperLanguage)
            {
                m_CurrentLanguage.Value = upperLanguage;
            }
        }

        public string GetLocalizedText(string key)
        {
            if (!m_IsInitialized || string.IsNullOrEmpty(key))
                return key;
            
            if (m_LocalizationData.TryGetValue(m_CurrentLanguage.Value, out var languageData) && 
                languageData.TryGetValue(key, out var localizedText))
            {
                return localizedText;
            }
            
            if (m_CurrentLanguage.Value != DEFAULT_LANGUAGE && 
                m_LocalizationData.TryGetValue(DEFAULT_LANGUAGE, out var defaultData) && 
                defaultData.TryGetValue(key, out var defaultText))
            {
                return defaultText;
            }
            
            return key;
        }

        public string GetLocalizedText(string key, params object[] args)
        {
            string localizedText = GetLocalizedText(key);
            
            if (args == null || args.Length == 0)
                return localizedText;

            try
            {
                return string.Format(localizedText, args);
            }
            catch (FormatException)
            {
                return localizedText;
            }
        }

        public bool HasKey(string key)
        {
            if (!m_IsInitialized || string.IsNullOrEmpty(key))
                return false;

            return m_LocalizationData.TryGetValue(m_CurrentLanguage.Value, out var languageData) && 
                   languageData.ContainsKey(key);
        }

        public List<string> GetAvailableLanguages()
        {
            return new List<string>(m_AvailableLanguages);
        }

        public void Dispose()
        {
            m_Disposables?.Dispose();
            m_OnLocalizationChanged?.Dispose();
            m_CurrentLanguage?.Dispose();
        }
    }
}