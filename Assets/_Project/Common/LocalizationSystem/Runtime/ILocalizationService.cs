using System;
using System.Collections.Generic;
using UniRx;

namespace Common.LocalizationSystem.Runtime
{
    public interface ILocalizationService
    {
        IReadOnlyReactiveProperty<string> CurrentLanguage { get; }
        IObservable<Unit> OnLocalizationChanged { get; }
        void Initialize(Dictionary<string, Dictionary<string, string>> localizationData, string initialLanguage = null);
        
        void SetLanguage(string languageCode);
        string GetLocalizedText(string key);
        string GetLocalizedText(string key, params object[] args);
        bool HasKey(string key);
        List<string> GetAvailableLanguages();
    }
}