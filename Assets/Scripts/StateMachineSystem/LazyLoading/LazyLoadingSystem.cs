using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineSystem.Interfaces;
using StateMachineSystem.Core;

namespace StateMachineSystem.LazyLoading
{
    /// <summary>
    /// Hệ thống LazyLoad cho State Machine - chỉ load states và resources khi thực sự cần thiết
    /// </summary>
    public class LazyLoadingSystem : MonoBehaviour
    {
        [Header("Lazy Loading Configuration")]
        [SerializeField] private bool enableLazyLoading = true;
        [SerializeField] private int maxConcurrentLoads = 3;
        [SerializeField] private float loadTimeoutSeconds = 30f;
        [SerializeField] private bool preloadAdjacentStates = true;
        [SerializeField] private int adjacentStateDepth = 1;

        private Dictionary<string, LazyState> lazyStates = new Dictionary<string, LazyState>();
        private Dictionary<string, LazyResource> lazyResources = new Dictionary<string, LazyResource>();
        private Queue<ILazyLoadable> loadingQueue = new Queue<ILazyLoadable>();
        private List<Coroutine> activeLoadCoroutines = new List<Coroutine>();

        /// <summary>
        /// Event được trigger khi một state được lazy load thành công
        /// </summary>
        public event Action<string, IState> OnStateLoaded;
        
        /// <summary>
        /// Event được trigger khi một resource được lazy load thành công
        /// </summary>
        public event Action<string, object> OnResourceLoaded;
        
        /// <summary>
        /// Event được trigger khi có lỗi trong quá trình lazy loading
        /// </summary>
        public event Action<string, string> OnLoadingError;

        #region Public Methods

        /// <summary>
        /// Đăng ký một state để lazy load
        /// </summary>
        /// <param name="stateID">ID của state</param>
        /// <param name="stateFactory">Factory function để tạo state</param>
        /// <param name="dependencies">Danh sách dependencies cần load trước</param>
        /// <summary>
        /// Đăng ký một state để lazy load
        /// State chỉ được tạo khi thực sự cần thiết
        /// </summary>
        /// <param name="stateID">ID duy nhất của state</param>
        /// <param name="stateFactory">Factory function để tạo state</param>
        /// <param name="dependencies">Danh sách dependencies cần load trước (optional)</param>
        public void RegisterLazyState(string stateID, Func<IState> stateFactory, List<string> dependencies = null)
        {
            if (string.IsNullOrEmpty(stateID))
            {
                Debug.LogError("[LazyLoadingSystem] StateID cannot be null or empty");
                return;
            }

            var lazyState = new LazyState(stateID, stateFactory, dependencies ?? new List<string>());
            lazyStates[stateID] = lazyState;
            
            Debug.Log($"[LazyLoadingSystem] Registered lazy state: {stateID}");
        }

        /// <summary>
        /// Đăng ký một resource để lazy load
        /// </summary>
        /// <param name="resourceID">ID của resource</param>
        /// <param name="resourcePath">Đường dẫn đến resource</param>
        /// <param name="resourceLoader">Function để load resource</param>
        /// <summary>
        /// Đăng ký một resource để lazy load
        /// Resource chỉ được load khi thực sự cần thiết
        /// </summary>
        /// <param name="resourceID">ID duy nhất của resource</param>
        /// <param name="resourcePath">Đường dẫn đến resource</param>
        /// <param name="resourceLoader">Function để load resource từ path</param>
        public void RegisterLazyResource(string resourceID, string resourcePath, Func<string, object> resourceLoader)
        {
            if (string.IsNullOrEmpty(resourceID))
            {
                Debug.LogError("[LazyLoadingSystem] ResourceID cannot be null or empty");
                return;
            }

            var lazyResource = new LazyResource(resourceID, resourcePath, resourceLoader);
            lazyResources[resourceID] = lazyResource;
            
            Debug.Log($"[LazyLoadingSystem] Registered lazy resource: {resourceID}");
        }

