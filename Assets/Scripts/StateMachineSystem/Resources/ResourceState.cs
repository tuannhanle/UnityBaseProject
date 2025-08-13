using System;
using System.Collections.Generic;
using UnityEngine;
using StateMachineSystem.Core;
using StateMachineSystem.Interfaces;

namespace StateMachineSystem.Resources
{
    public class ResourceState : StateBase
    {
        protected Dictionary<string, string> requiredResources = new Dictionary<string, string>();
        protected Dictionary<string, string> optionalResources = new Dictionary<string, string>();
        protected List<string> loadedResources = new List<string>();
        protected bool loadResourcesOnEnter = true;
        protected bool unloadResourcesOnExit = true;
        protected bool waitForResourcesBeforeEnter = true;

        private int totalResourcesToLoad = 0;
        private int loadedResourceCount = 0;
        private bool isLoadingResources = false;
        private Action onResourcesLoadedCallback;

        public bool IsLoadingResources => isLoadingResources;
        public float LoadingProgress => totalResourcesToLoad > 0 ? (float)loadedResourceCount / totalResourcesToLoad : 1f;

        public ResourceState(string stateID, string stateName = null) : base(stateID, stateName)
        {
        }

        public virtual void AddRequiredResource(string resourceID, string resourcePath)
        {
            requiredResources[resourceID] = resourcePath;
        }

        public virtual void AddOptionalResource(string resourceID, string resourcePath)
        {
            optionalResources[resourceID] = resourcePath;
        }

        public virtual void RemoveResource(string resourceID)
        {
            requiredResources.Remove(resourceID);
            optionalResources.Remove(resourceID);
        }

        public virtual void SetLoadResourcesOnEnter(bool load)
        {
            loadResourcesOnEnter = load;
        }

        public virtual void SetUnloadResourcesOnExit(bool unload)
        {
            unloadResourcesOnExit = unload;
        }

        public virtual void SetWaitForResourcesBeforeEnter(bool wait)
        {
            waitForResourcesBeforeEnter = wait;
        }

        protected override void OnEnterInternal()
        {
            if (loadResourcesOnEnter)
            {
                if (waitForResourcesBeforeEnter)
                {
                    LoadResourcesAsync(() => OnResourcesLoaded());
                }
                else
                {
                    LoadResourcesAsync();
                    OnResourcesLoaded();
                }
            }
            else
            {
                OnResourcesLoaded();
            }
        }

        protected override void OnExitInternal()
        {
            if (unloadResourcesOnExit)
            {
                UnloadAllStateResources();
            }

            OnStateExitWithResources();
        }

        protected override void OnUpdateInternal()
        {
            if (!isLoadingResources)
            {
                OnUpdateWithResources();
            }
        }

        protected override void OnLoadResourcesInternal()
        {
            LoadResourcesAsync();
        }

        protected override void OnUnloadResourcesInternal()
        {
            UnloadAllStateResources();
        }

        protected virtual void LoadResourcesAsync(Action onComplete = null)
        {
            if (isLoadingResources)
            {
                Debug.LogWarning($"[ResourceState] Already loading resources for state: {stateID}");
                return;
            }

            onResourcesLoadedCallback = onComplete;
            isLoadingResources = true;
            loadedResourceCount = 0;
            totalResourcesToLoad = requiredResources.Count + optionalResources.Count;

            if (totalResourcesToLoad == 0)
            {
                OnAllResourcesLoaded();
                return;
            }

            // Setup resource loader callbacks
            if (resourceLoader != null)
            {
                resourceLoader.OnResourceLoaded += OnSingleResourceLoaded;
                resourceLoader.OnResourceUnloaded += OnSingleResourceUnloaded;
            }

            // Load required resources
            foreach (var kvp in requiredResources)
            {
                LoadResource(kvp.Key, kvp.Value);
            }

            // Load optional resources
            foreach (var kvp in optionalResources)
            {
                LoadResource(kvp.Key, kvp.Value);
            }

            Debug.Log($"[ResourceState] Started loading {totalResourcesToLoad} resources for state: {stateID}");
        }

        protected virtual void OnSingleResourceLoaded(string resourceID, object resource)
        {
            if (!loadedResources.Contains(resourceID))
            {
                loadedResources.Add(resourceID);
                loadedResourceCount++;

                Debug.Log($"[ResourceState] Loaded resource: {resourceID} ({loadedResourceCount}/{totalResourcesToLoad})");

                // Check if all resources are loaded
                if (loadedResourceCount >= totalResourcesToLoad)
                {
                    OnAllResourcesLoaded();
                }
            }
        }

        protected virtual void OnSingleResourceUnloaded(string resourceID)
        {
            loadedResources.Remove(resourceID);
            Debug.Log($"[ResourceState] Unloaded resource: {resourceID}");
        }

