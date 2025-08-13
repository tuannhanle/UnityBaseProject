using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StateMachineSystem.Interfaces;
using StateMachineSystem.Models;

namespace StateMachineSystem.Core
{
    /// <summary>
    /// Core State Machine implementation với MVP pattern support
    /// Quản lý states, transitions, và nested state machines
    /// </summary>
    public class StateMachine : MonoBehaviour, IStateMachine
    {
        [Header("State Machine Configuration")]
        [SerializeField] protected StateMachineModel stateMachineModel;
        [SerializeField] protected ResourceLoaderModel resourceLoaderModel;
        [SerializeField] protected string stateMachineName = "StateMachine";
        [SerializeField] protected string stateMachineID = "";
        [SerializeField] protected bool autoStartOnAwake = false;
        [SerializeField] protected string initialStateID = "";

        protected Dictionary<string, IState> states = new Dictionary<string, IState>();
        protected Dictionary<string, IStateMachine> childStateMachines = new Dictionary<string, IStateMachine>();
        protected IState currentState;
        protected IStateMachine parentStateMachine;
        protected bool isRunning = false;

        public string StateMachineName => stateMachineName;
        public string StateMachineID => stateMachineID;
        public IState CurrentState => currentState;
        public IStateMachine ParentStateMachine 
        { 
            get => parentStateMachine; 
            set => parentStateMachine = value; 
        }
        public bool IsRunning => isRunning;
        public StateMachineModel Model => stateMachineModel;
        public ResourceLoaderModel ResourceLoader => resourceLoaderModel;

        public event Action<IStateMachine, IState, IState> OnStateChanged;
        public event Action<IStateMachine> OnStateMachineStarted;
        public event Action<IStateMachine> OnStateMachineStopped;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(stateMachineID))
                stateMachineID = GetInstanceID().ToString();

