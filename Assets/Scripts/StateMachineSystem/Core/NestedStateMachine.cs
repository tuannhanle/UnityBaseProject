using System;
using System.Collections.Generic;
using UnityEngine;
using StateMachineSystem.Interfaces;

namespace StateMachineSystem.Core
{
    public class NestedStateMachine : StateMachine
    {
        [Header("Nested State Machine Configuration")]
        [SerializeField] private bool exitOnParentStateChange = true;
        [SerializeField] private bool resumeLastState = false;
        [SerializeField] private string lastActiveStateID = "";

        private Dictionary<string, string> parentStateMapping = new Dictionary<string, string>();

        protected override void Awake()
        {
            base.Awake();
            stateMachineName = $"Nested_{stateMachineName}";
        }

        public virtual void SetParentStateMapping(string parentStateID, string childInitialStateID)
        {
            parentStateMapping[parentStateID] = childInitialStateID;
        }

        public virtual void StartForParentState(string parentStateID)
        {
            string initialState = "";

            // Check if we have a specific mapping for this parent state
            if (parentStateMapping.ContainsKey(parentStateID))
            {
                initialState = parentStateMapping[parentStateID];
            }
            // Check if we should resume last state
            else if (resumeLastState && !string.IsNullOrEmpty(lastActiveStateID) && HasState(lastActiveStateID))
            {
                initialState = lastActiveStateID;
            }

            Start(initialState);
        }

        public override void Stop()
        {
            // Remember last active state for resume
            if (resumeLastState && currentState != null)
            {
                lastActiveStateID = currentState.StateID;
            }

            base.Stop();
        }

        public override void TransitionTo(string stateID)
        {
            base.TransitionTo(stateID);

            // Update last active state
            if (resumeLastState && currentState != null)
            {
                lastActiveStateID = currentState.StateID;
            }
        }

        protected override void HandleChildStateMachine(string stateID)
        {
            // Nested state machines can have their own child state machines
            base.HandleChildStateMachine(stateID);
        }

        // Methods to communicate with parent
        protected virtual void NotifyParentOfCompletion(string completionData = null)
        {
            if (parentStateMachine != null)
            {
                // Send completion signal to parent
                Debug.Log($"[NestedStateMachine] Notifying parent of completion: {completionData}");
                
                // You can implement custom communication protocols here
                // For example, trigger a transition in parent state machine
            }
        }

        protected virtual void RequestParentTransition(string targetStateID)
        {
            parentStateMachine?.TransitionTo(targetStateID);
        }

        public virtual void OnParentStateChanged(IState newParentState)
        {
            if (exitOnParentStateChange && isRunning)
            {
                Stop();
            }

            Debug.Log($"[NestedStateMachine] Parent state changed to: {newParentState?.StateID}");
        }

        public virtual void SetExitOnParentStateChange(bool exit)
        {
            exitOnParentStateChange = exit;
        }

        public virtual void SetResumeLastState(bool resume)
        {
            resumeLastState = resume;
        }

        public virtual string GetLastActiveStateID()
        {
            return lastActiveStateID;
        }

        public virtual void ClearLastActiveState()
        {
            lastActiveStateID = "";
        }

        public override string ToString()
        {
            return $"NestedStateMachine '{stateMachineName}' - Parent: {parentStateMachine?.StateMachineName ?? "null"}, " +
                   $"Current: {currentState?.StateID ?? "null"}, Last: {lastActiveStateID}";
        }
    }

    // Specialized state that contains a nested state machine
    public class ContainerState : StateBase
    {
        protected IStateMachine nestedStateMachine;
        protected bool startNestedOnEnter = true;
        protected bool stopNestedOnExit = true;

        public IStateMachine NestedStateMachine => nestedStateMachine;

        public ContainerState(string stateID, IStateMachine nestedStateMachine, string stateName = null) 
            : base(stateID, stateName)
        {
            this.nestedStateMachine = nestedStateMachine;
            
            if (nestedStateMachine != null)
            {
                nestedStateMachine.ParentStateMachine = parentStateMachine;
            }
        }