        /// <summary>
        /// Load một state một cách bất đồng bộ
        /// </summary>
        /// <param name="stateID">ID của state cần load</param>
        /// <param name="onComplete">Callback khi load hoàn thành</param>
        /// <summary>
        /// Load một state một cách bất đồng bộ
        /// Nếu state chưa được đăng ký, sẽ log error
        /// </summary>
        /// <param name="stateID">ID của state cần load</param>
        /// <param name="onComplete">Callback khi load hoàn thành (optional)</param>
        public void LoadStateAsync(string stateID, Action<IState> onComplete = null)
        {
            if (!enableLazyLoading)
            {
                Debug.LogWarning("[LazyLoadingSystem] Lazy loading is disabled");
                return;
            }

            if (!lazyStates.ContainsKey(stateID))
            {
                Debug.LogError($"[LazyLoadingSystem] Lazy state not registered: {stateID}");
                return;
            }

            var lazyState = lazyStates[stateID];
            if (lazyState.IsLoaded)
            {
                onComplete?.Invoke(lazyState.LoadedState);
                return;
            }

            // Add to loading queue
            lazyState.OnLoadComplete += onComplete;
            EnqueueForLoading(lazyState);
        }

        /// <summary>
        /// Load một resource một cách bất đồng bộ
        /// </summary>
        /// <param name="resourceID">ID của resource cần load</param>
        /// <param name="onComplete">Callback khi load hoàn thành</param>
        /// <summary>
        /// Load một resource một cách bất đồng bộ
        /// Nếu resource chưa được đăng ký, sẽ log error
        /// </summary>
        /// <param name="resourceID">ID của resource cần load</param>
        /// <param name="onComplete">Callback khi load hoàn thành (optional)</param>
        public void LoadResourceAsync(string resourceID, Action<object> onComplete = null)
        {
            if (!enableLazyLoading)
            {
                Debug.LogWarning("[LazyLoadingSystem] Lazy loading is disabled");
                return;
            }

            if (!lazyResources.ContainsKey(resourceID))
            {
                Debug.LogError($"[LazyLoadingSystem] Lazy resource not registered: {resourceID}");
                return;
            }

            var lazyResource = lazyResources[resourceID];
            if (lazyResource.IsLoaded)
            {
                onComplete?.Invoke(lazyResource.LoadedResource);
                return;
            }

            // Add to loading queue
            lazyResource.OnLoadComplete += onComplete;
            EnqueueForLoading(lazyResource);
        }

        /// <summary>
        /// Preload các states liền kề với state hiện tại
        /// </summary>
        /// <param name="currentStateID">ID của state hiện tại</param>
        /// <param name="stateMachine">State machine chứa các transition</param>
        /// <summary>
        /// Preload các states liền kề với state hiện tại
        /// Giúp giảm thời gian chờ khi transition
        /// </summary>
        /// <param name="currentStateID">ID của state hiện tại</param>
        /// <param name="stateMachine">State machine chứa các transition</param>
        public void PreloadAdjacentStates(string currentStateID, IStateMachine stateMachine)
        {
            if (!preloadAdjacentStates || stateMachine == null) return;

            var currentState = stateMachine.GetState(currentStateID);
            if (currentState == null) return;

            // Get adjacent states through transitions (would need to extend IState interface)
            // For now, preload based on naming convention or configuration
            PreloadStatesInDepth(currentStateID, adjacentStateDepth);
        }

        /// <summary>
        /// Unload một state để giải phóng memory
        /// </summary>
        /// <param name="stateID">ID của state cần unload</param>
        /// <summary>
        /// Unload một state để giải phóng memory
        /// State sẽ cần được load lại nếu muốn sử dụng
        /// </summary>
        /// <param name="stateID">ID của state cần unload</param>
        public void UnloadState(string stateID)
        {
            if (lazyStates.ContainsKey(stateID))
            {
                var lazyState = lazyStates[stateID];
                lazyState.Unload();
                Debug.Log($"[LazyLoadingSystem] Unloaded state: {stateID}");
            }
        }

