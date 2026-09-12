using UnityEngine;

namespace Common.AssetManagement.External
{
    public class AssetInstantiateResult<T> where T : Component
    {
        public string Address { get; }
        public T Instance { get; }
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }

        public AssetInstantiateResult(string address, T instance, bool isSuccess, string errorMessage = null)
        {
            Address = address;
            Instance = instance;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static AssetInstantiateResult<T> Success(string address, T instance) 
            => new AssetInstantiateResult<T>(address, instance, true);

        public static AssetInstantiateResult<T> Failure(string address, string error) 
            => new AssetInstantiateResult<T>(address, null, false, error);
    }
}