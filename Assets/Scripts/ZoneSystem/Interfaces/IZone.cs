using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneSystem.Interfaces
{
    public interface IZone
    {
        int Priority { get; }
        ZoneType Type { get; }
        ZoneState State { get; }
        bool IsAlwaysVisible { get; }
        string ZoneName { get; }

        event Action<IZone> OnZoneActivated;
        event Action<IZone> OnZoneDeactivated;
        event Action<IZone> OnResourcesLoaded;
        event Action<IZone> OnResourcesUnloaded;

        void LoadResources();
        void UnloadResources();
        void Activate();
        void Deactivate();
        
        void AddScene(IScene scene);
        void RemoveScene(IScene scene);
        T GetScene<T>() where T : IScene;
        IEnumerable<IScene> GetAllScenes();
    }
}