            SetupModel();
        }

        protected virtual void Start()
        {
            if (autoStartOnAwake)
            {
                Start(initialStateID);
            }
        }

        protected virtual void Update()
        {
            if (!isRunning) return;

            // Update current state
            currentState?.Update();

            // Update child state machines
            foreach (var childSM in childStateMachines.Values)
            {
                if (childSM.IsRunning)
                {
                    childSM.Update();
                }
            }
        }

        protected virtual void SetupModel()
        {
            if (stateMachineModel != null)
            {
                stateMachineModel.OnStateChangeRequested += OnModelStateChangeRequested;
                stateMachineModel.OnStartRequested += OnModelStartRequested;
                stateMachineModel.OnStopRequested += OnModelStopRequested;
                stateMachineModel.SetStateMachineName(stateMachineName);
            }
        }

        protected virtual void OnModelStateChangeRequested(string fromStateID, string toStateID)
        {
            TransitionTo(toStateID);
        }

        protected virtual void OnModelStartRequested(string initialState)
        {
            Start(string.IsNullOrEmpty(initialState) ? initialStateID : initialState);
        }

        protected virtual void OnModelStopRequested()
        {
            Stop();
        }

        /// <summary>
        /// Thêm một state vào state machine
        /// </summary>
        /// <param name="state">State cần thêm</param>
        public virtual void AddState(IState state)
        {
            if (state == null) return;

            states[state.StateID] = state;
            state.ParentStateMachine = this;

            // Setup resource loader for state
            if (state is StateBase stateBase && resourceLoaderModel != null)
            {
                stateBase.SetResourceLoader(resourceLoaderModel);
            }

            Debug.Log($"[StateMachine] Added state: {state.StateID}");
        }

        /// <summary>
        /// Xóa một state khỏi state machine
        /// </summary>
        /// <param name="stateID">ID của state cần xóa</param>
        public virtual void RemoveState(string stateID)
        {
            if (states.ContainsKey(stateID))
            {
                var state = states[stateID];
                
                // Exit state if it's current
                if (currentState == state)
                {
                    state.Exit();
                    currentState = null;
                }

                state.ParentStateMachine = null;
                states.Remove(stateID);
                
                Debug.Log($"[StateMachine] Removed state: {stateID}");
            }
        }

        /// <summary>
        /// Lấy state theo ID
        /// </summary>
        /// <param name="stateID">ID của state</param>
        /// <returns>State instance hoặc null nếu không tìm thấy</returns>
        public virtual IState GetState(string stateID)
        {
            return states.ContainsKey(stateID) ? states[stateID] : null;
        }

        /// <summary>
        /// Kiểm tra xem state machine có chứa state với ID này không
        /// </summary>
        /// <param name="stateID">ID của state</param>
        /// <returns>True nếu state tồn tại</returns>
        public virtual bool HasState(string stateID)
        {
            return states.ContainsKey(stateID);
        }

        /// <summary>
        /// Bắt đầu chạy state machine với state ban đầu
        /// </summary>
        /// <param name="initialStateID">ID của state ban đầu (optional)</param>
        public virtual void Start(string initialStateID = null)
        {
            if (isRunning)
            {
                Debug.LogWarning($"[StateMachine] '{stateMachineName}' is already running");
                return;
            }

            string startStateID = initialStateID ?? this.initialStateID;
            
            if (string.IsNullOrEmpty(startStateID) && states.Count > 0)
            {
                startStateID = states.Keys.First();
            }

            if (string.IsNullOrEmpty(startStateID))
            {
                Debug.LogError($"[StateMachine] Cannot start '{stateMachineName}' - no initial state specified and no states available");
                return;
            }

            isRunning = true;
            stateMachineModel?.SetRunningState(true);
            
            TransitionTo(startStateID);
            
            OnStateMachineStarted?.Invoke(this);
            Debug.Log($"[StateMachine] Started: {stateMachineName} with initial state: {startStateID}");
        }

        /// <summary>
        /// Dừng state machine và exit tất cả states
        /// </summary>
        public virtual void Stop()
        {
            if (!isRunning)
            {
                Debug.LogWarning($"[StateMachine] '{stateMachineName}' is not running");
                return;
            }

            // Stop all child state machines
            foreach (var childSM in childStateMachines.Values)
            {
                if (childSM.IsRunning)
                {
                    childSM.Stop();
                }
            }

            // Exit current state
            currentState?.Exit();
            currentState = null;

            isRunning = false;
            stateMachineModel?.SetRunningState(false);

            OnStateMachineStopped?.Invoke(this);
            Debug.Log($"[StateMachine] Stopped: {stateMachineName}");
        }

        /// <summary>
        /// Kiểm tra xem có thể transition đến state này không
        /// </summary>
        /// <param name="stateID">ID của target state</param>
        /// <returns>True nếu có thể transition</returns>
        public virtual bool CanTransitionTo(string stateID)
        {
            if (!isRunning || !HasState(stateID))
                return false;

            if (currentState == null)
                return true;

            return currentState.CanTransitionTo(stateID);
        }

        /// <summary>
        /// Thực hiện transition đến state mới
        /// </summary>
        /// <param name="stateID">ID của target state</param>
        public virtual void TransitionTo(string stateID)
        {
            if (!isRunning)
            {
                Debug.LogWarning($"[StateMachine] Cannot transition - '{stateMachineName}' is not running");
                return;
            }

            if (!HasState(stateID))
            {
                Debug.LogError($"[StateMachine] State '{stateID}' not found in '{stateMachineName}'");
                return;
            }

            var newState = GetState(stateID);
            var previousState = currentState;

            // Check if transition is allowed
            if (previousState != null && !previousState.CanTransitionTo(stateID))
            {
                Debug.LogWarning($"[StateMachine] Transition from '{previousState.StateID}' to '{stateID}' is not allowed");
                return;
            }

            // Exit previous state
            previousState?.Exit();

            // Update current state
            currentState = newState;
            stateMachineModel?.SetCurrentState(stateID);

            // Enter new state
            currentState.Enter();

            // Handle child state machines
            HandleChildStateMachine(stateID);

            OnStateChanged?.Invoke(this, previousState, currentState);
            Debug.Log($"[StateMachine] Transitioned: {previousState?.StateID ?? "null"} → {currentState.StateID}");
        }

        protected virtual void HandleChildStateMachine(string stateID)
        {
            // Stop all child state machines first
            foreach (var childSM in childStateMachines.Values)
            {
                if (childSM.IsRunning)
                {
                    childSM.Stop();
                }
            }

            // Start child state machine for this state if exists
            if (childStateMachines.ContainsKey(stateID))
            {
                var childSM = childStateMachines[stateID];
                childSM.Start();
            }
        }

        /// <summary>
        /// Thêm nested state machine cho một state cụ thể
        /// </summary>
        /// <param name="stateID">ID của parent state</param>
        /// <param name="childStateMachine">Child state machine</param>
        public virtual void AddChildStateMachine(string stateID, IStateMachine childStateMachine)
        {
            if (childStateMachine == null) return;

            childStateMachines[stateID] = childStateMachine;
            childStateMachine.ParentStateMachine = this;

            Debug.Log($"[StateMachine] Added child state machine for state: {stateID}");
        }

        /// <summary>
        /// Xóa nested state machine của một state
        /// </summary>
        /// <param name="stateID">ID của parent state</param>
        public virtual void RemoveChildStateMachine(string stateID)
        {
            if (childStateMachines.ContainsKey(stateID))
            {
                var childSM = childStateMachines[stateID];
                
                if (childSM.IsRunning)
                {
                    childSM.Stop();
                }

                childSM.ParentStateMachine = null;
                childStateMachines.Remove(stateID);
                
                Debug.Log($"[StateMachine] Removed child state machine for state: {stateID}");
            }
        }

        /// <summary>
        /// Lấy nested state machine của một state
        /// </summary>
        /// <param name="stateID">ID của parent state</param>
        /// <returns>Child state machine hoặc null</returns>
        public virtual IStateMachine GetChildStateMachine(string stateID)
        {
            return childStateMachines.ContainsKey(stateID) ? childStateMachines[stateID] : null;
        }

        protected virtual void OnDestroy()
        {
            if (stateMachineModel != null)
            {
                stateMachineModel.OnStateChangeRequested -= OnModelStateChangeRequested;
                stateMachineModel.OnStartRequested -= OnModelStartRequested;
                stateMachineModel.OnStopRequested -= OnModelStopRequested;
            }

            Stop();
        }

        // Utility methods
        public void SetModel(StateMachineModel model)
        {
            if (stateMachineModel != null)
            {
                stateMachineModel.OnStateChangeRequested -= OnModelStateChangeRequested;
                stateMachineModel.OnStartRequested -= OnModelStartRequested;
                stateMachineModel.OnStopRequested -= OnModelStopRequested;
            }

            stateMachineModel = model;
            SetupModel();
        }

        public void SetResourceLoader(ResourceLoaderModel loader)
        {
            resourceLoaderModel = loader;
            
            // Update all states with new resource loader
            foreach (var state in states.Values)
            {
                if (state is StateBase stateBase)
                {
                    stateBase.SetResourceLoader(loader);
                }
            }
        }

        public List<string> GetStateIDs()
        {
            return new List<string>(states.Keys);
        }

        public List<string> GetChildStateMachineStateIDs()
        {
            return new List<string>(childStateMachines.Keys);
        }

        public override string ToString()
        {
            return $"StateMachine '{stateMachineName}' [{stateMachineID}] - Current: {currentState?.StateID ?? "null"}, Running: {isRunning}";
        }
    }
}
