using System;

namespace Common.SaveSystem.Runtime
{
    public interface IAutoSaveController : ISaveController
    {
        void RegisterAutoSave<T>(T saveData) where T : ISaveData;
        void UnregisterAutoSave(string saveKey);
        void SaveAll();
        void LoadAll();
        void SetAutoSaveInterval(float seconds);
        event Action<string> OnDataSaved;
        event Action<string> OnDataLoaded;
    }
}