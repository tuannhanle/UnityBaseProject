using System;
using System.Collections.Generic;
using UnityEngine;
using StateMachineSystem.Interfaces;
using StateMachineSystem.Models;
using StateMachineSystem.Core;

namespace StateMachineSystem.Presenters
{
    /// <summary>
    /// Presenter cho State Machine theo MVP pattern
    /// Quản lý tương tác giữa Model (ScriptableObject) và View (StateMachine)
    /// </summary>
    public class StateMachinePresenter : MonoBehaviour
    {
        [Header("MVP Configuration")]
        [SerializeField] private StateMachineModel model;
        [SerializeField] private ResourceLoaderModel resourceLoader;
        [SerializeField] private StateMachine stateMachineView;
        [SerializeField] private bool autoSetupOnAwake = true;

        [Header("Presenter Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool handleModelEvents = true;
        [SerializeField] private bool syncViewWithModel = true;

        private bool isInitialized = false;

        public StateMachineModel Model => model;
        public ResourceLoaderModel ResourceLoader => resourceLoader;
        public StateMachine StateMachineView => stateMachineView;
        public bool IsInitialized => isInitialized;

        public event Action<StateMachinePresenter> OnPresenterInitialized;
        public event Action<StateMachinePresenter, IState, IState> OnStateChangedInPresenter;
        public event Action<StateMachinePresenter> OnPresenterDestroyed;

        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Khởi tạo Presenter và thiết lập kết nối giữa Model-View
        /// </summary>
        public virtual void Initialize()
        {
            if (isInitialized)
            {
                LogDebug("Presenter already initialized");
                return;
            }

            ValidateComponents();
            SetupModelViewBinding();
            SubscribeToEvents();

            isInitialized = true;
            OnPresenterInitialized?.Invoke(this);
            
            LogDebug("StateMachine Presenter initialized");
        }

        protected virtual void ValidateComponents()
        {
            if (model == null)
            {
                LogWarning("StateMachineModel is not assigned!");
            }

            if (stateMachineView == null)
            {
                stateMachineView = GetComponent<StateMachine>();
                if (stateMachineView == null)
                {
                    LogWarning("StateMachine component not found!");
                }
            }

            if (resourceLoader == null)
            {
                LogWarning("ResourceLoaderModel is not assigned - resource loading will not work");
            }
        }

        protected virtual void SetupModelViewBinding()
        {
            if (stateMachineView != null && model != null)
            {
                stateMachineView.SetModel(model);
            }

            if (stateMachineView != null && resourceLoader != null)
            {
                stateMachineView.SetResourceLoader(resourceLoader);
            }
        }

        protected virtual void SubscribeToEvents()
        {
            if (!handleModelEvents) return;

            // Model events
            if (model != null)
            {
                model.OnStateChangeRequested += OnModelStateChangeRequested;
                model.OnStartRequested += OnModelStartRequested;
                model.OnStopRequested += OnModelStopRequested;
                model.OnStateDataUpdated += OnModelStateDataUpdated;
                model.OnModelUpdated += OnModelUpdated;
            }

            // View events
            if (stateMachineView != null)
            {
                stateMachineView.OnStateChanged += OnViewStateChanged;
                stateMachineView.OnStateMachineStarted += OnViewStateMachineStarted;
                stateMachineView.OnStateMachineStopped += OnViewStateMachineStopped;
            }

            // Resource loader events
            if (resourceLoader != null)
            {
                resourceLoader.OnResourceLoaded += OnResourceLoaded;
                resourceLoader.OnResourceUnloaded += OnResourceUnloaded;
                resourceLoader.OnResourceLoadProgress += OnResourceLoadProgress;
            }
        }

        protected virtual void UnsubscribeFromEvents()
        {
            // Model events
            if (model != null)
            {
                model.OnStateChangeRequested -= OnModelStateChangeRequested;
                model.OnStartRequested -= OnModelStartRequested;
                model.OnStopRequested -= OnModelStopRequested;
                model.OnStateDataUpdated -= OnModelStateDataUpdated;
                model.OnModelUpdated -= OnModelUpdated;
            }

            // View events
            if (stateMachineView != null)
            {
                stateMachineView.OnStateChanged -= OnViewStateChanged;
                stateMachineView.OnStateMachineStarted -= OnViewStateMachineStarted;
                stateMachineView.OnStateMachineStopped -= OnViewStateMachineStopped;
            }

            // Resource loader events
            if (resourceLoader != null)
            {
                resourceLoader.OnResourceLoaded -= OnResourceLoaded;
                resourceLoader.OnResourceUnloaded -= OnResourceUnloaded;
                resourceLoader.OnResourceLoadProgress -= OnResourceLoadProgress;
            }
        }

        // Model event handlers
        protected virtual void OnModelStateChangeRequested(string fromStateID, string toStateID)
        {
            LogDebug($"Model requested state change: {fromStateID} → {toStateID}");
        }

        protected virtual void OnModelStartRequested(string initialStateID)
        {
            LogDebug($"Model requested start with initial state: {initialStateID}");
        }

        protected virtual void OnModelStopRequested()
        {
            LogDebug("Model requested stop");
        }

        protected virtual void OnModelStateDataUpdated(string key, object value)
        {
            LogDebug($"Model state data updated: {key} = {value}");
        }

        protected virtual void OnModelUpdated()
        {
            if (syncViewWithModel)
            {
                SyncViewWithModel();
            }
        }

        // View event handlers
        protected virtual void OnViewStateChanged(IStateMachine stateMachine, IState previousState, IState newState)
        {
            LogDebug($"View state changed: {previousState?.StateID ?? "null"} → {newState?.StateID ?? "null"}");
            OnStateChangedInPresenter?.Invoke(this, previousState, newState);
        }

        protected virtual void OnViewStateMachineStarted(IStateMachine stateMachine)
        {
            LogDebug($"View state machine started: {stateMachine.StateMachineName}");
        }

        protected virtual void OnViewStateMachineStopped(IStateMachine stateMachine)
        {
            LogDebug($"View state machine stopped: {stateMachine.StateMachineName}");
        }

        // Resource loader event handlers
        protected virtual void OnResourceLoaded(string resourceID, object resource)
        {
            LogDebug($"Resource loaded: {resourceID}");
        }

        protected virtual void OnResourceUnloaded(string resourceID)
        {
            LogDebug($"Resource unloaded: {resourceID}");
        }

        protected virtual void OnResourceLoadProgress(string resourceID, float progress)
        {
            LogDebug($"Resource loading progress: {resourceID} - {progress:P1}");
        }

        // Presenter methods (API for external control)
        /// <summary>
        /// Bắt đầu chạy state machine thông qua Model hoặc trực tiếp
        /// </summary>
        /// <param name="initialStateID">ID của state ban đầu (optional)</param>
        public virtual void StartStateMachine(string initialStateID = null)
        {
            if (model != null)
            {
                model.RequestStart(initialStateID);
            }
            else if (stateMachineView != null)
            {
                stateMachineView.Start(initialStateID);
            }
        }

        /// <summary>
        /// Dừng state machine thông qua Model hoặc trực tiếp
        /// </summary>
        public virtual void StopStateMachine()
        {
            if (model != null)
            {
                model.RequestStop();
            }
            else if (stateMachineView != null)
            {
                stateMachineView.Stop();
            }
        }

        /// <summary>
        /// Thực hiện transition đến state mới thông qua Model
        /// </summary>
        /// <param name="stateID">ID của target state</param>
        public virtual void TransitionToState(string stateID)
        {
            if (model != null)
            {
                model.RequestStateChange(model.CurrentStateID, stateID);
            }
            else if (stateMachineView != null)
            {
                stateMachineView.TransitionTo(stateID);
            }
        }

        /// <summary>
        /// Cập nhật state data trong Model
        /// </summary>
        /// <param name="key">Key của data</param>
        /// <param name="value">Giá trị mới</param>
        public virtual void UpdateStateData(string key, object value)
        {
            model?.UpdateStateData(key, value);
        }

        /// <summary>
        /// Lấy state data từ Model
        /// </summary>
        /// <typeparam name="T">Kiểu của data</typeparam>
        /// <param name="key">Key của data</param>
        /// <returns>Giá trị data hoặc default</returns>
        public virtual T GetStateData<T>(string key)
        {
            return model != null ? model.GetStateData<T>(key) : default(T);
        }

        /// <summary>
        /// Load resource thông qua ResourceLoader
        /// </summary>
        /// <param name="resourceID">ID của resource</param>
        /// <param name="resourcePath">Đường dẫn đến resource</param>
        public virtual void LoadResource(string resourceID, string resourcePath)
        {
            resourceLoader?.LoadResource(resourceID, resourcePath);
        }

        /// <summary>
        /// Unload resource thông qua ResourceLoader
        /// </summary>
        /// <param name="resourceID">ID của resource cần unload</param>
        public virtual void UnloadResource(string resourceID)
        {
            resourceLoader?.UnloadResource(resourceID);
        }

        /// <summary>
        /// Lấy resource đã được load với kiểu cụ thể
        /// </summary>
        /// <typeparam name="T">Kiểu của resource</typeparam>
        /// <param name="resourceID">ID của resource</param>
        /// <returns>Resource instance hoặc null</returns>
        public virtual T GetResource<T>(string resourceID) where T : class
        {
            return resourceLoader?.GetResource<T>(resourceID);
        }

        public virtual bool IsResourceLoaded(string resourceID)
        {
            return resourceLoader?.IsResourceLoaded(resourceID) ?? false;
        }

        // Utility methods
        protected virtual void SyncViewWithModel()
        {
            if (model == null || stateMachineView == null) return;

            // Sync state if different
            if (model.CurrentStateID != stateMachineView.CurrentState?.StateID)
            {
                if (!string.IsNullOrEmpty(model.CurrentStateID) && stateMachineView.HasState(model.CurrentStateID))
                {
                    stateMachineView.TransitionTo(model.CurrentStateID);
                }
            }

            // Sync running state
            if (model.IsRunning != stateMachineView.IsRunning)
            {
                if (model.IsRunning)
                {
                    stateMachineView.Start(model.CurrentStateID);
                }
                else
                {
                    stateMachineView.Stop();
                }
            }
        }

        public virtual void SetModel(StateMachineModel newModel)
        {
            if (model != null)
            {
                UnsubscribeFromEvents();
            }

            model = newModel;

            if (isInitialized)
            {
                SetupModelViewBinding();
                SubscribeToEvents();
            }
        }

        public virtual void SetResourceLoader(ResourceLoaderModel newResourceLoader)
        {
            if (resourceLoader != null && isInitialized)
            {
                resourceLoader.OnResourceLoaded -= OnResourceLoaded;
                resourceLoader.OnResourceUnloaded -= OnResourceUnloaded;
                resourceLoader.OnResourceLoadProgress -= OnResourceLoadProgress;
            }

            resourceLoader = newResourceLoader;

            if (isInitialized)
            {
                SetupModelViewBinding();
                
                if (resourceLoader != null)
                {
                    resourceLoader.OnResourceLoaded += OnResourceLoaded;
                    resourceLoader.OnResourceUnloaded += OnResourceUnloaded;
                    resourceLoader.OnResourceLoadProgress += OnResourceLoadProgress;
                }
            }
        }

        public virtual void SetStateMachineView(StateMachine newView)
        {
            if (stateMachineView != null && isInitialized)
            {
                stateMachineView.OnStateChanged -= OnViewStateChanged;
                stateMachineView.OnStateMachineStarted -= OnViewStateMachineStarted;
                stateMachineView.OnStateMachineStopped -= OnViewStateMachineStopped;
            }

            stateMachineView = newView;

            if (isInitialized)
            {
                SetupModelViewBinding();
                
                if (stateMachineView != null)
                {
                    stateMachineView.OnStateChanged += OnViewStateChanged;
                    stateMachineView.OnStateMachineStarted += OnViewStateMachineStarted;
                    stateMachineView.OnStateMachineStopped += OnViewStateMachineStopped;
                }
            }
        }

        // Debug logging
        protected virtual void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[StateMachinePresenter] {message}");
            }
        }