        /// <summary>
        /// Unload một resource để giải phóng memory
        /// </summary>
        /// <param name="resourceID">ID của resource cần unload</param>
        /// <summary>
        /// Unload một resource để giải phóng memory
        /// Resource sẽ cần được load lại nếu muốn sử dụng
        /// </summary>
        /// <param name="resourceID">ID của resource cần unload</param>
        public void UnloadResource(string resourceID)
        {
            if (lazyResources.ContainsKey(resourceID))
            {
                var lazyResource = lazyResources[resourceID];
                lazyResource.Unload();
                Debug.Log($"[LazyLoadingSystem] Unloaded resource: {resourceID}");
            }
        }

        /// <summary>
        /// Kiểm tra xem một state đã được load chưa
        /// </summary>
        /// <param name="stateID">ID của state</param>
        /// <returns>True nếu state đã được load</returns>
        public bool IsStateLoaded(string stateID)
        {
            return lazyStates.ContainsKey(stateID) && lazyStates[stateID].IsLoaded;
        }

        /// <summary>
        /// Kiểm tra xem một resource đã được load chưa
        /// </summary>
        /// <param name="resourceID">ID của resource</param>
        /// <returns>True nếu resource đã được load</returns>
        public bool IsResourceLoaded(string resourceID)
        {
            return lazyResources.ContainsKey(resourceID) && lazyResources[resourceID].IsLoaded;
        }

        /// <summary>
        /// Lấy state đã được load (trả về null nếu chưa load)
        /// </summary>
        /// <param name="stateID">ID của state</param>
        /// <returns>State instance hoặc null</returns>
        public IState GetLoadedState(string stateID)
        {
            return lazyStates.ContainsKey(stateID) ? lazyStates[stateID].LoadedState : null;
        }

        /// <summary>
        /// Lấy resource đã được load (trả về null nếu chưa load)
        /// </summary>
        /// <param name="resourceID">ID của resource</param>
        /// <returns>Resource instance hoặc null</returns>
        public object GetLoadedResource(string resourceID)
        {
            return lazyResources.ContainsKey(resourceID) ? lazyResources[resourceID].LoadedResource : null;
        }

        /// <summary>
        /// Unload tất cả states và resources để giải phóng memory
        /// </summary>
        /// <summary>
        /// Unload tất cả states và resources để giải phóng memory
        /// Dừng tất cả loading coroutines và clear queue
        /// </summary>
        public void UnloadAll()
        {
            foreach (var lazyState in lazyStates.Values)
            {
                lazyState.Unload();
            }

            foreach (var lazyResource in lazyResources.Values)
            {
                lazyResource.Unload();
            }

            // Stop all loading coroutines
            foreach (var coroutine in activeLoadCoroutines)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }

            activeLoadCoroutines.Clear();
            loadingQueue.Clear();

            Debug.Log("[LazyLoadingSystem] Unloaded all states and resources");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Thêm một item vào queue để load
        /// </summary>
        /// <param name="loadable">Item cần load</param>
        private void EnqueueForLoading(ILazyLoadable loadable)
        {
            if (loadable.IsLoading) return;

            loadingQueue.Enqueue(loadable);
            ProcessLoadingQueue();
        }

        /// <summary>
        /// Xử lý queue loading
        /// </summary>
        private void ProcessLoadingQueue()
        {
            while (loadingQueue.Count > 0 && activeLoadCoroutines.Count < maxConcurrentLoads)
            {
                var loadable = loadingQueue.Dequeue();
                if (!loadable.IsLoaded && !loadable.IsLoading)
                {
                    var coroutine = StartCoroutine(LoadItemCoroutine(loadable));
                    activeLoadCoroutines.Add(coroutine);
                }
            }
        }

