using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StateMachineSystem.Interfaces;
using StateMachineSystem.Core;
using StateMachineSystem.Presenters;

namespace StateMachineSystem
{
    public class StateMachineSystemManager : MonoBehaviour
    {
        [Header("System Configuration")]
        [SerializeField] private bool autoRegisterStateMachines = true;
        [SerializeField] private bool enableGlobalLogging = true;
        [SerializeField] private bool pauseAllOnApplicationPause = true;

        [Header("Registered State Machines")]
        [SerializeField] private List<StateMachinePresenter> registeredPresenters = new List<StateMachinePresenter>();

        private Dictionary<string, StateMachinePresenter> presentersByID = new Dictionary<string, StateMachinePresenter>();
        private Dictionary<string, IStateMachine> stateMachinesByID = new Dictionary<string, IStateMachine>();
        private bool isSystemInitialized = false;

        public static StateMachineSystemManager Instance { get; private set; }

        public bool IsSystemInitialized => isSystemInitialized;
        public int RegisteredStateMachineCount => stateMachinesByID.Count;

        public event Action<StateMachineSystemManager> OnSystemInitialized;
        public event Action<IStateMachine> OnStateMachineRegistered;
        public event Action<IStateMachine> OnStateMachineUnregistered;
        public event Action<bool> OnSystemPausedChanged;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSystem();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (autoRegisterStateMachines)
            {
                AutoRegisterStateMachines();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseAllOnApplicationPause)
            {
                SetSystemPaused(pauseStatus);
            }
        }

        private void InitializeSystem()
        {
            LogDebug("Initializing State Machine System Manager");
            
            isSystemInitialized = true;
            OnSystemInitialized?.Invoke(this);
            
            LogDebug("State Machine System Manager initialized");
        }

        private void AutoRegisterStateMachines()
        {
            // Find all StateMachinePresenter components in the scene
            var presenters = FindObjectsOfType<StateMachinePresenter>();
            foreach (var presenter in presenters)
            {
                RegisterStateMachinePresenter(presenter);
            }

            // Find all standalone StateMachine components
            var stateMachines = FindObjectsOfType<StateMachine>();
            foreach (var sm in stateMachines)
            {
                // Only register if not already managed by a presenter
                if (!IsStateMachineRegistered(sm.StateMachineID))
                {
                    RegisterStateMachine(sm);
                }
            }

            LogDebug($"Auto-registered {registeredPresenters.Count} presenters and {stateMachinesByID.Count} state machines");
        }

        public virtual void RegisterStateMachinePresenter(StateMachinePresenter presenter)
        {
            if (presenter == null)
            {
                LogWarning("Cannot register null presenter");
                return;
            }

            string presenterID = GetPresenterID(presenter);
            
            if (presentersByID.ContainsKey(presenterID))
            {
                LogWarning($"Presenter already registered: {presenterID}");
                return;
            }

            presentersByID[presenterID] = presenter;
            
            if (!registeredPresenters.Contains(presenter))
            {
                registeredPresenters.Add(presenter);
            }

            // Register the associated state machine
            if (presenter.StateMachineView != null)
            {
                RegisterStateMachine(presenter.StateMachineView);
            }

            // Subscribe to presenter events
            presenter.OnPresenterDestroyed += OnPresenterDestroyed;

            LogDebug($"Registered state machine presenter: {presenterID}");
        }

        public virtual void RegisterStateMachine(IStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                LogWarning("Cannot register null state machine");
                return;
            }

            if (stateMachinesByID.ContainsKey(stateMachine.StateMachineID))
            {
                LogWarning($"State machine already registered: {stateMachine.StateMachineID}");
                return;
            }

            stateMachinesByID[stateMachine.StateMachineID] = stateMachine;
            OnStateMachineRegistered?.Invoke(stateMachine);