        protected virtual void LogWarning(string message)
        {
            Debug.LogWarning($"[StateMachinePresenter] {message}");
        }

        protected virtual void LogError(string message)
        {
            Debug.LogError($"[StateMachinePresenter] {message}");
        }

        // Settings
        public virtual void SetEnableDebugLogging(bool enable)
        {
            enableDebugLogging = enable;
        }

        public virtual void SetHandleModelEvents(bool handle)
        {
            handleModelEvents = handle;
        }

        public virtual void SetSyncViewWithModel(bool sync)
        {
            syncViewWithModel = sync;
        }

        // Status methods
        public virtual string GetCurrentStateID()
        {
            return stateMachineView?.CurrentState?.StateID ?? model?.CurrentStateID ?? "";
        }

        public virtual bool IsRunning()
        {
            return stateMachineView?.IsRunning ?? model?.IsRunning ?? false;
        }

        public virtual List<string> GetAvailableStates()
        {
            return stateMachineView?.GetStateIDs() ?? new List<string>();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            OnPresenterDestroyed?.Invoke(this);
            isInitialized = false;
        }

        public override string ToString()
        {
            return $"StateMachinePresenter - Model: {model?.name ?? "null"}, " +
                   $"View: {stateMachineView?.StateMachineName ?? "null"}, " +
                   $"Current State: {GetCurrentStateID()}, " +
                   $"Running: {IsRunning()}";
        }
    }
}
