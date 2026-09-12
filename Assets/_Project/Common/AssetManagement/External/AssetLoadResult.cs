namespace Common.AssetManagement.External
{
    public class AssetLoadResult<T> where T : UnityEngine.Object
    {
        public string Address { get; }
        public T Asset { get; }
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }

        public AssetLoadResult(string address, T asset, bool isSuccess, string errorMessage = null)
        {
            Address = address;
            Asset = asset;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static AssetLoadResult<T> Success(string address, T asset) 
            => new AssetLoadResult<T>(address, asset, true);

        public static AssetLoadResult<T> Failure(string address, string error) 
            => new AssetLoadResult<T>(address, null, false, error);
    }
}