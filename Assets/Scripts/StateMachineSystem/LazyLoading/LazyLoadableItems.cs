using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineSystem.Interfaces;

namespace StateMachineSystem.LazyLoading
{
    /// <summary>
    /// Interface cho các item có thể lazy load
    /// </summary>
    public interface ILazyLoadable
    {
        /// <summary>
        /// ID duy nhất của item
        /// </summary>
        string ID { get; }
        
        /// <summary>
        /// Danh sách dependencies cần load trước item này
        /// </summary>
        List<string> Dependencies { get; }
        
        /// <summary>
        /// Kiểm tra xem item đã được load chưa
        /// </summary>
        bool IsLoaded { get; }
        
        /// <summary>
        /// Kiểm tra xem item đang trong quá trình loading không
        /// </summary>
        bool IsLoading { get; }
        
        /// <summary>
        /// Bắt đầu quá trình loading
        /// </summary>
        void StartLoading();
        
        /// <summary>
        /// Coroutine để load item
        /// </summary>
        /// <returns>IEnumerator cho coroutine</returns>
        IEnumerator LoadCoroutine();
        
        /// <summary>
        /// Unload item để giải phóng memory
        /// </summary>
        void Unload();
        
        /// <summary>
        /// Được gọi khi loading thất bại
        /// </summary>
        /// <param name="error">Thông tin lỗi</param>
        void OnLoadingFailed(string error);
        
        /// <summary>
        /// Event được trigger khi loading hoàn thành
        /// </summary>
        event Action<object> OnLoadComplete;
    }

    /// <summary>
    /// Lazy loadable state - chỉ tạo state khi thực sự cần thiết
    /// </summary>
    public class LazyState : ILazyLoadable
    {
        private readonly Func<IState> stateFactory;
        private IState loadedState;
        private bool isLoading = false;

        public string ID { get; }
        public List<string> Dependencies { get; }
        public bool IsLoaded => loadedState != null;
        public bool IsLoading => isLoading;
        public IState LoadedState => loadedState;

        /// <summary>
        /// Event được trigger khi state được load xong
        /// </summary>
        public event Action<object> OnLoadComplete;

        /// <summary>
        /// Constructor cho LazyState
        /// </summary>
        /// <param name="stateID">ID của state</param>
        /// <param name="factory">Factory function để tạo state</param>
        /// <param name="dependencies">Danh sách dependencies</param>
        public LazyState(string stateID, Func<IState> factory, List<string> dependencies)
        {
            ID = stateID;
            stateFactory = factory;
            Dependencies = dependencies ?? new List<string>();
        }

        /// <summary>
        /// Bắt đầu quá trình loading state
        /// </summary>
        public void StartLoading()
        {
            if (IsLoaded || IsLoading) return;
            isLoading = true;
        }