        protected virtual void OnAllResourcesLoaded()
        {
            isLoadingResources = false;

            // Cleanup callbacks
            if (resourceLoader != null)
            {
                resourceLoader.OnResourceLoaded -= OnSingleResourceLoaded;
                resourceLoader.OnResourceUnloaded -= OnSingleResourceUnloaded;
            }

            Debug.Log($"[ResourceState] All resources loaded for state: {stateID}");

            onResourcesLoadedCallback?.Invoke();
            onResourcesLoadedCallback = null;
        }

        protected virtual void UnloadAllStateResources()
        {
            foreach (string resourceID in loadedResources.ToArray())
            {
                UnloadResource(resourceID);
            }

            loadedResources.Clear();
            Debug.Log($"[ResourceState] Unloaded all resources for state: {stateID}");
        }

        // Methods để subclass override
        protected virtual void OnResourcesLoaded()
        {
            // Called when resources are loaded and state can proceed
        }

        protected virtual void OnStateExitWithResources()
        {
            // Called when state is exiting after resources have been handled
        }

        protected virtual void OnUpdateWithResources()
        {
            // Called during Update when resources are available
        }

        // Utility methods
        public virtual bool AreAllResourcesLoaded()
        {
            return !isLoadingResources && loadedResourceCount >= totalResourcesToLoad;
        }

        public virtual bool IsResourceLoaded(string resourceID)
        {
            return loadedResources.Contains(resourceID);
        }

        public virtual List<string> GetLoadedResourceIDs()
        {
            return new List<string>(loadedResources);
        }

        public virtual List<string> GetRequiredResourceIDs()
        {
            return new List<string>(requiredResources.Keys);
        }

        public virtual List<string> GetOptionalResourceIDs()
        {
            return new List<string>(optionalResources.Keys);
        }

        public override string ToString()
        {
            return $"ResourceState[{stateID}] '{stateName}' - Resources: {loadedResourceCount}/{totalResourcesToLoad}, Loading: {isLoadingResources}";
        }
    }

    // Specialized state for loading screens
    public class LoadingState : ResourceState
    {
        private string targetStateID;
        private bool autoTransitionWhenLoaded = true;

        public LoadingState(string stateID, string targetStateID = null, string stateName = null) 
            : base(stateID, stateName ?? "Loading")
        {
            this.targetStateID = targetStateID;
            
            // Loading states typically load resources and wait
            waitForResourcesBeforeEnter = true;
            unloadResourcesOnExit = false; // Keep resources for next state
        }

        protected override void OnResourcesLoaded()
        {
            base.OnResourcesLoaded();

            if (autoTransitionWhenLoaded && !string.IsNullOrEmpty(targetStateID))
            {
                RequestTransition(targetStateID);
            }
        }

        public virtual void SetTargetState(string stateID)
        {
            targetStateID = stateID;
        }

        public virtual void SetAutoTransition(bool autoTransition)
        {
            autoTransitionWhenLoaded = autoTransition;
        }

        public virtual string GetTargetStateID()
        {
            return targetStateID;
        }
    }

    // State that preloads resources for other states
    public class PreloadState : ResourceState
    {
        private Dictionary<string, Dictionary<string, string>> stateResourceMappings = new Dictionary<string, Dictionary<string, string>>();

        public PreloadState(string stateID, string stateName = null) 
            : base(stateID, stateName ?? "Preload")
        {
            unloadResourcesOnExit = false; // Keep preloaded resources
        }

        public virtual void AddResourcesForState(string stateID, Dictionary<string, string> resources)
        {
            stateResourceMappings[stateID] = new Dictionary<string, string>(resources);
            
            // Add to our required resources
            foreach (var kvp in resources)
            {
                AddRequiredResource($"{stateID}_{kvp.Key}", kvp.Value);
            }
        }

        public virtual void RemoveResourcesForState(string stateID)
        {
            if (stateResourceMappings.ContainsKey(stateID))
            {
                var resources = stateResourceMappings[stateID];
                foreach (var kvp in resources)
                {
                    RemoveResource($"{stateID}_{kvp.Key}");
                }
                stateResourceMappings.Remove(stateID);
            }
        }

        public virtual List<string> GetPreloadedStates()
        {
            return new List<string>(stateResourceMappings.Keys);
        }

        public virtual bool AreResourcesLoadedForState(string stateID)
        {
            if (!stateResourceMappings.ContainsKey(stateID))
                return false;

            var resources = stateResourceMappings[stateID];
            foreach (var kvp in resources)
            {
                if (!IsResourceLoaded($"{stateID}_{kvp.Key}"))
                    return false;
            }

            return true;
        }
    }
}
