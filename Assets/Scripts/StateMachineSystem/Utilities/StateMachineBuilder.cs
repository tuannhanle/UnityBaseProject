using System;
using System.Collections.Generic;
using UnityEngine;
using StateMachineSystem.Core;
using StateMachineSystem.Interfaces;
using StateMachineSystem.Models;
using StateMachineSystem.Resources;

namespace StateMachineSystem.Utilities
{
    public class StateMachineBuilder
    {
        private StateMachine stateMachine;
        private Dictionary<string, IState> pendingStates = new Dictionary<string, IState>();
        private Dictionary<string, List<TransitionData>> pendingTransitions = new Dictionary<string, List<TransitionData>>();
        private StateMachineModel model;
        private ResourceLoaderModel resourceLoader;

        private struct TransitionData
        {
            public string fromStateID;
            public string toStateID;
            public ITransition transition;
        }

        public StateMachineBuilder(GameObject targetGameObject, string stateMachineName = null)
        {
            stateMachine = targetGameObject.GetComponent<StateMachine>();
            if (stateMachine == null)
            {
                stateMachine = targetGameObject.AddComponent<StateMachine>();
            }

            if (!string.IsNullOrEmpty(stateMachineName))
            {
                stateMachine.StateMachineName = stateMachineName;
            }
        }

        public StateMachineBuilder(StateMachine existingStateMachine)
        {
            stateMachine = existingStateMachine;
        }

        public StateMachineBuilder WithModel(StateMachineModel model)
        {
            this.model = model;
            return this;
        }

        public StateMachineBuilder WithResourceLoader(ResourceLoaderModel resourceLoader)
        {
            this.resourceLoader = resourceLoader;
            return this;
        }

        public StateMachineBuilder AddState(IState state)
        {
            if (state != null)
            {
                pendingStates[state.StateID] = state;
            }
            return this;
        }

        public StateMachineBuilder AddState(string stateID, Action onEnter = null, Action onExit = null, Action onUpdate = null)
        {
            var state = new SimpleState(stateID, onEnter, onExit, onUpdate);
            return AddState(state);
        }

        public StateMachineBuilder AddResourceState(string stateID, Dictionary<string, string> resources, bool waitForLoad = true)
        {
            var resourceState = new ResourceState(stateID);
            
            foreach (var kvp in resources)
            {
                resourceState.AddRequiredResource(kvp.Key, kvp.Value);
            }
            
            resourceState.SetWaitForResourcesBeforeEnter(waitForLoad);
            
            return AddState(resourceState);
        }

        public StateMachineBuilder AddLoadingState(string stateID, string targetStateID, Dictionary<string, string> resources = null)
        {
            var loadingState = new LoadingState(stateID, targetStateID);
            
            if (resources != null)
            {
                foreach (var kvp in resources)
                {
                    loadingState.AddRequiredResource(kvp.Key, kvp.Value);
                }
            }
            
            return AddState(loadingState);
        }

        public StateMachineBuilder AddTransition(string fromStateID, string toStateID, ITransition transition = null)
        {
            if (!pendingTransitions.ContainsKey(fromStateID))
            {
                pendingTransitions[fromStateID] = new List<TransitionData>();
            }

            pendingTransitions[fromStateID].Add(new TransitionData
            {
                fromStateID = fromStateID,
                toStateID = toStateID,
                transition = transition ?? new ImmediateTransition(fromStateID, toStateID)
            });

            return this;
        }

        public StateMachineBuilder AddConditionalTransition(string fromStateID, string toStateID, Func<bool> condition)
        {
            var transition = new ConditionalTransition(fromStateID, toStateID, condition);
            return AddTransition(fromStateID, toStateID, transition);
        }

        public StateMachineBuilder AddDelayedTransition(string fromStateID, string toStateID, float delay)
        {
            var transition = new DelayedTransition(fromStateID, toStateID, delay);
            return AddTransition(fromStateID, toStateID, transition);
        }

        public StateMachineBuilder AddTimerTransition(string fromStateID, string toStateID, float duration)
        {
            var transition = new TimerTransition(fromStateID, toStateID, duration);
            return AddTransition(fromStateID, toStateID, transition);
        }

        public StateMachineBuilder AddNestedStateMachine(string parentStateID, IStateMachine nestedStateMachine)
        {
            // This will be applied after Build() is called
            stateMachine.AddChildStateMachine(parentStateID, nestedStateMachine);
            return this;
        }

        public StateMachineBuilder SetInitialState(string stateID)
        {
            // Store initial state for later use
            return this;
        }

        public StateMachine Build()
        {
            // Apply model and resource loader
            if (model != null)
            {
                stateMachine.SetModel(model);
            }

            if (resourceLoader != null)
            {
                stateMachine.SetResourceLoader(resourceLoader);
            }

            // Add all states
            foreach (var state in pendingStates.Values)
            {
                stateMachine.AddState(state);
            }

            // Add all transitions
            foreach (var kvp in pendingTransitions)
            {
                string fromStateID = kvp.Key;
                var state = stateMachine.GetState(fromStateID);
                
                if (state != null)
                {
                    foreach (var transitionData in kvp.Value)
                    {
                        state.AddTransition(transitionData.toStateID, transitionData.transition);
                    }
                }
            }

            Debug.Log($"[StateMachineBuilder] Built state machine with {pendingStates.Count} states and {pendingTransitions.Count} state transition groups");

            return stateMachine;
        }

        // Helper methods for common patterns
        public static StateMachine CreateLinearStateMachine(GameObject target, params string[] stateIDs)
        {
            var builder = new StateMachineBuilder(target);
            
            for (int i = 0; i < stateIDs.Length; i++)
            {
                builder.AddState(stateIDs[i]);
                
                if (i < stateIDs.Length - 1)
                {
                    builder.AddTransition(stateIDs[i], stateIDs[i + 1]);
                }
            }

            return builder.Build();
        }

        public static StateMachine CreateMenuStateMachine(GameObject target, string menuStateID, params string[] subStateIDs)
        {
            var builder = new StateMachineBuilder(target);
            
            // Add menu state
            builder.AddState(menuStateID);
            
            // Add sub states with transitions back to menu
            foreach (var subStateID in subStateIDs)
            {
                builder.AddState(subStateID);
                builder.AddTransition(menuStateID, subStateID);
                builder.AddTransition(subStateID, menuStateID);
            }

            return builder.Build();
        }

        public static StateMachine CreateLoadingStateMachine(GameObject target, Dictionary<string, Dictionary<string, string>> stateResources)
        {
            var builder = new StateMachineBuilder(target);
            
            foreach (var kvp in stateResources)
            {
                string stateID = kvp.Key;
                var resources = kvp.Value;
                
                builder.AddResourceState(stateID, resources, true);
            }

            return builder.Build();
        }
    }

    // Simple state implementation for basic use cases
    public class SimpleState : StateBase
    {
        private readonly Action onEnterCallback;
        private readonly Action onExitCallback;
        private readonly Action onUpdateCallback;

        public SimpleState(string stateID, Action onEnter = null, Action onExit = null, Action onUpdate = null, string stateName = null) 
            : base(stateID, stateName)
        {
            onEnterCallback = onEnter;
            onExitCallback = onExit;
            onUpdateCallback = onUpdate;
        }

        protected override void OnEnterInternal()
        {
            onEnterCallback?.Invoke();
        }

        protected override void OnExitInternal()
        {
            onExitCallback?.Invoke();
        }

        protected override void OnUpdateInternal()
        {
            onUpdateCallback?.Invoke();
        }
    }
}