        /// <summary>
        /// Coroutine để load state
        /// </summary>
        /// <returns>IEnumerator cho Unity coroutine</returns>
        public IEnumerator LoadCoroutine()
        {
            if (IsLoaded) yield break;

            // Simulate loading time (in real scenario, this could be asset loading, etc.)
            yield return new WaitForSeconds(0.1f);

            try
            {
                // Create the state using factory
                loadedState = stateFactory?.Invoke();
                
                if (loadedState != null)
                {
                    // Initialize the state if needed
                    Debug.Log($"[LazyState] Successfully loaded state: {ID}");
                    OnLoadComplete?.Invoke(loadedState);
                }
                else
                {
                    Debug.LogError($"[LazyState] Factory returned null for state: {ID}");
                    OnLoadingFailed("Factory returned null");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LazyState] Failed to load state {ID}: {e.Message}");
                OnLoadingFailed(e.Message);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Unload state để giải phóng memory
        /// </summary>
        public void Unload()
        {
            if (loadedState != null)
            {
                // Cleanup state if needed
                if (loadedState.IsActive)
                {
                    loadedState.Exit();
                }
                
                loadedState = null;
                Debug.Log($"[LazyState] Unloaded state: {ID}");
            }
        }

        /// <summary>
        /// Xử lý khi loading thất bại
        /// </summary>
        /// <param name="error">Thông tin lỗi</param>
        public void OnLoadingFailed(string error)
        {
            isLoading = false;
            Debug.LogError($"[LazyState] Loading failed for state {ID}: {error}");
        }
    }

    /// <summary>
    /// Lazy loadable resource - chỉ load resource khi thực sự cần thiết
    /// </summary>
    public class LazyResource : ILazyLoadable
    {
        private readonly string resourcePath;
        private readonly Func<string, object> resourceLoader;
        private object loadedResource;
        private bool isLoading = false;

        public string ID { get; }
        public string ResourcePath => resourcePath;
        public List<string> Dependencies { get; }
        public bool IsLoaded => loadedResource != null;
        public bool IsLoading => isLoading;
        public object LoadedResource => loadedResource;

        /// <summary>
        /// Event được trigger khi resource được load xong
        /// </summary>
        public event Action<object> OnLoadComplete;

        /// <summary>
        /// Constructor cho LazyResource
        /// </summary>
        /// <param name="resourceID">ID của resource</param>
        /// <param name="path">Đường dẫn đến resource</param>
        /// <param name="loader">Function để load resource</param>
        /// <param name="dependencies">Danh sách dependencies</param>
        public LazyResource(string resourceID, string path, Func<string, object> loader, List<string> dependencies = null)
        {
            ID = resourceID;
            resourcePath = path;
            resourceLoader = loader;
            Dependencies = dependencies ?? new List<string>();
        }

        /// <summary>
        /// Bắt đầu quá trình loading resource
        /// </summary>
        public void StartLoading()
        {
            if (IsLoaded || IsLoading) return;
            isLoading = true;
        }

        /// <summary>
        /// Coroutine để load resource
        /// </summary>
        /// <returns>IEnumerator cho Unity coroutine</returns>
        public IEnumerator LoadCoroutine()
        {
            if (IsLoaded) yield break;

            // Simulate async loading
            yield return new WaitForSeconds(0.2f);

            try
            {
                // Load the resource using the provided loader
                loadedResource = resourceLoader?.Invoke(resourcePath);
                
                if (loadedResource != null)
                {
                    Debug.Log($"[LazyResource] Successfully loaded resource: {ID} from {resourcePath}");
                    OnLoadComplete?.Invoke(loadedResource);
                }
                else
                {
                    Debug.LogError($"[LazyResource] Loader returned null for resource: {ID}");
                    OnLoadingFailed("Loader returned null");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LazyResource] Failed to load resource {ID}: {e.Message}");
                OnLoadingFailed(e.Message);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Unload resource để giải phóng memory
        /// </summary>
        public void Unload()
        {
            if (loadedResource != null)
            {
                // Cleanup resource if it's a Unity Object
                if (loadedResource is UnityEngine.Object unityObj)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(unityObj);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(unityObj);
                    }
                }
                
                loadedResource = null;
                Debug.Log($"[LazyResource] Unloaded resource: {ID}");
            }
        }

        /// <summary>
        /// Xử lý khi loading thất bại
        /// </summary>
        /// <param name="error">Thông tin lỗi</param>
        public void OnLoadingFailed(string error)
        {
            isLoading = false;
            Debug.LogError($"[LazyResource] Loading failed for resource {ID}: {error}");
        }

        /// <summary>
        /// Lấy resource với kiểu cụ thể
        /// </summary>
        /// <typeparam name="T">Kiểu của resource</typeparam>
        /// <returns>Resource được cast về kiểu T, hoặc default(T) nếu không thể cast</returns>
        public T GetResource<T>() where T : class
        {
            return loadedResource as T;
        }
    }

    /// <summary>
    /// Lazy loadable state machine - chỉ tạo state machine khi cần thiết
    /// </summary>
    public class LazyStateMachine : ILazyLoadable
    {
        private readonly Func<IStateMachine> stateMachineFactory;
        private IStateMachine loadedStateMachine;
        private bool isLoading = false;

        public string ID { get; }
        public List<string> Dependencies { get; }
        public bool IsLoaded => loadedStateMachine != null;
        public bool IsLoading => isLoading;
        public IStateMachine LoadedStateMachine => loadedStateMachine;

        /// <summary>
        /// Event được trigger khi state machine được load xong
        /// </summary>
        public event Action<object> OnLoadComplete;

        /// <summary>
        /// Constructor cho LazyStateMachine
        /// </summary>
        /// <param name="stateMachineID">ID của state machine</param>
        /// <param name="factory">Factory function để tạo state machine</param>
        /// <param name="dependencies">Danh sách dependencies</param>
        public LazyStateMachine(string stateMachineID, Func<IStateMachine> factory, List<string> dependencies = null)
        {
            ID = stateMachineID;
            stateMachineFactory = factory;
            Dependencies = dependencies ?? new List<string>();
        }

        /// <summary>
        /// Bắt đầu quá trình loading state machine
        /// </summary>
        public void StartLoading()
        {
            if (IsLoaded || IsLoading) return;
            isLoading = true;
        }

        /// <summary>
        /// Coroutine để load state machine
        /// </summary>
        /// <returns>IEnumerator cho Unity coroutine</returns>
        public IEnumerator LoadCoroutine()
        {
            if (IsLoaded) yield break;

            yield return new WaitForSeconds(0.15f);

            try
            {
                // Create the state machine using factory
                loadedStateMachine = stateMachineFactory?.Invoke();
                
                if (loadedStateMachine != null)
                {
                    Debug.Log($"[LazyStateMachine] Successfully loaded state machine: {ID}");
                    OnLoadComplete?.Invoke(loadedStateMachine);
                }
                else
                {
                    Debug.LogError($"[LazyStateMachine] Factory returned null for state machine: {ID}");
                    OnLoadingFailed("Factory returned null");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LazyStateMachine] Failed to load state machine {ID}: {e.Message}");
                OnLoadingFailed(e.Message);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Unload state machine để giải phóng memory
        /// </summary>
        public void Unload()
        {
            if (loadedStateMachine != null)
            {
                if (loadedStateMachine.IsRunning)
                {
                    loadedStateMachine.Stop();
                }
                
                loadedStateMachine = null;
                Debug.Log($"[LazyStateMachine] Unloaded state machine: {ID}");
            }
        }

        /// <summary>
        /// Xử lý khi loading thất bại
        /// </summary>
        /// <param name="error">Thông tin lỗi</param>
        public void OnLoadingFailed(string error)
        {
            isLoading = false;
            Debug.LogError($"[LazyStateMachine] Loading failed for state machine {ID}: {error}");
        }
    }
}
