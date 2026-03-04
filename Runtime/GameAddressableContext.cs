using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityMVC
{
    /// <summary>
    /// Manages the loading and releasing of addressable assets in the game.
    /// </summary>
    public class GameAddressableContext
    {
        #region Variables

        /// <summary>
        /// List of async operation handles for tracking asset requests.
        /// </summary>
        private readonly List<AsyncOperationHandle> _requestHandles = new ();

        #endregion

        #region Public Methods

        /// <summary>
        /// Asynchronously loads a list of assets by their keys.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets to load.</typeparam>
        /// <param name="keys">The keys of the assets to load.</param>
        /// <param name="instantiate">Whether to instantiate the loaded assets.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of loaded assets.</returns>
        public async Task<List<TObject>> LoadAssetsAsync<TObject>(List<string> keys, bool instantiate = true) where TObject : Object
        {
            var request = _LoadAssetsAsync<TObject>(keys);
            _requestHandles.Add(request);
            var result = await request.Task;
            return ToResultList(result, instantiate);
        }

        /// <summary>
        /// Asynchronously loads a single asset by its key.
        /// </summary>
        /// <typeparam name="TObject">The type of the asset to load.</typeparam>
        /// <param name="key">The key of the asset to load.</param>
        /// <param name="instantiate">Whether to instantiate the loaded asset.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the loaded asset.</returns>
        public async Task<TObject> LoadAssetAsync<TObject>(string key, bool instantiate = true) where TObject : Object
        {
            var result = await LoadRawAssetAsync<TObject>(key);
            if (!instantiate || result == null)
            {
                return result;
            }

            return Object.Instantiate(result);
        }

        /// <summary>
        /// Synchronously loads a list of assets by their keys.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets to load.</typeparam>
        /// <param name="keys">The keys of the assets to load.</param>
        /// <param name="instantiate">Whether to instantiate the loaded assets.</param>
        /// <returns>The list of loaded assets.</returns>
        public List<TObject> LoadAssets<TObject>(List<string> keys, bool instantiate = true) where TObject : Object
        {
            var request = _LoadAssetsAsync<TObject>(keys);
            _requestHandles.Add(request);
            var result = request.WaitForCompletion();
            return ToResultList(result, instantiate);
        }

        /// <summary>
        /// Synchronously loads a single asset by its key.
        /// </summary>
        /// <typeparam name="TObject">The type of the asset to load.</typeparam>
        /// <param name="key">The key of the asset to load.</param>
        /// <param name="instantiate">Whether to instantiate the loaded asset.</param>
        /// <returns>The loaded asset.</returns>
        public TObject LoadAsset<TObject>(string key, bool instantiate = true) where TObject : Object
        {
            var result = LoadRawAsset<TObject>(key);
            if (!instantiate || result == null)
            {
                return result;
            }

            return Object.Instantiate(result);
        }

        /// <summary>
        /// Releases all tracked asset requests.
        /// </summary>
        public void Release()
        {
            // Release every tracked handle to avoid leaks, then clear the list.
            foreach (var handle in _requestHandles)
            {
                Addressables.Release(handle);
            }
            _requestHandles.Clear();
        }

        /// <summary>
        /// Asynchronously loads a single asset by its key, without instantiating it.
        /// </summary>
        /// <typeparam name="TObject">The type of the asset to load.</typeparam>
        /// <param name="key">The key of the asset to load.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the loaded asset (not instantiated).</returns>
        public async Task<TObject> LoadRawAssetAsync<TObject>(string key) where TObject : Object
        {
            var handle = Addressables.LoadAssetAsync<TObject>(key);
            _requestHandles.Add(handle);
            var result = await handle.Task;
            return result;
        }

        /// <summary>
        /// Synchronously loads a single asset by its key, without instantiating it.
        /// </summary>
        /// <typeparam name="TObject">The type of the asset to load.</typeparam>
        /// <param name="key">The key of the asset to load.</param>
        /// <returns>The loaded asset (not instantiated).</returns>
        public TObject LoadRawAsset<TObject>(string key) where TObject : Object
        {
            var handle = Addressables.LoadAssetAsync<TObject>(key);
            _requestHandles.Add(handle);
            var result = handle.WaitForCompletion();
            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates an async operation handle for loading a list of assets by their keys.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets to load.</typeparam>
        /// <param name="keys">The keys of the assets to load.</param>
        /// <returns>The async operation handle for the asset loading request.</returns>
        private AsyncOperationHandle<IList<TObject>> _LoadAssetsAsync<TObject>(List<string> keys) where TObject : Object
        {
            return Addressables.LoadAssetsAsync<TObject>(keys, null, Addressables.MergeMode.Union, false);
        }

        private static List<TObject> ToResultList<TObject>(IList<TObject> result, bool instantiate) where TObject : Object
        {
            var count = result?.Count ?? 0;
            var items = new List<TObject>(count);
            for (var index = 0; index < count; index++)
            {
                var item = result[index];
                items.Add(instantiate && item != null ? Object.Instantiate(item) : item);
            }

            return items;
        }

        #endregion
    }
}