            LogDebug($"Registered state machine: {stateMachine.StateMachineID} ({stateMachine.StateMachineName})");
        }

        public virtual void UnregisterStateMachinePresenter(StateMachinePresenter presenter)
        {
            if (presenter == null) return;

            string presenterID = GetPresenterID(presenter);
            
            if (presentersByID.ContainsKey(presenterID))
            {
                presentersByID.Remove(presenterID);
                presenter.OnPresenterDestroyed -= OnPresenterDestroyed;
            }

            registeredPresenters.Remove(presenter);

            // Unregister associated state machine
            if (presenter.StateMachineView != null)
            {
                UnregisterStateMachine(presenter.StateMachineView);
            }

            LogDebug($"Unregistered state machine presenter: {presenterID}");
        }

        public virtual void UnregisterStateMachine(IStateMachine stateMachine)
        {
            if (stateMachine == null) return;

            if (stateMachinesByID.ContainsKey(stateMachine.StateMachineID))
            {
                stateMachinesByID.Remove(stateMachine.StateMachineID);
                OnStateMachineUnregistered?.Invoke(stateMachine);
                
                LogDebug($"Unregistered state machine: {stateMachine.StateMachineID}");
            }
        }

        private void OnPresenterDestroyed(StateMachinePresenter presenter)
        {
            UnregisterStateMachinePresenter(presenter);
        }

        // Query methods
        public virtual StateMachinePresenter GetPresenter(string presenterID)
        {
            return presentersByID.ContainsKey(presenterID) ? presentersByID[presenterID] : null;
        }

        public virtual IStateMachine GetStateMachine(string stateMachineID)
        {
            return stateMachinesByID.ContainsKey(stateMachineID) ? stateMachinesByID[stateMachineID] : null;
        }

        public virtual List<StateMachinePresenter> GetAllPresenters()
        {
            return new List<StateMachinePresenter>(registeredPresenters);
        }

        public virtual List<IStateMachine> GetAllStateMachines()
        {
            return new List<IStateMachine>(stateMachinesByID.Values);
        }

        public virtual List<IStateMachine> GetRunningStateMachines()
        {
            return stateMachinesByID.Values.Where(sm => sm.IsRunning).ToList();
        }

        public virtual List<IStateMachine> GetStoppedStateMachines()
        {
            return stateMachinesByID.Values.Where(sm => !sm.IsRunning).ToList();
        }

        public virtual bool IsStateMachineRegistered(string stateMachineID)
        {
            return stateMachinesByID.ContainsKey(stateMachineID);
        }

        public virtual bool IsPresenterRegistered(string presenterID)
        {
            return presentersByID.ContainsKey(presenterID);
        }

        // Control methods
        public virtual void StartAllStateMachines()
        {
            foreach (var sm in stateMachinesByID.Values)
            {
                if (!sm.IsRunning)
                {
                    sm.Start();
                }
            }
            
            LogDebug("Started all state machines");
        }

        public virtual void StopAllStateMachines()
        {
            foreach (var sm in stateMachinesByID.Values)
            {
                if (sm.IsRunning)
                {
                    sm.Stop();
                }
            }
            
            LogDebug("Stopped all state machines");
        }

        public virtual void StartStateMachine(string stateMachineID, string initialStateID = null)
        {
            var sm = GetStateMachine(stateMachineID);
            if (sm != null && !sm.IsRunning)
            {
                sm.Start(initialStateID);
                LogDebug($"Started state machine: {stateMachineID}");
            }
        }

        public virtual void StopStateMachine(string stateMachineID)
        {
            var sm = GetStateMachine(stateMachineID);
            if (sm != null && sm.IsRunning)
            {
                sm.Stop();
                LogDebug($"Stopped state machine: {stateMachineID}");
            }
        }

        public virtual void TransitionStateMachine(string stateMachineID, string stateID)
        {
            var sm = GetStateMachine(stateMachineID);
            if (sm != null && sm.IsRunning)
            {
                sm.TransitionTo(stateID);
                LogDebug($"Transitioned state machine {stateMachineID} to state: {stateID}");
            }
        }

        // System control
        public virtual void SetSystemPaused(bool paused)
        {
            // Implementation depends on how you want to handle pausing
            // For now, we'll just stop/start all state machines
            if (paused)
            {
                StopAllStateMachines();
            }
            else
            {
                StartAllStateMachines();
            }

            OnSystemPausedChanged?.Invoke(paused);
            LogDebug($"System paused: {paused}");
        }

        // Utility methods
        private string GetPresenterID(StateMachinePresenter presenter)
        {
            return presenter.GetInstanceID().ToString();
        }

        public virtual void LogSystemStatus()
        {
            LogDebug("=== State Machine System Status ===");
            LogDebug($"Registered Presenters: {registeredPresenters.Count}");
            LogDebug($"Registered State Machines: {stateMachinesByID.Count}");
            LogDebug($"Running State Machines: {GetRunningStateMachines().Count}");
            
            foreach (var sm in stateMachinesByID.Values)
            {
                LogDebug($"  - {sm.StateMachineName} ({sm.StateMachineID}): {(sm.IsRunning ? "Running" : "Stopped")} - Current: {sm.CurrentState?.StateID ?? "null"}");
            }
        }

        // Debug methods
        [ContextMenu("Log System Status")]
        private void ContextLogSystemStatus()
        {
            LogSystemStatus();
        }

        [ContextMenu("Start All State Machines")]
        private void ContextStartAll()
        {
            StartAllStateMachines();
        }

        [ContextMenu("Stop All State Machines")]
        private void ContextStopAll()
        {
            StopAllStateMachines();
        }

        private void LogDebug(string message)
        {
            if (enableGlobalLogging)
            {
                Debug.Log($"[StateMachineSystemManager] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[StateMachineSystemManager] {message}");
        }

        // Settings
        public virtual void SetEnableGlobalLogging(bool enable)
        {
            enableGlobalLogging = enable;
        }

        public virtual void SetAutoRegisterStateMachines(bool autoRegister)
        {
            autoRegisterStateMachines = autoRegister;
        }

        public virtual void SetPauseAllOnApplicationPause(bool pauseAll)
        {
            pauseAllOnApplicationPause = pauseAll;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
