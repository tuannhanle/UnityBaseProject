using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZoneSystem.Interfaces;

namespace ZoneSystem.Core
{
    public abstract class ZoneBase : MonoBehaviour, IZone
    {
        [SerializeField] protected int priority;
        [SerializeField] protected ZoneType zoneType;
        [SerializeField] protected string zoneName;
        [SerializeField] protected GameObject resourceContainer;

        protected Dictionary<Type, IScene> scenes = new Dictionary<Type, IScene>();
        protected ZoneState currentState = ZoneState.Inactive;

        public int Priority => priority;
        public ZoneType Type => zoneType;
        public ZoneState State => currentState;
        public bool IsAlwaysVisible => zoneType.HasFlag(ZoneType.AlwaysVisible);
        public string ZoneName => zoneName;

        public event Action<IZone> OnZoneActivated;
        public event Action<IZone> OnZoneDeactivated;
        public event Action<IZone> OnResourcesLoaded;
        public event Action<IZone> OnResourcesUnloaded;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(zoneName))
                zoneName = gameObject.name;

            if (resourceContainer == null)
                resourceContainer = gameObject;
        }

        public virtual void LoadResources()
        {
            if (currentState == ZoneState.Loading || currentState == ZoneState.Active)
                return;

            currentState = ZoneState.Loading;
            
            // Override this method để implement loading logic cụ thể
            OnLoadResourcesInternal();
            
            currentState = ZoneState.Active;
            OnResourcesLoaded?.Invoke(this);
        }

        public virtual void UnloadResources()
        {
            if (currentState == ZoneState.Unloading || currentState == ZoneState.Inactive)
                return;

            currentState = ZoneState.Unloading;
            
            // Deactivate all scenes first
            foreach (var scene in scenes.Values)
            {
                scene.Deactivate();
            }
            
            // Override this method để implement unloading logic cụ thể
            OnUnloadResourcesInternal();
            
            currentState = ZoneState.Inactive;
            OnResourcesUnloaded?.Invoke(this);
        }

        public virtual void Activate()
        {
            if (currentState != ZoneState.Active)
                LoadResources();

            resourceContainer.SetActive(true);
            
            // Activate all scenes
            foreach (var scene in scenes.Values)
            {
                scene.Activate();
            }

            OnZoneActivated?.Invoke(this);
        }

        public virtual void Deactivate()
        {
            resourceContainer.SetActive(false);
            
            // Deactivate all scenes
            foreach (var scene in scenes.Values)
            {
                scene.Deactivate();
            }

            if (!IsAlwaysVisible)
            {
                UnloadResources();
            }

            OnZoneDeactivated?.Invoke(this);
        }

        public void AddScene(IScene scene)
        {
            if (scene == null) return;

            var sceneType = scene.GetType();
            scenes[sceneType] = scene;
            scene.ParentZone = this;
        }

        public void RemoveScene(IScene scene)
        {
            if (scene == null) return;

            var sceneType = scene.GetType();
            if (scenes.ContainsKey(sceneType))
            {
                scenes[sceneType].ParentZone = null;
                scenes.Remove(sceneType);
            }
        }

        public T GetScene<T>() where T : IScene
        {
            var sceneType = typeof(T);
            return scenes.ContainsKey(sceneType) ? (T)scenes[sceneType] : default(T);
        }

        public IEnumerable<IScene> GetAllScenes()
        {
            return scenes.Values;
        }

        // Abstract methods để subclass implement
        protected abstract void OnLoadResourcesInternal();
        protected abstract void OnUnloadResourcesInternal();

        // Utility method để tìm và add scenes từ children
        protected virtual void AutoDiscoverScenes()
        {
            var sceneComponents = GetComponentsInChildren<IScene>(true);
            foreach (var scene in sceneComponents)
            {
                AddScene(scene);
            }
        }
    }
}
