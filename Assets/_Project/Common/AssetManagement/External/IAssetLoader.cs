using System;
using UniRx;
using UnityEngine;

namespace Common.AssetManagement.External
{
    public interface IAssetLoader
    {
        T LoadSync<T>(string address) where T : UnityEngine.Object;
        
        T InstantiateSync<T>(string address) where T : Component;
        T InstantiateSync<T>(string address, Transform parent) where T : Component;
        
        bool IsLoaded(string address);
        void Release(string address);
        void ReleaseAll();
        
        IObservable<AssetLoadResult<T>> LoadAsync<T>(string address) where T : UnityEngine.Object;
        IObservable<AssetInstantiateResult<T>> InstantiateAsync<T>(string address) where T : Component;
        IObservable<AssetInstantiateResult<T>> InstantiateAsync<T>(string address, Transform parent) where T : Component;
        
        IReadOnlyReactiveProperty<int> LoadedAssetsCount { get; }
    }
}