        /// <summary>
        /// Coroutine để load một item
        /// </summary>
        /// <param name="loadable">Item cần load</param>
        private IEnumerator LoadItemCoroutine(ILazyLoadable loadable)
        {
            loadable.StartLoading();
            float startTime = Time.time;

            // Load dependencies first
            foreach (var dependency in loadable.Dependencies)
            {
                yield return StartCoroutine(LoadDependencyCoroutine(dependency));
            }

            // Load the item itself
            yield return StartCoroutine(loadable.LoadCoroutine());

            // Check for timeout
            if (Time.time - startTime > loadTimeoutSeconds)
            {
                Debug.LogError($"[LazyLoadingSystem] Loading timeout for: {loadable.ID}");
                OnLoadingError?.Invoke(loadable.ID, "Loading timeout");
                loadable.OnLoadingFailed("Timeout");
            }
            else if (loadable.IsLoaded)
            {
                // Loading successful
                if (loadable is LazyState lazyState)
                {
                    OnStateLoaded?.Invoke(lazyState.ID, lazyState.LoadedState);
                }
                else if (loadable is LazyResource lazyResource)
                {
                    OnResourceLoaded?.Invoke(lazyResource.ID, lazyResource.LoadedResource);
                }
            }

            // Remove from active coroutines
            activeLoadCoroutines.Remove(StartCoroutine(LoadItemCoroutine(loadable)));
            
            // Process next items in queue
            ProcessLoadingQueue();
        }

        /// <summary>
        /// Load dependency
        /// </summary>
        /// <param name="dependencyID">ID của dependency</param>
        private IEnumerator LoadDependencyCoroutine(string dependencyID)
        {
            if (lazyStates.ContainsKey(dependencyID))
            {
                var lazyState = lazyStates[dependencyID];
                if (!lazyState.IsLoaded)
                {
                    yield return StartCoroutine(lazyState.LoadCoroutine());
                }
            }
            else if (lazyResources.ContainsKey(dependencyID))
            {
                var lazyResource = lazyResources[dependencyID];
                if (!lazyResource.IsLoaded)
                {
                    yield return StartCoroutine(lazyResource.LoadCoroutine());
                }
            }
        }

        /// <summary>
        /// Preload states trong một độ sâu nhất định
        /// </summary>
        /// <param name="startStateID">State bắt đầu</param>
        /// <param name="depth">Độ sâu preload</param>
        private void PreloadStatesInDepth(string startStateID, int depth)
        {
            if (depth <= 0) return;

            // Implementation would depend on how transitions are structured
            // For now, preload based on naming pattern or configuration
            var statePrefix = startStateID.Substring(0, Math.Max(0, startStateID.Length - 1));
            
            foreach (var kvp in lazyStates)
            {
                if (kvp.Key.StartsWith(statePrefix) && kvp.Key != startStateID)
                {
                    LoadStateAsync(kvp.Key);
                }
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Bật/tắt lazy loading
        /// </summary>
        /// <param name="enable">True để bật lazy loading</param>
        public void SetEnableLazyLoading(bool enable)
        {
            enableLazyLoading = enable;
        }

        /// <summary>
        /// Thiết lập số lượng load đồng thời tối đa
        /// </summary>
        /// <param name="maxLoads">Số lượng load đồng thời tối đa</param>
        public void SetMaxConcurrentLoads(int maxLoads)
        {
            maxConcurrentLoads = Mathf.Max(1, maxLoads);
        }

        /// <summary>
        /// Thiết lập timeout cho loading
        /// </summary>
        /// <param name="timeoutSeconds">Timeout tính bằng giây</param>
        public void SetLoadTimeout(float timeoutSeconds)
        {
            loadTimeoutSeconds = Mathf.Max(1f, timeoutSeconds);
        }

        /// <summary>
        /// Bật/tắt preload states liền kề
        /// </summary>
        /// <param name="enable">True để bật preload</param>
        public void SetPreloadAdjacentStates(bool enable)
        {
            preloadAdjacentStates = enable;
        }

        /// <summary>
        /// Thiết lập độ sâu preload cho adjacent states
        /// </summary>
        /// <param name="depth">Độ sâu preload</param>
        public void SetAdjacentStateDepth(int depth)
        {
            adjacentStateDepth = Mathf.Max(0, depth);
        }

        #endregion

        private void OnDestroy()
        {
            UnloadAll();
        }
    }
}
