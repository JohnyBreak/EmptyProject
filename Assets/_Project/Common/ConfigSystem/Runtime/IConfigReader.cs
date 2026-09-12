using Utils.Result;

namespace Common.ConfigSystem.Runtime
{
    public interface IConfigReader
    {
        Result<T> Deserialize<T>(string json);
    }
}