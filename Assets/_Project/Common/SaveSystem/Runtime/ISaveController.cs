namespace Common.SaveSystem.Runtime
{
    public interface ISaveController
    {
        void SaveData<T>(T saveData) where T : ISaveData;
        T LoadData<T>(string saveKey) where T : ISaveData, new();
        bool HasSave(string saveKey);
        void DeleteSave(string saveKey);
        void DeleteAllSaves();
    }
}