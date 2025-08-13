using System;
using System.Collections.Generic;
using UnityEngine;
using StateMachineSystem.Interfaces;

namespace StateMachineSystem.Models
{
    [CreateAssetMenu(fileName = "ResourceLoaderModel", menuName = "StateMachine/Models/Resource Loader Model")]
    public class ResourceLoaderModel : ScriptableObject, IResourceLoader
    {
        [Header("Resource Loading Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool unloadOnDestroy = true;
        [SerializeField] private int maxCacheSize = 100;

        [Header("Resource Status")]
        [SerializeField] private List<ResourceEntry> loadedResources = new List<ResourceEntry>();
        [SerializeField] private List<string> loadingQueue = new List<string>();

        private Dictionary<string, object> resourceCache = new Dictionary<string, object>();
        private Dictionary<string, float> loadingProgress = new Dictionary<string, float>();

        public event Action<string, object> OnResourceLoaded;
        public event Action<string> OnResourceUnloaded;
        public event Action<string, float> OnResourceLoadProgress;

        [Serializable]
        public class ResourceEntry
        {
            public string resourceID;
            public string resourcePath;
            public string resourceType;
            public bool isLoaded;
            public float loadTime;
        }

        private void OnEnable()
        {
            RebuildResourceCache();
        }

        private void OnDestroy()
        {
            if (unloadOnDestroy)
            {
                UnloadAllResources();
            }
        }

        private void RebuildResourceCache()
        {
            resourceCache.Clear();
            // Note: Actual resources would need to be reloaded in a real implementation
            // This is just for tracking purposes
        }

        public void LoadResource(string resourceID, string resourcePath)
        {
            if (IsResourceLoaded(resourceID))
            {
                LogOperation($"Resource '{resourceID}' already loaded");
                return;
            }

            if (loadingQueue.Contains(resourceID))
            {
                LogOperation($"Resource '{resourceID}' already in loading queue");
                return;
            }

            loadingQueue.Add(resourceID);
            LogOperation($"Loading resource: {resourceID} from {resourcePath}");

            // Simulate async loading
            StartCoroutineSimulation(resourceID, resourcePath);
        }

        private void StartCoroutineSimulation(string resourceID, string resourcePath)
        {
            // In a real implementation, this would be actual async loading
            // For now, we'll simulate completion
            
            float startTime = Time.time;
            
            // Simulate loading progress
            for (float progress = 0f; progress <= 1f; progress += 0.1f)
            {
                loadingProgress[resourceID] = progress;
                OnResourceLoadProgress?.Invoke(resourceID, progress);
            }

            // Simulate resource loading (would be actual Unity Resources.Load, Addressables, etc.)
            object loadedResource = SimulateResourceLoad(resourcePath);
            
            if (loadedResource != null)
            {
                resourceCache[resourceID] = loadedResource;
                
                var entry = new ResourceEntry
                {
                    resourceID = resourceID,
                    resourcePath = resourcePath,
                    resourceType = loadedResource.GetType().Name,
                    isLoaded = true,
                    loadTime = Time.time - startTime
                };
                
                loadedResources.Add(entry);
                loadingQueue.Remove(resourceID);
                loadingProgress.Remove(resourceID);

                OnResourceLoaded?.Invoke(resourceID, loadedResource);
                LogOperation($"Resource loaded successfully: {resourceID} ({entry.resourceType})");
            }
            else
            {
                loadingQueue.Remove(resourceID);
                loadingProgress.Remove(resourceID);
                LogOperation($"Failed to load resource: {resourceID}");
            }
        }

        private object SimulateResourceLoad(string resourcePath)
        {
            // Simulate different resource types based on path
            if (resourcePath.Contains(".prefab"))
                return new GameObject("SimulatedPrefab");
            else if (resourcePath.Contains(".txt"))
                return $"Text content from {resourcePath}";
            else if (resourcePath.Contains(".asset"))
                return ScriptableObject.CreateInstance<ScriptableObject>();
            else
                return $"Generic resource: {resourcePath}";
        }

        public void UnloadResource(string resourceID)
        {
            if (!IsResourceLoaded(resourceID))
            {
                LogOperation($"Resource '{resourceID}' is not loaded");
                return;
            }

            resourceCache.Remove(resourceID);
            loadedResources.RemoveAll(r => r.resourceID == resourceID);

            OnResourceUnloaded?.Invoke(resourceID);
            LogOperation($"Resource unloaded: {resourceID}");
        }

        public T GetResource<T>(string resourceID) where T : class
        {
            if (resourceCache.ContainsKey(resourceID))
            {
                try
                {
                    return resourceCache[resourceID] as T;
                }
                catch (Exception e)
                {
                    LogOperation($"Failed to cast resource '{resourceID}' to type {typeof(T)}: {e.Message}");
                }
            }
            return null;
        }

        public bool IsResourceLoaded(string resourceID)
        {
            return resourceCache.ContainsKey(resourceID);
        }

        public void LoadResourcesAsync(Dictionary<string, string> resources, Action onComplete = null)
        {
            int totalResources = resources.Count;
            int loadedCount = 0;

            foreach (var kvp in resources)
            {
                string resourceID = kvp.Key;
                string resourcePath = kvp.Value;

                if (IsResourceLoaded(resourceID))
                {
                    loadedCount++;
                    if (loadedCount >= totalResources)
                    {
                        onComplete?.Invoke();
                    }
                    continue;
                }

                // Subscribe to completion for this resource
                Action<string, object> onResourceLoaded = null;
                onResourceLoaded = (id, resource) =>
                {
                    if (id == resourceID)
                    {
                        OnResourceLoaded -= onResourceLoaded;
                        loadedCount++;
                        
                        if (loadedCount >= totalResources)
                        {
                            onComplete?.Invoke();
                            LogOperation($"Batch loading completed: {totalResources} resources");
                        }
                    }
                };

                OnResourceLoaded += onResourceLoaded;
                LoadResource(resourceID, resourcePath);
            }

            LogOperation($"Started batch loading: {totalResources} resources");
        }

        public void UnloadAllResources()
        {
            var resourceIDs = new List<string>(resourceCache.Keys);
            
            foreach (string resourceID in resourceIDs)
            {
                UnloadResource(resourceID);
            }

            resourceCache.Clear();
            loadedResources.Clear();
            loadingQueue.Clear();
            loadingProgress.Clear();

            LogOperation("All resources unloaded");
        }

        private void LogOperation(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[ResourceLoader] {message}");
            }
        }

        // Context menu methods for testing
        [ContextMenu("Test Load Resource")]
        private void TestLoadResource()
        {
            LoadResource("TestResource", "Assets/TestResource.prefab");
        }

        [ContextMenu("Test Unload All")]
        private void TestUnloadAll()
        {
            UnloadAllResources();
        }

        [ContextMenu("Log Resource Status")]
        private void TestLogStatus()
        {
            Debug.Log($"Loaded Resources: {resourceCache.Count}, Loading Queue: {loadingQueue.Count}");
            foreach (var entry in loadedResources)
            {
                Debug.Log($"  - {entry.resourceID} ({entry.resourceType}) - {entry.loadTime:F2}s");
            }
        }

        // Utility methods
        public void SetEnableLogging(bool enable)
        {
            enableLogging = enable;
        }

        public void SetUnloadOnDestroy(bool unload)
        {
            unloadOnDestroy = unload;
        }

        public void SetMaxCacheSize(int size)
        {
            maxCacheSize = Mathf.Max(0, size);
            
            // Trim cache if needed
            while (resourceCache.Count > maxCacheSize)
            {
                var firstKey = resourceCache.Keys.First();
                UnloadResource(firstKey);
            }
        }

        public int GetLoadedResourceCount()
        {
            return resourceCache.Count;
        }

        public List<string> GetLoadedResourceIDs()
        {
            return new List<string>(resourceCache.Keys);
        }

        public float GetLoadingProgress(string resourceID)
        {
            return loadingProgress.ContainsKey(resourceID) ? loadingProgress[resourceID] : 0f;
        }

        public bool IsResourceLoading(string resourceID)
        {
            return loadingQueue.Contains(resourceID);
        }
    }
}
