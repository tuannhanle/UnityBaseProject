using System;
using System.Collections.Generic;
using UnityEngine;
using StateMachineSystem.Interfaces;

namespace StateMachineSystem.Core
{
    /// <summary>
    /// Lớp cơ sở cho tất cả states trong state machine
    /// Implements IState interface và cung cấp functionality cơ bản
    /// </summary>
    public abstract class StateBase : IState
    {
        protected string stateName;
        protected string stateID;
        protected IStateMachine parentStateMachine;
        protected bool isActive = false;
        protected Dictionary<string, ITransition> transitions = new Dictionary<string, ITransition>();
        protected IResourceLoader resourceLoader;

        public string StateName => stateName;
        public string StateID => stateID;
        public IStateMachine ParentStateMachine 
        { 
            get => parentStateMachine; 
            set => parentStateMachine = value; 
        }
        public bool IsActive => isActive;

        public event Action<IState> OnStateEntered;
        public event Action<IState> OnStateExited;
        public event Action<IState> OnStateUpdated;
        public event Action<IState, string> OnResourceLoaded;
        public event Action<IState, string> OnResourceUnloaded;

        protected StateBase(string stateID, string stateName = null)
        {
            this.stateID = stateID;
            this.stateName = stateName ?? stateID;
        }

        /// <summary>
        /// Được gọi khi state được kích hoạt
        /// </summary>
        public virtual void Enter()
        {
            if (isActive) return;

            isActive = true;
            OnEnterInternal();
            OnStateEntered?.Invoke(this);
            
            Debug.Log($"[State] Entered: {stateName} ({stateID})");
        }

        /// <summary>
        /// Được gọi khi state bị thoát
        /// </summary>
        public virtual void Exit()
        {
            if (!isActive) return;

            OnExitInternal();
            isActive = false;
            OnStateExited?.Invoke(this);
            
            Debug.Log($"[State] Exited: {stateName} ({stateID})");
        }

        /// <summary>
        /// Được gọi mỗi frame khi state đang active
        /// </summary>
        public virtual void Update()
        {
            if (!isActive) return;

            OnUpdateInternal();
            OnStateUpdated?.Invoke(this);
        }

        /// <summary>
        /// Load resources cần thiết cho state này
        /// </summary>
        public virtual void LoadResources()
        {
            OnLoadResourcesInternal();
        }

        /// <summary>
        /// Unload resources để giải phóng memory
        /// </summary>
        public virtual void UnloadResources()
        {
            OnUnloadResourcesInternal();
        }

        /// <summary>
        /// Kiểm tra xem có thể transition đến target state không
        /// </summary>
        /// <param name="targetStateID">ID của target state</param>
        /// <returns>True nếu có thể transition</returns>
        public virtual bool CanTransitionTo(string targetStateID)
        {
            if (!transitions.ContainsKey(targetStateID))
                return false;

            return transitions[targetStateID].CanTransition();
        }

        /// <summary>
        /// Thêm transition đến target state
        /// </summary>
        /// <param name="targetStateID">ID của target state</param>
        /// <param name="transition">Transition logic</param>
        public void AddTransition(string targetStateID, ITransition transition)
        {
            transitions[targetStateID] = transition;
        }

        /// <summary>
        /// Xóa transition đến target state
        /// </summary>
        /// <param name="targetStateID">ID của target state</param>
        public void RemoveTransition(string targetStateID)
        {
            transitions.Remove(targetStateID);
        }

        /// <summary>
        /// Kiểm tra xem có transition đến target state không
        /// </summary>
        /// <param name="targetStateID">ID của target state</param>
        /// <returns>True nếu có transition</returns>
        public bool HasTransition(string targetStateID)
        {
            return transitions.ContainsKey(targetStateID);
        }

        protected void SetResourceLoader(IResourceLoader loader)
        {
            resourceLoader = loader;
            
            if (resourceLoader != null)
            {
                resourceLoader.OnResourceLoaded += (id, resource) => OnResourceLoaded?.Invoke(this, id);
                resourceLoader.OnResourceUnloaded += (id) => OnResourceUnloaded?.Invoke(this, id);
            }
        }

        /// <summary>
        /// Load một resource thông qua resource loader
        /// </summary>
        /// <param name="resourceID">ID của resource</param>
        /// <param name="resourcePath">Đường dẫn đến resource</param>
        protected void LoadResource(string resourceID, string resourcePath)
        {
            resourceLoader?.LoadResource(resourceID, resourcePath);
        }

        /// <summary>
        /// Unload một resource thông qua resource loader
        /// </summary>
        /// <param name="resourceID">ID của resource cần unload</param>
        protected void UnloadResource(string resourceID)
        {
            resourceLoader?.UnloadResource(resourceID);
        }

        /// <summary>
        /// Lấy resource đã được load với kiểu cụ thể
        /// </summary>
        /// <typeparam name="T">Kiểu của resource</typeparam>
        /// <param name="resourceID">ID của resource</param>
        /// <returns>Resource instance hoặc null</returns>
        protected T GetResource<T>(string resourceID) where T : class
        {
            return resourceLoader?.GetResource<T>(resourceID);
        }

        /// <summary>
        /// Kiểm tra xem resource đã được load chưa
        /// </summary>
        /// <param name="resourceID">ID của resource</param>
        /// <returns>True nếu resource đã load</returns>
        protected bool IsResourceLoaded(string resourceID)
        {
            return resourceLoader?.IsResourceLoaded(resourceID) ?? false;
        }

        // Abstract methods để subclass implement
        protected abstract void OnEnterInternal();
        protected abstract void OnExitInternal();
        protected abstract void OnUpdateInternal();
        protected virtual void OnLoadResourcesInternal() { }
        protected virtual void OnUnloadResourcesInternal() { }

        // Utility methods
        /// <summary>
        /// Yêu cầu parent state machine thực hiện transition
        /// </summary>
        /// <param name="targetStateID">ID của target state</param>
        protected void RequestTransition(string targetStateID)
        {
            parentStateMachine?.TransitionTo(targetStateID);
        }

        /// <summary>
        /// Lưu data vào state machine model
        /// </summary>
        /// <param name="key">Key của data</param>
        /// <param name="value">Giá trị data</param>
        protected void SetStateData(string key, object value)
        {
            if (parentStateMachine is StateMachine sm && sm.Model != null)
            {
                sm.Model.UpdateStateData(key, value);
            }
        }

        /// <summary>
        /// Lấy data từ state machine model
        /// </summary>
        /// <typeparam name="T">Kiểu của data</typeparam>
        /// <param name="key">Key của data</param>
        /// <returns>Giá trị data hoặc default</returns>
        protected T GetStateData<T>(string key)
        {
            if (parentStateMachine is StateMachine sm && sm.Model != null)
            {
                return sm.Model.GetStateData<T>(key);
            }
            return default(T);
        }

        public override string ToString()
        {
            return $"State[{stateID}] '{stateName}' - Active: {isActive}";
        }
    }
}