        protected override void OnEnterInternal()
        {
            if (startNestedOnEnter && nestedStateMachine != null && !nestedStateMachine.IsRunning)
            {
                if (nestedStateMachine is NestedStateMachine nested)
                {
                    nested.StartForParentState(stateID);
                }
                else
                {
                    nestedStateMachine.Start();
                }
            }
        }

        protected override void OnExitInternal()
        {
            if (stopNestedOnExit && nestedStateMachine != null && nestedStateMachine.IsRunning)
            {
                nestedStateMachine.Stop();
            }
        }

        protected override void OnUpdateInternal()
        {
            // Nested state machine will update itself through parent's Update()
            // Additional logic can be added here if needed
        }

        public virtual void SetStartNestedOnEnter(bool start)
        {
            startNestedOnEnter = start;
        }

        public virtual void SetStopNestedOnExit(bool stop)
        {
            stopNestedOnExit = stop;
        }

        public virtual void SetNestedStateMachine(IStateMachine nested)
        {
            // Stop old nested state machine
            if (nestedStateMachine != null && nestedStateMachine.IsRunning)
            {
                nestedStateMachine.Stop();
            }

            nestedStateMachine = nested;
            
            if (nestedStateMachine != null)
            {
                nestedStateMachine.ParentStateMachine = parentStateMachine;
            }
        }

        public override string ToString()
        {
            return $"ContainerState[{stateID}] '{stateName}' - Nested: {nestedStateMachine?.StateMachineName ?? "null"}";
        }
    }

    // Utility class for managing nested state machine hierarchies
    public static class NestedStateMachineUtility
    {
        public static void ConnectNestedStateMachine(IStateMachine parent, string parentStateID, IStateMachine child)
        {
            if (parent == null || child == null) return;

            parent.AddChildStateMachine(parentStateID, child);
            child.ParentStateMachine = parent;

            Debug.Log($"Connected nested state machine: {child.StateMachineName} to parent state: {parentStateID}");
        }

        public static void CreateContainerState(IStateMachine parent, string stateID, IStateMachine nested, string stateName = null)
        {
            var containerState = new ContainerState(stateID, nested, stateName ?? stateID);
            parent.AddState(containerState);
            
            Debug.Log($"Created container state: {stateID} with nested state machine: {nested.StateMachineName}");
        }

        public static List<IStateMachine> GetAllNestedStateMachines(IStateMachine root)
        {
            var result = new List<IStateMachine>();
            CollectNestedStateMachines(root, result);
            return result;
        }

        private static void CollectNestedStateMachines(IStateMachine stateMachine, List<IStateMachine> result)
        {
            if (stateMachine == null || result.Contains(stateMachine)) return;

            result.Add(stateMachine);

            // Collect child state machines (this would require extending IStateMachine interface)
            if (stateMachine is StateMachine sm)
            {
                var childStateIDs = sm.GetChildStateMachineStateIDs();
                foreach (var stateID in childStateIDs)
                {
                    var childSM = sm.GetChildStateMachine(stateID);
                    CollectNestedStateMachines(childSM, result);
                }
            }
        }

        public static void StopAllNestedStateMachines(IStateMachine root)
        {
            var allNested = GetAllNestedStateMachines(root);
            
            // Stop in reverse order (deepest first)
            for (int i = allNested.Count - 1; i >= 0; i--)
            {
                if (allNested[i].IsRunning)
                {
                    allNested[i].Stop();
                }
            }
        }

        public static IStateMachine FindNestedStateMachine(IStateMachine root, string stateMachineID)
        {
            if (root == null) return null;
            if (root.StateMachineID == stateMachineID) return root;

            var allNested = GetAllNestedStateMachines(root);
            return allNested.FirstOrDefault(sm => sm.StateMachineID == stateMachineID);
        }
    }
}
