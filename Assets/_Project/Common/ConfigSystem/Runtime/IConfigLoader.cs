namespace Common.ConfigSystem.Runtime
{
    public interface IConfigLoader
    {
        T GetConfig<T>(string configName) where T : class;
        bool IsConfigLoaded(string configName);
        void PreloadConfig(string configName);
    }
}