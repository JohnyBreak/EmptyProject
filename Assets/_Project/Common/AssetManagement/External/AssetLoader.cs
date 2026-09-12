using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Common.AssetManagement.External
{
    public class AssetLoader : MonoBehaviour, IAssetLoader
    {
        private readonly Dictionary<string, UnityEngine.Object> m_LoadedAssets = new();
        private readonly Dictionary<string, List<GameObject>> m_InstantiatedObjects = new();
        private readonly ReactiveProperty<int> m_LoadedAssetsCount = new(0);

        public IReadOnlyReactiveProperty<int> LoadedAssetsCount => m_LoadedAssetsCount;

        public T LoadSync<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
                return null;

            if (m_LoadedAssets.TryGetValue(address, out var cachedAsset) && cachedAsset is T cached)
            {
                Debug.Log($"[AssetLoader] Found in cache: {address}");
                return cached;
            }

            try
            {
                var handle = Addressables.LoadAssetAsync<T>(address);
                var asset = handle.WaitForCompletion();

                if (asset != null)
                {
                    m_LoadedAssets[address] = asset;
                    m_LoadedAssetsCount.Value = m_LoadedAssets.Count;
                    Debug.Log($"[AssetLoader] Loaded: {address}");
                }
                else
                {
                    Debug.LogError($"[AssetLoader] Failed to load: {address}");
                }

                return asset;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetLoader] Error loading {address}: {ex.Message}");
                return null;
            }
        }

        public T InstantiateSync<T>(string address) where T : Component
        {
            return InstantiateSync<T>(address, null);
        }

        public T InstantiateSync<T>(string address, Transform parent) where T : Component
        {
            var prefab = LoadSync<GameObject>(address);
            if (prefab == null)
            {
                Debug.LogError($"[AssetLoader] Cannot instantiate - prefab not loaded: {address}");
                return null;
            }

            var instance = Instantiate(prefab, parent);
            TrackInstance(address, instance);

            if (instance.TryGetComponent<T>(out var component))
            {
                Debug.Log($"[AssetLoader] Instantiated: {address}");
                return component;
            }

            Debug.LogError($"[AssetLoader] Component {typeof(T)} not found on {address}");
            Destroy(instance);
            RemoveTrackedInstance(address, instance);
            return null;
        }

        public bool IsLoaded(string address)
        {
            return !string.IsNullOrEmpty(address) && m_LoadedAssets.ContainsKey(address);
        }

        public void Release(string address)
        {
            if (string.IsNullOrEmpty(address))
                return;

            ReleaseInstances(address);

            if (m_LoadedAssets.TryGetValue(address, out var asset))
            {
                Addressables.Release(asset);
                m_LoadedAssets.Remove(address);
                m_LoadedAssetsCount.Value = m_LoadedAssets.Count;
                Debug.Log($"[AssetLoader] Released: {address}");
            }
        }

        public void ReleaseAll()
        {
            foreach (var kvp in m_InstantiatedObjects)
            {
                foreach (var instance in kvp.Value)
                {
                    if (instance != null)
                        Destroy(instance);
                }
            }

            foreach (var kvp in m_LoadedAssets)
            {
                Addressables.Release(kvp.Value);
            }

            m_LoadedAssets.Clear();
            m_InstantiatedObjects.Clear();
            m_LoadedAssetsCount.Value = 0;
            Debug.Log("[AssetLoader] Released all assets");
        }

        public IObservable<AssetLoadResult<T>> LoadAsync<T>(string address) where T : UnityEngine.Object
        {
            return Observable.Create<AssetLoadResult<T>>(observer =>
            {
                if (m_LoadedAssets.TryGetValue(address, out var cachedAsset) && cachedAsset is T cached)
                {
                    observer.OnNext(AssetLoadResult<T>.Success(address, cached));
                    observer.OnCompleted();
                    return Disposable.Empty;
                }

                StartCoroutine(LoadAssetCoroutine<T>(address, observer));
                return Disposable.Empty;
            });
        }

        public IObservable<AssetInstantiateResult<T>> InstantiateAsync<T>(string address) where T : Component
        {
            return InstantiateAsync<T>(address, null);
        }

        public IObservable<AssetInstantiateResult<T>> InstantiateAsync<T>(string address, Transform parent) where T : Component
        {
            return LoadAsync<GameObject>(address)
                .SelectMany(loadResult =>
                {
                    if (!loadResult.IsSuccess)
                        return Observable.Return(AssetInstantiateResult<T>.Failure(address, loadResult.ErrorMessage));

                    try
                    {
                        var instance = InstantiateSync<T>(address, parent);
                        var result = instance != null
                            ? AssetInstantiateResult<T>.Success(address, instance)
                            : AssetInstantiateResult<T>.Failure(address, $"Failed to instantiate: {address}");

                        return Observable.Return(result);
                    }
                    catch (Exception ex)
                    {
                        return Observable.Return(AssetInstantiateResult<T>.Failure(address, ex.Message));
                    }
                });
        }

        private IEnumerator LoadAssetCoroutine<T>(string address, IObserver<AssetLoadResult<T>> observer) where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                m_LoadedAssets[address] = handle.Result;
                m_LoadedAssetsCount.Value = m_LoadedAssets.Count;
                Debug.Log($"[AssetLoader] Async loaded: {address}");
                observer.OnNext(AssetLoadResult<T>.Success(address, handle.Result));
            }
            else
            {
                Debug.LogError($"[AssetLoader] Async load failed: {address}");
                observer.OnNext(AssetLoadResult<T>.Failure(address, $"Failed to load asset: {address}"));
            }

            observer.OnCompleted();
        }

        private void TrackInstance(string address, GameObject instance)
        {
            if (!m_InstantiatedObjects.ContainsKey(address))
            {
                m_InstantiatedObjects[address] = new List<GameObject>();
            }
            m_InstantiatedObjects[address].Add(instance);
        }

        private void RemoveTrackedInstance(string address, GameObject instance)
        {
            if (m_InstantiatedObjects.TryGetValue(address, out var instances))
            {
                instances.Remove(instance);
                if (instances.Count == 0)
                {
                    m_InstantiatedObjects.Remove(address);
                }
            }
        }

        private void ReleaseInstances(string address)
        {
            if (m_InstantiatedObjects.TryGetValue(address, out var instances))
            {
                foreach (var instance in instances)
                {
                    if (instance != null)
                        Destroy(instance);
                }
                m_InstantiatedObjects.Remove(address);
            }
        }
    }
}